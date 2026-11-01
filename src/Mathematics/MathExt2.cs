using System;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Mathematics {
#if NETSTANDARD
using UltimateOrb.Mathematics;
using UInt128 = UltimateOrb.UInt128;
#endif

public static partial class MathExt {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void BigMulU64(in ulong left, in ulong right, out ulong high, out ulong low) {
#if NETSTANDARD
    UInt128 umul128 = (UInt128)left * right;
    high            = (ulong)(umul128 >> 64);
    low             = (ulong)(umul128 & ~0ul);
#else
    high = Math.BigMul(left, right, out low);
#endif
  }

  /**
   * <summary>
   * Lemire's Fastrange for wrapping <param name="word"/>
   * in [0,<paramref name="p"/>) without division.
   * </summary>
   */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint FastRange32(uint word, uint p) {
    return (uint)(((ulong)word * p) >> 32);
  }

  /**
   * <summary>
   * Lemire's Fastrange for wrapping <param name="word"/>
   * in [0,<paramref name="p"/>) without division.
   * </summary>
   */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int FastRange32(int word, int p) {
    return unchecked((int)(((ulong)word * (uint)p) >> 32));
  }

  /**
   * <summary>
   * Lemire's Fastrange for wrapping <param name="word"/>
   * in [0,<paramref name="p"/>) without division.
   * </summary>
   */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong FastRange64(ulong word, ulong p) {
    BigMulU64(word, p, out ulong high, out _);
    return high;
  }
}
}
