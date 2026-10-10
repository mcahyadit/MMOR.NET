using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using MMOR.NET.Collections;

namespace MMOR.NET.Benchmarks.Collections {
[MemoryDiagnoser]
public class RentedListBench {
  [Params(1024, 65536, 1048576)]
  public int N;

  private const int kChunk  = 1024;
  private const int kAbsent = -1;

  private int[] template_ = null!;
  private int probe_present_;
  private int mid_;
  private int[] copy_dst_ = null!;

  private RentedList<int> rented_ = null!;
  private List<int> list_         = null!;

  [GlobalSetup]
  public void Setup() {
    template_ = new int[N];
    var rng   = new System.Random(42);
    for (int i = 0; i < N; ++i) {
      template_[i] = rng.Next(0, N);
    }

    // Probe value absent from the random range, placed uniquely at mid_ so a
    // "hit" scan is deterministic regardless of N.
    mid_            = N / 2;
    probe_present_  = int.MaxValue;
    template_[mid_] = probe_present_;

    // +1 so Insert benches never trigger a resize on the measured path.
    rented_ = new RentedList<int>(N + 1);
    list_   = new List<int>(N + 1);
    for (int i = 0; i < N; ++i) {
      rented_.Add(template_[i]);
      list_.Add(template_[i]);
    }

    copy_dst_ = new int[N];
  }

  [GlobalCleanup]
  public void Cleanup() {
    rented_.Dispose();
  }

  [Benchmark]
  public int RentedList_Add() {
    using RentedList<int> list = new();
    for (int i = 0; i < N; ++i) {
      list.Add(template_[i]);
    }
    return list.Count;
  }

  [Benchmark]
  public int List_Add() {
    List<int> list = new();
    for (int i = 0; i < N; ++i) {
      list.Add(template_[i]);
    }
    return list.Count;
  }

  [Benchmark]
  public int RentedList_Add_Presized() {
    using RentedList<int> list = new(N);
    for (int i = 0; i < N; ++i) {
      list.Add(template_[i]);
    }
    return list.Count;
  }

  [Benchmark]
  public int List_Add_Presized() {
    List<int> list = new(N);
    for (int i = 0; i < N; ++i) {
      list.Add(template_[i]);
    }
    return list.Count;
  }

  [Benchmark]
  public int RentedList_AddRange_Chunked() {
    using RentedList<int> list = new();
    for (int i = 0; i < N; i += kChunk) {
      int len = Math.Min(kChunk, N - i);
      list.AddRange(template_.AsSpan(i, len));
    }
    return list.Count;
  }

  [Benchmark]
  public int List_AddRange_Chunked() {
    List<int> list = new();
    for (int i = 0; i < N; i += kChunk) {
      int len = Math.Min(kChunk, N - i);
      list.AddRange(new ArraySegment<int>(template_, i, len));
    }
    return list.Count;
  }

  [Benchmark]
  public int RentedList_IndexRead() {
    RentedList<int> list = rented_;
    int acc              = 0;
    for (int i = 0; i < N; ++i) {
      acc += list[i];
    }
    return acc;
  }

  [Benchmark]
  public int List_IndexRead() {
    List<int> list = list_;
    int acc        = 0;
    for (int i = 0; i < N; ++i) {
      acc += list[i];
    }
    return acc;
  }

  [Benchmark]
  public long RentedList_Enumerator() {
    long acc = 0;
    foreach (int v in rented_) {
      acc += v;
    }
    return acc;
  }

  [Benchmark]
  public long List_Enumerator() {
    long acc = 0;
    foreach (int v in list_) {
      acc += v;
    }
    return acc;
  }

  [Benchmark]
  public int RentedList_IndexOf_Hit() => rented_.IndexOf(probe_present_);

  [Benchmark]
  public int List_IndexOf_Hit() => list_.IndexOf(probe_present_);

  [Benchmark]
  public int RentedList_IndexOf_Miss() => rented_.IndexOf(kAbsent);

  [Benchmark]
  public int List_IndexOf_Miss() => list_.IndexOf(kAbsent);

  [Benchmark]
  public int RentedList_CopyTo() {
    rented_.CopyTo(copy_dst_, 0);
    return copy_dst_[N - 1];
  }

  [Benchmark]
  public int List_CopyTo() {
    list_.CopyTo(copy_dst_, 0);
    return copy_dst_[N - 1];
  }

  [Benchmark]
  public int RentedList_InsertRemoveAt_Middle() {
    rented_.Insert(mid_, template_[0]);
    rented_.RemoveAt(mid_);
    return rented_.Count;
  }

  [Benchmark]
  public int List_InsertRemoveAt_Middle() {
    list_.Insert(mid_, template_[0]);
    list_.RemoveAt(mid_);
    return list_.Count;
  }

  [Benchmark]
  public int RentedList_InsertRemoveAt_Front() {
    rented_.Insert(0, template_[0]);
    rented_.RemoveAt(0);
    return rented_.Count;
  }

  [Benchmark]
  public int List_InsertRemoveAt_Front() {
    list_.Insert(0, template_[0]);
    list_.RemoveAt(0);
    return list_.Count;
  }
}
}
