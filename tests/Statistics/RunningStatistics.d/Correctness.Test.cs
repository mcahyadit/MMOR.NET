using System;
using Xunit;

namespace MMOR.NET.Statistics {
public partial class RunningStatisticsTest {
  public struct CorrectnessParam {
    public string identifier { get; init; } = "Generic Test";
    public required double[] values { get; init; }
    public required double count { get; init; }
    public required double mean { get; init; }
    public required double variance { get; init; }
    public required double standard_deviation { get; init; }
    public required double standard_error { get; init; }
    public required double sum { get; init; }
    public required double minimum { get; init; }
    public required double maximum { get; init; }
    public required double skewness { get; init; }
    public required double kurtosis { get; init; }
    public double? geometric_mean { get; init; }
    public double? harmonic_mean { get; init; }
    public required double root_mean_square { get; init; }
    public int? precision { get; init; }
    public double? tolerance { get; init; }
    public CorrectnessParam() {}
  }

  [Theory]
  [MemberData(nameof(kCorrectnessParams))]
  public void CorrectnessTest(CorrectnessParam p) {
    RunningStatisticsAdvanced s = new();
    for (int i = 0; i < p.values.Length; ++i) {
      s.Push(p.values[i]);
    }

    static void AssertClose(int? precision, double? tolerance, double expected, double actual) {
      if (precision is int p)
        Assert.Equal(expected, actual, p);
      else if (tolerance is double t)
        Assert.Equal(expected, actual, t);
      else
        Assert.Equal(expected, actual);
    }

    AssertClose(p.precision, p.tolerance, p.count, s.Count);
    AssertClose(p.precision, p.tolerance, p.mean, s.Mean);
    AssertClose(p.precision, p.tolerance, p.variance, s.Variance);
    AssertClose(p.precision, p.tolerance, p.standard_deviation, s.StandardDeviation);
    AssertClose(p.precision, p.tolerance, p.standard_error, s.StandardError);
    AssertClose(p.precision, p.tolerance, p.sum, s.Sum);
    AssertClose(p.precision, p.tolerance, p.minimum, s.Minimum);
    AssertClose(p.precision, p.tolerance, p.maximum, s.Maximum);
    AssertClose(p.precision, p.tolerance, p.skewness, s.Skewness);
    AssertClose(p.precision, p.tolerance, p.kurtosis, s.Kurtosis);
    if (p.geometric_mean is double geometricMean)
      AssertClose(p.precision, p.tolerance, geometricMean, s.GeometricMean);
    if (p.harmonic_mean is double harmonicMean)
      AssertClose(p.precision, p.tolerance, harmonicMean, s.HarmonicMean);
    AssertClose(p.precision, p.tolerance, p.root_mean_square, s.RootMeanSquare);
  }
}
}
