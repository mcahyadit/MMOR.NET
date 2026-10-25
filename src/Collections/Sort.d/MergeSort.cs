using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  /**
   * <summary>
   *  Stable Sort <paramref name="slice"/> in ascending order using merge sort. <br/>
   *  <i>O(n log n)</i> all cases. <br/>
   * </summary>
   */
  public static void MergeSort<T, TComparer>(this Span<T> slice, in TComparer cmp)
      where T : unmanaged
      where TComparer : IComparer<T> {
    int length = slice.Length;
    if (length <= 1)
      return;

    int midx = length / 2;
    MergeSort(slice[..midx], cmp);
    MergeSort(slice[midx..], cmp);
    MergeSortMutate(slice, cmp, midx);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void MergeSortMutate<T, TComparer>(in Span<T> slice, in TComparer cmp, in int midx)
      where T : unmanaged
      where TComparer : IComparer<T> {
    int length = slice.Length;
    int l_len  = midx;
    T[]? l_mem = length > kHeapThreshold ? ArrayPool<T>.Shared.Rent(l_len) : null;
    try {
      Span<T> l_buff = l_mem is null ? stackalloc T[l_len] : l_mem.AsSpan(0, l_len);
      slice[..midx].CopyTo(l_buff);

      int idx   = 0;
      int l_idx = 0;
      int r_idx = midx;
      for (; l_idx < l_len && r_idx < length; ++idx) {
        if (cmp.Compare(l_buff[l_idx], slice[r_idx]) <= 0) {
          slice[idx] = l_buff[l_idx++];
        } else {
          slice[idx] = slice[r_idx++];
        }
      }
      for (; l_idx < l_len; ++l_idx, ++idx) {
        slice[idx] = l_buff[l_idx];
      }
    } finally {
      if (l_mem is not null) {
        ArrayPool<T>.Shared.Return(l_mem, true);
      }
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void MergeSort<T>(this Span<T> slice)
      where T : unmanaged, IComparable<T> => MergeSort(slice, Comparer<T>.Default);
}
}
