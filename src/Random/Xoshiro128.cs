// https://prng.di.unimi.it
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Random {
public abstract class Xoshiro128Base<T> : IRandom<T>
    where T : Xoshiro128Base<T>, new() {
  public uint s0, s1, s2, s3;

  public Xoshiro128Base(uint seed) {
    Seed = seed;

    s0 = RandUtils.SplitMix32(ref seed);
    s1 = RandUtils.SplitMix32(ref seed);
    s2 = RandUtils.SplitMix32(ref seed);
    s3 = RandUtils.SplitMix32(ref seed);
  }

  public override uint NextUInt() {
    ++StateCount;
    unchecked {
      uint result = StateToResult();
      uint tmp    = s1 << 9;
      s2 ^= s0;
      s3 ^= s1;
      s1 ^= s2;
      s0 ^= s3;
      s2 ^= tmp;
      s3 = BitOperations.RotateLeft(s3, 11);
      return result;
    }
  }

  public abstract uint StateToResult();
}

public class Xoshiro128StarStar : Xoshiro128Base<Xoshiro128StarStar> {
  public Xoshiro128StarStar() : this(RandUtils.GenericU32RandomSeed()) {}
  public Xoshiro128StarStar(uint seed) : base(seed) {}

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override uint StateToResult() {
    unchecked {
      return BitOperations.RotateLeft(s1 * 5, 7) * 9;
    }
  }
}

public class Xoshiro128PlusPlus : Xoshiro128Base<Xoshiro128PlusPlus> {
  public Xoshiro128PlusPlus() : this(RandUtils.GenericU32RandomSeed()) {}
  public Xoshiro128PlusPlus(uint seed) : base(seed) {}

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override uint StateToResult() {
    unchecked {
      return BitOperations.RotateLeft(s0 + s3, 7) + s0;
    }
  }
}

public class Xoshiro128Plus : Xoshiro128Base<Xoshiro128Plus> {
  public Xoshiro128Plus() : this(RandUtils.GenericU32RandomSeed()) {}
  public Xoshiro128Plus(uint seed) : base(seed) {}
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override uint StateToResult() => unchecked(s0 + s3);
}
}
