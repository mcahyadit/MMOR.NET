using System.Runtime.Intrinsics.X86;
using Xunit;

namespace MMOR.NET.Bits {
using Random = System.Random;
public partial class BitOpsTest {
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
  [InlineData(260)]
  public void Bzhi32_SaturatesAtOperandSize(int index) {
    Random rng = new Random(0xdeadbef);
    for (int i = 0; i < 30; ++i) {
      uint value = rng.NextU32();
      Assert.Equal(Bzhi32Reference(value, index), BitOps.Bzhi32Sw(value, index));
    }
  }

  [Theory]
  [InlineData(0)]
  [InlineData(1)]
  [InlineData(63)]
  [InlineData(64)]
  [InlineData(65)]
  [InlineData(255)]
  [InlineData(260)]
  public void Bzhi64_SaturatesAtOperandSize(int index) {
    Random rng = new Random(0xdeadbef);
    for (int i = 0; i < 30; ++i) {
      ulong value = rng.NextU64();
      Assert.Equal(Bzhi64Reference(value, index), BitOps.Bzhi64Sw(value, index));
    }
  }

  [Fact]
  public void Bzhi32_MatchesSwFallback() {
    if (!Bmi2.IsSupported)
      return;

    Random rng = new Random(31337);
    for (int i = 0; i < 30; ++i) {
      uint value = rng.NextU32();
      for (int index = 0; index <= 2 * byte.MaxValue; ++index) {
        Assert.Equal(BitOps.Bzhi32(value, index), BitOps.Bzhi32Sw(value, index));
      }
    }
  }

  [Fact]
  public void Bzhi64_MatchesSwFallback() {
    if (!Bmi2.X64.IsSupported)
      return;

    Random rng = new System.Random(31337);
    for (int i = 0; i < 30; ++i) {
      ulong value = rng.NextU64();
      {
        for (int index = 0; index <= 2 * byte.MaxValue; ++index)
          Assert.Equal(BitOps.Bzhi64(value, index), BitOps.Bzhi64Sw(value, index));
      }
    }
  }
}
}
