using System;
#if NETSTANDARD
using UltimateOrb.Mathematics;
using UInt128 = UltimateOrb.UInt128;
#endif

namespace MMOR.NET.Random {
public static partial class RandomExtensions {
  public static uint NextUInt32Simple<T>(this T rng, uint min_inclusive, uint max_inclusive)
      where T : IRandom {
    ThrowIfMaxInclIsLt(min_inclusive, max_inclusive);
    uint range = max_inclusive - min_inclusive;
    if (range == 0)
      return min_inclusive;
    if (range == uint.MaxValue)
      return rng.NextUInt();
    range += 1;

    uint limit = uint.MaxValue - uint.MaxValue % range;
    uint result;

    do {
      result = rng.NextUInt();
    } while (result >= limit);

    return min_inclusive + result % range;
  }

  // https://salivity.github.io/binary/article/lemire-s-fast-alternative-to-modulo-reduction
  public static uint NextUInt32Lemire<T>(this T rng, uint min_inclusive, uint max_inclusive)
      where T : IRandom {
    ThrowIfMaxInclIsLt(min_inclusive, max_inclusive);
    uint range = max_inclusive - min_inclusive;
    if (range == 0)
      return min_inclusive;
    if (range == uint.MaxValue)
      return rng.NextUInt();
    range += 1;

    ulong full_product = rng.NextUInt() * (ulong)range;
    uint low_bits      = (uint)full_product;
    if (low_bits < range) {
      uint threshold = (0u - range) % range;
      while (low_bits < threshold) {
        full_product = rng.NextUInt() * (ulong)range;
        low_bits     = (uint)full_product;
      }
    }
    return min_inclusive + (uint)(full_product >> 32);
  }

  public static ulong NextUInt64Lemire<T>(this T rng, uint min_inclusive, uint max_inclusive)
      where T : IRandom64 {
    ThrowIfMaxInclIsLt(min_inclusive, max_inclusive);
    ulong range = max_inclusive - min_inclusive;
    if (range == 0)
      return min_inclusive;
    if (range == ulong.MaxValue)
      return rng.NextUInt64();

    UInt128 full_product = rng.NextUInt64() * (UInt128)range;
    ulong low_bits       = (ulong)full_product;
    if (low_bits < range) {
      ulong threshold = (0ul - range) % range;
      while (low_bits < threshold) {
        full_product = rng.NextUInt64() * (UInt128)range;
        low_bits     = (ulong)full_product;
      }
    }
    return min_inclusive + (ulong)(full_product >> 64);
  }

  // https://github.com/boostorg/random/blob/develop/include/boost/random/uniform_int_distribution.hpp
  public static uint NextUInt32Boost<T>(this T rng, uint min_inclusive, uint max_inclusive)
      where T : IRandom {
    ThrowIfMaxInclIsLt(min_inclusive, max_inclusive);
    uint range = max_inclusive - min_inclusive;
    if (range == 0)
      return min_inclusive;
    if (range == uint.MaxValue)
      return rng.NextUInt();
    range += 1;

    uint brange = uint.MaxValue;
    uint bucket = brange / range;
    if (brange % range == range - 1)
      bucket += 1;
    while (true) {
      uint rand = rng.NextUInt() / bucket;
      if (rand <= range)
        return min_inclusive + rand;
    }
  }

  public static ulong NextUInt64Boost<T>(this T rng, ulong min_inclusive, ulong max_inclusive)
      where T : IRandom64 {
    ThrowIfMaxInclIsLt(min_inclusive, max_inclusive);
    ulong range = max_inclusive - min_inclusive;
    if (range == 0)
      return min_inclusive;
    if (range == ulong.MaxValue)
      return rng.NextUInt64();
    range += 1;

    ulong brange = ulong.MaxValue;
    ulong bucket = brange / range;
    if (brange % range == range - 1)
      bucket += 1;
    while (true) {
      ulong rand = rng.NextUInt64() / bucket;
      if (rand <= range)
        return min_inclusive + rand;
    }
  }
}
}
