using System;
using System.Collections.Generic;
using System.Threading;

namespace MMOR.NET.MultiThreadSimulation {

/**
 * <summary>
 *  A component needed by <see cref="TestHarness{T}"/>. <br/>
 *  <param>
 *    For non power users, just define with
 *    <c>= new()</c> and leave it be.
 * </param>
 * </summary>
 */
public class SimulationMetadata {
  public ulong total_iterations { get; internal set; } = 0;
  public readonly SemaphoreSlim process_lock           = new(1, 1);
  public readonly ManualResetEventSlim pause_gate      = new();
  public string? rng { get; internal set; }
}

public interface ISimulationObject<T>
    where T : ISimulationObject<T> {
  /// <inheritdoc cref="SimulationMetadata"/>
  public SimulationMetadata sim_meta { get; }

  /**
   * <summary>
   *  This function needs to define how two <see cref="ISimulationObject{T}"/>
   *  can combine their dataset.
   * </summary>
   */
  public void Combine(T add_data);

  /**
   * <summary>
   *  This function needs to define how the dataset
   *  are reset to its default state, as if no
   *  simulations are ran yet.
   * </summary>
   */
  public void Clear();

  /**
   * <summary>
   *  This function will be the one repeated over
   *  <see cref="SimulationConfig{T}.target_iteration"/>.
   * </summary>
   */
  public void SingleSim();
}

public static class SimulationExtensions {
  public static void InterlockedCombine<T>(this T sim_obj, T add_data)
      where T : ISimulationObject<T> {
    sim_obj.sim_meta.process_lock.Wait();
    try {
      add_data.sim_meta.process_lock.Wait();
      try {
        sim_obj.Combine(add_data);
        sim_obj.sim_meta.total_iterations += add_data.sim_meta.total_iterations;
      } finally {
        add_data.sim_meta.process_lock.Release();
      }
    } finally {
      sim_obj.sim_meta.process_lock.Release();
    }
  }

  public static void InterlockedClear<T>(this T sim_obj)
      where T : ISimulationObject<T> {
    sim_obj.sim_meta.process_lock.Wait();
    try {
      sim_obj.Clear();
      sim_obj.sim_meta.total_iterations = 0ul;
    } finally {
      sim_obj.sim_meta.process_lock.Release();
    }
  }

  public static void InterlockedSingleSim<T>(this T sim_obj, CancellationToken cancel_token)
      where T : ISimulationObject<T> {
    sim_obj.sim_meta.pause_gate.Wait(cancel_token);

#pragma warning disable CA2016
    // CA2016: Forward the 'CancellationToken' parameter to methods
    // This is not valid as the CancellationToken is for a different purpose
    sim_obj.sim_meta.process_lock.Wait();
#pragma warning restore CA2016
    try {
      sim_obj.SingleSim();
      ++sim_obj.sim_meta.total_iterations;
    } finally {
      sim_obj.sim_meta.process_lock.Release();
    }
  }

  public static void InterlockedDispose<T>(this T sim_obj)
      where T : ISimulationObject<T> {
    sim_obj.sim_meta.pause_gate.Dispose();
    sim_obj.sim_meta.process_lock.Dispose();
    if (sim_obj is IDisposable d)
      d.Dispose();
  }

  public static void Pause<T>(this T sim_obj)
      where T : ISimulationObject<T> {
    sim_obj.sim_meta.pause_gate.Reset();
  }

  public static void Unpause<T>(this T sim_obj)
      where T : ISimulationObject<T> {
    sim_obj.sim_meta.pause_gate.Set();
  }
}
}
