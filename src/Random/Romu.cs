// http://romu-random.org/romupaper.pdf
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Random {
public abstract class Romu64Base<T> : IRandom<T>, IRandom64
    where T : Romu64Base<T>, new() {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override uint NextUInt() => (uint)RandomExtensions.NextUInt64(this, 0, uint.MaxValue);
  public abstract ulong NextUInt64();
}

public class RomuQuad : Romu64Base<RomuQuad> {
  ulong state_w_, state_x_, state_y_, state_z_;

  public RomuQuad() : this(RandUtils.GenericU32RandomSeed()) {}
  public RomuQuad(ulong seed) {
    Seed = seed;
    seed = RandUtils.SplitMix64(seed, out state_w_);
    seed = RandUtils.SplitMix64(seed, out state_x_);
    seed = RandUtils.SplitMix64(seed, out state_y_);
    _    = RandUtils.SplitMix64(seed, out state_z_);
  }

  public override ulong NextUInt64() {
    ++StateCount;
    unchecked {
      ulong prev_w = state_w_, prev_x = state_x_, prev_y = state_y_, prev_z = state_z_;
      state_w_ = 15241094284759029579u * prev_z;
      state_x_ = prev_z + BitOperations.RotateLeft(prev_w, 52);
      state_y_ = prev_y - prev_x;
      state_z_ = BitOperations.RotateLeft(prev_y + prev_w, 19);
      return prev_x;
    }
  }
}

public class RomuTrio : Romu64Base<RomuTrio> {
  ulong state_x_, state_y_, state_z_;

  public RomuTrio() : this(RandUtils.GenericU32RandomSeed()) {}
  public RomuTrio(ulong seed) {
    Seed = seed;
    seed = RandUtils.SplitMix64(seed, out state_x_);
    seed = RandUtils.SplitMix64(seed, out state_y_);
    _    = RandUtils.SplitMix64(seed, out state_z_);
  }

  public override ulong NextUInt64() {
    ++StateCount;
    unchecked {
      ulong prev_x = state_x_, prev_y = state_y_, prev_z = state_z_;
      state_x_ = 15241094284759029579u * prev_z;
      state_y_ = BitOperations.RotateLeft(prev_y - prev_x, 12);
      state_z_ = BitOperations.RotateLeft(prev_z - prev_y, 44);
      return prev_x;
    }
  }
}

public class RomuDuo : Romu64Base<RomuDuo> {
  ulong state_x_, state_y_;

  public RomuDuo() : this(RandUtils.GenericU32RandomSeed()) {}
  public RomuDuo(ulong seed) {
    Seed = seed;
    RandUtils.SplitMix64(RandUtils.SplitMix64(seed, out state_x_), out state_y_);
  }

  public override ulong NextUInt64() {
    ++StateCount;
    unchecked {
      ulong prev_x = state_x_;
      state_x_     = 15241094284759029579u * state_y_;
      state_y_     = BitOperations.RotateLeft(state_y_, 36)  //
                 + BitOperations.RotateLeft(state_y_, 15) - prev_x;
      return prev_x;
    }
  }
}

public class RomuDuoJr : Romu64Base<RomuDuoJr> {
  ulong state_x_, state_y_;

  public RomuDuoJr() : this(RandUtils.GenericU32RandomSeed()) {}
  public RomuDuoJr(ulong seed) {
    Seed = seed;
    RandUtils.SplitMix64(RandUtils.SplitMix64(seed, out state_x_), out state_y_);
  }

  public override ulong NextUInt64() {
    ++StateCount;
    unchecked {
      ulong prev_x = state_x_;
      state_x_     = 15241094284759029579u * state_y_;
      state_y_     = BitOperations.RotateLeft(state_y_ - prev_x, 27);
      return prev_x;
    }
  }
}

public class RomuQuad32 : IRandom<RomuQuad32> {
  uint state_w_, state_x_, state_y_, state_z_;

  public RomuQuad32() : this(RandUtils.GenericU32RandomSeed()) {}
  public RomuQuad32(uint seed) {
    Seed = seed;
    seed = RandUtils.SplitMix32(seed, out state_w_);
    seed = RandUtils.SplitMix32(seed, out state_x_);
    seed = RandUtils.SplitMix32(seed, out state_y_);
    _    = RandUtils.SplitMix32(seed, out state_z_);
  }

  public override uint NextUInt() {
    ++StateCount;
    unchecked {
      uint prev_w = state_w_, prev_x = state_x_, prev_y = state_y_, prev_z = state_z_;
      state_w_ = 3323815723u * prev_z;
      state_x_ = prev_z + BitOperations.RotateLeft(prev_w, 26);
      state_y_ = prev_y - prev_x;
      state_z_ = BitOperations.RotateLeft(prev_y + prev_w, 9);
      return prev_x;
    }
  }
}

public class RomuTrio32 : IRandom<RomuTrio32> {
  uint state_x_, state_y_, state_z_;

  public RomuTrio32() : this(RandUtils.GenericU32RandomSeed()) {}
  public RomuTrio32(uint seed) {
    Seed = seed;
    seed = RandUtils.SplitMix32(seed, out state_x_);
    seed = RandUtils.SplitMix32(seed, out state_y_);
    _    = RandUtils.SplitMix32(seed, out state_z_);
  }

  public override uint NextUInt() {
    ++StateCount;
    unchecked {
      uint prev_x = state_x_, prev_y = state_y_, prev_z = state_z_;
      state_x_ = 3323815723u * prev_z;
      state_y_ = BitOperations.RotateLeft(prev_y - prev_x, 6);
      state_z_ = BitOperations.RotateLeft(prev_z - prev_y, 22);
      return prev_x;
    }
  }
}
}
