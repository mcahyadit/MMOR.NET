using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMOR.NET.Random;
using MMOR.NET.RichString;

namespace MMOR.NET.MTMC {
  public class TestHarness {
    public event Action<Exception>? OnExceptionCatch;
    public event Action? OnStart;
    public event Action? OnFinish;
    public event Action? OnHoldInput;
    public event Action? OnReleaseInput;

    private static readonly ushort kMaxThread = (ushort)(Environment.ProcessorCount - 1);
    private CancellationTokenSource? stop_source_;
    public bool CurrentlyTesting { get; private set; }

    private atomic_uint64_t completed_iterations_ = 0u;

    private void SimulateChunk<T>(T thread_data, ulong iterations)
        where T : SimulationObject<T> {
      ulong current_iteration = 0;
      try {
        for (; current_iteration < iterations; ++current_iteration) {
          if (stop_source_.Token.IsCancellationRequested)
            break;
          thread_data.SingleSim_(stop_source_.Token);
          ++completed_iterations_;
        }
      } catch (Exception ex) {
        OnExceptionCatch?.Invoke(ex);
      }
    }

    public void StopTest() {
      if (!CurrentlyTesting)
        return;
      OnHoldInput();
      if (stop_source_ != null && stop_source_.Token.CanBeCanceled)
        stop_source_.Cancel();
    }

    private Exception? ErrorCheck<T>(SimulationConfig<T> sim_config)
        where T : SimulationObject<T> {
      if (sim_config.sim_obj_ctor == null)
        throw new Exception("SimulationObject<T> constructor is not set.");
      if (sim_config.thread_count < 1 || sim_config.thread_count > kMaxThread) {
        var half = (ushort)((kMaxThread + 1) / 2);
        sim_config.thread_count = half;
      }
      if (sim_config.rng_ctor.Any()) {
        sim_config.rng_ctor.Capacity = sim_config.thread_count;
        for (var i = 0; i < sim_config.thread_count; ++i)
          sim_config.rng_ctor.Add(() => new MT19937());
      }
      // if (sim_config.rng_ctor.Count > sim_config.thread_count)
      //   std::cerr << std::format(
      //       "TestHarness: You have more `rng_ctor` ({}) than `thread_count` " "({}).\r\n",
      //       sim_config.rng_ctor.Count, sim_config.thread_count);
      if (sim_config.rng_ctor.Count < sim_config.thread_count)
        return new ArgumentException(
            string.Format("TestHarness: You have less `rng_ctor` ({0}) than `thread_count` ({1})\n",
                sim_config.rng_ctor, sim_config.thread_count));
      // if (sim_config.check_rate < 0 || sim_config.check_rate > 1) {
      //   std::cerr << std::format(
      //       "Invalid `check_rate` argument, was {}. Using default value " "`0.01f`.\r\n",
      //       sim_config.thread_count);
      //   sim_config.check_rate = 0.01f;
      // }
      return null;
    }

