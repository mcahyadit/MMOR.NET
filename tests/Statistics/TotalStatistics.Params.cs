using Xunit;

namespace MMOR.NET.Statistics {
public partial class TotalStatisticsTest {
  public static readonly TheoryData<TotalStatsParam> kTotalStatisticsParams = [
    new TotalStatsParam {
      data               = SampleData.len4_json,
      kurtosis_precision = 14,
    },
    new TotalStatsParam {
      data               = SampleData.len8_json,
      kurtosis_precision = 14,
    },
    new TotalStatsParam {
      data               = SampleData.lognorm100_json,
      mean_precision     = 13,
      skewness_precision = 13,
      kurtosis_precision = 13,
    },
    new TotalStatsParam {
      data               = SampleData.lognorm1000_json,
      mean_precision     = 14,
      skewness_precision = 13,
      kurtosis_precision = 13,
    },
    new TotalStatsParam {
      data               = SampleData.gauss1000_json,
      variance_precision = 14,
      kurtosis_precision = 13,
    },
    new TotalStatsParam {
      data               = SampleData.heavy_tail_155_json,
      mean_precision     = 14,
      variance_precision = 12,
      skewness_precision = 13,
      kurtosis_precision = 12,
    },
  ];
}
}
