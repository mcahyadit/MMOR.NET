using System;
using System.Numerics;
using Xunit;

namespace MMOR.NET.Bits {
public class BitReverseTest {
  private static readonly uint[] kSamples32 = [
    0u,
    1u,
    uint.MaxValue,
    0xAAAAAAAAu,
    0x55555555u,
    0x80000001u,
    0x0000FFFFu,
    0xDEADBEEFu,
  ];

  private static readonly ulong[] kSamples64 = [
    0ul,
    1ul,
    ulong.MaxValue,
    0xAAAAAAAAAAAAAAAAul,
    0x5555555555555555ul,
    0x8000000000000001ul,
    0x00000000FFFFFFFFul,
    0xDEADBEEFCAFEBABEul,
  ];

  private static uint RandomU32(System.Random rng) {
    return (uint)rng.Next();
  }

  private static ulong RandomU64(System.Random rng) {
    return ((ulong)rng.Next() << 32) | (uint)rng.Next();
  }

  /*
   * Deliberately naive bit-by-bit references. They share no logic with the SWAR masks or the
   * ARM/x86 intrinsics under test, so a disagreement points at the implementation, not the oracle.
   */

  private static uint ReferenceReverse(uint value) {
    uint result = 0;
    for (int i = 0; i < 32; ++i) result |= ((value >> i) & 1u) << (31 - i);
    return result;
  }

  private static ulong ReferenceReverse(ulong value) {
    ulong result = 0;
    for (int i = 0; i < 64; ++i) result |= ((value >> i) & 1ul) << (63 - i);
    return result;
  }

  /*  Mask of the bits covered by [start, start + length); callers guarantee start + length <= 32.
   */
  private static uint ReferenceFieldMask32(int start, int length) {
    uint low = length >= 32 ? ~0u : (1u << length) - 1;
    return low << start;
  }

  private static ulong ReferenceFieldMask64(int start, int length) {
    ulong low = length >= 64 ? ~0ul : (1ul << length) - 1;
    return low << start;
  }

  /*  Reverses bits [start, start + length) in place, leaving every other bit untouched. */
  private static uint ReferenceReverse(uint value, int start, int length) {
    uint result = value & ~ReferenceFieldMask32(start, length);
    for (int i = 0; i < length; ++i)
      result |= ((value >> (start + i)) & 1u) << (start + length - 1 - i);
    return result;
  }

  private static ulong ReferenceReverse(ulong value, int start, int length) {
    ulong result = value & ~ReferenceFieldMask64(start, length);
    for (int i = 0; i < length; ++i)
      result |= ((value >> (start + i)) & 1ul) << (start + length - 1 - i);
    return result;
  }

  [Fact]
  public void ReverseBitsByte_MatchesReference_Exhaustively() {
    for (int i = 0; i <= byte.MaxValue; ++i) {
      byte value = (byte)i;
      // A byte lives in the low 8 bits of the 32-bit reference reverse, which puts it in the high
      // 8 bits, hence the shift back down.
      byte expect = (byte)(ReferenceReverse((uint)value) >> 24);
      Assert.Equal(expect, BitOps.ReverseBits(value));
    }
  }

  [Fact]
  public void ReverseBitsByte_IsSelfInverse() {
    for (int i = 0; i <= byte.MaxValue; ++i) {
      byte value = (byte)i;
      Assert.Equal(value, BitOps.ReverseBits(BitOps.ReverseBits(value)));
    }
  }

  [Fact]
  public void ReverseBitsU32_MatchesReference() {
    var rng = new System.Random(42);
    foreach (uint sample in kSamples32)
      Assert.Equal(ReferenceReverse(sample), BitOps.ReverseBits(sample));
    for (int iter = 0; iter < 4096; ++iter) {
      uint value = RandomU32(rng);
      Assert.Equal(ReferenceReverse(value), BitOps.ReverseBits(value));
    }
  }

