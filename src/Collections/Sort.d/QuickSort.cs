using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  /**
   * <summary>
   *  Unstable Sort <paramref name="slice"/> in ascending order using quicksort. <br/>
   *  <i>O(n log n)</i> average case. <i>O(n^2)</i> worst case. <br/>
   * </summary>
   */
  public static void QuickSort<T, TComparer>(this Span<T> slice, in TComparer cmp)
      where T : unmanaged
      where TComparer : IComparer<T> {
    int length = slice.Length;
    if (length <= 1)
      return;

    int pivot_idx = IndexMedian3<T, TComparer>(slice, cmp);
    QuickSortMutate(slice, cmp, pivot_idx, out int lt_idx, out int gt_idx);

    QuickSort(slice[..lt_idx], cmp);
    QuickSort(slice[gt_idx..], cmp);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool QuickSortMutate<T, TComparer>(in Span<T> slice, in TComparer cmp, in int pidx,
      out int lt_idx, out int gt_idx)
      where T : unmanaged
      where TComparer : IComparer<T> {
    int length = slice.Length;
    T pivot    = slice[pidx];

    lt_idx = 0;
    gt_idx = length;

    bool saw_greater = false;
    bool partitioned = true;

    for (int i = 0; i < gt_idx;) {
      switch (cmp.Compare(slice[i], pivot)) {
        case < 0:
          (slice[lt_idx], slice[i]) = (slice[i], slice[lt_idx]);
          ++lt_idx;
          ++i;

          partitioned = !saw_greater && partitioned;
          break;
        case > 0:
          --gt_idx;
          (slice[gt_idx], slice[i]) = (slice[i], slice[gt_idx]);

          saw_greater = true;
          break;
        default:
          ++i;
          break;
      }
    }
    return partitioned;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void QuickSort<T>(this Span<T> slice)
      where T : unmanaged, IComparable<T> => QuickSort(slice, Comparer<T>.Default);
}
}
