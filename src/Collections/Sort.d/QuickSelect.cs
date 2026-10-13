using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  public const int kHeapThreshold = 128;

  /**
   * <summary>
   *  Return the <paramref name="k"/>th (1-Indexed) smallest value
   *  in unordered <paramref name="slice"/>. <br/>
   *  <i>O(n)</i> average case. <i>O(n^2)</i> worst case. <br/>
   * </summary>
   */
  [Pure]
  public static T QuickSelect<T>(this ReadOnlySpan<T> slice, int k)
      where T : unmanaged, IComparable<T> {
    int length = slice.Length;
    if ((uint)(k - 1) >= length)
      throw new ArgumentOutOfRangeException(
          $"[ERROR]: Trying to QuickSelect {k}th element in an array of Length: {length}");
    T[]? mut_data = length > kHeapThreshold ? ArrayPool<T>.Shared.Rent(length) : null;
    try {
      Span<T> mut_buffer = mut_data is null ? stackalloc T[length] : mut_data.AsSpan(0, length);
      slice.CopyTo(mut_buffer);
      return QuickSelectMutable(mut_buffer, k);
    } finally {
      if (mut_data is not null) {
        ArrayPool<T>.Shared.Return(mut_data, true);
      }
    }
  }

  /**
   * <inheritdoc cref="QuickSelect{T}(ReadOnlySpan{T}, int)"/>
   * <remarks>
   *  This method will non-deterministically re-order the elements in the collection.
   * </remarks>
   */
  public static T QuickSelectMutable<T>(this Span<T> slice, int k)
      where T : unmanaged, IComparable<T> {
    int length = slice.Length;
    if ((uint)(k - 1) >= length)
      throw new ArgumentOutOfRangeException(
          $"[ERROR]: Trying to QuickSelect {k}th element in an array of Length: {length}");
    if (length == 1)
      return slice[0];
    if (k == 1)
      return MinElement<T>(slice);
    if (length == k)
      return MaxElement<T>(slice);

    int pivot_idx = IndexMedian3<T>(slice);
    QuickSortMutate(slice, pivot_idx, out int lt_idx, out int gt_idx);

    if (k <= lt_idx) {
      return QuickSelectMutable(slice[..lt_idx], k);
    } else if (k > gt_idx) {
      return QuickSelectMutable(slice[gt_idx..], k - gt_idx);
    } else {
      return slice[lt_idx];
    }
  }

  /// <inheritdoc cref="QuickSelect{T}(ReadOnlySpan{T}, int)"/>
  [Pure]
  public static T QuickSelect<T>(this ICollection<T> collection, int k)
      where T : unmanaged, IComparable<T> {
    int length = collection.Count;
    if ((uint)(k - 1) >= length)
      throw new ArgumentOutOfRangeException(
          $"[ERROR]: Trying to QuickSelect {k}th element in an array of Length: {length}");
    T[] mut_data = ArrayPool<T>.Shared.Rent(length);
    try {
      collection.CopyTo(mut_data, 0);
      T result = QuickSelectMutable(mut_data.AsSpan(0, length), k);
      return result;
    } finally {
      ArrayPool<T>.Shared.Return(mut_data, true);
    }
  }
}
}
