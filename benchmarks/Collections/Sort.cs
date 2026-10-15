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
  public int HeapSort() {
    Array.Copy(template_, work_, N);
    CollectionUtils.HeapSort(work_);
    return work_[0];
  }

  [Benchmark]
  public int MergeSort() {
    Array.Copy(template_, work_, N);
    CollectionUtils.MergeSort(work_);
    return work_[0];
  }

  [Benchmark]
  public int QuickSort() {
    Array.Copy(template_, work_, N);
    CollectionUtils.QuickSort(work_);
    return work_[0];
  }

  [Benchmark]
  public int IntroSort() {
    Array.Copy(template_, work_, N);
    CollectionUtils.IntroSort(work_);
    return work_[0];
  }

  [Benchmark]
  public int PdqSort() {
    Array.Copy(template_, work_, N);
    CollectionUtils.PdqSort(work_);
    return work_[0];
  }

  [Benchmark]
  public int TimSort() {
    Array.Copy(template_, work_, N);
    CollectionUtils.TimSort(work_);
    return work_[0];
  }
}
}
