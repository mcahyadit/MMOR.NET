using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MMOR.NET.Mathematics;

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

    uint range = max_exclusive - min_inclusive;
    uint limit = uint.MaxValue - uint.MaxValue % range;
    uint result;

    do {
      result = rng.NextUInt();
    } while (result >= limit);

    return min_inclusive + result % range;
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
    var range = (uint)(max_exclusive - min_inclusive);
    return min_inclusive + (int)rng.NextUInt(0, range);
  }

  public const float kU32MaxF32 = uint.MaxValue;

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
    return min_inclusive + rng.NextUInt() / kU32MaxF32 * (max_inclusive - min_inclusive);
  }

  public const double kU32MaxF64 = uint.MaxValue;

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
    return min_inclusive + rng.NextUInt() / kU32MaxF64 * (max_inclusive - min_inclusive);
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
