using System.Threading;
using MMOR.NET.RichString;

namespace MMOR.NET.MTMC {
/**
 * <summary>
 *  <para>
 *    CRTP base for thread-safe simulation data.
 *    <see cref="TestHarness{T}"/> calls <see cref="InterlockedSingleSim"/>,
 *    <see cref="InterlockedCombine"/>, <see cref="InterlockedClear"/>
 *    which manage locking internally.
 *  </para>
 *  <para>
 *    Override <see cref="SingleSim"/>, <see cref="Combine"/>,
 *    and <see cref="Clear"/> to define simulation logic.
 *  </para>
 * </summary>
 */
public abstract class SimulationObject<T>
    where T : SimulationObject<T> {
  //================
  // Generic Data
  //================
  internal string? kRngIdentifier { get; set; }
  public ulong total_iterations { get; private set; }
  private readonly ManualResetEventSlim pause_gate_ = new(true);
  private readonly SemaphoreSlim process_lock_      = new(1, 1);

  //================
  // Pretty Print
  //================
  public abstract IRichString PrettyPrintHeader();
  public abstract IRichString PrettyPrintBody();

  //================
  // Methods
  //================
  public void InterlockedCombine(T add_data) {
    process_lock_.Wait();
    try {
      add_data.process_lock_.Wait();
      try {
        Combine(add_data);
        total_iterations += add_data.total_iterations;
      } finally {
        add_data.process_lock_.Release();
      }
    } finally {
      process_lock_.Release();
    }
  }

  /**
   * <summary>
   *  Defines how two separate <see cref="SimulationObject{T}"/> combines their data.
   * </summary>
   */
  protected abstract void Combine(T add_data);

  internal void InterlockedClear() {
    process_lock_.Wait();
    try {
      Clear();
      total_iterations = 0ul;
    } finally {
      process_lock_.Release();
    }
  }

  protected abstract void Clear();

  internal void InterlockedSingleSim(CancellationToken cancel_token) {
    pause_gate_.Wait(cancel_token);

    process_lock_.Wait();
    try {
      SingleSim();
      ++total_iterations;
    } finally {
      process_lock_.Release();
    }
  }

  protected abstract void SingleSim();

  internal void InterlockedDispose() {
    Clear();
    Dispose();
    pause_gate_.Dispose();
    process_lock_.Dispose();
  }

  protected virtual void Dispose() {}

  internal void Pause() => pause_gate_.Reset();

  internal void Unpause() => pause_gate_.Set();
}
}
