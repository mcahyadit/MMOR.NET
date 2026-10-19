using System;
using Xunit;

namespace MMOR.NET.Statistics {
public partial class RunningStatisticsTest {
  /**
   * <summary>
   *  Wraps a shared <see cref="SampleData"/> fixture with the precisions the
   *  <see cref="RunningStatistics"/> path needs. Note <see cref="RunningStatistics"/> only
   * implements a subset of the statistics <see cref="TotalStatistics"/> reports; everything else
   * on the fixture is simply ignored here.
   * </summary>
   */
  public struct RunningStatsParam {
    public required SampleData data { get; init; }
    public int count_precision { get; init; }          = 15;
    public int mean_precision { get; init; }           = 15;
    public int variance_precision { get; init; }       = 15;
    public int path_variance_precision { get; init; }  = 13;
    public int? path_skewness_precision { get; init; } = null;
    public int? path_kurtosis_precision { get; init; } = null;
    public int skewness_precision { get; init; }       = 15;
    public int kurtosis_precision { get; init; }       = 15;
    public int min_value_precision { get; init; }      = 15;
    public int max_value_precision { get; init; }      = 15;
    public RunningStatsParam() {}
    public override string ToString() => data.identifier;
  }

  [Theory]
  [MemberData(nameof(kCorrectnessParams))]
  public void CorrectnessTest(RunningStatsParam p) {
    SampleData d        = p.data;
    RunningStatistics s = new();
    for (int i = 0; i < d.values.Length; ++i) {
      s.Push(d.values[i]);
    }

    try {
      TestUtils.AssertApproximately(d.count, s.Count, p.count_precision);
      TestUtils.AssertApproximately(d.mean, s.Mean, p.mean_precision);
      TestUtils.AssertApproximately(d.variance, s.Variance, p.variance_precision);
      TestUtils.AssertApproximately(d.skewness, s.Skewness, p.skewness_precision);
      TestUtils.AssertApproximately(d.kurtosis, s.Kurtosis, p.kurtosis_precision);
      TestUtils.AssertApproximately(d.min_value, s.MinValue, p.min_value_precision);
      TestUtils.AssertApproximately(d.max_value, s.MaxValue, p.max_value_precision);
      Assert.Equal(d.min_count, s.MinCount);
      Assert.Equal(d.max_count, s.MaxCount);
    } catch {
      Console.Error.WriteLine(d.identifier);
      throw;
    }
  }
}
}
