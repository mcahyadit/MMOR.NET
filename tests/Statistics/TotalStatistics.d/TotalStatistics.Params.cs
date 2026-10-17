using Xunit;

namespace MMOR.NET.Statistics {
public partial class TotalStatisticsTest {
  public static readonly TheoryData<TotalStatsParam> kTotalStatisticsParams = [
    new TotalStatsParam { data = StatFixtures.len4 },
    new TotalStatsParam { data = StatFixtures.len8 },
    new TotalStatsParam {
      data               = StatFixtures.lognorm100,
      mean_precision     = 13,
      skewness_precision = 13,
      kurtosis_precision = 13,
    },
    new TotalStatsParam {
      data               = StatFixtures.lognorm1000,
      skewness_precision = 13,
      kurtosis_precision = 13,
    },
    new TotalStatsParam {
      data               = StatFixtures.gauss1000,
      variance_precision = 14,
      kurtosis_precision = 13,
    },
    new TotalStatsParam {
      data               = StatFixtures.heavyTail155,
      variance_precision = 14,
      skewness_precision = 13,
      kurtosis_precision = 13,
    },
  ];
}
}
