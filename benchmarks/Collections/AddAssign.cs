using System;
using BenchmarkDotNet.Attributes;
using MMOR.NET.Collections;

namespace MMOR.NET.Benchmarks.Collections {
public class AddAssignBench {
  [Params(1024, 65536, 1048576)]
  public int N;

  private ulong[] template_ = null!;
  private ulong[] other_    = null!;
  private ulong[] target_   = null!;

  [GlobalSetup]
  public void Setup() {
    template_ = new ulong[N];
    other_    = new ulong[N];
    var rng   = new System.Random(42);
    for (int i = 0; i < N; ++i) {
      template_[i] = (ulong)rng.Next() % 1000;
      other_[i]    = (ulong)rng.Next() % 1000;
    }
    target_ = (ulong[])template_.Clone();
  }

  [Benchmark]
  public void AddAssign_Span() {
    CollectionUtils.AddAssign(target_.AsSpan(), other_.AsSpan());
  }

  [Benchmark]
  public void AddAssign_Scalar() {
    for (int i = 0; i < target_.Length; ++i) {
      target_[i] += other_[i];
    }
  }
}
}
