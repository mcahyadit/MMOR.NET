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
    RunningStatistics a = new();
    for (int i = 0; i < p.values.Length; ++i) {
      a.Push(p.values[i]);
    }

    RunningStatistics f = new();
    RunningStatistics g = new();

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
    }

    try {
      TestUtils.AssertApproximately(a.Count, f.Count, p.precision);
      TestUtils.AssertApproximately(a.Mean, f.Mean, p.precision);
      TestUtils.AssertApproximately(a.Variance, f.Variance, p.variance_precision);
      TestUtils.AssertApproximately(a.Skewness, f.Skewness, p.skewness_precision);
      TestUtils.AssertApproximately(a.Kurtosis, f.Kurtosis, p.kurtosis_precision);
      TestUtils.AssertApproximately(a.MinValue, f.MinValue, p.precision);
      TestUtils.AssertApproximately(a.MaxValue, f.MaxValue, p.precision);
      Assert.Equal(a.MinCount, f.MinCount);
      Assert.Equal(a.MaxCount, f.MaxCount);
    } catch {
      Console.Error.WriteLine(p.identifier);
      throw;
    }
  }
}
}
