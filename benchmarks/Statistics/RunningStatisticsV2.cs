using System;
using System.Collections.Generic;
using System.Numerics;
using BenchmarkDotNet.Attributes;
using MMOR.NET.Statistics;

namespace MMOR.NET.Benchmarks.Statistics {
[MemoryDiagnoser]
public class RunningStatisticsV2Bench {
  [Params(1024, 65536, 1048576)]
  public int N;

  private double[] values_  = null!;
  private ulong[] counts_   = null!;
  private List<int> strides = null!;

  [GlobalSetup]
  public void Setup() {
    values_ = new double[N];
    counts_ = new ulong[N];
    var rng = new System.Random(42);
    for (int i = 0; i < N; ++i) {
      values_[i] = rng.NextDouble() * 1000.0;
      counts_[i] = (ulong)rng.Next(1, 1000);
    }

    int n = N;

    strides = new();
    while (n > 0) {
      int stride = rng.Next(3, Vector<double>.Count * 3);
      strides.Add(Math.Min(stride, n));
      n -= stride;
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
  public RunningStatisticsV2 Push_Span_Strides() {
    var stats = new RunningStatisticsV2();
    int i     = 0;
    foreach (int stride in strides) {
      stats.Push(values_.AsSpan().Slice(i, stride));
      i += stride;
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

  [Benchmark]
  public RunningStatisticsV2 Push_Span_WithCount_Strides() {
    var stats = new RunningStatisticsV2();
    int i     = 0;
    foreach (int stride in strides) {
      stats.Push(values_.AsSpan().Slice(i, stride));
      i += stride;
    }
    return stats;
  }
}
}
