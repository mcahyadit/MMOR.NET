using Xunit;

namespace MMOR.NET.Statistics {
public partial class RunningStatisticsTest {
  public static readonly TheoryData<OrderParam> kOrderTestParams = BuildOrderParams();

  static TheoryData<OrderParam> BuildOrderParams() {
    TheoryData<OrderParam> data = [];
    foreach (CorrectnessParam p in kCorrectnessParams) {
      data.Add(new OrderParam {
        identifier         = string.Format("{0}-seed{1}x{2}", p.identifier, 12345, 54321),
        values             = p.values,
        seed_a             = 12345,
        seed_b             = 54321,
        variance_precision = p.path_variance_precision,
        skewness_precision = p.path_skewness_precision ?? 13,
        kurtosis_precision = p.path_kurtosis_precision ?? 12,
      });
      data.Add(new OrderParam {
        identifier         = string.Format("{0}-seed{1}x{2}", p.identifier, 101, 999),
        values             = p.values,
        seed_a             = 101,
        seed_b             = 999,
        variance_precision = p.path_variance_precision,
        skewness_precision = p.path_skewness_precision ?? 13,
        kurtosis_precision = p.path_kurtosis_precision ?? 12,
      });
    }
    foreach (CountParam p in kCountTestParams) {
      data.Add(new OrderParam {
        identifier = string.Format("{0}-cnt{1}x{2}", p.identifier, 12345, 54321),
        values     = p.values,
        counts     = p.counts,
        seed_a     = 12345,
        seed_b     = 54321,
        relative   = p.relative,
        tolerance  = p.tolerance,
        precision  = p.precision,
      });
      data.Add(new OrderParam {
        identifier = string.Format("{0}-cnt{1}x{2}", p.identifier, 101, 999),
        values     = p.values,
        counts     = p.counts,
        seed_a     = 101,
        seed_b     = 999,
        relative   = p.relative,
        tolerance  = p.tolerance,
        precision  = p.precision,
      });
    }
    return data;
  }
}
}