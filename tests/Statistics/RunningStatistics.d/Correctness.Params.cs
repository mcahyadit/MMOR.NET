using Xunit;

namespace MMOR.NET.Statistics {
public partial class RunningStatisticsTest {
  public static readonly TheoryData<RunningStatsParam> kCorrectnessParams = [
    new RunningStatsParam {
      data               = SampleData.len4_json,
      kurtosis_precision = 14,
    },
    new RunningStatsParam {
      data               = SampleData.len8_json,
      kurtosis_precision = 14,
    },
    new RunningStatsParam {
      data               = SampleData.lognorm100_json,
      mean_precision     = 13,
      skewness_precision = 13,
      kurtosis_precision = 13,
    },
    new RunningStatsParam {
      data               = SampleData.lognorm1000_json,
      mean_precision     = 14,
      skewness_precision = 13,
      kurtosis_precision = 13,
    },
    new RunningStatsParam {
      data               = SampleData.gauss1000_json,
      variance_precision = 14,
      kurtosis_precision = 13,
    },
    new RunningStatsParam {
      data                    = SampleData.heavy_tail_155_json,
      mean_precision          = 14,
      variance_precision      = 12,
      path_variance_precision = 11,
      path_skewness_precision = 12,
      path_kurtosis_precision = 12,
      skewness_precision      = 13,
      kurtosis_precision      = 12,
    },
  ];
}
}
