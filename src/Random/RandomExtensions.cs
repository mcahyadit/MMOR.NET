using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MMOR.NET.Mathematics;

#if NETSTANDARD
using UltimateOrb.Mathematics;
using UInt128 = UltimateOrb.UInt128;
#endif

namespace MMOR.NET.Random {
public static partial class RandomExtensions {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void ThrowIfMaxExclIsLt<T>(T min_inclusive, T max_exclusive)
      where T : IComparable<T> {
    if (max_exclusive.CompareTo(min_inclusive) <= 0)
      throw new ArgumentException(
          $"[ERROR]: max_exclusive: {max_exclusive} must be greater than min_inclusive: {min_inclusive}.");
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void ThrowIfMaxInclIsLt<T>(T min_inclusive, T max_inclusive)
      where T : IComparable<T> {
    if (max_inclusive.CompareTo(min_inclusive) < 0)
      throw new ArgumentException(
          $"[ERROR]: max_inclusive: {max_inclusive} must be greater than min_inclusive: {min_inclusive}.");
  }

  /**
   * <summary>
   *  Generates a random 32-bit unsigned integer between <paramref name="min_inclusive"/> and
   *  <paramref name="max_exclusive"/>.
   *  <br/> Respecting modulo bias.
   *  <br/> In turn, <see cref="IRandom.StateCount"/> might be incremented more than once.
   * </summary>
   * <returns>
   *  Random 32-bit unsigned integer between <paramref name="min_inclusive"/> and
   *  <paramref name="max_exclusive"/>.
   * </returns>
   * <exception cref="ArgumentException">
   *  Thrown when <paramref name="max_exclusive"/> is less than or equal to
   *  <paramref name="min_inclusive"/>.
   * </exception>
   */
  public static uint NextUInt<T>(this T rng, uint min_inclusive, uint max_exclusive)
      where T : IRandom {
    ThrowIfMaxExclIsLt(min_inclusive, max_exclusive);

    // Based on Lemire
    uint range         = max_exclusive - min_inclusive;
    ulong range_u64    = range;
    ulong full_product = rng.NextUInt() * range_u64;
    uint low_bits      = (uint)full_product;
    if (low_bits < range) {
      uint threshold = (0u - range) % range;
      while (low_bits < threshold) {
        full_product = rng.NextUInt() * range_u64;
        low_bits     = (uint)full_product;
      }
    }
    return min_inclusive + (uint)(full_product >> 32);
  }

  /**
   * <summary>
   *  Generates a random 64-bit unsigned integer from the full range of <see cref="ulong"/>.
   *  <br/> Uses <see cref="IRandom64.NextUInt64"/> directly if supported, otherwise combines
   *  two calls to <see cref="IRandom.NextUInt"/>, incrementing <see cref="IRandom.StateCount"/>
   *  twice.
   * </summary>
   * <returns>
   *  Random 64-bit unsigned integer from the full range of <see cref="ulong"/>.
   * </returns>
   */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong NextUInt64<T>(this T rng)
      where T : IRandom {
    if (rng is IRandom64 rng_u64)
      return rng_u64.NextUInt64();
    return ((ulong)rng.NextUInt() << 32) | rng.NextUInt();
  }

  /**
   * <summary>
   *  Generates a random 64-bit unsigned integer between <paramref name="min_inclusive"/> and
   *  <paramref name="max_exclusive"/>.
   *  <br/> Respecting modulo bias.
   *  <br/> In turn, <see cref="IRandom.StateCount"/> might be incremented more than once.
   * </summary>
   * <returns>
   *  Random 64-bit unsigned integer between <paramref name="min_inclusive"/> and
   *  <paramref name="max_exclusive"/>.
   * </returns>
   * <exception cref="ArgumentException">
   *  Thrown when <paramref name="max_exclusive"/> is less than or equal to
   *  <paramref name="min_inclusive"/>.
   * </exception>
   */
  public static ulong NextUInt64<T>(this T rng, ulong min_inclusive, ulong max_exclusive)
      where T : IRandom {
    ThrowIfMaxExclIsLt(min_inclusive, max_exclusive);

    ulong range          = max_exclusive - min_inclusive;
    UInt128 range_u128   = range;
    UInt128 full_product = NextUInt64(rng) * range_u128;
    ulong low_bits       = (ulong)full_product;
    if (low_bits < range) {
      ulong threshold = (0ul - range) % range;
      while (low_bits < threshold) {
        full_product = NextUInt64(rng) * range_u128;
        low_bits     = (ulong)full_product;
      }
    }
    return min_inclusive + (ulong)(full_product >> 64);
  }

  /**
   * <summary>
   *  Generates a random 32-bit signed integer between <paramref name="min_inclusive"/> and
   *  <paramref name="max_exclusive"/>.
   *  <br/> Respecting modulo bias.
   *  <br/> In turn, <see cref="IRandom.StateCount"/> might be incremented more than once.
   * </summary>
   * <returns>
   *  Random 32-bit signed integer between <paramref name="min_inclusive"/> and
   *  <paramref name="max_exclusive"/>.
   * </returns>
   * <exception cref="ArgumentException">
   *  Thrown when <paramref name="max_exclusive"/> is less than or equal to
   *  <paramref name="min_inclusive"/>.
   * </exception>
   */
  public static int NextInt<T>(this T rng, int min_inclusive, int max_exclusive)
      where T : IRandom {
    ThrowIfMaxExclIsLt(min_inclusive, max_exclusive);
    uint range = (uint)(max_exclusive - min_inclusive);
    return min_inclusive + (int)rng.NextUInt(0, range);
  }

  /**
   * <summary>
   *  Generates a random 64-bit signed integer between <paramref name="min_inclusive"/> and
   *  <paramref name="max_exclusive"/>.
   *  <br/> Respecting modulo bias.
   *  <br/> In turn, <see cref="IRandom.StateCount"/> might be incremented more than once.
   * </summary>
   * <returns>
   *  Random 64-bit signed integer between <paramref name="min_inclusive"/> and
   *  <paramref name="max_exclusive"/>.
   * </returns>
   * <exception cref="ArgumentException">
   *  Thrown when <paramref name="max_exclusive"/> is less than or equal to
   *  <paramref name="min_inclusive"/>.
   * </exception>
   */
  public static long NextInt64<T>(this T rng, long min_inclusive, long max_exclusive)
      where T : IRandom {
    ThrowIfMaxExclIsLt(min_inclusive, max_exclusive);
    ulong range = (ulong)(max_exclusive - min_inclusive);
    return min_inclusive + (long)rng.NextUInt64(0, range);
  }

  public const float k0x1p24 = 5.9604645E-08f;  // 0x1p-24 | 2^-24

  /**
   * <summary>
   *  Generates a random <see cref="float"/> between <paramref name="min_inclusive"/> and
   *  <paramref name="max_inclusive"/>.
   *  <br/> Respecting modulo bias.
   *  <br/> In turn, <see cref="IRandom.StateCount"/> might be incremented more than once.
   * </summary>
   * <returns>
   *  Random <see cref="float"/> between <paramref name="min_inclusive"/> and
   *  <paramref name="max_inclusive"/>.
   * </returns>
   * <exception cref="ArgumentException">
   *  Thrown when <paramref name="max_inclusive"/> is less than
   *  <paramref name="min_inclusive"/>.
   * </exception>
   */
  public static float NextFloat<T>(this T rng, float min_inclusive = 0, float max_inclusive = 1)
      where T : IRandom {
    if (MathExt.Approximately(min_inclusive, max_inclusive))
      return min_inclusive;
    ThrowIfMaxInclIsLt(min_inclusive, max_inclusive);
    float val = rng.NextUInt(0, (1u << 24) + 1) * k0x1p24;
    return min_inclusive + val * (max_inclusive - min_inclusive);
  }

  public const double k0x1p53 = 1.1102230246251565E-16;  // 0x1p-53 | 2^-53

  /**
   * <summary>
   *  Generates a random <see cref="double"/> between <paramref name="min_inclusive"/> and
   *  <paramref name="max_inclusive"/>.
   *  <br/> Respecting modulo bias.
   *  <br/> In turn, <see cref="IRandom.StateCount"/> might be incremented more than once.
   * </summary>
   * <returns>
   *  Random <see cref="double"/> between <paramref name="min_inclusive"/> and
   *  <paramref name="max_inclusive"/>.
   * </returns>
   * <exception cref="ArgumentException">
   *  Thrown when <paramref name="max_inclusive"/> is less than
   *  <paramref name="min_inclusive"/>.
   * </exception>
   */
  public static double NextDouble<T>(this T rng, double min_inclusive = 0, double max_inclusive = 1)
      where T : IRandom {
    if (MathExt.Approximately(min_inclusive, max_inclusive))
      return min_inclusive;
    ThrowIfMaxInclIsLt(min_inclusive, max_inclusive);
    double val = rng.NextUInt64(0, (1ul << 53) + 1) * k0x1p53;
    return min_inclusive + val * (max_inclusive - min_inclusive);
  }

  /**
   * <summary>
   *  Calls <see cref="Jump{T}(T, ulong)"/> in respect to the current <see
   * cref="IRandom.StateCount"/>.
   * </summary>
   * <param name="state_count_target">The desired final <see cref="IRandom.StateCount"/>
   * value.</param> <exception cref="InvalidOperationException"> Thrown when <paramref
   * name="state_count_target"/> is lower than the current PRNG's <see cref="IRandom.StateCount"/>.
   * As jumping backwards is unsupported.
   * </exception>
   */
  public static void JumpTo<T>(this T rng, ulong state_count_target)
      where T : IRandom {
    if (state_count_target < rng.StateCount)
      throw new InvalidOperationException(
          $"[ERROR] Jumping Backwards is unsupported. Current state is {rng.StateCount}, while the target state is {state_count_target}");

    Jump(rng, state_count_target - rng.StateCount);
  }

  /// <inheritdoc cref="IRandomJumpable.Jump(ulong)"/>
  public static void Jump<T>(this T rng, ulong steps)
      where T : IRandom {
    if (rng is IRandomJumpable jmp) {
      jmp.Jump(steps);
      return;
    }

    for (ulong i = 0; i < steps; ++i) rng.NextUInt();
  }

  public static void WriteToBytes<T>(this T rng, Span<byte> buffer)
      where T : IRandom {
    int length     = buffer.Length;
    int c_len      = length >> 4;
    int p_len      = (c_len + 3) & ~3;
    Span<uint> tmp = stackalloc uint[p_len];
    for (int i = 0; i < p_len; ++i) {
      tmp[i] = rng.NextUInt();
    }
    MemoryMarshal.Cast<uint, byte>(tmp) [..length].CopyTo(buffer);
  }

  public static byte[] NextBytes<T>(this T rng)
      where T : IRandom {
    byte[] result = new byte[4];
    WriteToBytes(rng, result);
    return result;
  }
}
}
