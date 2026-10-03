using System;
using Xunit;

namespace MMOR.NET.Statistics {
public partial class RunningStatisticsTest {
  public struct ZeroCountParam {
    public string identifier { get; init; } = "Generic Test";
    public required double[] values { get; init; }
    public required ulong[] counts { get; init; }
    public int precision { get; init; }          = 13;
    public int variance_precision { get; init; } = 13;
    public int skewness_precision { get; init; } = 12;
    public int kurtosis_precision { get; init; } = 12;
    public double tolerance { get; init; }       = 1e-9;
    public bool relative { get; init; }          = false;
    public ZeroCountParam() {}
    public override string ToString() => identifier;
  }

  [Theory]
  [MemberData(nameof(kZeroCountTestParams))]
  public void ZeroCountTest(ZeroCountParam p) {
    if (p.values.Length != p.counts.Length)
      throw new ArgumentException(string.Format("[ERROR]: values.Length: {0} != counts.Length: {1}",
          p.values.Length, p.counts.Length));

    RunningStatistics indv = new();
    for (int i = 0; i < p.values.Length; ++i) {
      for (ulong c = 0; c < p.counts[i]; ++c) {
        indv.Push(p.values[i]);
      }
    }

    RunningStatistics scalar = new();
    for (int i = 0; i < p.values.Length; ++i) {
      scalar.Push(p.values[i], p.counts[i]);
    }

    RunningStatistics simd = new();
    simd.Push(p.values.AsSpan(), p.counts.AsSpan());

    try {
      Assert.Equal(indv.Count, scalar.Count);
      Assert.Equal(indv.Count, simd.Count);
      AssertMatch(indv, scalar, p);
      AssertMatch(indv, simd, p);
    } catch {
      Console.Error.WriteLine(p.identifier);
      throw;
    }
  }

  static void AssertMatch(RunningStatistics expected, RunningStatistics actual, ZeroCountParam p) {
    Assert.Equal(expected.MinValue, actual.MinValue);
    Assert.Equal(expected.MaxValue, actual.MaxValue);
    Assert.Equal(expected.MinCount, actual.MinCount);
    Assert.Equal(expected.MaxCount, actual.MaxCount);
    AssertStat(expected.Mean, actual.Mean, p, p.precision);
    AssertStat(expected.Variance, actual.Variance, p, p.variance_precision);
    AssertStat(expected.Skewness, actual.Skewness, p, p.skewness_precision);
    AssertStat(expected.Kurtosis, actual.Kurtosis, p, p.kurtosis_precision);
  }

  static void AssertStat(double expected, double actual, ZeroCountParam p, int precision) {
    if (!p.relative) {
      TestUtils.AssertApproximately(expected, actual, precision);
      return;
    }
    TestUtils.AssertApproximately(expected, actual, p.tolerance);
  }
}
}