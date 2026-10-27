// https://prng.di.unimi.it

using System.Numerics;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Random {
/**
 * <summary>
 * <strong><u>Xo</u>r <u>Ro</u>tate <u>S</u>hift <u>Ro</u>tate</strong>
 * <br/> - Developed by David Blackman and Sebastiano Vigna (2018).
 * <br/> - Statistical improvement from xorshift.
 * </summary>
 */
public abstract class XoroshiroBase<T> : IRandom<T>, IRandomDoubleSeed
    where T : XoroshiroBase<T>, new() {
  public ulong Seed2 { get; }
  public bool IsDoubleSeeded { get; }

  public ulong state0_;
  public ulong state1_;

  public XoroshiroBase(ulong seed, ulong? mix = null) {
    Seed = seed;

    if (mix == null) {
      RandUtils.SplitMix64(seed, out state0_, out state1_);
    } else {
      IsDoubleSeeded = true;

      Seed2   = mix.Value;
      state0_ = seed;
      state1_ = mix.Value;
    }
  }
}

/**
 * <inheritdoc cref="XoroshiroBase{T}"/>
 * <remarks>
 * <br/> 128++ Variant.
 * <br/> Has a period of 2^128 - 1 󰾞 3.4e+38.
 * </remarks>
 */
public class Xoroshiro : XoroshiroBase<Xoroshiro>, IRandom64 {
  public ulong NextUInt64() {
    ++StateCount;
    ulong s0     = state0_;
    ulong s1     = state1_;
    ulong result = unchecked(BitOperations.RotateLeft(unchecked(s0 + s1), 17) + s0);

    s1 ^= s0;
    state0_ = BitOperations.RotateLeft(s0, 49) ^ s1 ^ (s1 << 21);
    state1_ = BitOperations.RotateLeft(s1, 28);

    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override uint NextUInt() => (uint)this.NextUInt64(0, uint.MaxValue);

  public Xoroshiro() : this(RandUtils.GenericU32RandomSeed()) {}
  public Xoroshiro(ulong seed, ulong? mix = null) : base(seed, mix) {}
}

/**
 * <inheritdoc cref="XoroshiroBase{T}"/>
 * <remarks>
 * <br/> 64* Variant.
 * <br/> Has a period of 2^64 - 1 󰾞 1.8e+19.
 * </remarks>
 */
public class Xoroshiro64s : XoroshiroBase<Xoroshiro64s> {
  public override uint NextUInt() {
    ++StateCount;
    uint s0     = (uint)state0_;
    uint s1     = (uint)state1_;
    uint result = unchecked(s0 * 0x9E3779BBU);

    s1 ^= s0;
    state0_ = BitOperations.RotateLeft(s0, 26) ^ s1 ^ (s1 << 9);
    state1_ = BitOperations.RotateLeft(s1, 13);

    return result;
  }

  public Xoroshiro64s() : this(RandUtils.GenericU32RandomSeed()) {}
  public Xoroshiro64s(ulong seed, ulong? mix = null) : base(seed, mix) {}
}

//
/**
 * <inheritdoc cref="XoroshiroBase{T}"/>
 * <remarks>
 * <br/> 64** Variant.
 * <br/> Has a period of 2^64 - 1 󰾞 1.8e+19.
 * </remarks>
 */
public class Xoroshiro64ss : XoroshiroBase<Xoroshiro64ss> {
  public override uint NextUInt() {
    ++StateCount;
    uint s0     = (uint)state0_;
    uint s1     = (uint)state1_;
    uint result = unchecked(BitOperations.RotateLeft(unchecked(s0 * 0x9E3779BBU), 5) * 5);

    s1 ^= s0;
    state0_ = BitOperations.RotateLeft(s0, 26) ^ s1 ^ (s1 << 9);
    state1_ = BitOperations.RotateLeft(s1, 13);

    return result;
  }

  public Xoroshiro64ss() : this(RandUtils.GenericU32RandomSeed()) {}
  public Xoroshiro64ss(ulong seed, ulong? mix = null) : base(seed, mix) {}
}

/**
 * <inheritdoc cref="XoroshiroBase{T}"/>
 * <remarks>
 * <br/> 128** Variant.
 * <br/> Has a period of 2^128 - 1 󰾞 3.4e+38.
 * </remarks>
 */
public class Xoroshiro128ss : XoroshiroBase<Xoroshiro128ss>, IRandom64 {
  public ulong NextUInt64() {
    ++StateCount;
    ulong s0     = state0_;
    ulong s1     = state1_;
    ulong result = unchecked(BitOperations.RotateLeft(unchecked(s0 * 5), 7) * 9);

    s1 ^= s0;
    state0_ = BitOperations.RotateLeft(s0, 24) ^ s1 ^ (s1 << 16);
    state1_ = BitOperations.RotateLeft(s1, 37);

    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override uint NextUInt() => (uint)this.NextUInt64(0, uint.MaxValue);

  public Xoroshiro128ss() : this(RandUtils.GenericU32RandomSeed()) {}
  public Xoroshiro128ss(ulong seed, ulong? mix = null) : base(seed, mix) {}
}

/**
 * <inheritdoc cref="XoroshiroBase{T}"/>
 * <remarks>
 * <br/> 128+ Variant.
 * <br/> Has a period of 2^128 - 1 󰾞 3.4e+38.
 * </remarks>
 */
public class Xoroshiro128p : XoroshiroBase<Xoroshiro128p>, IRandom64 {
  public ulong NextUInt64() {
    ++StateCount;
    ulong s0     = state0_;
    ulong s1     = state1_;
    ulong result = unchecked(s0 + s1);

    s1 ^= s0;
    state0_ = BitOperations.RotateLeft(s0, 24) ^ s1 ^ (s1 << 16);
    state1_ = BitOperations.RotateLeft(s1, 37);

    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override uint NextUInt() => (uint)this.NextUInt64(0, uint.MaxValue);

  public Xoroshiro128p() : this(RandUtils.GenericU32RandomSeed()) {}
  public Xoroshiro128p(ulong seed, ulong? mix = null) : base(seed, mix) {}
}
}
