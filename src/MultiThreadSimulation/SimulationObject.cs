using System;
using MMOR.NET.RichString;
using MMOR.NET.Utilities;

namespace MMOR.NET.MultiThreadSimulation {
/**
 * <summary>
 *  <para>
 *    CRTP base for thread-safe simulation data.
 *    <see cref="TestHarness{T}"/> calls
 *    <see cref="SimulationExtensions.InterlockedSingleSim"/>,
 *    <see cref="SimulationExtensions.InterlockedCombine"/>,
 *    <see cref="SimulationExtensions.InterlockedClear"/>
 *    which manage locking internally.
 *  </para>
 *  <para>
 *    Override <see cref="SingleSim"/>, <see cref="Combine"/>,
 *    and <see cref="Clear"/> to define simulation logic.
 *  </para>
 * </summary>
 */
[Obsolete("Deprecated in favor of ISimulationObject<T> interface.")]
public abstract class SimulationObject<T> : ISimulationObject<T>
    where T : SimulationObject<T> {
  public SimulationMetadata sim_meta { get; } = new();

  public ulong total_iterations => sim_meta.total_iterations;

  public abstract void Clear();

  public abstract void Combine(T add_data);

  public abstract IRichString PrettyPrintBody();

  public abstract IRichString PrettyPrintHeader();

  public abstract void SingleSim();

  public (IRichString header, IRichString body) OnReportCompat(ReportMetadata report_extra) {
    RichStringBuilder result = new();

    double speed             = report_extra.this_iterations / report_extra.this_time.TotalSeconds;
    ulong current_iterations = total_iterations;
    double estimated_time    = (report_extra.target_iterations - current_iterations) / speed;
    float completion_percentage = (float)current_iterations / report_extra.target_iterations;

    if (report_extra.currently_testing) {
      result.AppendLine(string.Format("Current {0:N0} ({1})", current_iterations,
          completion_percentage.ToPercentage()));

      // There was some error when
      // ..averageSpeed == 0
      // .. do a check, just to prevent throw
      if (speed > 0 && report_extra.target_iterations - current_iterations > 0) {
        result.AppendLine(
            string.Format("Current Speed: {0:N0}/s | Est. time remaining: {1} | Time Elapsed: {2}",
                speed, estimated_time.ToTime(), report_extra.total_time.TotalSeconds.ToTime()));
      }
    } else {
      if (current_iterations >= report_extra.target_iterations)
        result.AppendLine(string.Format("Completed {0:N0}", current_iterations));
      else
        result.AppendLine(string.Format("Aborted After {0:N0} ({1})", current_iterations,
            completion_percentage.ToPercentage()));

      result.AppendLine(string.Format("Average Speed: {0:N2}/s | Completed in {1}", speed,
          report_extra.total_time.TotalSeconds.ToTime()));
    }
    result.AppendLine(report_extra.rng_identifiers.Join(", "));

    result.AppendLine();
    result.AppendLine(PrettyPrintHeader());

    return (result, PrettyPrintBody());
  }
}
}
