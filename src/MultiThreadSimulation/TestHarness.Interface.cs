using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MMOR.NET.Random;
using MMOR.NET.RichString;

namespace MMOR.NET.MultiThreadSimulation {

public partial class TestHarness<T> : ITestHarness
    where T : ISimulationObject<T> {
  public event Action<Exception, string>? OnExceptionCatch;
  public event Action? OnStart;
  public event Action? OnFinish;
  public event Action? OnHoldInput;
  public event Action? OnReleaseInput;

  /**
   * <summary>
   *  <para>
   *    When this fires off, sends a reference to a
   *    cummulation of the data from each thread and a metadata
   *    for extra information about the simulation progress.
   *  </para>
   *  <br/> For non-asynchronous functions, return a <see cref="Task.CompletedTask"/>.
   * </summary>
   */
  public Func<T, ReportMetadata, Task>? OnReport;

  public bool CurrentlyTesting { get; private set; }

  public void StopTest() {
    if (!CurrentlyTesting)
      return;
    OnHoldInput?.Invoke();
    if (stop_source_ != null && stop_source_.Token.CanBeCanceled)
      stop_source_.Cancel();
  }

  private static TaskCompletionSource<bool> ResetPokeTask() {
    return new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
  }
  private TaskCompletionSource<bool> poke_task_ = ResetPokeTask();
  public void PokeReport() {
    if (!CurrentlyTesting)
      return;
    OnHoldInput?.Invoke();
    poke_task_.TrySetResult(true);
  }
}

}
