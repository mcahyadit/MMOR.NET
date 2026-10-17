using Xunit;

namespace MMOR.NET.Statistics {
public partial class RunningStatisticsTest {
  public static readonly TheoryData<RunningStatsParam> kCorrectnessParams = [
    new RunningStatsParam { data = StatFixtures.len4 },
    new RunningStatsParam { data = StatFixtures.len8 },
    new RunningStatsParam {
      data               = StatFixtures.lognorm100,
      mean_precision     = 13,
      skewness_precision = 13,
      kurtosis_precision = 13,
    },
    new RunningStatsParam {
      data               = StatFixtures.lognorm1000,
      skewness_precision = 13,
      kurtosis_precision = 13,
    },
    new RunningStatsParam {
      data               = StatFixtures.gauss1000,
      variance_precision = 14,
      kurtosis_precision = 13,
    },
    new RunningStatsParam {
      data                    = StatFixtures.heavyTail155,
      variance_precision      = 14,
      path_variance_precision = 11,
      path_skewness_precision = 12,
      path_kurtosis_precision = 12,
      skewness_precision      = 13,
      kurtosis_precision      = 13,
    },
  ];
}
}
