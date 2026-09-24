using Xunit;

namespace MMOR.NET.Statistics {
public partial class RunningStatisticsTest {
  [Fact]
  public void MergeSmallIntoLargeCountImbalance() {
    double[] values = { 1.0, 2.0, 10.0, 20.0, 30.0, 40.0, 50.0 };

    RunningStatisticsAdvanced reference = new();
    foreach (double v in values) {
      reference.Push(v);
    }

    RunningStatisticsAdvanced a = new();
    RunningStatisticsAdvanced b = new();
    RunningStatisticsAdvanced c = new();
    for (int i = 0; i < 2; ++i) {
      a.Push(values[i]);
    }
    for (int i = 2; i < 5; ++i) {
      b.Push(values[i]);
    }
    for (int i = 5; i < values.Length; ++i) {
      c.Push(values[i]);
    }

    RunningStatisticsAdvanced mid = new();
    mid.Push(a);
    mid.Push(b);

    RunningStatisticsAdvanced merged = new();
    merged.Push(mid);
    merged.Push(c);

    Assert.Equal(reference.Count, merged.Count);
    TestUtils.AssertApproximately(reference.Mean, merged.Mean, 15);
    TestUtils.AssertApproximately(reference.Variance, merged.Variance, 13);
    TestUtils.AssertApproximately(reference.Skewness, merged.Skewness, 12);
    TestUtils.AssertApproximately(reference.Kurtosis, merged.Kurtosis, 12);
  }
}
}
