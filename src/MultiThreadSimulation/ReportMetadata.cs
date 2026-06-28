using System;
using System.Collections.Generic;
using MMOR.NET.Random;

namespace MMOR.NET.MultiThreadSimulation {

public struct ReportMetadata {
  /**
   * <summary>
   *  Number of iterations from last <see cref="TestHarness{T}.OnReport"/>.
   * </summary>
   */
  public ulong this_iterations { get; init; }
  /**
   * <summary>
   *  <see cref="TimeSpan"/> elapsed from last <see cref="TestHarness{T}.OnReport"/>.
   * </summary>
   */
  public TimeSpan this_time { get; init; }

  /**
   * <summary>
   *  Number of iterations from beginning.
   * </summary>
   */
  public ulong total_iterations { get; init; }
  /**
   * <summary>
   *  <see cref="TimeSpan"/> elapsed from beginning.
   * </summary>
   */
  public TimeSpan total_time { get; init; }

  public ulong target_iterations { get; init; }
  /**
   * <summary>
   *  Will be <c>false</c> on the last <see cref="TestHarness{T}.OnReport"/>.
   * </summary>
   */
  public bool currently_testing { get; init; }

  /**
   * <summary>
   *  Arrays of the <see cref="IRandom"/> passed through <see cref="object.ToString()"/>.
   * </summary>
   */
  public IReadOnlyList<string> rng_identifiers { get; init; }
}

}
