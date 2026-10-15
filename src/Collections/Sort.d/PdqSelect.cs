using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Numerics;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  public static readonly PdqSortConfig kDefaultPdqSelectConfig = new() {
    insertion_sort_threshold     = 11,
    ninther_threshold            = 49,
    partial_insertion_sort_limit = 8,
  };

  public static void BreakDescendingUnsafe<T>(Span<T> slice)
      where T : unmanaged, IComparable<T> {
    int length = slice.Length;

    int qp = length / 4;
    int q1 = slice[qp - 1].CompareTo(slice[qp]);
    int q2 = slice[2 * qp - 1].CompareTo(slice[2 * qp]);
    int q3 = slice[3 * qp - 1].CompareTo(slice[3 * qp]);

    if (Math.Min(q1, Math.Min(q2, q3)) > 0) {
      slice.Reverse();
    }
  }

  /**
   * <summary>
   *  Return the <paramref name="k"/>th (1-Indexed) smallest value
   *  in unordered <paramref name="slice"/>. <br/>
   *  <i>O(n)</i> average case. <i>O(n)</i> worst case. <br/>
   * </summary>
   */
  [Pure]
  public static T PdqSelect<T>(this ReadOnlySpan<T> slice, int k, PdqSortConfig? config = null,
      int? depth_limit = null)
      where T : unmanaged, IComparable<T> {
    int length = slice.Length;
    if ((uint)(k - 1) >= length)
      throw new ArgumentOutOfRangeException(
          $"[ERROR]: Trying to PdqSelect {k}th element in an array of Length: {length}");
    T[]? mut_data = length > kHeapThreshold ? ArrayPool<T>.Shared.Rent(length) : null;
    try {
      Span<T> mut_buffer = mut_data is null ? stackalloc T[length] : mut_data.AsSpan(0, length);
      slice.CopyTo(mut_buffer);
      return PdqSelectMutable(mut_buffer, k, config ?? kDefaultPdqSelectConfig, depth_limit);
    } finally {
      if (mut_data is not null) {
        ArrayPool<T>.Shared.Return(mut_data, true);
      }
    }
  }

  /**
   * <inheritdoc cref="PdqSelect{T}(ReadOnlySpan{T}, int, PdqSortConfig?, int?)"/>
   * <remarks>
   *  This method will non-deterministically re-order the elements in the collection.
   * </remarks>
   */
  public static T PdqSelectMutable<T>(this Span<T> slice, int k, PdqSortConfig? config = null,
      int? depth_limit = null)
      where T : unmanaged, IComparable<T> {
    PdqSortConfig cfg = config ?? kDefaultPdqSelectConfig;
    int length        = slice.Length;
    if ((uint)(k - 1) >= length)
      throw new ArgumentOutOfRangeException(
          $"[ERROR]: Trying to PdqSelect {k}th element in an array of Length: {length}");
    if (length == 1)
      return slice[0];
    if (k == 1)
      return MinElement<T>(slice);
    if (length == k)
      return MaxElement<T>(slice);
    if (length <= Math.Max(k, cfg.insertion_sort_threshold)) {
      if (length == 3)
        Sort3Unsafe(slice, 1);
      else
        IntroSort(slice);
      return slice[k - 1];
    }

    int pivot_idx = length / 2;
    if (length > cfg.ninther_threshold) {
      BreakDescendingUnsafe(slice);
      OrlpNintherSortUnsafe(slice);
    } else {
      Sort3Unsafe(slice, pivot_idx);
    }
    QuickSortMutate(slice, pivot_idx, out int lt_idx, out int gt_idx);

    if (k > lt_idx && k <= gt_idx)
      return slice[lt_idx];

    int l8                 = length / 8;
    Span<T> lt_span        = slice[..lt_idx];
    Span<T> gt_span        = slice[gt_idx..];
    bool highly_unbalanced = lt_span.Length < l8 || gt_span.Length < l8;

    if (highly_unbalanced) {
      depth_limit ??= BitOperations.Log2((uint)length);
      if (--depth_limit == 0)
        return MedianOfMediansMutable(slice, k);
    }
    if (k <= lt_idx) {
      if (highly_unbalanced)
        BreakPatterns(lt_span, cfg.insertion_sort_threshold, cfg.ninther_threshold);
      return PdqSelectMutable(lt_span, k, cfg, depth_limit);
    } else if (k > gt_idx) {
      if (highly_unbalanced)
        BreakPatterns(gt_span, cfg.insertion_sort_threshold, cfg.ninther_threshold);
      return PdqSelectMutable(gt_span, k - gt_idx, cfg, depth_limit);
    } else {
      return slice[lt_idx];
    }
  }

  /// <inheritdoc cref="PdqSelect{T}(ReadOnlySpan{T}, int, PdqSortConfig?, int?)"/>
  [Pure]
  public static T PdqSelect<T>(this ICollection<T> collection, int k, PdqSortConfig? config = null,
      int? depth_limit = null)
      where T : unmanaged, IComparable<T> {
    int length = collection.Count;
    if ((uint)(k - 1) >= length)
      throw new ArgumentOutOfRangeException(
          $"[ERROR]: Trying to PdqSelect {k}th element in an array of Length: {length}");
    T[] mut_data = ArrayPool<T>.Shared.Rent(length);
    try {
      collection.CopyTo(mut_data, 0);
      return PdqSelectMutable(mut_data.AsSpan(0, length), k, config ?? kDefaultPdqSelectConfig,
          depth_limit);
    } finally {
      ArrayPool<T>.Shared.Return(mut_data, true);
    }
  }
}
}
