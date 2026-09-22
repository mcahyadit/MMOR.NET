using System;
using Xunit;

namespace MMOR.NET.Statistics {
public partial class RunningStatisticsTest {
  public struct SegmentParam {
    public string identifier { get; init; } = "Generic Test";
    public required double[] values { get; init; }
    public required int segments { get; init; }
    public int precision { get; init; }          = 13;
    public int variance_precision { get; init; } = 13;
    public int skewness_precision { get; init; } = 13;
    public int kurtosis_precision { get; init; } = 13;
    public SegmentParam() {}
    public override string ToString() => identifier;
  }

  [Theory]
  [MemberData(nameof(kSegmentTestParams))]
  public void SegmentTest(SegmentParam p) {
    RunningStatisticsAdvanced a = new();
    for (int i = 0; i < p.values.Length; ++i) {
      a.Push(p.values[i]);
    }

    RunningStatisticsAdvanced f = new();
    RunningStatisticsAdvanced g = new();

    int n      = p.values.Length;
    int chunk  = n / p.segments;
    int rem    = n % p.segments;
    int offset = 0;
    for (int s = 0; s < p.segments; ++s) {
      int size = chunk + (s < rem ? 1 : 0);
      for (int i = 0; i < size; ++i) {
        g.Push(p.values[offset + i]);
      }
      offset += size;

      f.Push(g);
      g.Clear();

      Assert.Equal(0UL, g.Count_uint64);
    }

    try {
      TestUtils.AssertApproximately(a.Count, f.Count, p.precision);
      TestUtils.AssertApproximately(a.Mean, f.Mean, p.precision);
      TestUtils.AssertApproximately(a.Variance, f.Variance, p.variance_precision);
      TestUtils.AssertApproximately(a.Skewness, f.Skewness, p.skewness_precision);
      TestUtils.AssertApproximately(a.Kurtosis, f.Kurtosis, p.kurtosis_precision);
      TestUtils.AssertApproximately(a.GeometricMean, f.GeometricMean, p.precision);
      TestUtils.AssertApproximately(a.HarmonicMean, f.HarmonicMean, p.precision);
      TestUtils.AssertApproximately(a.RootMeanSquare, f.RootMeanSquare, p.precision);
    } catch {
      Console.Error.WriteLine(p.identifier);
      throw;
    }
  }
}
}