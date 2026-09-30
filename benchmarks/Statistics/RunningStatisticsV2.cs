using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using MMOR.NET.Statistics;

namespace MMOR.NET.Benchmarks.Statistics {
[MemoryDiagnoser]
public class RunningStatisticsV2Bench {
  [Params(1024, 65536, 1048576)]
  public int N;

  private double[] values_ = null!;
  private ulong[] counts_  = null!;

  [GlobalSetup]
  public void Setup() {
    values_ = new double[N];
    counts_ = new ulong[N];
    var rng = new System.Random(42);
    for (int i = 0; i < N; ++i) {
      values_[i] = rng.NextDouble() * 1000.0;
      counts_[i] = (ulong)rng.Next(1, 1000);
    }
  }

  [Benchmark(Baseline = true)]
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

  [Benchmark]
  public RunningStatisticsV2 Push_Scalar_WithCount() {
    var stats = new RunningStatisticsV2();
    for (int i = 0; i < values_.Length; ++i) {
      stats.Push(values_[i], counts_[i]);
    }
    return stats;
  }

  [Benchmark]
  public RunningStatisticsV2 Push_Span_WithCount() {
    var stats = new RunningStatisticsV2();
    stats.Push(values_.AsSpan(), counts_.AsSpan());
    return stats;
  }
}
}
