
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
    RunningStatisticsAdvanced s1 = new();
    RunningStatisticsAdvanced s2 = new();

    for (int i = 0; i < p.values.Length; ++i) {
      s1.Push(p.values[i]);
    }

    s2.Push(p.values.AsSpan());

    if (p.precision is int precision) {
      Assert.Equal(s1.Mean, s2.Mean, precision);
      Assert.Equal(s1.StandardDeviation, s2.StandardDeviation, precision);
      Assert.Equal(s1.Skewness, s2.Skewness, precision);
      Assert.Equal(s1.Kurtosis, s2.Kurtosis, precision);
    } else if (p.tolerance is double tolerance) {
      Assert.Equal(s1.Mean, s2.Mean, tolerance);
      Assert.Equal(s1.StandardDeviation, s2.StandardDeviation, tolerance);
      Assert.Equal(s1.Skewness, s2.Skewness, tolerance);
      Assert.Equal(s1.Kurtosis, s2.Kurtosis, tolerance);
    } else {
      Assert.Equal(s1.Mean, s2.Mean);
      Assert.Equal(s1.StandardDeviation, s2.StandardDeviation);
      Assert.Equal(s1.Skewness, s2.Skewness);
      Assert.Equal(s1.Kurtosis, s2.Kurtosis);
    }
  }
}
}