  [Fact]
  public void ReverseBitsU64_MatchesReference() {
    var rng = new System.Random(42);
    foreach (ulong sample in kSamples64)
      Assert.Equal(ReferenceReverse(sample), BitOps.ReverseBits(sample));
    for (int iter = 0; iter < 4096; ++iter) {
      ulong value = RandomU64(rng);
      Assert.Equal(ReferenceReverse(value), BitOps.ReverseBits(value));
    }
  }

  [Fact]
  public void ReverseBits_IsSelfInverse() {
    var rng = new System.Random(7);
    for (int iter = 0; iter < 4096; ++iter) {
      uint u32  = RandomU32(rng);
      ulong u64 = RandomU64(rng);
      Assert.Equal(u32, BitOps.ReverseBits(BitOps.ReverseBits(u32)));
      Assert.Equal(u64, BitOps.ReverseBits(BitOps.ReverseBits(u64)));
    }
  }

  /*  ReverseBits(value, length) is the special case start == 0 of the field overload. */
  [Fact]
  public void ReverseBitsU32_WithLength_MatchesReference_ForEveryLength() {
    foreach (uint sample in kSamples32) {
      for (int length = 0; length <= 32; ++length)
        Assert.Equal(ReferenceReverse(sample, 0, length), BitOps.ReverseBits(sample, length));
    }
  }

  [Fact]
  public void ReverseBitsU64_WithLength_MatchesReference_ForEveryLength() {
    foreach (ulong sample in kSamples64) {
      for (int length = 0; length <= 64; ++length)
        Assert.Equal(ReferenceReverse(sample, 0, length), BitOps.ReverseBits(sample, length));
    }
  }

  [Fact]
  public void ReverseBitsU32_Field_MatchesReference_ForEveryValidRange() {
    foreach (uint sample in kSamples32) {
      for (int start = 0; start < 32; ++start)
        for (int length = 0; start + length <= 32; ++length)
          Assert.Equal(ReferenceReverse(sample, start, length),
              BitOps.ReverseBits(sample, start, length));
    }
  }

  [Fact]
  public void ReverseBitsU64_Field_MatchesReference_ForEveryValidRange() {
    foreach (ulong sample in kSamples64) {
      for (int start = 0; start < 64; ++start)
        for (int length = 0; start + length <= 64; ++length)
          Assert.Equal(ReferenceReverse(sample, start, length),
              BitOps.ReverseBits(sample, start, length));
    }
  }

  [Fact]
  public void ReverseBitsU32_Field_MatchesReference_ForRandomValues() {
    var rng = new System.Random(1234);
    for (int iter = 0; iter < 2048; ++iter) {
      uint value = RandomU32(rng);
      int start  = rng.Next(32);
      int length = rng.Next(32 - start + 1);
      Assert.Equal(ReferenceReverse(value, start, length),
          BitOps.ReverseBits(value, start, length));
    }
  }

  [Fact]
  public void ReverseBitsU64_Field_MatchesReference_ForRandomValues() {
    var rng = new System.Random(1234);
    for (int iter = 0; iter < 2048; ++iter) {
      ulong value = RandomU64(rng);
      int start   = rng.Next(64);
      int length  = rng.Next(64 - start + 1);
      Assert.Equal(ReferenceReverse(value, start, length),
          BitOps.ReverseBits(value, start, length));
    }
  }

  [Fact]
  public void ReverseBitsU32_Field_IsSelfInverse() {
    foreach (uint sample in kSamples32) {
      for (int start = 0; start < 32; ++start)
        for (int length = 0; start + length <= 32; ++length) {
          uint once  = BitOps.ReverseBits(sample, start, length);
          uint twice = BitOps.ReverseBits(once, start, length);
          Assert.Equal(sample, twice);
        }
    }
  }

  [Fact]
  public void ReverseBitsU64_Field_IsSelfInverse() {
    foreach (ulong sample in kSamples64) {
      for (int start = 0; start < 64; ++start)
        for (int length = 0; start + length <= 64; ++length) {
          ulong once  = BitOps.ReverseBits(sample, start, length);
          ulong twice = BitOps.ReverseBits(once, start, length);
          Assert.Equal(sample, twice);
        }
    }
  }

