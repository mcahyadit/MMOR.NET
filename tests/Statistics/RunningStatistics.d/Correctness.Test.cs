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
    public int variance_precision { get; init; } = 15;
    public required double skewness { get; init; }
    public int skewness_precision { get; init; } = 15;
    public required double kurtosis { get; init; }
    public int kurtosis_precision { get; init; } = 15;
    public required double geometric_mean { get; init; }
    public int geometric_mean_precision { get; init; } = 15;
    public required double harmonic_mean { get; init; }
    public int harmonic_mean_precision { get; init; } = 15;
    public required double root_mean_square { get; init; }
    public int root_mean_square_precision { get; init; } = 15;
    public CorrectnessParam() {}
    public override string ToString() => identifier;
  }

  [Theory]
  [MemberData(nameof(kCorrectnessParams))]
  public void CorrectnessTest(CorrectnessParam p) {
    RunningStatisticsAdvanced s = new();
    for (int i = 0; i < p.values.Length; ++i) {
      s.Push(p.values[i]);
    }

    try {
      TestUtils.AssertApproximately(p.count, s.Count, p.count_precision);
      TestUtils.AssertApproximately(p.mean, s.Mean, p.mean_precision);
      TestUtils.AssertApproximately(p.variance, s.Variance, p.variance_precision);
      TestUtils.AssertApproximately(p.skewness, s.Skewness, p.skewness_precision);
      TestUtils.AssertApproximately(p.kurtosis, s.Kurtosis, p.kurtosis_precision);
      TestUtils.AssertApproximately(p.geometric_mean, s.GeometricMean, p.geometric_mean_precision);
      TestUtils.AssertApproximately(p.harmonic_mean, s.HarmonicMean, p.harmonic_mean_precision);
      TestUtils.AssertApproximately(p.root_mean_square, s.RootMeanSquare,
          p.root_mean_square_precision);
    } catch {
      Console.Error.WriteLine(p.identifier);
      throw;
    }
  }
}
}
