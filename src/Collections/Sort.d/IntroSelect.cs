using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Numerics;
using MMOR.NET.Random;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  /**
   * <summary>
   *  Return the <paramref name="k"/>th (1-Indexed) smallest value
   *  in unordered <paramref name="slice"/>. <br/>
   *  <i>O(n)</i> average case. <i>O(n^2)</i> worst case. <br/>
   * </summary>
   */
  [Pure]
  public static T IntroSelect<T>(this ReadOnlySpan<T> slice, int k, int? depth_limit = null)
      where T : unmanaged, IComparable<T> {
    int length = slice.Length;
    if ((uint)(k - 1) >= length)
      throw new ArgumentOutOfRangeException(
          $"[ERROR]: Trying to IntroSelect {k}th element in an array of Length: {length}");
    T[]? mut_data = length > kHeapThreshold ? ArrayPool<T>.Shared.Rent(length) : null;
    try {
      Span<T> mut_buffer = mut_data is null ? stackalloc T[length] : mut_data.AsSpan(0, length);
      slice.CopyTo(mut_buffer);
      return IntroSelectMutable(mut_buffer, k, depth_limit ?? 2 * BitOperations.Log2((uint)length));
    } finally {
      if (mut_data is not null) {
        ArrayPool<T>.Shared.Return(mut_data, true);
      }
    }
  }

  /**
   * <inheritdoc cref="IntroSelect{T}(ReadOnlySpan{T}, int, int?)"/>
   * <remarks>
   *  This method will non-deterministically re-order the elements in the collection.
   * </remarks>
   */
  public static T IntroSelectMutable<T>(this Span<T> slice, int k, int? depth_limit = null)
      where T : unmanaged, IComparable<T> {
    int length = slice.Length;
    if ((uint)(k - 1) >= length)
      throw new ArgumentOutOfRangeException(
          $"[ERROR]: Trying to IntroSelect {k}th element in an array of Length: {length}");
    if (length == 1)
      return slice[0];
    if (length == 3) {
      Sort3Unsafe(slice, 1);
      return slice[k - 1];
    }

    depth_limit ??= 2 * BitOperations.Log2((uint)slice.Length);
    if (depth_limit <= 0) {
      return MedianOfMediansMutable(slice, k);
    }

    int pivot_idx = length / 2;
    Sort3Unsafe(slice, pivot_idx);
    QuickSelectMutate(slice, pivot_idx, out int lt_idx, out int gt_idx);

    if (k <= lt_idx) {
      return IntroSelectMutable(slice[..lt_idx], k, depth_limit - 1);
    } else if (k > gt_idx) {
      return IntroSelectMutable(slice[gt_idx..], k - gt_idx, depth_limit - 1);
    } else {
      return slice[lt_idx];
    }
  }

  /// <inheritdoc cref="IntroSelect{T}(ReadOnlySpan{T}, int, int?)"/>
  [Pure]
  public static T IntroSelect<T>(this ICollection<T> collection, int k, int? depth_limit = null)
      where T : unmanaged, IComparable<T> {
    int length = collection.Count;
    if ((uint)(k - 1) >= length)
      throw new ArgumentOutOfRangeException(
          $"[ERROR]: Trying to IntroSelect {k}th element in an array of Length: {length}");
    T[] mut_data   = ArrayPool<T>.Shared.Rent(length);
    depth_limit  ??= 2 * BitOperations.Log2((uint)collection.Count);
    try {
      collection.CopyTo(mut_data, 0);
      return IntroSelectMutable(mut_data.AsSpan(0, length), k,
          depth_limit ?? 2 * BitOperations.Log2((uint)length));
    } finally {
      ArrayPool<T>.Shared.Return(mut_data, true);
    }
  }
}
}
