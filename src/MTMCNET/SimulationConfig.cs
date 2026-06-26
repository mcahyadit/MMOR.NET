using System;
using System.Collections.Generic;
using MMOR.NET.Random;

namespace MMOR.NET.MTMC {

/**
 * <summary>
 *  Specifies the options of <see cref="TestHarness{T}.RunTest(SimulationConfig{T})"/>.
 * </summary>
 */
public class SimulationConfig<T>
// Null checks are performed on the TestHarness side.
    where T : SimulationObject<T> {
  /**
   * <summary>
   *  <br/> Constructor function to create the <see cref="SimulationObject{T}"/> to be tested.
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
   *  <br/> Total number of iterations <see cref="SimulationObject{T}.SingleSim()"/> is ran.
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
   *    will fires <see cref="ITestHarness.OnReport"/> approximately every
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
   *  <br/> Size needs to either match <see cref="thread_count"/>, or <c>1</c>.
   *  <br/> If there is only one, it will use the same constructor for all threads.
   * </summary>
   */
  public List<Func<IRandom>> rng_ctor = null!;

  /**
   * <summary> <para>
   *  This boolean toggles whether <see cref="ITestHarness.OnReport"/>, excluding the last one,
   *  Also calls <see cref="SimulationObject{T}.PrettyPrintBody()"/>.
   * </para> </summary>
   */
  public bool periodic_check_prints_body = false;

  /**
   * <summary>
   *  <br/> On fast simulations, the reporting can just bottleneck the process.
   *  <br/> This <see cref="TimeSpan"/> defines the minimum wait between each report,
   *        that takes priority over <see cref="check_rate"/>.
   * </summary>
   */
  public TimeSpan minimum_wait = TimeSpan.FromMilliseconds(2368);

  /**
   * <summary>
   *  On slow simulations, sets the limit for maximum wait, ignoring the value of
   *  <see cref="check_rate"/>
   * </summary>
   */
  public TimeSpan maximum_wait = TimeSpan.FromSeconds(60);

  /**
   * <summary>
   *  <br/> When this contains value, overrides the <see cref="check_rate"/> for the first trigger.
   *  <br/> Particularly useful to provide feedback that the simulation does starts properly.
   * </summary>
   */
  public uint? initial_sprint = null;
}
}
