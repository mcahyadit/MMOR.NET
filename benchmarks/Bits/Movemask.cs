using System;
using System.Numerics;
using BenchmarkDotNet.Attributes;
using MMOR.NET.Bits;

namespace MMOR.NET.Benchmarks.Bits {
public class MovemaskBench {
  private const int kVecCount = 4096;

  private Vector<ulong>[] vectors64_ = null!;
  private Vector<uint>[] vectors32_  = null!;

  [GlobalSetup]
  public void Setup() {
    var rng    = new System.Random(42);
    vectors64_ = new Vector<ulong>[kVecCount];
    for (int j = 0; j < vectors64_.Length; ++j) {
      var a = new ulong[Vector<ulong>.Count];
      for (int i = 0; i < a.Length; ++i) {
        a[i] = ((ulong)rng.Next() << 32) | (uint)rng.Next();
      }
      vectors64_[j] = new Vector<ulong>(a);
    }

    vectors32_ = new Vector<uint>[kVecCount];
    for (int j = 0; j < vectors32_.Length; ++j) {
      var a = new uint[Vector<uint>.Count];
      for (int i = 0; i < a.Length; ++i) {
        a[i] = (uint)rng.Next();
      }
      vectors32_[j] = new Vector<uint>(a);
    }
  }

  [Benchmark]
  public ulong MmMovemaskEpi64() {
    ulong acc = 0;
    for (int i = 0; i < vectors64_.Length; ++i) {
      acc |= BitOps.MmMovemaskEpi64(vectors64_[i]);
    }
    return acc;
  }

  [Benchmark]
  public ulong MmMovemaskEpi64Sw() {
    ulong acc = 0;
    for (int i = 0; i < vectors64_.Length; ++i) {
      acc |= BitOps.MmMovemaskEpi64Sw(vectors64_[i]);
    }
    return acc;
  }

  [Benchmark]
  public ulong MmMovemaskEpi32() {
    ulong acc = 0;
    for (int i = 0; i < vectors32_.Length; ++i) {
      acc |= BitOps.MmMovemaskEpi32(vectors32_[i]);
    }
    return acc;
  }

  [Benchmark]
  public ulong MmMovemaskEpi32Sw() {
    ulong acc = 0;
    for (int i = 0; i < vectors32_.Length; ++i) {
      acc |= BitOps.MmMovemaskEpi32Sw(vectors32_[i]);
    }
    return acc;
  }
}
}
