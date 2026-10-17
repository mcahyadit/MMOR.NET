using System;
using System.Collections.Generic;
using Xunit;

namespace MMOR.NET.Statistics {
public partial class TotalStatisticsTest {
  /**
   * <summary>
   *  Wraps a shared <see cref="SampleData"/> fixture with the precisions this suite asserts.
   *  The moments shared with <see cref="RunningStatistics"/> are held to a fixed number of decimal
   *  places (they are bit-for-bit the same Welford update), while the statistics that only
   *  <see cref="TotalStatisticsV2"/> reports are held to a relative <see cref="tolerance"/> because
   *  several of them accumulate over the whole sample.
   * </summary>
   */
  public struct TotalStatsParam {
    public required SampleData data { get; init; }
    public int mean_precision { get; init; }     = 15;
    public int variance_precision { get; init; } = 15;
    public int skewness_precision { get; init; } = 15;
    public int kurtosis_precision { get; init; } = 15;
    public int value_precision { get; init; }    = 15;
    public double tolerance { get; init; }       = 1e-11;
    public TotalStatsParam() {}
    public override string ToString() => data.identifier;
  }

  [Theory]
  [MemberData(nameof(kTotalStatisticsParams))]
  public void TotalStatisticsV2Test(TotalStatsParam p) {
    SampleData d                = p.data;
    ReadOnlySpan<double> values = d.values;
    TotalStatisticsV2 v2        = values.Calculate();

    Assert.Equal(d.count, v2.Count);
    Assert.Equal(d.count0, v2.Count0);
    Assert.Equal(d.count_negative, v2.CountNegative);
    Assert.Equal(d.count_positive, v2.CountPositive);

    // Core moments: same Welford update as RunningStatistics, so the shared truth holds.
    TestUtils.AssertApproximately(d.mean, v2.Mean, p.mean_precision);
    TestUtils.AssertApproximately(d.variance, v2.Variance, p.variance_precision);
    TestUtils.AssertApproximately(d.skewness, v2.Skewness, p.skewness_precision);
    TestUtils.AssertApproximately(d.kurtosis, v2.Kurtosis, p.kurtosis_precision);

    TestUtils.AssertApproximately(d.min_value, v2.MinValue, p.value_precision);
    Assert.Equal(d.min_count, v2.MinCount);
    TestUtils.AssertApproximately(d.max_value, v2.MaxValue, p.value_precision);
    Assert.Equal(d.max_count, v2.MaxCount);

    TestUtils.AssertApproximately(d.mode_value, v2.ModeValue, p.value_precision);
    Assert.Equal(d.mode_count, v2.ModeCount);

    // Derived from the same moments.
    TestUtils.AssertApproximately(d.mean * d.count, v2.Sum, p.tolerance);
    TestUtils.AssertApproximately(Math.Sqrt(d.variance), v2.StandardDeviation, p.tolerance);

    TestUtils.AssertApproximately(d.geometric_mean, v2.GeometricMean, p.tolerance);
    TestUtils.AssertApproximately(d.harmonic_mean, v2.HarmonicMean, p.tolerance);
    TestUtils.AssertApproximately(d.interquartile_mean, v2.InterQuartileMean, p.tolerance);
    TestUtils.AssertApproximately(d.mean_absolute_deviation, v2.MeanAbsoluteDeviation, p.tolerance);
    TestUtils.AssertApproximately(d.median_absolute_deviation, v2.MedianAbsoluteDeviation,
        p.tolerance);

    TestUtils.AssertApproximately(d.entropy, v2.Entropy, p.tolerance);
    TestUtils.AssertApproximately(d.theil_index, v2.TheilIndex, p.tolerance);
    TestUtils.AssertApproximately(d.simpson_diversity_index, v2.SimpsonDiversityIndex, p.tolerance);

    // Gini and HHI are scaled by the sample sum, so they are only asserted when the sample is
    // not near-zero-sum (the fixture leaves them NaN otherwise).
    if (!double.IsNaN(d.gini_coefficient))
      TestUtils.AssertApproximately(d.gini_coefficient, v2.GiniCoefficient, p.tolerance);
    if (!double.IsNaN(d.herfindahl_hirschman_index))
      TestUtils.AssertApproximately(d.herfindahl_hirschman_index, v2.HerfindahlHirschmanIndex,
          p.tolerance);

    Assert.Equal(d.outlier_count, v2.OutlierCount);

    foreach (KeyValuePair<CommonQuantile, double> q in d.quantiles)
      TestUtils.AssertApproximately(q.Value, v2.CommonQuantiles[q.Key], p.tolerance);

    // Derived quantile metrics.
    TestUtils.AssertApproximately(d.quantiles[CommonQuantile.Median], v2.Median, p.tolerance);
    TestUtils.AssertApproximately(d.quantiles[CommonQuantile.Quartile1st], v2.Quartile1st,
        p.tolerance);
    TestUtils.AssertApproximately(d.quantiles[CommonQuantile.Quartile3rd], v2.Quartile3rd,
        p.tolerance);
    TestUtils.AssertApproximately(
        d.quantiles[CommonQuantile.Quartile3rd] - d.quantiles[CommonQuantile.Quartile1st],
        v2.InterQuartileRange, p.tolerance);
  }
}
}
