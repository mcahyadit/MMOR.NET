using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  /**
   * <summary>
   *  Internal function for <see cref="PdqSort{T}"/>. <br/>
   *  Use at your own risk.
   * </summary>
   */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void OrlpNintherSortUnsafe<T>(in Span<T> slice)
      where T : unmanaged, IComparable<T> {
    int length = slice.Length;
    // Cheat from orlp definition
    // From: ABC...BAC...CBA
    //   To: ABC...ABC...CBA
    int midx = length / 2 - 1;
    for (int i = 0; i < 3; ++i) {
      Span<T> group = slice[i..(length - i)];
      // We don't do `midx + i` because the Slice already does it
      Sort3Unsafe(group, midx);
    }
    Sort3Unsafe(slice.Slice(midx, 3), 1);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool PartialInsertionSort<T>(in Span<T> slice, int limit)
      where T : unmanaged, IComparable<T> {
    int length = slice.Length;
    for (int c_idx = 1; c_idx < length; ++c_idx) {
      T pivot   = slice[c_idx];
      int s_idx = c_idx - 1;

      while (s_idx >= 0 && slice[s_idx].CompareTo(pivot) > 0) {
        slice[s_idx + 1] = slice[s_idx];
        --s_idx;
      }
      slice[s_idx + 1] = pivot;

      limit -= c_idx - s_idx - 1;
      if (limit <= 0)
        return false;
    }
    return true;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void BreakPatterns<T>(in Span<T> slice, in int t1, in int t2) {
    if (slice.Length >= t1) {
      int l4                  = slice.Length / 4;
      (slice[0], slice[l4])   = (slice[l4], slice[0]);
      (slice[^1], slice[^l4]) = (slice[^l4], slice[^1]);

      if (slice.Length > t2) {
        (slice[1], slice[l4 + 1])     = (slice[l4 + 1], slice[1]);
        (slice[2], slice[l4 + 2])     = (slice[l4 + 2], slice[2]);
        (slice[^2], slice[^(l4 + 1)]) = (slice[^(l4 + 1)], slice[^2]);
        (slice[^3], slice[^(l4 + 2)]) = (slice[^(l4 + 2)], slice[^3]);
      }
    }
  }

  /**
   * <summary>
   *  Unstable Sort <paramref name="slice"/> in ascending order using pdqsort. <br/>
   *  <i>O(n)</i> best case. <i>O(n log n)</i> worst case. <br/>
   * </summary>
   */
  public static void PdqSort<T>(this Span<T> slice, in int insertion_sort_threshold = 24,
      in int ninther_threshold = 128, in int partial_insertion_sort_limit = 8,
      int? depth_limit = null)
      where T : unmanaged, IComparable<T> {
    int length = slice.Length;
    if (length <= 1)
      return;

    if (length < insertion_sort_threshold) {
      InsertionSort(slice);
      return;
    }

    int pivot_idx = length / 2;
    if (length > ninther_threshold) {
      OrlpNintherSortUnsafe(slice);
    } else {
      Sort3Unsafe(slice, pivot_idx);
    }
    bool partitioned = QuickSortMutate(slice, pivot_idx, out int lt_idx, out int gt_idx);

    int l8                 = length / 8;
    Span<T> lt_span        = slice[..lt_idx];
    Span<T> gt_span        = slice[gt_idx..];
    bool highly_unbalanced = lt_span.Length < l8 || gt_span.Length < l8;

    bool lt_sort = false;
    bool gt_sort = false;
    if (highly_unbalanced) {
      depth_limit ??= BitOperations.Log2((uint)length);
      if (--depth_limit == 0) {
        HeapSort(slice);
        return;
      }

      BreakPatterns(lt_span, insertion_sort_threshold, ninther_threshold);
      BreakPatterns(gt_span, insertion_sort_threshold, ninther_threshold);
    } else if (partitioned) {
      lt_sort = PartialInsertionSort(slice[..lt_idx], partial_insertion_sort_limit);
      gt_sort = PartialInsertionSort(slice[gt_idx..], partial_insertion_sort_limit);
    }

    if (!lt_sort)
      PdqSort(slice[..lt_idx], insertion_sort_threshold, ninther_threshold,
          partial_insertion_sort_limit, depth_limit);
    if (!gt_sort)
      PdqSort(slice[gt_idx..], insertion_sort_threshold, ninther_threshold,
          partial_insertion_sort_limit, depth_limit);
  }
}
}
