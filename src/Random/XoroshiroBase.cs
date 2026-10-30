// https://prng.di.unimi.it
using System;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Random {
/**
 * <summary>
 * <strong><u>Xo</u>r <u>Ro</u>tate <u>S</u>hift <u>Ro</u>tate</strong>
 * <br/> - Developed by David Blackman and Sebastiano Vigna (2018).
 * <br/> - Statistical improvement from xorshift.
 * </summary>
 */
public abstract class Xoroshiro128Base<T> : IRandom<T>, IRandomDoubleSeed, IRandom64
    where T : Xoroshiro128Base<T>, new() {
  public ulong Seed2 { get; }
  public bool IsDoubleSeeded { get; }

  public ulong state0_;
  public ulong state1_;

  public Xoroshiro128Base(ulong seed, ulong? mix = null) {
    Seed = seed;

    if (mix == null) {
      RandUtils.SplitMix64(RandUtils.SplitMix64(seed, out state0_), out state1_);
    } else {
      IsDoubleSeeded = true;

      Seed2   = mix.Value;
      state0_ = seed;
      state1_ = mix.Value;
    }
  }
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override uint NextUInt() =>  //
      (uint)RandomExtensions.NextUInt64(this, 0, uint.MaxValue + 1ul);
  public abstract ulong NextUInt64();
}

public abstract class Xoroshiro64Base<T> : IRandom<T>, IRandomDoubleSeed
    where T : Xoroshiro64Base<T>, new() {
  public ulong Seed2 { get; }
  public bool IsDoubleSeeded { get; }

  public readonly int kPolyDeg;
  public uint state0_;
  public uint state1_;

  public Xoroshiro64Base(uint seed, uint? mix = null) {
    Seed = seed;

    if (mix == null) {
      RandUtils.SplitMix32(RandUtils.SplitMix32(seed, out state0_), out state1_);
    } else {
      IsDoubleSeeded = true;

      Seed2   = mix.Value;
      state0_ = seed;
      state1_ = mix.Value;
    }
  }
}

// NOTE: I don't think this is worth for steps under 2^32
public interface IXoroshiro {
  ReadOnlySpan<ulong> kCharpoly { get; }
  int kPolyDeg { get; }
}

public static class XoroshiroUtils {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void F2xJumppolyCE<T>(this T self, ulong c, uint e, Span<ulong> result)
      where T : IXoroshiro {
    result.Clear();
    result[0] = 1;
    for (int k = 63; k >= 0; --k) {
      F2xMulMod(self, result, result);
      if (((c >> k) & 1) != 0)
        F2xMulx(self, result);
    }
    while (e != 0) {
      --e;
      F2xMulMod(self, result, result);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void F2xMulMod<T>(this T self, Span<ulong> a, Span<ulong> b)
      where T : IXoroshiro {
    int poly_deg   = self.kPolyDeg;
    int poly_words = (poly_deg + 63) / 64;
    Span<ulong> r  = stackalloc ulong[poly_words];
    for (int k = poly_deg - 1; k >= 0; --k) {
      F2xMulx(self, r);
      if (((a[k >> 6] >> (k & 63)) & 1) != 0) {
        for (int i = 0; i < poly_words; ++i) r[i] ^= b[i];
      }
    }
    r.CopyTo(a);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void F2xMulx<T>(this T self, Span<ulong> a)
      where T : IXoroshiro {
    int poly_deg   = self.kPolyDeg;
    int poly_words = (poly_deg + 63) / 64;
    ulong carry    = 0;
    for (int i = 0; i < poly_words; ++i) {
      ulong next_carry = a[i] >> 63;
      a[i]             = (a[i] << 1) | carry;
      carry            = next_carry;
    }
    int top;
    if (poly_deg % 64 == 0) {
      top = (int)carry;
    } else {
      top = (int)(a[poly_deg >> 6] >> (poly_deg & 63)) & 1;
      a[poly_deg >> 6] &= ~(1ul << (poly_deg & 63));
    }
    for (int i = 0; top != 0 && i < poly_words; ++i) {
      a[i] ^= self.kCharpoly[i];
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void F2xJumppolyN<T>(this T self, ReadOnlySpan<ulong> jump, Span<ulong> result)
      where T : IXoroshiro {
    result.Clear();
    result[0] = 1;
    int len   = jump.Length;
    for (int k = len * 64 - 1; k >= 0; --k) {
      F2xMulMod(self, result, result);
      if (((jump[k >> 6] >> (k & 63)) & 1) != 0)
        F2xMulx(self, result);
    }
  }
}
}
