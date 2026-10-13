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

  [ParamsSource(typeof(SortData), nameof(SortData.KFractions))]
  public KFraction K;

  private int[] template_ = null!;
  private int[] work_     = null!;
  private int k_;

  [GlobalSetup]
  public void Setup() {
    template_ = SortData.Build(N, Distribution);
    work_     = new int[N];
    k_        = SortData.ResolveK(N, K);
  }

  [Benchmark]
  public int QuickSelect() {
    Array.Copy(template_, work_, N);
    return CollectionUtils.QuickSelectMutable(work_, k_);
  }

  [Benchmark]
  public int MedianOfMedians() {
    Array.Copy(template_, work_, N);
    return CollectionUtils.MedianOfMediansMutable(work_, k_);
  }

  [Benchmark]
  public int HeapSelect() {
    Array.Copy(template_, work_, N);
    return CollectionUtils.HeapSelectMutable(work_, k_);
  }

  [Benchmark(Baseline = true)]
  public int IntroSelect() {
    Array.Copy(template_, work_, N);
    return CollectionUtils.IntroSelectMutable(work_, k_);
  }

  [Benchmark]
  public int IntroSelectHeap() {
    Array.Copy(template_, work_, N);
    return CollectionUtils.IntroSelectMutable(work_, k_,
        bad_path: CollectionUtils.IntroSelectBadPath.HeapSelect);
  }

  [Benchmark]
  public int PdqSelect() {
    Array.Copy(template_, work_, N);
    return CollectionUtils.PdqSelect(work_, k_);
  }
}
}
