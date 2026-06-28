using System;
using MMOR.NET.RichString;

namespace MMOR.NET.MultiThreadSimulation {
/**
 * <summary>
 *  <br/> Sets of the more generic <see cref="TestHarness{T}"/> methods.
 *  <br/> Can be used when need to interact regardless of the <see cref="ISimulationObject{T}"/>
 * </summary>
 */
public interface ITestHarness {
  /**
   * <summary>
   *  Triggers a proper <see cref="TestHarness{T}.OnReport"/>
   *  ignoring the progress of <see cref="SimulationConfig{T}.check_rate"/>.
   * </summary>
   */
  public void PokeReport();

  public void StopTest();

  public bool CurrentlyTesting { get; }

  /**
   * <summary>
   *  <br/> Raised whenever there is an <see cref="Exception"/> caught during the process.
   *  <br/> Passes the exception and an additional string containing extra context.
   * </summary>
   */
  public event Action<Exception, string>? OnExceptionCatch;

  /**
   * <summary>
   *  Raised once when all threads have started.
   * </summary>
   */
  public event Action? OnStart;

  /**
   * <summary>
   *  <br/> Raised once when all threads have finished.
   *  <br/> The last <see cref="ISimulationObject{T}"/> will be raised first before this.
   * </summary>
   */
  public event Action? OnFinish;

  /**
   * <summary>
   *  <br/> Utility event to be used in pairs with <see cref="OnReleaseInput"/>.
   *  <para>
   *    I made this as I needed a way to signal the UI whether the <see cref="TestHarness{T}"/>
   *    is ready to accept new input including stopping it.
   *  </para>
   *  <para>
   *    There was a case where clicking the button that triggers
   *    <see cref="TestHarness{T}.RunTest(SimulationConfig{T})"/>, results in multiple
   *    simulations running at the same time, with only one having proper reference.
   *  </para>
   *  <br/> This one is meant to signal the UI to stop receiving input.
   * </summary>
   */
  public event Action? OnHoldInput;

  /**
   * <summary>
   *  Paired with <see cref="OnHoldInput"/>.
   *  Signals UI that it is ready to accept input again.
   * </summary>
   */
  public event Action? OnReleaseInput;
}
}
