using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using MMOR.NET.Collections;

namespace MMOR.NET.Benchmarks.Collections {
public class MaxElementBench {
  [Params(1024, 65536, 1048576)]
  public int N;

  private double[] values_double_ = null!;

  [GlobalSetup]
  public void Setup() {
    values_double_ = new double[N];
    var rng        = new System.Random(42);
    for (int i = 0; i < N; ++i) {
      values_double_[i] = rng.NextDouble() * 1000.0;
    }
  }

  [Benchmark]
  public double MaxElement_Span() {
    return CollectionUtils.MaxElement(values_double_.AsSpan());
  }

  [Benchmark]
  public double MaxElement_Linq() {
    return Enumerable.Max(values_double_);
  }
}
}
