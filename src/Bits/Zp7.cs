using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;

#if !NETSTANDARD
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif

namespace MMOR.NET.Bits {

/**
 * <summary>
 *  C# port of <c>github:zwegner/zp7</c> <br/>
 *  SPDX-License-Identifier: MIT <br/>
 *  Polyfills PDEP and PEXT on non-Bmi2 CPUs. <br/>
 * </summary>
 */
public static partial class Zp7 {
  public const int kBitLen = 6;

  public readonly struct Zp7Masks {
    public readonly ulong ppp0, ppp1, ppp2, ppp3, ppp4, ppp5;

    public Zp7Masks(Span<ulong> ppp) {
      ppp0 = ppp[0];
      ppp1 = ppp[1];
      ppp2 = ppp[2];
      ppp3 = ppp[3];
      ppp4 = ppp[4];
      ppp5 = ppp[5];
    }

    public ulong this[int index] {
      get => index switch {
        0 => ppp0,
        1 => ppp1,
        2 => ppp2,
        3 => ppp3,
        4 => ppp4,
        _ => ppp5,
      };
    }
  }

  public static ulong PrefixSum(ulong x) {
    for (int i = 0; i < kBitLen; ++i) {
      x ^= x << (1 << i);
    }
    return x;
  }

  public static Zp7Masks PppPre(ulong mask) {
    Span<ulong> ppp = stackalloc ulong[kBitLen];
    ulong m         = ~mask;

#if !NETSTANDARD
    if (Pclmulqdq.IsSupported) {
      // 0xFFFFFFFFFFFFFFFE = -2 casted to ulong
      Vector128<ulong> n2 = Vector128.Create(0ul, 0xFFFFFFFFFFFFFFFE);
      for (int i = 0; i < kBitLen - 1; ++i) {
        Vector128<ulong> mv  = Vector128.Create(m, 0UL);
        Vector128<ulong> bit = Pclmulqdq.CarrylessMultiply(mv, n2, 0);
        ulong bit_low        = bit.ToScalar();
        ppp[i]               = bit_low;
        m &= bit_low;
      }
      ppp[kBitLen - 1] = unchecked((ulong) - (long)m << 1);
      return new Zp7Masks(ppp);
    }
#endif

    for (int i = 0; i < kBitLen - 1; i++) {
      ulong bit = PrefixSum(m << 1);
      ppp[i]    = bit;
      m &= bit;
    }
    ppp[kBitLen - 1] = unchecked((ulong) - (long)m << 1);
    return new Zp7Masks(ppp);
  }

  static readonly ConcurrentDictionary<ulong, Zp7Masks> pppm_ = new();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Zp7Masks GetPpp(ulong mask) {
    return pppm_.GetOrAdd(mask, PppPre);
  }

  [Pure]
  public static ulong Pext64(ulong value, ulong mask) {
    Zp7Masks ppp = pppm_.GetOrAdd(mask, PppPre);
    value &= mask;
    for (int i = 0; i < kBitLen; ++i) {
      int shift = 1 << i;
      ulong bit = ppp[i];
      value     = (value & ~bit) | ((value & bit) >> shift);
    }
    return value;
  }

  [Pure]
  public static ulong Pdep64(ulong value, ulong mask) {
    Zp7Masks ppp = pppm_.GetOrAdd(mask, PppPre);
    value        = BitOps.Bzhi64(value, (ulong)BitOperations.PopCount(mask));

    for (int i = kBitLen - 1; i >= 0; --i) {
      int shift = 1 << i;
      ulong bit = ppp[i] >> shift;
      value     = (value & ~bit) + ((value & bit) << shift);
    }
    return value;
  }
}

}
