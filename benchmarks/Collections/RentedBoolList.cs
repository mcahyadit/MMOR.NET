using System.Collections;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using MMOR.NET.Collections;

namespace MMOR.NET.Benchmarks.Collections {
[MemoryDiagnoser]
public class RentedBoolListBench {
  [Params(1024, 65536, 1048576)]
  public int N;

  private bool[] template_ = null!;
  private int mid_;
  private bool[] copy_dst_ = null!;

  private RentedBoolList rented_ = null!;
  private BitArray bitarray_     = null!;
  private List<bool> list_       = null!;

  [GlobalSetup]
  public void Setup() {
    template_ = new bool[N];
    var rng   = new System.Random(42);
    for (int i = 0; i < N; ++i) {
      template_[i] = rng.Next(2) == 0;
    }
    mid_            = N / 2;
    template_[mid_] = true;

    // +1 so Insert benches never trigger a resize on the measured path.
    rented_   = new RentedBoolList(N + 1);
    bitarray_ = new BitArray(N);
    list_     = new List<bool>(N + 1);
    for (int i = 0; i < N; ++i) {
      rented_.Add(template_[i]);
      list_.Add(template_[i]);
      bitarray_[i] = template_[i];
    }

    copy_dst_ = new bool[N];
  }

  [GlobalCleanup]
  public void Cleanup() {
    rented_.Dispose();
  }

  [Benchmark]
  public int RentedBoolList_Add() {
    using RentedBoolList list = new();
    for (int i = 0; i < N; ++i) {
      list.Add(template_[i]);
    }
    return list.Count;
  }

  [Benchmark]
  public int ListBool_Add() {
    List<bool> list = new();
    for (int i = 0; i < N; ++i) {
      list.Add(template_[i]);
    }
    return list.Count;
  }

  [Benchmark]
  public int RentedBoolList_Add_Presized() {
    using RentedBoolList list = new(N);
    for (int i = 0; i < N; ++i) {
      list.Add(template_[i]);
    }
    return list.Count;
  }

  [Benchmark]
  public int ListBool_Add_Presized() {
    List<bool> list = new(N);
    for (int i = 0; i < N; ++i) {
      list.Add(template_[i]);
    }
    return list.Count;
  }

  [Benchmark]
  public int RentedBoolList_GetBit() {
    int count = 0;
    for (int i = 0; i < N; ++i) {
      if (rented_[i]) {
        ++count;
      }
    }
    return count;
  }

  [Benchmark]
  public int ListBool_GetBit() {
    int count = 0;
    for (int i = 0; i < N; ++i) {
      if (list_[i]) {
        ++count;
      }
    }
    return count;
  }

  [Benchmark]
  public int BitArray_GetBit() {
    int count = 0;
    for (int i = 0; i < N; ++i) {
      if (bitarray_[i]) {
        ++count;
      }
    }
    return count;
  }

  [Benchmark]
  public int RentedBoolList_SetBit() {
    for (int i = 0; i < N; ++i) {
      rented_[i] = true;
    }
    return rented_.Count;
  }

  [Benchmark]
  public int ListBool_SetBit() {
    for (int i = 0; i < N; ++i) {
      list_[i] = true;
    }
    return list_.Count;
  }

  [Benchmark]
  public int BitArray_SetBit() {
    for (int i = 0; i < N; ++i) {
      bitarray_[i] = true;
    }
    return bitarray_.Length;
  }

  [Benchmark]
  public bool RentedBoolList_CopyTo() {
    rented_.CopyTo(copy_dst_, 0);
    return copy_dst_[N - 1];
  }

  [Benchmark]
  public bool ListBool_CopyTo() {
    list_.CopyTo(copy_dst_, 0);
    return copy_dst_[N - 1];
  }

  [Benchmark]
  public bool BitArray_CopyTo() {
    bitarray_.CopyTo(copy_dst_, 0);
    return copy_dst_[N - 1];
  }

  [Benchmark]
  public int RentedBoolList_InsertRemoveAt_Middle() {
    rented_.Insert(mid_, true);
    rented_.RemoveAt(mid_);
    return rented_.Count;
  }

  [Benchmark]
  public int ListBool_InsertRemoveAt_Middle() {
    list_.Insert(mid_, true);
    list_.RemoveAt(mid_);
    return list_.Count;
  }

  [Benchmark]
  public int RentedBoolList_InsertRemoveAt_Front() {
    rented_.Insert(0, true);
    rented_.RemoveAt(0);
    return rented_.Count;
  }

  [Benchmark]
  public int ListBool_InsertRemoveAt_Front() {
    list_.Insert(0, true);
    list_.RemoveAt(0);
    return list_.Count;
  }
}
}
