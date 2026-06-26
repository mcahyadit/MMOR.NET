using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MMOR.NET.Random;

namespace MMOR.NET.MTMC {

public partial class TestHarness<T> : ITestHarness
    where T : SimulationObject<T> {
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
      string rng = thread_data.kRngIdentifier!;
      string ctx = $"TestHarness: Exception caught on rng: {rng}, iteration: {current_iteration}.";
      OnExceptionCatch?.Invoke(ex, ctx);
    }
  }

  public async Task RunTest(SimulationConfig<T> sim_config) {
    Exception? error = ErrorCheck(sim_config);
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
    List<Task> thread_list = new(sim_config.thread_count);
    thread_data_list_      = new(sim_config.thread_count);
    rng_identifiers_       = new(sim_config.thread_count);

    Stopwatch stop_watch = new();
    stop_watch.Start();
    completed_iterations_ = 0;

    try {
      for (var i = 0; i < sim_config.thread_count; ++i) {
        ulong iterations = thread_iteration + (i == 0 ? thread_leftover : 0);
        IRandom rng_algo = sim_config.rng_ctor[i].Invoke();
        T thread_data    = sim_config.sim_obj_ctor(rng_algo);

        string rng_identifier      = rng_algo.ToString()!;
        thread_data.kRngIdentifier = rng_identifier;
        rng_identifiers_.Add(rng_identifier);

        thread_data_list_.Add(thread_data);
        thread_list.Add(Task.Factory.StartNew(() => SimulateChunk(thread_data, iterations),
            TaskCreationOptions.LongRunning));
      }
    } catch (Exception ex) {
      OnExceptionCatch?.Invoke(ex,
          "Test Harness: Exception caught during instantiation of SimulationObject.");
    };

    // Fire Event
    OnStart?.Invoke();

    //================
    // Simulation Progress Checking
    // .. using increment and greater than comparison
    // .. for more flexibility in checking
    //================
    TimeSpan last_check_time    = stop_watch.Elapsed;
    ulong next_report_threshold = Math.Max(1ul, sim_config.initial_sprint.GetValueOrDefault(100));
    TimeSpan smart_wait         = sim_config.minimum_wait;

    full_sim_data    = sim_config.sim_obj_ctor(null!);
    ulong last_check = sim_config.target_iteration - check_threshold;

    while (completed_iterations_.Value < last_check) {
      if (stop_source_.IsCancellationRequested)
        break;

      bool poke_report = poke_report_.Task.IsCompleted;
      if (poke_report || completed_iterations_.Value >= next_report_threshold) {
        // if (completed_iterations_ >= next_report_threshold) {
        if (poke_report) {
          poke_report_ = new(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        OnHoldInput?.Invoke();
        ulong last_iteration_count = full_sim_data.total_iterations;
        //================
        // Collection
        //================
        foreach (T thread_data in thread_data_list_) {
          thread_data.Pause();
          try {
            full_sim_data.InterlockedCombine(thread_data);
            thread_data.InterlockedClear();
          } catch (Exception ex) {
            OnExceptionCatch?.Invoke(ex, "TestHarness: Exception caught during combine and clear.");
          }
          thread_data.Unpause();
        }
        // //================
        // // Fail Safe Exit
        // /// This error is most likely caused by
        // /// ..the imperfect implementation of <see cref="AtomicUInt64"/>.
        // /// ..as there are cases where <see cref="completed_iterations_"/>
        // /// ..is less than <see cref="full_sim_data"/>'s total_iteration.
        // //================
        // if (full_sim_data.total_iterations >= last_check) {
        //   break;
        // };
        //================
        // Mark Time
        //================
        TimeSpan elapsed_time = stop_watch.Elapsed - last_check_time;
        double last_speed =
            (full_sim_data.total_iterations - last_iteration_count) / elapsed_time.TotalSeconds;
        //================
        // Setup for next Wait
        //================
        var interpolated_wait = TimeSpan.FromSeconds(check_threshold / last_speed);
        smart_wait = TimeSpan.FromMilliseconds(Math.Clamp(interpolated_wait.TotalMilliseconds,
            sim_config.minimum_wait.TotalMilliseconds, sim_config.maximum_wait.TotalMilliseconds));
        next_report_threshold += check_threshold;
        last_check_time = stop_watch.Elapsed;
        //================
        // Fire Report Events
        //================
        try {
          ReportFull(sim_config.target_iteration, stop_watch.Elapsed, last_speed, full_sim_data,
              sim_config.periodic_check_prints_body);
        } catch (Exception ex) {
          OnExceptionCatch?.Invoke(ex, "TestHarness: Exception caught during PrettyPrinting.");
        }
        OnReleaseInput?.Invoke();
      }
      await Task.WhenAny(Task.Delay(smart_wait), stop_tcs.Task, poke_report_.Task);
    }

    //================
    // Simulation Finishing
    //================
    await Task.WhenAll(thread_list);
    TimeSpan total_time_taken = stop_watch.Elapsed;
    stop_watch.Stop();

    //================
    // Final Collection
    //================
    foreach (T thread_data in thread_data_list_) {
      thread_data.Pause();
      try {
        full_sim_data.InterlockedCombine(thread_data);
      } catch (Exception ex) {
        OnExceptionCatch?.Invoke(ex, "TestHarness: Exception caught during combine and clear.");
      }
      thread_data.Unpause();
      thread_data.InterlockedDispose();
    }

    //================
    // Finishing Up
    //================
    double avg_speed = full_sim_data.total_iterations / total_time_taken.TotalSeconds;
    CurrentlyTesting = false;
    try {
      ReportFull(sim_config.target_iteration, total_time_taken, avg_speed, full_sim_data, true);
    } catch (Exception ex) {
      OnExceptionCatch?.Invoke(ex, "TestHarness: Exception caught during PrettyPrinting.");
    }
    OnFinish?.Invoke();
    OnReleaseInput?.Invoke();
    stop_source_.Dispose();
    stop_source_ = null;
  }
}
}
