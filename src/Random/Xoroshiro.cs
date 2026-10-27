using System;
using System.Collections.Immutable;
using System.Numerics;

namespace MMOR.NET.Random {
/**
 * <inheritdoc cref="Xoroshiro128Base{T}"/>
 * <remarks>
 * <br/> 128++ Variant.
 * <br/> Has a period of 2^128 - 1 󰾞 3.4e+38.
 * </remarks>
 */
public class Xoroshiro : Xoroshiro128Base<Xoroshiro>, IXoroshiro {
  public override ulong NextUInt64() {
    ++StateCount;
    unchecked {
      ulong s0     = state0_;
      ulong s1     = state1_;
      ulong result = BitOperations.RotateLeft(s0 + s1, 17) + s0;

      s1 ^= s0;
      state0_ = BitOperations.RotateLeft(s0, 49) ^ s1 ^ (s1 << 21);
      state1_ = BitOperations.RotateLeft(s1, 28);

      return result;
    }
  }

  public Xoroshiro() : this(RandUtils.GenericU32RandomSeed()) {}
  public Xoroshiro(ulong seed, ulong? mix = null) : base(seed, mix) {}

  public void JumpApply(ReadOnlySpan<ulong> poly) {
    int poly_words = (kPolyDeg + 63) / 64;
    ulong s0       = 0;
    ulong s1       = 0;
    for (int i = 0; i < poly_words; i++)
      for (int b = 0; b < 64; b++) {
        if ((poly[i] & 1ul << b) != 0) {
          s0 ^= state0_;
          s1 ^= state1_;
        }
        NextUInt64();
      }

    state0_ = s0;
    state1_ = s1;
  }

  public void JumpCE(ulong c, uint e) {
    Span<ulong> poly = stackalloc ulong[(kPolyDeg + 63) / 64];
    this.F2xJumppolyCE(c, e, poly);
    JumpApply(poly);
  }

  // NOTE: I don't think this is worth for steps under 2^32
  // public void Jump(ulong steps) {
  //   int e    = BitOperations.Log2(steps);
  //   uint rem = (uint)(steps - (1u << e));
  //
  //   JumpCE(1, (uint)e);
  //
  //   for (uint i = 0; i < rem; ++i) {
  //     _ = NextUInt64();
  //   }
  // }

  public static readonly ImmutableArray<ulong> charpoly_  = ImmutableArray.Create(  //
      stackalloc ulong[2] { 0x8dae70779760b081, 0x0031bcf2f855d6e5 });
  public ReadOnlySpan<ulong> kCharpoly                   => charpoly_.AsSpan();
  public int kPolyDeg                                    => 128;
}

/**
 * <inheritdoc cref="Xoroshiro128Base{T}"/>
 * <remarks>
 * <br/> 64* Variant.
 * <br/> Has a period of 2^64 - 1 󰾞 1.8e+19.
 * </remarks>
 */
public class Xoroshiro64s : Xoroshiro64Base<Xoroshiro64s> {
  public override uint NextUInt() {
    ++StateCount;
    unchecked {
      uint s0     = state0_;
      uint s1     = state1_;
      uint result = s0 * 0x9E3779BBU;

      s1 ^= s0;
      state0_ = BitOperations.RotateLeft(s0, 26) ^ s1 ^ (s1 << 9);
      state1_ = BitOperations.RotateLeft(s1, 13);

      return result;
    }
  }

  public Xoroshiro64s() : this(RandUtils.GenericU32RandomSeed()) {}
  public Xoroshiro64s(uint seed, uint? mix = null) : base(seed, mix) {}
}

//
/**
 * <inheritdoc cref="Xoroshiro128Base{T}"/>
 * <remarks>
 * <br/> 64** Variant.
 * <br/> Has a period of 2^64 - 1 󰾞 1.8e+19.
 * </remarks>
 */
public class Xoroshiro64ss : Xoroshiro64Base<Xoroshiro64ss> {
  public override uint NextUInt() {
    ++StateCount;
    unchecked {
      uint s0     = state0_;
      uint s1     = state1_;
      uint result = BitOperations.RotateLeft(s0 * 0x9E3779BBU, 5) * 5;

      s1 ^= s0;
      state0_ = BitOperations.RotateLeft(s0, 26) ^ s1 ^ (s1 << 9);
      state1_ = BitOperations.RotateLeft(s1, 13);

      return result;
    }
  }

  public Xoroshiro64ss() : this(RandUtils.GenericU32RandomSeed()) {}
  public Xoroshiro64ss(uint seed, uint? mix = null) : base(seed, mix) {}
}

/**
 * <inheritdoc cref="Xoroshiro128Base{T}"/>
 * <remarks>
 * <br/> 128** Variant.
 * <br/> Has a period of 2^128 - 1 󰾞 3.4e+38.
 * </remarks>
 */
public class Xoroshiro128ss : Xoroshiro128Base<Xoroshiro128ss> {
  public override ulong NextUInt64() {
    ++StateCount;
    unchecked {
      ulong s0     = state0_;
      ulong s1     = state1_;
      ulong result = unchecked(BitOperations.RotateLeft(unchecked(s0 * 5), 7) * 9);

      s1 ^= s0;
      state0_ = BitOperations.RotateLeft(s0, 24) ^ s1 ^ (s1 << 16);
      state1_ = BitOperations.RotateLeft(s1, 37);

      return result;
    }
  }

  public Xoroshiro128ss() : this(RandUtils.GenericU32RandomSeed()) {}
  public Xoroshiro128ss(ulong seed, ulong? mix = null) : base(seed, mix) {}
}

/**
 * <inheritdoc cref="Xoroshiro128Base{T}"/>
 * <remarks>
 * <br/> 128+ Variant.
 * <br/> Has a period of 2^128 - 1 󰾞 3.4e+38.
 * </remarks>
 */
public class Xoroshiro128p : Xoroshiro128Base<Xoroshiro128p> {
  public override ulong NextUInt64() {
    ++StateCount;
    unchecked {
      ulong s0     = state0_;
      ulong s1     = state1_;
      ulong result = unchecked(s0 + s1);

      s1 ^= s0;
      state0_ = BitOperations.RotateLeft(s0, 24) ^ s1 ^ (s1 << 16);
      state1_ = BitOperations.RotateLeft(s1, 37);

      return result;
    }
  }

  public Xoroshiro128p() : this(RandUtils.GenericU32RandomSeed()) {}
  public Xoroshiro128p(ulong seed, ulong? mix = null) : base(seed, mix) {}
}
}
