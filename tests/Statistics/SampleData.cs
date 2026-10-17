using System.Collections.Frozen;

namespace MMOR.NET.Statistics {
/**
 * <summary>
 *  Shared sample fixture: the input samples plus the ground truth for every statistic
 *  <see cref="TotalStatisticsV2"/> reports. <br/>
 *  <br/>
 *  Both the <see cref="RunningStatistics"/> and <see cref="TotalStatisticsV2"/> suites wrap an
 *  instance of this in their own param struct, which carries only the precisions that suite is
 *  able to assert.
 * </summary>
 * <remarks>
 *  A statistic left at <see cref="double.NaN"/> is not asserted for this fixture, either because
 * the quantity is undefined at that size, or because it is numerically ill-conditioned for the
 * sample (for example the Gini / Herfindahl-Hirschman / Theil indices are scaled by the sample sum,
 * so a sample whose sum is close to zero leaves them meaningless).
 * </remarks>
 */
public struct SampleData {
  public string identifier { get; init; } = "Generic Test";

  public double[] values { get; init; } = [];

  /// <summary>Per-value weights. Empty means every value carries unit weight.</summary>
  public ulong[] counts { get; init; } = [];

  public ulong count { get; init; }
  public ulong count0 { get; init; }
  public ulong count_negative { get; init; }
  public ulong count_positive { get; init; }

  public double mean { get; init; }     = double.NaN;
  public double variance { get; init; } = double.NaN;
  public double skewness { get; init; } = double.NaN;
  public double kurtosis { get; init; } = double.NaN;

  public double min_value { get; init; } = double.NaN;
  public ulong min_count { get; init; }
  public double max_value { get; init; } = double.NaN;
  public ulong max_count { get; init; }
  public double mode_value { get; init; } = double.NaN;
  public ulong mode_count { get; init; }

  public double geometric_mean { get; init; }     = double.NaN;
  public double harmonic_mean { get; init; }      = double.NaN;
  public double interquartile_mean { get; init; } = double.NaN;

  public double mean_absolute_deviation { get; init; }   = double.NaN;
  public double median_absolute_deviation { get; init; } = double.NaN;

  public double entropy { get; init; }                    = double.NaN;
  public double gini_coefficient { get; init; }           = double.NaN;
  public double herfindahl_hirschman_index { get; init; } = double.NaN;
  public double theil_index { get; init; }                = double.NaN;
  public double simpson_diversity_index { get; init; }    = double.NaN;
  public ulong outlier_count { get; init; }

  /// <summary>R-7 quantiles keyed by <see cref="CommonQuantile"/>.</summary>
  public FrozenDictionary<CommonQuantile, double> quantiles { get; init; } = null!;

  public SampleData() {}
  public override string ToString() => identifier;
}
}
