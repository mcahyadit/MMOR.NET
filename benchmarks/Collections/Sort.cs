using System;
using BenchmarkDotNet.Attributes;
using MMOR.NET.Collections;

namespace MMOR.NET.Benchmarks.Collections {
[MemoryDiagnoser]
public class SortBench {
  [ParamsSource(typeof(SortData), nameof(SortData.N))]
  public int N;

  [ParamsSource(typeof(SortData), nameof(SortData.Distributions))]
  public Distribution Distribution;

  private int[] template_ = null!;
  private int[] work_     = null!;

  [GlobalSetup]
  public void Setup() {
    template_ = SortData.Build(N, Distribution);
    work_     = new int[N];
  }

  [Benchmark(Baseline = true)]
  public int Array_Sort() {
    Array.Copy(template_, work_, N);
    Array.Sort(work_);
    return work_[0];
  }

  [Benchmark]
  public int HeapSort_Span() {
    Array.Copy(template_, work_, N);
    work_.AsSpan().HeapSort();
    return work_[0];
  }

  [Benchmark]
  public int MergeSort_Span() {
    Array.Copy(template_, work_, N);
    work_.AsSpan().MergeSort();
    return work_[0];
  }

  [Benchmark]
  public int QuickSort_Span() {
    Array.Copy(template_, work_, N);
    work_.AsSpan().QuickSort();
    return work_[0];
  }

  [Benchmark]
  public int PdqSort_Span() {
    Array.Copy(template_, work_, N);
    work_.AsSpan().PdqSort();
    return work_[0];
  }
}
}