  /*  length == bitcount is the only length at which the range form must agree with the plain one.
   */
  [Fact]
  public void ReverseBitsU32_FullWidthLength_AgreesWithPlainOverload() {
    foreach (uint sample in kSamples32)
      Assert.Equal(BitOps.ReverseBits(sample), BitOps.ReverseBits(sample, 32));
  }

  [Fact]
  public void ReverseBitsU64_FullWidthLength_AgreesWithPlainOverload() {
    foreach (ulong sample in kSamples64)
      Assert.Equal(BitOps.ReverseBits(sample), BitOps.ReverseBits(sample, 64));
  }

  [Theory]
  [InlineData(0, 0)]
  [InlineData(31, 1)]
  [InlineData(31, 0)]
  [InlineData(0, 1)]
  public void ReverseBitsU32_Field_ZeroOrOneBit_IsIdentity(int start, int length) {
    foreach (uint sample in kSamples32)
      Assert.Equal(sample, BitOps.ReverseBits(sample, start, length));
  }

  [Theory]
  [InlineData(63, 1)]
  [InlineData(63, 0)]
  [InlineData(0, 1)]
  public void ReverseBitsU64_Field_ZeroOrOneBit_IsIdentity(int start, int length) {
    foreach (ulong sample in kSamples64)
      Assert.Equal(sample, BitOps.ReverseBits(sample, start, length));
  }

  [Fact]
  public void ReverseBitsVector_MatchesScalarPerLane() {
    var rng   = new System.Random(20260905);
    var lanes = new ulong[Vector<ulong>.Count];
    for (int iter = 0; iter < 512; ++iter) {
      for (int i = 0; i < lanes.Length; ++i) lanes[i] = RandomU64(rng);
      Vector<ulong> reversed = BitOps.ReverseBits(new Vector<ulong>(lanes));
      for (int i = 0; i < lanes.Length; ++i) Assert.Equal(ReferenceReverse(lanes[i]), reversed[i]);
    }
  }

  [Fact]
  public void ReverseBitsVector_IsSelfInversePerLane() {
    var rng   = new System.Random(20260905);
    var lanes = new ulong[Vector<ulong>.Count];
    for (int iter = 0; iter < 512; ++iter) {
      for (int i = 0; i < lanes.Length; ++i) lanes[i] = RandomU64(rng);
      Vector<ulong> v     = new(lanes);
      Vector<ulong> twice = BitOps.ReverseBits(BitOps.ReverseBits(v));
      for (int i = 0; i < lanes.Length; ++i) Assert.Equal(lanes[i], twice[i]);
    }
  }

  [Fact]
  public void ReverseBitsVector_HandlesAllOnesAndZero() {
    Vector<ulong> ones = Vector<ulong>.One * ulong.MaxValue;
    Vector<ulong> zero = Vector<ulong>.Zero;
    for (int i = 0; i < Vector<ulong>.Count; ++i) {
      Assert.Equal(ulong.MaxValue, BitOps.ReverseBits(ones)[i]);
      Assert.Equal(0ul, BitOps.ReverseBits(zero)[i]);
    }
  }

  /*
   * BZHI saturation, which is what an "ignore length > bitcount" policy in ReverseBits would lean
   * on: index >= OperandSize leaves the value untouched (it does NOT clear everything), and the
   * index is taken modulo 256. Cf. the Intel SDM operation for BZHI.
   */

  [Theory]
  [InlineData(0)]
  [InlineData(1)]
  [InlineData(31)]
  [InlineData(32)]
  [InlineData(33)]
  [InlineData(255)]
  public void Bzhi32_SaturatesAtOperandSize(int index) {
    const uint value = 0xDEADBEEFu;
    uint expect      = index >= 32 ? value : value & ((1u << index) - 1);
    Assert.Equal(expect, BitOps.Bzhi32(value, index));
    Assert.Equal(expect, BitOps.Bzhi32Sw(value, index));
  }

