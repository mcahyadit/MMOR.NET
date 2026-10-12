using System;
using BenchmarkDotNet.Attributes;
using MMOR.NET.Collections;

namespace MMOR.NET.Benchmarks.Collections {
[MemoryDiagnoser]
public class SelectionBench {
  [ParamsSource(typeof(SortData), nameof(SortData.N))]
  public int N;

  [ParamsSource(typeof(SortData), nameof(SortData.Distributions))]
  public Distribution Distribution;

  private int[] template_ = null!;
  private int[] work_     = null!;
  private int k_;

  [GlobalSetup]
  public void Setup() {
    template_ = SortData.Build(N, Distribution);
    work_     = new int[N];
    k_        = N / 2;
  }

  [Benchmark(Baseline = true)]
  public int SortThenPick() {
    Array.Copy(template_, work_, N);
    Array.Sort(work_);
    return work_[k_ - 1];
  }

  [Benchmark]
  public int QuickSelect() {
    Array.Copy(template_, work_, N);
    return work_.AsSpan().QuickSelectMutable(k_);
  }

  [Benchmark]
  public int MedianOfMedians() {
    Array.Copy(template_, work_, N);
    return work_.AsSpan().MedianOfMediansMutable(k_);
  }

  [Benchmark]
  public int IntroSelect() {
    Array.Copy(template_, work_, N);
    return work_.AsSpan().IntroSelectMutable(k_);
  }

  [Benchmark]
  public int PdqSelect() {
    Array.Copy(template_, work_, N);
    return work_.AsSpan().PdqSelect(k_);
  }
}
}
