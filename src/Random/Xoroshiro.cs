using System;

namespace MMOR.NET.Random {
  internal class SplitMix64 {
    private ulong x_;

    public SplitMix64(ulong seed) => x_ = seed;

    public ulong Next() {
      ulong z = x_ += 0x9e3779b97f4a7c15;
      z       = (z ^ (z >> 30)) * 0xbf58476d1ce4e5b9;
      z       = (z ^ (z >> 27)) * 0x94d049bb133111eb;
      return z ^ (z >> 31);
    }
  }

  /**
   * <summary>
   * <strong><u>Xo</u>r <u>Ro</u>tate <u>S</u>hift <u>Ro</u>tate</strong>
   * <br/> - Developed by David Blackman and Sebastiano Vigna (2018).
   * <br/> - Statistical improvement from xorshift.
   * </summary>
   * */
  internal interface IXoroshiroDocs {}

  /**
   * <inheritdoc cref="IXoroshiroDocs"/>
   * <remarks>
   * <br/> 128++ Variant.
   * <br/> Has a period of 2^128 - 1 󰾞 3.4e+38.
   * </remarks>
   * */
  public class Xoroshiro : IRandom<Xoroshiro> {
    protected (ulong x, ulong y) s;

    protected virtual ulong RotateLeft(ulong x, int k) => (x << k) | (x >> (64 - k));

    public override string ToString() => @$"XSR_128pp-0x{Seed:X}";

    public override uint NextUInt() {
      ++StateCount;
      ulong s0     = s.x;
      ulong s1     = s.y;
      ulong result = unchecked(RotateLeft(unchecked(s0 + s1), 17) + s0);

      s1 ^= s0;
      s.x = RotateLeft(s0, 49) ^ s1 ^ (s1 << 21);
      s.y = RotateLeft(s1, 28);

      return (uint)(result >> 32);
    }

    public Xoroshiro() : this(DefaultReSeed()) {}

    public Xoroshiro(ulong seed, ulong? mix = null) {
      Seed = seed;

      if (mix == null) {
        SplitMix64 sm = new(seed);
        s             = new(sm.Next(), sm.Next());
      } else {
        s = new(seed, mix.Value);
      }
    }
  }

  /**
   * <inheritdoc cref="IXoroshiroDocs"/>
   * <remarks>
   * <br/> 64* Variant.
   * <br/> Has a period of 2^64 - 1 󰾞 1.8e+19.
   * </remarks>
   * */
  public class Xoroshiro64s : IRandom<Xoroshiro64s> {
    protected (uint x, uint y) s;

    protected uint RotateLeft(uint x, int k) => (x << k) | (x >> (32 - k));

    public override string ToString() => @$"XSR_64s-0x{Seed:X}";

    public override uint NextUInt() {
      ++StateCount;
      uint s0     = s.x;
      uint s1     = s.y;
      uint result = unchecked(s0 * 0x9E3779BBU);

      s1 ^= s0;
      s.x = RotateLeft(s0, 26) ^ s1 ^ (s1 << 9);
      s.y = RotateLeft(s1, 13);

      return result;
    }
  }

  //
  /**
   * <inheritdoc cref="IXoroshiroDocs"/>
   * <remarks>
   * <br/> 64** Variant.
   * <br/> Has a period of 2^64 - 1 󰾞 1.8e+19.
   * </remarks>
   * */
  public class Xoroshiro64ss : Xoroshiro64s {
    public override string ToString() => @$"XSR_64ss-0x{Seed:X}";

    public override uint NextUInt() {
      ++StateCount;
      uint s0     = s.x;
      uint s1     = s.y;
      uint result = unchecked(RotateLeft(unchecked(s0 * 0x9E3779BBU), 5) * 5);

      s1 ^= s0;
      s.x = RotateLeft(s0, 26) ^ s1 ^ (s1 << 9);
      s.y = RotateLeft(s1, 13);

      return result;
    }
  }

  /**
   * <inheritdoc cref="IXoroshiroDocs"/>
   * <remarks>
   * <br/> 128** Variant.
   * <br/> Has a period of 2^128 - 1 󰾞 3.4e+38.
   * </remarks>
   * */
  public class Xoroshiro128ss : Xoroshiro {
    public override string ToString() => @$"XSR_128ss-0x{Seed:X}";

    public override uint NextUInt() {
      ++StateCount;
      ulong s0     = s.x;
      ulong s1     = s.y;
      ulong result = unchecked(RotateLeft(unchecked(s0 * 5), 7) * 9);

      s1 ^= s0;
      s.x = RotateLeft(s0, 24) ^ s1 ^ (s1 << 16);
      s.y = RotateLeft(s1, 37);

      return (uint)(result >> 32);
    }
  }

  /**
   * <inheritdoc cref="IXoroshiroDocs"/>
   * <remarks>
   * <br/> 128+ Variant.
   * <br/> Has a period of 2^128 - 1 󰾞 3.4e+38.
   * </remarks>
   * */
  public class Xoroshiro128p : Xoroshiro {
    public override string ToString() => @$"XSR_128p-0x{Seed:X}";

    public override uint NextUInt() {
      ++StateCount;
      ulong s0     = s.x;
      ulong s1     = s.y;
      ulong result = unchecked(s0 + s1);

      s1 ^= s0;
      s.x = RotateLeft(s0, 24) ^ s1 ^ (s1 << 16);
      s.y = RotateLeft(s1, 37);

      return (uint)(result >> 32);
    }
  }
}