  [Theory]
  [InlineData(0)]
  [InlineData(1)]
  [InlineData(63)]
  [InlineData(64)]
  [InlineData(65)]
  [InlineData(255)]
  public void Bzhi64_SaturatesAtOperandSize(int index) {
    const ulong value = 0xDEADBEEFCAFEBABEul;
    ulong expect      = index >= 64 ? value : value & ((1ul << index) - 1);
    Assert.Equal(expect, BitOps.Bzhi64(value, index));
    Assert.Equal(expect, BitOps.Bzhi64Sw(value, index));
  }

  [Fact]
  public void Bzhi32_MatchesSwFallback_AcrossTheWholeByteIndexRange() {
    var rng = new System.Random(31337);
    for (int iter = 0; iter < 16; ++iter) {
      uint value = RandomU32(rng);
      for (int index = 0; index <= byte.MaxValue; ++index)
        Assert.Equal(BitOps.Bzhi32Sw(value, index), BitOps.Bzhi32(value, index));
    }
  }

  [Fact]
  public void Bzhi64_MatchesSwFallback_AcrossTheWholeByteIndexRange() {
    var rng = new System.Random(31337);
    for (int iter = 0; iter < 16; ++iter) {
      ulong value = RandomU64(rng);
      for (int index = 0; index <= byte.MaxValue; ++index)
        Assert.Equal(BitOps.Bzhi64Sw(value, index), BitOps.Bzhi64(value, index));
    }
  }

  /*
   * ------------------------------------------------------------------------------------------
   * Out-of-range policy.
   *
   * These pin the current ArgumentOutOfRangeException behaviour. If the policy flips to
   * saturating ("ignore length > bitcount"), only this region changes:
   *   length >= 32 (64)                    must behave like ReverseBits(value)
   *   length > 32 - start (64 - start)    must behave like length == 32 - start (64 - start)
   * ------------------------------------------------------------------------------------------
   */

  [Theory]
  [InlineData(-1)]
  [InlineData(33)]
  [InlineData(int.MaxValue)]
  [InlineData(int.MinValue)]
  public void ReverseBitsU32_WithLength_RejectsOutOfRangeLength(int length) {
    ArgumentOutOfRangeException ex =
        Assert.Throws<ArgumentOutOfRangeException>(() => BitOps.ReverseBits(0xDEADBEEFu, length));
    Assert.Equal("length", ex.ParamName);
  }

  [Theory]
  [InlineData(-1)]
  [InlineData(65)]
  [InlineData(int.MaxValue)]
  [InlineData(int.MinValue)]
  public void ReverseBitsU64_WithLength_RejectsOutOfRangeLength(int length) {
    ArgumentOutOfRangeException ex = Assert.Throws<ArgumentOutOfRangeException>(
        () => BitOps.ReverseBits(0xDEADBEEFCAFEBABEul, length));
    Assert.Equal("length", ex.ParamName);
  }

  [Theory]
  [InlineData(-1, 1)]  // negative start
  [InlineData(32, 1)]  // start at or past the bit count
  [InlineData(0, 33)]  // length past the bit count
  [InlineData(30, 3)]  // start + length past the bit count
  [InlineData(31, 2)]
  [InlineData(int.MaxValue, 1)]
  public void ReverseBitsU32_Field_RejectsOutOfRangeStartOrLength(int start, int length) {
    ArgumentOutOfRangeException ex = Assert.Throws<ArgumentOutOfRangeException>(
        () => BitOps.ReverseBits(0xDEADBEEFu, start, length));
    Assert.True(ex.ParamName is "start" or "length", $"Unexpected ParamName: {ex.ParamName}");
  }

  [Theory]
  [InlineData(-1, 1)]
  [InlineData(64, 1)]
  [InlineData(0, 65)]
  [InlineData(62, 3)]
  [InlineData(63, 2)]
  [InlineData(int.MaxValue, 1)]
  public void ReverseBitsU64_Field_RejectsOutOfRangeStartOrLength(int start, int length) {
    ArgumentOutOfRangeException ex = Assert.Throws<ArgumentOutOfRangeException>(
        () => BitOps.ReverseBits(0xDEADBEEFCAFEBABEul, start, length));
    Assert.True(ex.ParamName is "start" or "length", $"Unexpected ParamName: {ex.ParamName}");
  }
}
}