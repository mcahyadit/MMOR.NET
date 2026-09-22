using System;
using Xunit;

namespace MMOR.NET.Statistics {
public partial class RunningStatisticsTest {
  public struct CountParam {
    public string identifier { get; init; } = "Generic Test";
    public required double[] values { get; init; }
    public required ulong[] counts { get; init; }
    public int precision { get; init; }    = 13;
    public double tolerance { get; init; } = 1e-9;
    public bool relative { get; init; }    = false;
    public CountParam() {}
    public override string ToString() => identifier;
  }

  [Theory]
  [MemberData(nameof(kCountTestParams))]
  public void CountTest(CountParam p) {
    if (p.values.Length != p.counts.Length)
      throw new ArgumentException(string.Format("[ERROR]: values.Length: {0} != counts.Length: {1}",
          p.values.Length, p.counts.Length));

    RunningStatisticsAdvanced freq = new();
    RunningStatisticsAdvanced indv = new();

    double expected_count = 0;
    for (int i = 0; i < p.values.Length; ++i) {
      freq.Push(p.values[i], p.counts[i]);
      for (ulong c = 0; c < p.counts[i]; ++c) {
        indv.Push(p.values[i]);
        ++expected_count;
      }
    }

    try {
      AssertStat(expected_count, freq.Count, p);
      AssertStat(indv.Count, freq.Count, p);
      AssertStat(indv.Count0, freq.Count0, p);
      AssertStat(indv.Mean, freq.Mean, p);
      AssertStat(indv.Variance, freq.Variance, p);
      AssertStat(indv.Skewness, freq.Skewness, p);
      AssertStat(indv.Kurtosis, freq.Kurtosis, p);
      AssertStat(indv.GeometricMean, freq.GeometricMean, p);
      AssertStat(indv.HarmonicMean, freq.HarmonicMean, p);
      AssertStat(indv.RootMeanSquare, freq.RootMeanSquare, p);
    } catch {
      Console.Error.WriteLine(p.identifier);
      throw;
    }
  }

  static void AssertStat(double expected, double actual, CountParam p) {
    if (!p.relative) {
      TestUtils.AssertApproximately(expected, actual, p.precision);
      return;
    }
    TestUtils.AssertApproximately(expected, actual, p.tolerance);
  }
}
}