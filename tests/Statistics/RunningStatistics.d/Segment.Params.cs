using Xunit;

namespace MMOR.NET.Statistics {
public partial class RunningStatisticsTest {
  public static readonly TheoryData<SegmentParam> kSegmentTestParams = BuildSegmentParams();

  static TheoryData<SegmentParam> BuildSegmentParams() {
    TheoryData<SegmentParam> data = [];
    foreach (CorrectnessParam p in kCorrectnessParams) {
      data.Add(new SegmentParam {
        identifier         = string.Format("{0}-seg{1}", p.identifier, 3),
        values             = p.values,
        segments           = 3,
        variance_precision = p.path_variance_precision,
        skewness_precision = p.path_skewness_precision ?? 13,
        kurtosis_precision = p.path_kurtosis_precision ?? 13,
      });
      data.Add(new SegmentParam {
        identifier         = string.Format("{0}-seg{1}", p.identifier, 4),
        values             = p.values,
        segments           = 4,
        variance_precision = p.path_variance_precision,
        skewness_precision = p.path_skewness_precision ?? 13,
        kurtosis_precision = p.path_kurtosis_precision ?? 13,
      });
    }
    return data;
  }
}
}