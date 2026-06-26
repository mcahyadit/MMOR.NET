using System;
using System.Linq;
using MMOR.NET.RichString;
using MMOR.NET.Utilities;

namespace MMOR.NET.MTMC {

public partial class TestHarness<T> : ITestHarness
    where T : SimulationObject<T> {
  //================
  // Print Handlers
  //================
  private void ReportFull(ulong target_iteration, in TimeSpan total_time_elapsed, double speed,
      in T sim_data, bool print_body = false) {
    IRichString header = GenerateHeaderText(target_iteration, total_time_elapsed, speed, sim_data);
    IRichString body   = print_body ? sim_data.PrettyPrintBody() : RichStringUtils.kRichEmpty;
    OnReport?.Invoke(header, body);
  }

  private IRichString GenerateHeaderText(ulong target_iterations, TimeSpan total_time_elapsed,
      double speed, in T sim_data) {
    RichStringBuilder result = new();

    //-+-+-+-+-+-+-+-+
    // Progress Information
    //-+-+-+-+-+-+-+-+
    ulong current_iterations   = sim_data.total_iterations;
    double estimated_time      = (target_iterations - current_iterations) / speed;
    float completionPercentage = (float)current_iterations / target_iterations;

    if (CurrentlyTesting) {
      result.Append("Current ")
          .Append(string.Format("{0:N0}", current_iterations))
          .Append(" (")
          .Append(completionPercentage.ToPercentage())
          .AppendLine(")");

      // There was some error when
      // ..averageSpeed == 0
      // .. do a check, just to prevent throw
      if (speed > 0 && target_iterations - current_iterations > 0) {
        result.Append("Current Speed: ")
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
        result.Append("Completed ")
            .Append(string.Format("{0:N0}", current_iterations))
            .AppendLine();
      else
        result.Append("Aborted After ")
            .Append(string.Format("{0:N0}", current_iterations))
            .Append(" (")
            .Append(completionPercentage.ToPercentage())
            .AppendLine(")");

      result.Append("Average Speed: ")
          .Append(string.Format("{0:N2}", speed))
          .Append("/s")
          .Append(" | ")
          .Append("Completed in ")
          .AppendLine(total_time_elapsed.TotalSeconds.ToTime());
    }
    result.AppendLine(rng_identifiers.Join(", "));

    result.AppendLine();

    result.AppendLine(sim_data.PrettyPrintHeader());

    return result;
  }
}
}
