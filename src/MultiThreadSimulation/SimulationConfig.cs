using System;
using System.Collections.Generic;
using MMOR.NET.Random;

namespace MMOR.NET.MultiThreadSimulation {

/**
 * <summary>
 *  Specifies the options of <see cref="TestHarness{T}.RunTest(SimulationConfig{T})"/>.
 * </summary>
 */
public class SimulationConfig<T>
    where T : ISimulationObject<T> {
  /**
   * <summary>
   *  <br/> Constructor function to create the <see cref="ISimulationObject{T}"/> to be tested.
   *  <br/>
   *  <br/> The Constructor needs to take in <see cref="IRandom"/>.
   *  <br/> For determinism, each thread will need to be assigned it's own RNG.
   *  <br/>
   *  <br/> This will pass in what you declare in <see cref="rng_ctor"/>.
   * </summary>
   */
  public Func<IRandom, T> sim_obj_ctor = null!;

  /**
   * <summary>
   *  <br/> Number of threads to spawn.
   *  <br/> Defaults to half of your Logical Processor count.
   * </summary>
   */
  public int thread_count = Environment.ProcessorCount / 2;

  /**
   * <summary>
   *  <br/> Total number of iterations <see cref="ISimulationObject{T}.SingleSim()"/> is ran.
   *  <br/>
   *  <br/> Each thread will be assigned equal number of iterations to spin.
   *  <br/> If dividing this by <see cref="thread_count"/> results in a remainder.
   *        It will be assigned to the <strong>first</strong> thread.
   *  <br/>
   *  <br/> <strong>DETERMINISTIC</strong>:
   *  <br/> Even if some threads were to finish first, it <strong> WILL NOT </strong>
   *        redistribute the iterations.
   * </summary>
   */
  public ulong target_iteration = 1_000_000_000;

  /**
   * <summary>
   *  <br/> For Example:
   *  <para>
   *    a <c>check_rate = 0.01f</c> on <c>target_iteration = 1B</c>,
   *    will fires <see cref="TestHarness{T}.OnReport"/> approximately every
   *    <i>10,000,000</i> iterations.
   *  </para>
   *  <br/> Defaults to 10%.
   * </summary>
   */
  public float check_rate = 0.01f;

  /**
   * <summary>
   *  <br/> List of Constructor functions that creates the RNG for each thread.
   *  <br/>
   *  <br/> Size needs to either match <see cref="thread_count"/>, <c>0</c> or <c>1</c>.
   *  <br/> If there is only one, it will use the same constructor for all threads.
   *  <br/> If this is empty or null, it will use <see cref="MT19937"/> of random seeds.
   * </summary>
   */
  public List<Func<IRandom>> rng_ctor = null!;

  /**
   * <summary>
   *  <br/> On fast simulations, the reporting can just bottleneck the process.
   *  <br/> This <see cref="TimeSpan"/> defines the minimum wait between each report,
   *        that takes priority over <see cref="check_rate"/>.
   *  <br/> Defaults to 0.333 seconds.
   * </summary>
   */
  public TimeSpan minimum_wait = TimeSpan.FromMilliseconds(333);

  /**
   * <summary>
   *  <br/> On slow simulations, sets the limit for maximum wait, ignoring the value of
   *        <see cref="check_rate"/>.
   *  <br/> Defaults to 1 minute.
   * </summary>
   */
  public TimeSpan maximum_wait = TimeSpan.FromMinutes(1);

  /**
   * <summary>
   *  <br/> When this contains value, overrides the <see cref="check_rate"/> for the first check.
   *  <br/> Particularly useful to provide feedback that the simulation does starts properly.
   *  <br/> Defaults to <see langword="null"/> which resolved to the <see cref="check_rate"/>.
   * </summary>
   */
  public uint? initial_sprint = null;
}

internal static class SimulationConfigExtension {
  private static readonly int kMaxThread = Environment.ProcessorCount - 1;

  internal static Exception? AssertValues<T>(this SimulationConfig<T> sim_config)
      where T : ISimulationObject<T> {
    if (sim_config.sim_obj_ctor == null)
      return new ArgumentException("[ERROR]: ISimulationObject<T> constructor is not set.");

    if (sim_config.thread_count < 1 || sim_config.thread_count > kMaxThread) {
      int half                = (kMaxThread + 1) / 2;
      sim_config.thread_count = half;
    }

    // Validate RNG Ctor
    sim_config.rng_ctor ??= new();
    if (sim_config.rng_ctor.Capacity < sim_config.thread_count)
      sim_config.rng_ctor.Capacity = sim_config.thread_count;
    switch (sim_config.rng_ctor.Count) {
      case 0: {
        for (int i = 0; i < sim_config.thread_count; ++i) {
          sim_config.rng_ctor.Add(() => new MT19937());
        }
        break;
      }

      case 1: {
        if (sim_config.rng_ctor[0] == null)
          goto default;

        for (int i = 1; i < sim_config.thread_count; ++i) {
          sim_config.rng_ctor.Add(sim_config.rng_ctor[0]);
        }
        break;
      }

      default:
        if (sim_config.rng_ctor.Count < sim_config.thread_count) {
          return new ArgumentException(string.Format(
              "[ERROR]: there are less rng_ctor: {0} in SimulationConfig, than thread_count: {1}.",
              sim_config.rng_ctor, sim_config.thread_count));
        } else {
          for (int i = 0; i < sim_config.thread_count; ++i) {
            if (sim_config.rng_ctor[i] == null) {
              return new ArgumentNullException($"[ERROR]: rng_ctor[] of index {i} is null.");
            }
          }
        }
        break;
    }

    if (sim_config.minimum_wait > sim_config.maximum_wait) {
      return new ArgumentException(string.Format(
          "[ERROR]: minimum_wait {0}, needs to be less or equal than maximum_wait {1}",
          sim_config.minimum_wait, sim_config.maximum_wait));
    }
    return null;
  }
}
}
