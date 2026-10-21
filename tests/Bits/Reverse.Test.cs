using System;
using Xunit;

namespace MMOR.NET.Bits {
using Random = System.Random;
public partial class BitReverseTest {
  [Fact]
  public void ReverseBitsU8_MatchesReference_Exhaustively() {
    for (int i = 0; i <= byte.MaxValue; ++i) {
      byte value  = (byte)i;
      byte expect = Reverse8Reference(value);
      Assert.Equal(expect, BitOps.ReverseBits(value));
    }
  }

  [Fact]
  public void ReverseBitsU8_IsSelfInverse() {
    for (int i = 0; i <= byte.MaxValue; ++i) {
      byte value = (byte)i;
      Assert.Equal(value, BitOps.ReverseBits(BitOps.ReverseBits(value)));
    }
  }

  [Fact]
  public void ReverseBitsU32_MatchesReference() {
    var rng = new Random(42);
    foreach (uint sample in kSamples32)
      Assert.Equal(Reverse32Reference(sample), BitOps.ReverseBits(sample));
    for (int i = 0; i < 30; ++i) {
      uint value = rng.NextU32();
      Assert.Equal(Reverse32Reference(value), BitOps.ReverseBits(value));
    }
  }

  [Fact]
  public void ReverseBitsU64_MatchesReference() {
    var rng = new Random(42);
    foreach (ulong sample in kSamples64)
      Assert.Equal(Reverse64Reference(sample), BitOps.ReverseBits(sample));
    for (int i = 0; i < 30; ++i) {
      ulong value = rng.NextU64();
      Assert.Equal(Reverse64Reference(value), BitOps.ReverseBits(value));
    }
  }

  [Fact]
  public void ReverseBits_IsSelfInverse() {
    var rng = new Random(7);
    for (int i = 0; i < 30; ++i) {
      uint u32  = rng.NextU32();
      ulong u64 = rng.NextU64();
      Assert.Equal(u32, BitOps.ReverseBits(BitOps.ReverseBits(u32)));
      Assert.Equal(u64, BitOps.ReverseBits(BitOps.ReverseBits(u64)));
    }
  }

  /*  ReverseBits(value, length) is the special case start == 0 of the field overload. */
  [Fact]
  public void ReverseBitsU32_WithLength_MatchesReference_ForEveryLength() {
    foreach (uint sample in kSamples32) {
      for (int length = 0; length <= 32; ++length)
        Assert.Equal(Reverse32Reference(sample, length: length),
            BitOps.ReverseBits(sample, length));
    }
  }

  [Fact]
  public void ReverseBitsU64_WithLength_MatchesReference_ForEveryLength() {
    foreach (ulong sample in kSamples64) {
      for (int length = 0; length <= 64; ++length)
        Assert.Equal(Reverse64Reference(sample, length: length),
            BitOps.ReverseBits(sample, length));
    }
  }

  [Fact]
  public void ReverseBitsU32_Field_MatchesReference_ForEveryValidRange() {
    foreach (uint sample in kSamples32) {
      for (int start = 0; start < 32; ++start)
        for (int length = 0; start + length <= 32; ++length)
          Assert.Equal(Reverse32Reference(sample, start, length),
              BitOps.ReverseBits(sample, start, length));
    }
  }

  [Fact]
  public void ReverseBitsU64_Field_MatchesReference_ForEveryValidRange() {
    foreach (ulong sample in kSamples64) {
      for (int start = 0; start < 64; ++start)
        for (int length = 0; start + length <= 64; ++length)
          Assert.Equal(Reverse64Reference(sample, start, length),
              BitOps.ReverseBits(sample, start, length));
    }
  }

  [Fact]
  public void ReverseBitsU32_Field_MatchesReference_ForRandomValues() {
    var rng = new Random(1234);
    for (int i = 0; i < 30; ++i) {
      uint value = rng.NextU32();
      int start  = rng.Next(32);
      int length = rng.Next(32 - start + 1);
      Assert.Equal(Reverse32Reference(value, start, length),
          BitOps.ReverseBits(value, start, length));
    }
  }

  [Fact]
  public void ReverseBitsU64_Field_MatchesReference_ForRandomValues() {
    var rng = new Random(1234);
    for (int i = 0; i < 30; ++i) {
      ulong value = rng.NextU64();
      int start   = rng.Next(64);
      int length  = rng.Next(64 - start + 1);
      Assert.Equal(Reverse64Reference(value, start, length),
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