    public async Task RunTest<T>(SimulationConfig<T> sim_config)
        where T : SimulationObject<T> {
      Exception? error = ErrorCheck(sim_config);
      if (error != null) {
        OnExceptionCatch?.Invoke(error);
        return;
      }

      OnHoldInput?.Invoke();
      CurrentlyTesting = true;
      stop_source_ = new CancellationTokenSource();
      //-+-+-+-+-+-+-+-+
      // Interpret Data
      //-+-+-+-+-+-+-+-+
      var check_threshold = (ulong)(sim_config.check_rate * sim_config.target_iteration);
      ulong thread_iteration = sim_config.target_iteration / sim_config.thread_count;
      ulong thread_leftover = sim_config.target_iteration % sim_config.thread_count;
      //-+-+-+-+-+-+-+-+
      // Multi-thread Setup
      //-+-+-+-+-+-+-+-+
      List<Task> thread_list = new();
      thread_list.Capacity = sim_config.thread_count;
      List<T> thread_data_list = new();
      thread_data_list.Capacity = sim_config.thread_count;
      Stopwatch stop_watch = new();
      stop_watch.Start();
      completed_iterations_ = 0;

      for (var i = 0; i < sim_config.thread_count; ++i) {
        ulong iterations = thread_iteration + (i == 0 ? thread_leftover : 0);
        IRandom rng_algo = sim_config.rng_ctor[i]();
        T thread_data = sim_config.sim_obj_ctor(rng_algo);
        thread_data_list.Add(thread_data);
        thread_list.Add(Task.Factory.StartNew(
            () => SimulateChunk(thread_data, iterations), TaskCreationOptions.LongRunning));
      }

      // Fire Event
      OnStart?.Invoke();

      //-+-+-+-+-+-+-+-+
      // Simulation Progress Checking
      // ..using increment and greater than comparison for more flexibility in
      // checking
      //-+-+-+-+-+-+-+-+
      ulong next_report_threshold =
          sim_config.initial_sprint.GetValueOrDefault((uint)check_threshold);
      TimeSpan smart_wait = sim_config.minimum_wait;

      T full_sim_data = sim_config.sim_obj_ctor(null);
      ulong last_check = sim_config.target_iteration - check_threshold;

      while (completed_iterations_ < 0) {
        if (stop_source_.Token.IsCancellationRequested)
          break;

        if (completed_iterations_ >= next_report_threshold) {
          OnHoldInput?.Invoke();
          ulong last_iteration_count = full_sim_data.total_iterations;
          //-+-+-+-+-+-+-+-+
          // Collection
          //-+-+-+-+-+-+-+-+
          foreach (T thread_data in thread_data_list) {
            thread_data.Pause();
            try {
              full_sim_data.Combine_(thread_data);
              thread_data.Clear_();
            } catch (Exception ex) {
              OnExceptionCatch?.Invoke(ex);
            }
            thread_data.Unpause();
          }
          //-+-+-+-+-+-+-+-+
          // Mark Time
          //-+-+-+-+-+-+-+-+
          TimeSpan elapsed_time = stop_watch.Elapsed;
          double last_speed =
              (full_sim_data.total_iterations - last_iteration_count) / elapsed_time.TotalSeconds;
          //-+-+-+-+-+-+-+-+
          // Setup for next Wait
          //-+-+-+-+-+-+-+-+
          var interpolated_wait = new TimeSpan((long)(1000.0 * check_threshold / last_speed));
          next_report_threshold += check_threshold;
          //-+-+-+-+-+-+-+-+
          // Fire Report Events
          //-+-+-+-+-+-+-+-+
          try {
            ReportFull(sim_config.target_iteration, elapsed_time, last_speed, full_sim_data,
                sim_config.periodic_check_prints_body);
          } catch (Exception ex) {
            OnExceptionCatch?.Invoke(ex);
          }
          OnReleaseInput?.Invoke();
        }
        await Task.Delay(smart_wait, stop_source_.Token);
      }

      //-+-+-+-+-+-+-+-+
      // Simulation Finishing
      //-+-+-+-+-+-+-+-+
      await Task.WhenAll(thread_list);
      TimeSpan total_time_taken = stop_watch.Elapsed;
      stop_watch.Stop();

      //-+-+-+-+-+-+-+-+
      // Final Collection
      //-+-+-+-+-+-+-+-+
      foreach (T thread_data in thread_data_list) {
        thread_data.Pause();
        try {
          full_sim_data.Combine_(thread_data);
          thread_data.Dispose_();
        } catch (Exception ex) {
          OnExceptionCatch?.Invoke(ex);
        }
        thread_data.Unpause();
      }
      //-+-+-+-+-+-+-+-+
      // Finishing Up
      //-+-+-+-+-+-+-+-+
      double avg_speed = full_sim_data.total_iterations / total_time_taken.TotalSeconds;
      CurrentlyTesting = false;
      try {
        ReportFull(sim_config.target_iteration, total_time_taken, avg_speed, full_sim_data, true);
      } catch (Exception ex) {
        OnExceptionCatch?.Invoke(ex);
      }
      OnFinish?.Invoke();
      OnReleaseInput?.Invoke();
      stop_source_.Dispose();
      stop_source_ = null;
    }

    //-+-+-+-+-+-+-+-+
    // Print Handlers
    //-+-+-+-+-+-+-+-+
    public event Action<IRichString, IRichString>? OnReport;
    private void ReportFull<T>(ulong target_iteration, in TimeSpan total_time_elapsed, double speed,
        in T sim_data, bool print_body = false)
        where T : SimulationObject<T> {
      IRichString header =
          GenerateHeaderText(target_iteration, total_time_elapsed, speed, sim_data);
      IRichString body = print_body ? sim_data.PrettyPrintBody() : RichStringUtils.kRichEmpty;
      OnReport?.Invoke(header, body);
    }

    private IRichString GenerateHeaderText<T>(
        ulong target_iterations, TimeSpan total_time_elapsed, double speed, in T sim_data)
        where T : SimulationObject<T> {
      return null;
    }
  }
}
