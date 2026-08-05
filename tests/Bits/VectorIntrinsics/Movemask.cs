using System;
using System.Numerics;
using Xunit;

namespace MMOR.NET.Bits {
public partial class VectorIntrinsicsTest {
  private static ulong ReferenceMovemask64(Vector<ulong> v) {
    ulong result = 0;
    for (int i = 0; i < Vector<ulong>.Count; ++i) {
      result |= (v[i] >> 63) << i;
    }
    return result;
  }

  private static ulong ReferenceMovemask32(Vector<uint> v) {
    ulong result = 0;
    for (int i = 0; i < Vector<uint>.Count; ++i) {
      result |= (ulong)(v[i] >> 31) << i;
    }
    return result;
  }

  private static ulong RandomU64(System.Random rng) {
    return ((ulong)rng.Next() << 32) | (uint)rng.Next();
  }

  [Fact]
  public void MmMovemaskEpi64_MatchesSoftwareReference() {
    var rng = new System.Random(42);
    for (int iter = 0; iter < 512; ++iter) {
      var lanes = new ulong[Vector<ulong>.Count];
      for (int i = 0; i < lanes.Length; ++i) {
        lanes[i] = RandomU64(rng);
      }
      Vector<ulong> v = new(lanes);
      Assert.Equal(ReferenceMovemask64(v), BitOps.MmMovemaskEpi64(v));
      Assert.Equal(ReferenceMovemask64(v), BitOps.MmMovemaskEpi64Sw(v));
    }
  }

  [Fact]
  public void MmMovemaskEpi32_MatchesSoftwareReference() {
    var rng = new System.Random(42);
    for (int iter = 0; iter < 512; ++iter) {
      var lanes = new uint[Vector<uint>.Count];
      for (int i = 0; i < lanes.Length; ++i) {
        lanes[i] = (uint)rng.Next();
      }
      Vector<uint> v = new(lanes);
      Assert.Equal(ReferenceMovemask32(v), BitOps.MmMovemaskEpi32(v));
      Assert.Equal(ReferenceMovemask32(v), BitOps.MmMovemaskEpi32Sw(v));
    }
  }

  [Fact]
  public void MmMovemaskEpi64_HandlesAllNegativeLanes() {
    var lanes = new ulong[Vector<ulong>.Count];
    for (int i = 0; i < lanes.Length; ++i) {
      lanes[i] = ulong.MaxValue;
    }
    Vector<ulong> v = new(lanes);
    ulong expect    = (1ul << Vector<ulong>.Count) - 1;
    Assert.Equal(expect, BitOps.MmMovemaskEpi64(v));
    Assert.Equal(expect, BitOps.MmMovemaskEpi64Sw(v));
  }

  [Fact]
  public void MmMovemaskEpi64_HandlesAllZeroLanes() {
    Vector<ulong> v = Vector<ulong>.Zero;
    Assert.Equal(0ul, BitOps.MmMovemaskEpi64(v));
    Assert.Equal(0ul, BitOps.MmMovemaskEpi64Sw(v));
  }

  [Fact]
  public void MmMovemaskEpi32_HandlesAllNegativeLanes() {
    var lanes = new uint[Vector<uint>.Count];
    for (int i = 0; i < lanes.Length; ++i) {
      lanes[i] = uint.MaxValue;
    }
    Vector<uint> v = new(lanes);
    ulong expect   = (1ul << Vector<uint>.Count) - 1;
    Assert.Equal(expect, BitOps.MmMovemaskEpi32(v));
    Assert.Equal(expect, BitOps.MmMovemaskEpi32Sw(v));
  }

  [Fact]
  public void MmMovemaskEpi32_HandlesAllZeroLanes() {
    Vector<uint> v = Vector<uint>.Zero;
    Assert.Equal(0ul, BitOps.MmMovemaskEpi32(v));
    Assert.Equal(0ul, BitOps.MmMovemaskEpi32Sw(v));
  }

  [Fact]
  public void MmMovmEpi64Emu_ExpandsActiveBits() {
    var rng = new System.Random(7);
    for (int iter = 0; iter < 512; ++iter) {
      ulong mask      = RandomU64(rng);
      Vector<ulong> v = BitOps.MmMovmEpi64Emu(mask);
      for (int i = 0; i < Vector<ulong>.Count; ++i) {
        ulong expect = ((mask >> i) & 1) == 1 ? ulong.MaxValue : 0;
        Assert.Equal(expect, v[i]);
      }
    }
  }

  [Fact]
  public void MmMovmEpi32Emu_ExpandsActiveBits() {
    var rng = new System.Random(7);
    for (int iter = 0; iter < 512; ++iter) {
      uint mask      = (uint)rng.Next();
      Vector<uint> v = BitOps.MmMovmEpi32Emu(mask);
      for (int i = 0; i < Vector<uint>.Count; ++i) {
        uint expect = ((mask >> i) & 1) == 1 ? uint.MaxValue : 0;
        Assert.Equal(expect, v[i]);
      }
    }
  }

  [Fact]
  public void MovemaskEpi64_Movm_Roundtrip() {
    var rng = new System.Random(1234);
    for (int iter = 0; iter < 512; ++iter) {
      ulong mask   = RandomU64(rng);
      ulong expect = mask & ((1ul << Vector<ulong>.Count) - 1);
      Assert.Equal(expect, BitOps.MmMovemaskEpi64(BitOps.MmMovmEpi64Emu(mask)));
      Assert.Equal(expect, BitOps.MmMovemaskEpi64Sw(BitOps.MmMovmEpi64Emu(mask)));
    }
  }

  [Fact]
  public void MovemaskEpi32_Movm_Roundtrip() {
    var rng = new System.Random(1234);
    for (int iter = 0; iter < 512; ++iter) {
      uint mask    = (uint)rng.Next();
      ulong expect = mask & ((1ul << Vector<uint>.Count) - 1);
      Assert.Equal(expect, BitOps.MmMovemaskEpi32(BitOps.MmMovmEpi32Emu(mask)));
      Assert.Equal(expect, BitOps.MmMovemaskEpi32Sw(BitOps.MmMovmEpi32Emu(mask)));
    }
  }
}
}
