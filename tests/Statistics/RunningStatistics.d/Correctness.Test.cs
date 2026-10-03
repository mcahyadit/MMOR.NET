using System;
using Xunit;

namespace MMOR.NET.Statistics {
public partial class RunningStatisticsTest {
  public struct CorrectnessParam {
    public string identifier { get; init; } = "Generic Test";
    public required double[] values { get; init; }
    public required double count { get; init; }
    public int count_precision { get; init; } = 15;
    public required double mean { get; init; }
    public int mean_precision { get; init; } = 15;
    public required double variance { get; init; }
    public int variance_precision { get; init; }       = 15;
    public int path_variance_precision { get; init; }  = 13;
    public int? path_skewness_precision { get; init; } = null;
    public int? path_kurtosis_precision { get; init; } = null;
    public required double skewness { get; init; }
    public int skewness_precision { get; init; } = 15;
    public required double kurtosis { get; init; }
    public int kurtosis_precision { get; init; } = 15;
    public required double min_value { get; init; }
    public int min_value_precision { get; init; } = 15;
    public required double max_value { get; init; }
    public int max_value_precision { get; init; } = 15;
    public required double min_count { get; init; }
    public int min_count_precision { get; init; } = 15;
    public required double max_count { get; init; }
    public int max_count_precision { get; init; } = 15;
    public CorrectnessParam() {}
    public override string ToString() => identifier;
  }

  [Theory]
  [MemberData(nameof(kCorrectnessParams))]
  public void CorrectnessTest(CorrectnessParam p) {
    RunningStatistics s = new();
    for (int i = 0; i < p.values.Length; ++i) {
      s.Push(p.values[i]);
    }

    try {
      TestUtils.AssertApproximately(p.count, s.Count, p.count_precision);
      TestUtils.AssertApproximately(p.mean, s.Mean, p.mean_precision);
      TestUtils.AssertApproximately(p.variance, s.Variance, p.variance_precision);
      TestUtils.AssertApproximately(p.skewness, s.Skewness, p.skewness_precision);
      TestUtils.AssertApproximately(p.kurtosis, s.Kurtosis, p.kurtosis_precision);
      TestUtils.AssertApproximately(p.min_value, s.MinValue, p.min_value_precision);
      TestUtils.AssertApproximately(p.max_value, s.MaxValue, p.max_value_precision);
      Assert.Equal(p.min_count, s.MinCount);
      Assert.Equal(p.max_count, s.MaxCount);
    } catch {
      Console.Error.WriteLine(p.identifier);
      throw;
    }
  }
}
}
