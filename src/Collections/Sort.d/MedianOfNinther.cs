using System;
using System.Buffers;
using System.Collections.Generic;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  /**
   * <summary>
   *  Return the <paramref name="k"/>th (1-Indexed) smallest value
   *  in unordered <paramref name="slice"/>. <br/>
   *  Deterministic <i>O(n)</i> worst case via the Median-of-Medians pivot.
   * </summary>
   */
  public static T MedianOfNinther<T>(this ReadOnlySpan<T> slice, int k,
      int insertion_sort_threshold = 16)
      where T : unmanaged, IComparable<T> {
    int length = slice.Length;
    if ((uint)(k - 1) >= length)
      throw new ArgumentOutOfRangeException(
          $"[ERROR]: Trying to QuickSelect {k}th element in an array of Length: {length}");
    T[]? mut_data = length > kHeapThreshold ? ArrayPool<T>.Shared.Rent(length) : null;
    try {
      Span<T> mut_buffer = mut_data is null ? stackalloc T[length] : mut_data.AsSpan(0, length);
      slice.CopyTo(mut_buffer);
      return MedianOfNintherMutable(mut_buffer, k, insertion_sort_threshold);
    } finally {
      if (mut_data is not null) {
        ArrayPool<T>.Shared.Return(mut_data, true);
      }
    }
  }

  /**
   * <inheritdoc cref="MedianOfNinther{T}(ReadOnlySpan{T}, int, int)"/>
   * <remarks>
   *  This method will deterministically re-order the elements in the collection.
   * </remarks>
   */
  public static T MedianOfNintherMutable<T>(this Span<T> slice, int k,
      int insertion_sort_threshold = 16)
      where T : unmanaged, IComparable<T> {
    int length = slice.Length;
    if ((uint)(k - 1) >= length)
      throw new ArgumentOutOfRangeException(
          $"[ERROR]: Trying to QuickSelect {k}th element in an array of Length: {length}");
    if (k == 1) {
      return MinElement<T>(slice);
    }
    if (k == length) {
      return MaxElement<T>(slice);
    }
    if (length <= insertion_sort_threshold) {
      InsertionSort(slice);
      return slice[k - 1];
    }

    int frac = length <= 1024 ? length / 12 : length <= 128 * 1024 ? length / 64 : length / 1024;

    int middle = frac / 2;
    int lo     = length / 2 - middle;
    int hi     = frac + lo;
    int gap    = (length - 9 * frac) / 4;
    int left   = lo - 4 * frac - gap;
    int right  = hi + gap;

    for (int i = lo; i < hi; ++i, left += 3, right += 3) {
      int midx = i - frac;
      for (int x = 0; x < 3; ++x) {
        Span<T> group = slice[(left + x)..(right + x + 1)];
        Sort3Unsafe(group, midx + frac * x - (left + x));
      }
      Sort3Unsafe(slice.Slice(i - frac, 2 * frac + 1), frac);
    }

    T pivot  = MedianOfNintherMutable(slice.Slice(lo, frac), middle + 1, insertion_sort_threshold);
    int pidx = 0;
    while (pidx < length && Comparer<T>.Default.Compare(slice[pidx], pivot) != 0) ++pidx;
    QuickSortMutate(slice, pidx, out int lt_idx, out int gt_idx);

    if (k <= lt_idx) {
      return MedianOfNintherMutable(slice[..lt_idx], k, insertion_sort_threshold);
    } else if (k > gt_idx) {
      return MedianOfNintherMutable(slice[gt_idx..], k - gt_idx, insertion_sort_threshold);
    } else {
      return slice[lt_idx];
    }
  }
}
}
