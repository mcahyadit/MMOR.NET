using System;
using Xunit;

namespace MMOR.NET.Statistics {
public partial class RunningStatisticsTest {
  public struct SimdParam {
    public string identifier { get; init; } = "Generic Test";
    public required double[] values { get; init; }
    public int mean_precision { get; init; }               = 15;
    public int standard_deviation_precision { get; init; } = 15;
    public int geometric_mean_precision { get; init; }     = 15;
    public int root_mean_square_precision { get; init; }   = 15;
    public int harmonic_mean_precision { get; init; }      = 15;
    public int skewness_precision { get; init; }           = 15;
    public int kurtosis_precision { get; init; }           = 15;
    public SimdParam() {}
    public override string ToString() => identifier;
  }

  [Theory]
  [MemberData(nameof(kSimdTestParams))]
  public void SimdTest(SimdParam p) {
    RunningStatisticsAdvanced s1 = new();
    RunningStatisticsAdvanced s2 = new();

    for (int i = 0; i < p.values.Length; ++i) {
      s1.Push(p.values[i]);
    }

    s2.Push(p.values.AsSpan());

    try {
      TestUtils.AssertApproximately(s1.Mean, s2.Mean, p.mean_precision);
      TestUtils.AssertApproximately(s1.StandardDeviation, s2.StandardDeviation,
          p.standard_deviation_precision);
      TestUtils.AssertApproximately(s1.GeometricMean, s2.GeometricMean, p.geometric_mean_precision);
      TestUtils.AssertApproximately(s1.RootMeanSquare, s2.RootMeanSquare,
          p.root_mean_square_precision);
      TestUtils.AssertApproximately(s1.HarmonicMean, s2.HarmonicMean, p.harmonic_mean_precision);
      TestUtils.AssertApproximately(s1.Skewness, s2.Skewness, p.skewness_precision);
      TestUtils.AssertApproximately(s1.Kurtosis, s2.Kurtosis, p.kurtosis_precision);
    } catch {
      Console.Error.WriteLine(p.identifier);
      throw;
    }
  }
}
}
