using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using MMOR.NET.Collections;

namespace MMOR.NET.Benchmarks.Collections {
public class SumBench {
  [Params(1024, 65536, 1048576)]
  public int N;

  private double[] values_double_ = null!;
  private ulong[] values_uint64_  = null!;

  [GlobalSetup]
  public void Setup() {
    values_double_ = new double[N];
    values_uint64_ = new ulong[N];
    var rng        = new System.Random(42);
    for (int i = 0; i < N; ++i) {
      values_double_[i] = rng.NextDouble() * 1000.0;
      values_uint64_[i] = (ulong)rng.Next();
    }
  }

  [Benchmark]
  public double Sum_DoubleSpan() {
    return CollectionUtils.Sum(values_double_.AsSpan());
  }

  [Benchmark]
  public double Sum_DoubleLinq() {
    return Enumerable.Sum(values_double_);
  }

  [Benchmark]
  public ulong Sum_UInt64Span() {
    return CollectionUtils.Sum(values_uint64_.AsSpan());
  }

  [Benchmark]
  public ulong Sum_UInt64Scalar() {
    ulong acc = 0;
    for (int i = 0; i < values_uint64_.Length; ++i) {
      acc += values_uint64_[i];
    }
    return acc;
  }
}
}
