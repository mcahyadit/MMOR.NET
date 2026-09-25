using System;
using BenchmarkDotNet.Attributes;
using MMOR.NET.Statistics;

namespace MMOR.NET.Benchmarks.Statistics {
public class RunningStatisticsV2Bench {
  [Params(1024, 65536, 1048576)]
  public int N;

  private double[] values_ = null!;

  [GlobalSetup]
  public void Setup() {
    values_ = new double[N];
    var rng = new System.Random(42);
    for (int i = 0; i < N; ++i) {
      values_[i] = rng.NextDouble() * 1000.0;
    }
  }

  [Benchmark]
  public RunningStatisticsV2 Push_Scalar() {
    var stats = new RunningStatisticsV2();
    for (int i = 0; i < values_.Length; ++i) {
      stats.Push(values_[i]);
    }
    return stats;
  }

  [Benchmark]
  public RunningStatisticsV2 Push_Span() {
    var stats = new RunningStatisticsV2();
    stats.Push(values_.AsSpan());
    return stats;
  }
}
}
