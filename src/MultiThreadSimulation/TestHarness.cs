using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MMOR.NET.Random;

namespace MMOR.NET.MultiThreadSimulation {

public partial class TestHarness<T> : ITestHarness
    where T : ISimulationObject<T> {
  private AtomicUInt64 completed_iterations_ = 0u;
  private CancellationTokenSource? stop_source_;

  private void SimulateChunk(T thread_data, ulong target_iterations) {
    ulong current_iteration = 0;
    try {
      for (; current_iteration < target_iterations; ++current_iteration) {
        if (stop_source_!.Token.IsCancellationRequested)
          break;

        thread_data.InterlockedSingleSim(stop_source_.Token);
        completed_iterations_.Increment();
      }
    } catch (Exception ex) {
      string rng = thread_data.sim_meta.rng!;
      string ctx = $"TestHarness: Exception caught on rng: {rng}, iteration: {current_iteration}.";
      OnExceptionCatch?.Invoke(ex, ctx);
    }
  }

  public async Task RunTest(SimulationConfig<T> sim_config) {
    Exception? error = sim_config.AssertValues();
    if (error != null) {
      OnExceptionCatch?.Invoke(error, "TestHarness: Error during initialization.");
      return;
    }

    OnHoldInput?.Invoke();
    CurrentlyTesting = true;
    stop_source_     = new CancellationTokenSource();

    // Needed to allow Report Poking
    TaskCompletionSource<bool> stop_tcs = new();
    stop_source_.Token.Register(() => stop_tcs.TrySetResult(true));

    //================
    // Interpret Data
    //================
    ulong check_threshold  = (ulong)(sim_config.check_rate * sim_config.target_iteration);
    ulong thread_iteration = sim_config.target_iteration / (uint)sim_config.thread_count;
    ulong thread_leftover  = sim_config.target_iteration % (uint)sim_config.thread_count;

    //================
    // Multi-thread Setup
    //================
    List<Task> tasks    = new(sim_config.thread_count);
    List<T> sim_objects = new(sim_config.thread_count);
    List<string> rngs   = new(sim_config.thread_count);
    Barrier barrier     = new(sim_config.thread_count + 1);

    completed_iterations_ = 0;
    try {
      for (int i = 0; i < sim_config.thread_count; ++i) {
        ulong iterations = thread_iteration + (i == 0 ? thread_leftover : 0);
        IRandom rng_algo = sim_config.rng_ctor[i].Invoke();
        T thread_data    = sim_config.sim_obj_ctor(rng_algo);

        string rng_identifier    = rng_algo.ToString()!;
        thread_data.sim_meta.rng = rng_identifier;
        rngs.Add(rng_identifier);

        sim_objects.Add(thread_data);
        tasks.Add(Task.Factory.StartNew(() => {
          barrier.SignalAndWait();  // Fair Start Timing
          SimulateChunk(thread_data, iterations);
        }, TaskCreationOptions.LongRunning));
      }
    } catch (Exception ex) {
      string ctx = "[ERROR]: Failed to construct ISimulationObject.";
      OnExceptionCatch?.Invoke(ex, ctx);
    };

    // Mark Start
    Stopwatch stop_watch = new();
    stop_watch.Start();
    OnStart?.Invoke();
    barrier.SignalAndWait();  // Wait for All to be ready before dispose
    barrier.Dispose();

    //================
    // Simulation Progress Checking
    // .. using increment and greater than comparison
    // .. for more flexibility in checking
    //================
    TimeSpan last_time   = new(0);
    ulong next_threshold = sim_config.initial_sprint.GetValueOrDefault((uint)check_threshold);
    next_threshold       = Math.Max(1ul, next_threshold);

    T full_sim_obj        = sim_config.sim_obj_ctor(null!);
    ulong final_threshold = sim_config.target_iteration - check_threshold;

    while (completed_iterations_.Value < final_threshold) {
      if (stop_source_.IsCancellationRequested)
        break;

      bool poke_report = poke_task_.Task.IsCompleted;
      double speed;
      if (poke_report || completed_iterations_.Value >= next_threshold) {
        OnHoldInput?.Invoke();
        if (poke_report) {
          poke_task_ = ResetPokeTask();
        }

        //================
        // Collection
        //================
        // Track before collection
        ulong prev_iterations = full_sim_obj.sim_meta.total_iterations;
        foreach (T sim_obj in sim_objects) {
          sim_obj.Pause();
          try {
            full_sim_obj.InterlockedCombine(sim_obj);
            sim_obj.InterlockedClear();
          } catch (Exception ex) {
            string ctx = "[ERROR]: Failed to Combine and Clear ISimulationObject.";
            OnExceptionCatch?.Invoke(ex, ctx);
          }
          sim_obj.Unpause();
        }

        //================
        // Mark Time
        //================
        TimeSpan total_time = stop_watch.Elapsed;
        TimeSpan this_time  = total_time - last_time;
        last_time           = total_time;

        ulong total_iterations = full_sim_obj.sim_meta.total_iterations;
        ulong this_iterations  = total_iterations - prev_iterations;

        ReportMetadata report_meta = new() {
          this_time         = this_time,
          this_iterations   = this_iterations,
          total_time        = total_time,
          total_iterations  = total_iterations,
          target_iterations = sim_config.target_iteration,
          currently_testing = true,
          rng_identifiers   = rngs,
        };

        //================
        // Fire Report Events
        //================
        try {
          await OnReport?.Invoke(full_sim_obj, report_meta)!;
        } catch (Exception ex) {
          string ctx = "[ERROR]: Failed to run OnReport";
          OnExceptionCatch?.Invoke(ex, ctx);
        }
        OnReleaseInput?.Invoke();

        //================
        // Dynamic Waits
        // .. adjust wait time depending
        // .. on current speed
        //================
        speed = this_iterations / this_time.TotalSeconds;
      } else {
        speed = completed_iterations_.Value / stop_watch.Elapsed.TotalSeconds;
      }

      // Sets Cancellable Delay
      TimeSpan wait_time = speed == 0 ? sim_config.minimum_wait  //
                                      : TimeSpan.FromSeconds(check_threshold / speed);
      wait_time          = TimeSpan.FromMilliseconds(Math.Clamp(wait_time.TotalMilliseconds,
                   sim_config.minimum_wait.TotalMilliseconds, sim_config.maximum_wait.TotalMilliseconds));
      await Task.WhenAny(Task.Delay(wait_time), stop_tcs.Task, poke_task_.Task);
    }

    //================
    // Simulation Finishing
    //================
    await Task.WhenAll(tasks);
    TimeSpan total_time_taken = stop_watch.Elapsed;
    stop_watch.Stop();

    //================
    // Final Collection
    //================
    foreach (T thread_data in sim_objects) {
      thread_data.Pause();
      try {
        full_sim_obj.InterlockedCombine(thread_data);
      } catch (Exception ex) {
        string ctx = "[ERROR]: Failed to do final Combine and Clear ISimulationObject.";
        OnExceptionCatch?.Invoke(ex, ctx);
      }
      thread_data.Unpause();
      thread_data.InterlockedDispose();
    }

    //================
    // Finishing Up
    //================
    ulong final_iterations    = full_sim_obj.sim_meta.total_iterations;
    TimeSpan final_time       = stop_watch.Elapsed;
    ReportMetadata final_meta = new() {
      this_time         = final_time,
      this_iterations   = final_iterations,
      total_time        = final_time,
      total_iterations  = final_iterations,
      target_iterations = sim_config.target_iteration,
      currently_testing = false,
      rng_identifiers   = rngs,
    };

    CurrentlyTesting = false;
    try {
      await OnReport?.Invoke(full_sim_obj, final_meta)!;
    } catch (Exception ex) {
      string ctx = "[ERROR]: Failed to run OnReport";
      OnExceptionCatch?.Invoke(ex, ctx);
    }
    OnFinish?.Invoke();
    OnReleaseInput?.Invoke();
    stop_source_.Dispose();
    stop_source_ = null;
  }
}

}
