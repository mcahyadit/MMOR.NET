using System.Numerics;
using Xunit;

namespace MMOR.NET.Bits {
using Random = System.Random;
public partial class BitOpsTest {
  [Fact]
  public void MmMovemaskEpi64_MatchesSoftwareReference() {
    Random rng = new Random(42);
    for (int i = 0; i < 512; ++i) {
      ulong[] lanes = new ulong[Vector<ulong>.Count];
      for (int j = 0; j < lanes.Length; ++j) {
        lanes[j] = rng.NextU64();
      }
      Vector<ulong> v = new(lanes);
      Assert.Equal(ReferenceMovemask64(v), BitOps.MmMovEpi64Mask(v));
      Assert.Equal(ReferenceMovemask64(v), BitOps.MmMovEpi64MaskSw(v));
    }
  }

  [Fact]
  public void MmMovemaskEpi32_MatchesSoftwareReference() {
    Random rng = new Random(42);
    for (int iter = 0; iter < 512; ++iter) {
      uint[] lanes = new uint[Vector<uint>.Count];
      for (int j = 0; j < lanes.Length; ++j) {
        lanes[j] = (uint)rng.Next();
      }
      Vector<uint> v = new(lanes);
      Assert.Equal(ReferenceMovemask32(v), BitOps.MmMovEpi32Mask(v));
      Assert.Equal(ReferenceMovemask32(v), BitOps.MmMovEpi32MaskSw(v));
    }
  }

  [Fact]
  public void MmMovemaskEpi64_HandlesAllNegativeLanes() {
    ulong[] lanes = new ulong[Vector<ulong>.Count];
    for (int j = 0; j < lanes.Length; ++j) {
      lanes[j] = ulong.MaxValue;
    }
    Vector<ulong> v = new(lanes);
    ulong expect    = (1ul << Vector<ulong>.Count) - 1;
    Assert.Equal(expect, BitOps.MmMovEpi64Mask(v));
    Assert.Equal(expect, BitOps.MmMovEpi64MaskSw(v));
  }

  [Fact]
  public void MmMovemaskEpi64_HandlesAllZeroLanes() {
    Vector<ulong> v = Vector<ulong>.Zero;
    Assert.Equal(0ul, BitOps.MmMovEpi64Mask(v));
    Assert.Equal(0ul, BitOps.MmMovEpi64MaskSw(v));
  }

  [Fact]
  public void MmMovemaskEpi32_HandlesAllNegativeLanes() {
    uint[] lanes = new uint[Vector<uint>.Count];
    for (int j = 0; j < lanes.Length; ++j) {
      lanes[j] = uint.MaxValue;
    }
    Vector<uint> v = new(lanes);
    ulong expect   = (1ul << Vector<uint>.Count) - 1;
    Assert.Equal(expect, BitOps.MmMovEpi32Mask(v));
    Assert.Equal(expect, BitOps.MmMovEpi32MaskSw(v));
  }

  [Fact]
  public void MmMovemaskEpi32_HandlesAllZeroLanes() {
    Vector<uint> v = Vector<uint>.Zero;
    Assert.Equal(0ul, BitOps.MmMovEpi32Mask(v));
    Assert.Equal(0ul, BitOps.MmMovEpi32MaskSw(v));
  }

  [Fact]
  public void MmMovmEpi64Emu_ExpandsActiveBits() {
    Random rng = new Random(7);
    for (int i = 0; i < 512; ++i) {
      ulong mask      = rng.NextU64();
      Vector<ulong> v = BitOps.MmMovMaskEpi64Emu(mask);
      for (int j = 0; j < Vector<ulong>.Count; ++j) {
        ulong expect = ((mask >> j) & 1) == 1 ? ulong.MaxValue : 0;
        Assert.Equal(expect, v[j]);
      }
    }
  }

  [Fact]
  public void MmMovmEpi32Emu_ExpandsActiveBits() {
    Random rng = new Random(7);
    for (int i = 0; i < 512; ++i) {
      uint mask      = (uint)rng.Next();
      Vector<uint> v = BitOps.MmMovMaskEpi32Emu(mask);
      for (int j = 0; j < Vector<uint>.Count; ++j) {
        uint expect = ((mask >> j) & 1) == 1 ? uint.MaxValue : 0;
        Assert.Equal(expect, v[j]);
      }
    }
  }

  [Fact]
  public void MovemaskEpi64_Movm_Roundtrip() {
    Random rng = new Random(1234);
    for (int i = 0; i < 512; ++i) {
      ulong mask   = rng.NextU64();
      ulong expect = mask & ((1ul << Vector<ulong>.Count) - 1);
      Assert.Equal(expect, BitOps.MmMovEpi64Mask(BitOps.MmMovMaskEpi64Emu(mask)));
      Assert.Equal(expect, BitOps.MmMovEpi64MaskSw(BitOps.MmMovMaskEpi64Emu(mask)));
    }
  }

  [Fact]
  public void MovemaskEpi32_Movm_Roundtrip() {
    Random rng = new Random(1234);
    for (int i = 0; i < 512; ++i) {
      uint mask    = (uint)rng.Next();
      ulong expect = mask & ((1ul << Vector<uint>.Count) - 1);
      Assert.Equal(expect, BitOps.MmMovEpi32Mask(BitOps.MmMovMaskEpi32Emu(mask)));
      Assert.Equal(expect, BitOps.MmMovEpi32MaskSw(BitOps.MmMovMaskEpi32Emu(mask)));
    }
  }
}
}
