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
  public static T MedianOfMedians<T>(this ReadOnlySpan<T> slice, int k)
      where T : unmanaged, IComparable<T> {
    int length = slice.Length;
    if ((uint)(k - 1) >= length)
      throw new ArgumentOutOfRangeException(
          $"[ERROR]: Trying to QuickSelect {k}th element in an array of Length: {length}");
    T[]? mut_data = length > kHeapThreshold ? ArrayPool<T>.Shared.Rent(length) : null;
    try {
      Span<T> mut_buffer = mut_data is null ? stackalloc T[length] : mut_data.AsSpan(0, length);
      slice.CopyTo(mut_buffer);
      return MedianOfMediansMutable(mut_buffer, k);
    } finally {
      if (mut_data is not null) {
        ArrayPool<T>.Shared.Return(mut_data, true);
      }
    }
  }

  /**
   * <inheritdoc cref="MedianOfMedians{T}(ReadOnlySpan{T}, int)"/>
   * <remarks>
   *  This method will deterministically re-order the elements in the collection.
   * </remarks>
   */
  public static T MedianOfMediansMutable<T>(this Span<T> slice, int k)
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
    if (length <= 5) {
      InsertionSort(slice);
      return slice[k - 1];
    }

    int grp_cnt  = (length + 4) / 5;
    T[]? med_mem = length > kHeapThreshold ? ArrayPool<T>.Shared.Rent(grp_cnt) : null;
    try {
      Span<T> med_buff = med_mem is null ? stackalloc T[grp_cnt] : med_mem.AsSpan(0, grp_cnt);

      for (int gidx = 0, i = 0; gidx < grp_cnt; i += 5, ++gidx) {
        Span<T> group = slice.Slice(i, Math.Min(5, length - i));
        InsertionSort(group);
        med_buff[gidx] = group[group.Length / 2];
      }

      T pivot  = MedianOfMediansMutable(med_buff, grp_cnt / 2 + 1);
      int pidx = 0;
      while (pidx < length && Comparer<T>.Default.Compare(slice[pidx], pivot) != 0) ++pidx;
      QuickSortMutate(slice, Comparer<T>.Default, pidx, out int lt_idx, out int gt_idx);

      if (k <= lt_idx)
        return MedianOfMediansMutable(slice[..lt_idx], k);
      else if (k > gt_idx)
        return MedianOfMediansMutable(slice[gt_idx..], k - gt_idx);
      else
        return slice[lt_idx];
    } finally {
      if (med_mem is not null) {
        ArrayPool<T>.Shared.Return(med_mem, true);
      }
    }
  }
}
}
