using System;
using BenchmarkDotNet.Attributes;
using MMOR.NET.Statistics;

namespace MMOR.NET.Benchmarks.Statistics {
public class RunningStatisticsAdvancedBench {
  [Params(1024, 65536, 1048576)]
  public int N;

  private double[] values_ = null!;

  [GlobalSetup]
  public void Setup() {
    values_ = new double[N];
    var rng = new System.Random(42);
    for (int i = 0; i < N; ++i) {
      values_[i] = rng.NextDouble() * 1000.0 + 1.0;
    }
  }

  [Benchmark]
  public RunningStatisticsAdvanced Push_Scalar() {
    var stats = new RunningStatisticsAdvanced();
    for (int i = 0; i < values_.Length; ++i) {
      stats.Push(values_[i]);
    }
    return stats;
  }

  [Benchmark]
  public RunningStatisticsAdvanced Push_Span() {
    var stats = new RunningStatisticsAdvanced();
    stats.Push(values_.AsSpan());
    return stats;
  }
}
}
