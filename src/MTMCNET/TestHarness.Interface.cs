using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMOR.NET.Random;
using MMOR.NET.RichString;

namespace MMOR.NET.MTMC {

public partial class TestHarness<T> : ITestHarness
    where T : SimulationObject<T> {
  public event Action<Exception, string>? OnExceptionCatch;
  public event Action? OnStart;
  public event Action? OnFinish;
  public event Action? OnHoldInput;
  public event Action? OnReleaseInput;
  public event Action<IRichString, IRichString>? OnReport;

  private static readonly int kMaxThread = Environment.ProcessorCount - 1;
  public bool CurrentlyTesting { get; private set; }

  public void StopTest() {
    if (!CurrentlyTesting)
      return;
    OnHoldInput?.Invoke();
    if (stop_source_ != null && stop_source_.Token.CanBeCanceled)
      stop_source_.Cancel();
  }

  private TaskCompletionSource<bool> poke_report_ =
      new(TaskCreationOptions.RunContinuationsAsynchronously);
  public void PokeReport() {
    if (!CurrentlyTesting)
      return;
    poke_report_.TrySetResult(true);
  }

  private Exception? ErrorCheck(SimulationConfig<T> sim_config) {
    if (sim_config.sim_obj_ctor == null)
      throw new Exception("SimulationObject<T> constructor is not set.");
    if (sim_config.thread_count < 1 || sim_config.thread_count > kMaxThread) {
      int half                = (kMaxThread + 1) / 2;
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
}
}
