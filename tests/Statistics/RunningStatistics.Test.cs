
namespace MMOR.NET.Statistics {
using System;
using Xunit;
public partial class RunningStatisticsTest {
  public struct SimdParam {
    public string identifier { get; init; } = "Generic Test";
    public required double[] values { get; init; }
    public int? precision { get; init; }
    public double? tolerance { get; init; }
    public SimdParam() {}
  }

  [Theory]
  [MemberData(nameof(kSimdTestParams))]
  public void SimdTest(SimdParam p) {
    RunningStatistics s1 = new();
    RunningStatistics s2 = new();

    for (int i = 0; i < p.values.Length; ++i) {
      s1.Push(p.values[i]);
    }

    s2.Push(p.values.AsSpan());

    if (p.precision is int precision) {
      Assert.Equal(s1.Mean, s2.Mean, precision);
    } else if (p.tolerance is double tolerance) {
      Assert.Equal(s1.Mean, s2.Mean, tolerance);
    } else {
      Assert.Equal(s1.Mean, s2.Mean);
    }
  }
}
}
