using System.Runtime.Intrinsics.X86;
using Xunit;

namespace MMOR.NET.Bits {
using Random = System.Random;
public partial class BitOpsTest {
  [Fact]
  public void PextPdep64_Roundtrip() {
    Random rng = new Random(20261021);
    for (int i = 0; i < 30; ++i) {
      ulong value = rng.NextU64();
      for (int j = 0; j < 30; ++j) {
        ulong mask = rng.NextU64();
        Assert.Equal(value & mask, Zp7.Pdep64(Zp7.Pext64(value, mask), mask));
      }
    }
  }

  [Fact]
  public void Pdep64_MatchesReference() {
    Random rng = new Random(20261021);
    for (int i = 0; i < 30; ++i) {
      ulong value = rng.NextU64();
      for (int j = 0; j < 30; ++j) {
        ulong mask = rng.NextU64();
        Assert.Equal(Pdep64Reference(value, mask), Zp7.Pdep64(value, mask));
      }
    }
  }

  [Fact]
  public void Pext64_MatchesReference() {
    Random rng = new Random(20261021);
    for (int i = 0; i < 30; ++i) {
      ulong value = rng.NextU64();
      for (int j = 0; j < 30; ++j) {
        ulong mask = rng.NextU64();
        Assert.Equal(Pext64Reference(value, mask), Zp7.Pext64(value, mask));
      }
    }
  }

  [Fact]
  public void Pdep64_MatchesSwFallback() {
    if (!Bmi2.X64.IsSupported)
      return;

    Random rng = new Random(20261021);
    for (int i = 0; i < 30; ++i) {
      ulong value = rng.NextU64();
      for (int j = 0; j < 30; ++j) {
        ulong mask = rng.NextU64();
        Assert.Equal(Bmi2.X64.ParallelBitDeposit(value, mask), Zp7.Pdep64(value, mask));
      }
    }
  }

  [Fact]
  public void Pext64_MatchesSwFallback() {
    if (!Bmi2.X64.IsSupported)
      return;

    Random rng = new Random(20261021);
    for (int i = 0; i < 30; ++i) {
      ulong value = rng.NextU64();
      for (int j = 0; j < 30; ++j) {
        ulong mask = rng.NextU64();
        Assert.Equal(Bmi2.X64.ParallelBitExtract(value, mask), Zp7.Pext64(value, mask));
      }
    }
  }
}
}
