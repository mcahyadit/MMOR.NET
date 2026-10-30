// https://prng.di.unimi.it
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Random {
public abstract class Xoshiro256Base<T> : IRandom<T>, IRandom64
    where T : Xoshiro256Base<T>, new() {
  public ulong s0, s1, s2, s3;

  public Xoshiro256Base(ulong seed) {
    Seed = seed;

    s0 = RandUtils.SplitMix64(ref seed);
    s1 = RandUtils.SplitMix64(ref seed);
    s2 = RandUtils.SplitMix64(ref seed);
    s3 = RandUtils.SplitMix64(ref seed);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override uint NextUInt() => (
      uint)RandomExtensions.NextUInt64(this, 0, uint.MaxValue + 1ul);
  public ulong NextUInt64() {
    ++StateCount;
    unchecked {
      ulong result = StateToResult();
      ulong tmp    = s1 << 17;
      s2 ^= s0;
      s3 ^= s1;
      s1 ^= s2;
      s0 ^= s3;
      s2 ^= tmp;
      s3 = BitOperations.RotateLeft(s3, 45);
      return result;
    }
  }

  public abstract ulong StateToResult();
}

public class Xoshiro256StarStar : Xoshiro256Base<Xoshiro256StarStar> {
  public Xoshiro256StarStar() : this(RandUtils.GenericU32RandomSeed()) {}
  public Xoshiro256StarStar(ulong seed) : base(seed) {}

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override ulong StateToResult() {
    unchecked {
      return BitOperations.RotateLeft(s1 * 5, 7) * 9;
    }
  }
}

public class Xoshiro256PlusPlus : Xoshiro256Base<Xoshiro256PlusPlus> {
  public Xoshiro256PlusPlus() : this(RandUtils.GenericU32RandomSeed()) {}
  public Xoshiro256PlusPlus(ulong seed) : base(seed) {}

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override ulong StateToResult() {
    unchecked {
      return BitOperations.RotateLeft(s0 + s3, 23) + s0;
    }
  }
}

public class Xoshiro256Plus : Xoshiro256Base<Xoshiro256Plus> {
  public Xoshiro256Plus() : this(RandUtils.GenericU32RandomSeed()) {}
  public Xoshiro256Plus(ulong seed) : base(seed) {}
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override ulong StateToResult() => unchecked(s0 + s3);
}
}
