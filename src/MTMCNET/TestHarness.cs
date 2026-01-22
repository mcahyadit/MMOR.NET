using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMOR.NET.Random;
using MMOR.NET.RichString;
using MMOR.NET.Utilities;

namespace MMOR.NET.MTMC {
  public class TestHarness<T>
      where T : SimulationObject<T> {
    public event Action<Exception, string>? OnExceptionCatch;
    public event Action? OnStart;
    public event Action? OnFinish;
    public event Action? OnHoldInput;
    public event Action? OnReleaseInput;

    private static readonly ushort kMaxThread = (ushort)(Environment.ProcessorCount - 1);
    private CancellationTokenSource? stop_source_;
    public bool CurrentlyTesting { get; private set; }

    private atomic_uint64_t completed_iterations_ = 0u;

    private void SimulateChunk(T thread_data, ulong iterations) {
      ulong current_iteration = 0;
      try {
        for (; current_iteration < iterations; ++current_iteration) {
          if (stop_source_!.Token.IsCancellationRequested)
            break;
          thread_data.SingleSim_(stop_source_.Token);
          ++completed_iterations_;
        }
      } catch (Exception ex) {
        OnExceptionCatch?.Invoke(ex,
            $"TestHarness: Exception caught on rng: {thread_data.kRngIdentifier}, iteration: {current_iteration}.");
      }
    }

    public void StopTest() {
      if (!CurrentlyTesting)
        return;
      OnHoldInput?.Invoke();
      if (stop_source_ != null && stop_source_.Token.CanBeCanceled)
        stop_source_.Cancel();
    }

    private Exception? ErrorCheck(SimulationConfig<T> sim_config) {
      if (sim_config.sim_obj_ctor == null)
        throw new Exception("SimulationObject<T> constructor is not set.");
      if (sim_config.thread_count < 1 || sim_config.thread_count > kMaxThread) {
        var half                = (ushort)((kMaxThread + 1) / 2);
        sim_config.thread_count = half;
      }
      if (!sim_config.rng_ctor.Any()) {
        sim_config.rng_ctor.Capacity = sim_config.thread_count;
        for (var i = 0; i < sim_config.thread_count; ++i)
          sim_config.rng_ctor.Add(() => new MT19937());
      }
      // if (sim_config.rng_ctor.Count > sim_config.thread_count)
      //   std::cerr << std::format(
      //       "TestHarness: You have more `rng_ctor` ({}) than `thread_count` " "({}).\r\n",
      //       sim_config.rng_ctor.Count, sim_config.thread_count);
      if (sim_config.rng_ctor.Count < sim_config.thread_count) {
        return new ArgumentException(
            string.Format("TestHarness: You have less `rng_ctor` ({0}) than `thread_count` ({1})\n",
                sim_config.rng_ctor, sim_config.thread_count));
      }
      // if (sim_config.check_rate < 0 || sim_config.check_rate > 1) {
      //   std::cerr << std::format(
      //       "Invalid `check_rate` argument, was {}. Using default value " "`0.01f`.\r\n",
      //       sim_config.thread_count);
      //   sim_config.check_rate = 0.01f;
      // }
      if (sim_config.minimum_wait > sim_config.maximum_wait) {
        return new ArgumentException(string.Format(
            "TestHarness: `minimum_wait` {0}, needs to be less or equal than `maximum_wait` {1}",
            sim_config.minimum_wait, sim_config.maximum_wait));
      }
      return null;
    }

    // SimData
    private List<T> thread_data_list_             = null!;
    public IReadOnlyList<T> thread_data_list     => thread_data_list_;
    private List<string> rng_identifiers_         = null!;
    public IReadOnlyList<string> rng_identifiers => rng_identifiers_;
    public T full_sim_data                        = null!;

    public async Task RunTest(SimulationConfig<T> sim_config) {
      Exception? error = ErrorCheck(sim_config);
      if (error != null) {
        OnExceptionCatch?.Invoke(error, "TestHarness: Error during initialization.");
        return;
      }

      OnHoldInput?.Invoke();
      CurrentlyTesting = true;
      stop_source_     = new CancellationTokenSource();
      //================
      // Interpret Data
      //================
      var check_threshold    = (ulong)(sim_config.check_rate * sim_config.target_iteration);
      ulong thread_iteration = sim_config.target_iteration / sim_config.thread_count;
      ulong thread_leftover  = sim_config.target_iteration % sim_config.thread_count;
      //================
      // Multi-thread Setup
      //================
      List<Task> thread_list     = new();
      thread_list.Capacity       = sim_config.thread_count;
      thread_data_list_          = new();
      thread_data_list_.Capacity = sim_config.thread_count;
      rng_identifiers_           = new();
      Stopwatch stop_watch       = new();
      stop_watch.Start();
      completed_iterations_ = 0;

      try {
        for (var i = 0; i < sim_config.thread_count; ++i) {
          ulong iterations           = thread_iteration + (i == 0 ? thread_leftover : 0);
          IRandom rng_algo           = sim_config.rng_ctor[i]();
          T thread_data              = sim_config.sim_obj_ctor(rng_algo);
          thread_data.kRngIdentifier = rng_algo.ToString();
          rng_identifiers_.Add(rng_algo.ToString());
          thread_data_list_.Add(thread_data);
          thread_list.Add(Task.Factory.StartNew(() => SimulateChunk(thread_data, iterations),
              TaskCreationOptions.LongRunning));
        }
      } catch (Exception ex) {
        OnExceptionCatch?.Invoke(ex, "Test Harness: Exception caught during instantiation of SimulationObject.");
      };

      // Fire Event
      OnStart?.Invoke();

      //================
      // Simulation Progress Checking
      // ..using increment and greater than comparison for more flexibility in
      // checking
      //================
      TimeSpan last_check_time    = stop_watch.Elapsed;
      ulong next_report_threshold = sim_config.initial_sprint.GetValueOrDefault(100);
      TimeSpan smart_wait         = sim_config.minimum_wait;

      full_sim_data    = sim_config.sim_obj_ctor(null!);
      ulong last_check = sim_config.target_iteration - check_threshold;

      while (completed_iterations_ < last_check) {
        if (stop_source_.IsCancellationRequested)
          break;

        if (completed_iterations_ >= next_report_threshold) {
          OnHoldInput?.Invoke();
          ulong last_iteration_count = full_sim_data.total_iterations;
          //================
          // Collection
          //================
          foreach (T thread_data in thread_data_list_) {
            thread_data.Pause();
            try {
              full_sim_data.Combine_(thread_data);
              thread_data.Clear_();
            } catch (Exception ex) {
              OnExceptionCatch?.Invoke(ex,
                  "TestHarness: Exception caught during combine and clear.");
            }
            thread_data.Unpause();
          }
          //================
          // Fail Safe Exit
          /// This error is most likely caused by
          /// ..the imperfect implementation of <see cref="atomic_uint64_t"/>.
          /// ..as there are cases where <see cref="completed_iterations_"/>
          /// ..is less than <see cref="full_sim_data"/>'s total_iteration.
          //================
          if (full_sim_data.total_iterations >= last_check) {
            break;
          };
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
              sim_config.minimum_wait.TotalMilliseconds,
              sim_config.maximum_wait.TotalMilliseconds));
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
        try {
          await Task.Delay(smart_wait, stop_source_.Token);
        } catch (OperationCanceledException) {
          // The fact that I need to catch it feels weird
          break;
        }
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
          full_sim_data.Combine_(thread_data);
        } catch (Exception ex) {
          OnExceptionCatch?.Invoke(ex, "TestHarness: Exception caught during combine and clear.");
        }
        thread_data.Unpause();
        thread_data.Dispose_();
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

    //================
    // Print Handlers
    //================
    public event Action<IRichString, IRichString>? OnReport;
    private void ReportFull(ulong target_iteration, in TimeSpan total_time_elapsed, double speed,
        in T sim_data, bool print_body = false) {
      IRichString header =
          GenerateHeaderText(target_iteration, total_time_elapsed, speed, sim_data);
      IRichString body = print_body ? sim_data.PrettyPrintBody() : RichStringUtils.kRichEmpty;
      OnReport?.Invoke(header, body);
    }

    private IRichString GenerateHeaderText(ulong target_iterations, TimeSpan total_time_elapsed,
        double speed, in T sim_data) {
      RichStringBuilder strResult = new();

      //-+-+-+-+-+-+-+-+
      // Progress Information
      //-+-+-+-+-+-+-+-+
      ulong current_iterations   = sim_data.total_iterations;
      double estimated_time      = (target_iterations - current_iterations) / speed;
      float completionPercentage = (float)current_iterations / target_iterations;

      if (CurrentlyTesting) {
        strResult.Append("Current ")
            .Append(string.Format("{0:N0}", current_iterations))
            .Append(" (")
            .Append(completionPercentage.ToPercentage())
            .AppendLine(")");

        // There was some error when
        // ..averageSpeed == 0
        // .. do a check, just to prevent throw
        if (speed > 0 && target_iterations - current_iterations > 0) {
          strResult.Append("Current Speed: ")
              .Append(string.Format("{0:N2}", speed))
              .Append("/s")
              .Append(" | ")
              .Append("Est. time remaining: ")
              .Append(estimated_time.ToTime())
              .Append(" | ")
              .Append("Time Elapsed: ")
              .AppendLine(total_time_elapsed.TotalSeconds.ToTime());
        }
      } else {
        if (current_iterations >= target_iterations)
          strResult.Append("Completed ")
              .Append(string.Format("{0:N0}", current_iterations))
              .AppendLine();
        else
          strResult.Append("Aborted After ")
              .Append(string.Format("{0:N0}", current_iterations))
              .Append(" (")
              .Append(completionPercentage.ToPercentage())
              .AppendLine(")");

        strResult.Append("Average Speed: ")
            .Append(string.Format("{0:N2}", speed))
            .Append("/s")
            .Append(" | ")
            .Append("Completed in ")
            .AppendLine(total_time_elapsed.TotalSeconds.ToTime());
      }
      strResult.AppendLine(rng_identifiers.Join(", "));

      strResult.AppendLine();

      strResult.AppendLine(sim_data.PrettyPrintHeader());

      return strResult;
    }
  }
}
