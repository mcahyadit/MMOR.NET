using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MMOR.NET.Random;

#if UNITY_5_3_OR_NEWER || UNITY_2017_1_OR_NEWER
using Unity.Mathematics;
#else
using System.Numerics;
#endif

namespace MMOR.NET.Utilities {
public static partial class Utilities {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int PopCount(ulong x) {
#if UNITY_5_3_OR_NEWER || UNITY_2017_1_OR_NEWER
    return math.countbits(x);
#else
    return BitOperations.PopCount(x);
#endif
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int CountRZero(ulong x) {
#if UNITY_5_3_OR_NEWER || UNITY_2017_1_OR_NEWER
    return math.tzcnt(x);
#else
    return BitOperations.TrailingZeroCount(x);
#endif
  }

  /**
   * <summary>
   * Converts a list of indices into a bitmask representation.
   * </summary>
   * <exception cref="ArgumentOutOfRangeException">
   * Thrown when one of the indices inside the list is either greater than or equal to 64 or is a
   * negative value.
   * </exception>
   * */
  public static ulong IndicesToMask(this IReadOnlyList<int> indices) {
    ulong result = 0;
    foreach (int index in indices) {
      if (index < 0 || index >= 64) {
        throw new ArgumentOutOfRangeException(nameof(indices), index,
            $"List contains index: {index} that cannot be stored in a UInt64.");
      }
      result |= 1ul << index;
    }
    return result;
  }

  /// <inheritdoc cref="IndicesToMask(IReadOnlyList{int})"/>
  public static ulong IndicesToMask(this ReadOnlySpan<int> indices) {
    ulong result = 0;
    foreach (int index in indices) {
      if (index < 0 || index >= 64) {
        throw new ArgumentOutOfRangeException(nameof(indices), index,
            $"List contains index: {index} that cannot be stored in a UInt64.");
      }
      result |= 1ul << index;
    }
    return result;
  }

  /**
   * <summary>
   * Converts a list of booleans into its bitmask representation.
   * </summary>
   * <exception cref="ArgumentOutOfRangeException">
   * Thrown when the size of <paramref name="bools"/> is greater than 64;
   * </exception>
   * */
  public static ulong ToBitmask(this IReadOnlyList<bool> bools) {
    int len = bools.Count;
    if (len > 64) {
      throw new ArgumentOutOfRangeException(nameof(bools),
          $"Tried to convert a List of boolean that is too big (Count: {len}) for UInt64.");
    }
    ulong result = 0;
    for (int i = 0; i < len; ++i) {
      if (bools[i])
        result |= 1ul << i;
    }
    return result;
  }

  /// <inheritdoc cref="ToBitmask(IReadOnlyList{bool})"/>
  public static ulong ToBitmask(this ReadOnlySpan<bool> bools) {
    int len = bools.Length;
    if (len > 64) {
      throw new ArgumentOutOfRangeException(nameof(bools),
          $"Tried to convert a List of boolean that is too big (Count: {len}) for UInt64.");
    }
    ulong result = 0;
    for (int i = 0; i < len; ++i) {
      if (bools[i])
        result |= 1ul << i;
    }
    return result;
  }

  /**
   * <summary>
   * Converts the bitmask to a list. Listing position of each active bits.
   * </summary>
   * */
  public static void MaskToIndices(this ulong bitmask, IList<int> indices) {
    indices.Clear();
    if (indices is List<int> list) {
      int count = PopCount(bitmask);
      if (list.Capacity < count)
        list.Capacity = count;
    }
    while (bitmask != 0) {
      indices.Add(CountRZero(bitmask));
      bitmask &= (bitmask - 1);
    }
  }

  /// <inheritdoc cref="MaskToIndices(ulong, IList{int})"/>
  public static List<int> MaskToIndices(this ulong bitmask) {
    List<int> result = new(PopCount(bitmask));
    bitmask.MaskToIndices(result);
    return result;
  }

  /**
   * <summary>
   * Converts the bitmask to a List of booleans.
   * </summary>
   * <param name="size">
   *  <br/> Target Size for the List, in case where all 64 bits are not needed.
   *  <br/> Defaults to 64.
   * </param>
   * */
  public static void ToBoolList(this ulong bitmask, IList<bool> bools, int size = 64) {
    bools.Clear();
    if (bools is List<bool> list) {
      if (list.Capacity < size)
        list.Capacity = size;
    }
    for (int i = 0; i < size; ++i) {
      bools.Add((bitmask & (1ul << i)) != 0);
    }
  }

  /// <inheritdoc cref="ToBoolList(ulong, IList{bool}, int)"/>
  public static List<bool> ToBoolList(this ulong bitmask, int size = 64) {
    List<bool> result = new(size);
    bitmask.ToBoolList(result, size);
    return result;
  }

  /**
   * <summary>
   *  Randomly selects an active bit in a bitmask and returns the position.
   * </summary>
   * <param name="bitmask">The bitmask to select from.</param>
   * <param name="rng">The random engine.</param>
   * <returns>
   *  <c>-1</c> if <paramref name="bitmask"/> is <c>0</c>,
   *  otherwise the index of the randomly selected active bit.
   * </returns>
   * */
  public static int GetRandomActiveBit(ulong bitmask, IRandom rng) {
    if (bitmask == 0)
      return -1;

    int count  = PopCount(bitmask);
    int target = rng.NextInt(0, count);
    for (int i = 0; i < target; ++i) {
      bitmask &= bitmask - 1;
    }

    return CountRZero(bitmask);
  }
}
}
