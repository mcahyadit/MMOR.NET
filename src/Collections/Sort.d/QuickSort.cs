using System;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  /**
   * <summary>
   *  Unstable Sort <paramref name="slice"/> in ascending order using quicksort. <br/>
   *  <i>O(n log n)</i> average case. <i>O(n^2)</i> worst case. <br/>
   * </summary>
   */
  public static void QuickSort<T>(this Span<T> slice)
      where T : unmanaged, IComparable<T> {
    int length = slice.Length;
    if (length <= 1)
      return;

    int pivot_idx = IndexMedian3<T>(slice);
    QuickSortMutate(slice, pivot_idx, out int lt_idx, out int gt_idx);

    QuickSort(slice[..lt_idx]);
    QuickSort(slice[gt_idx..]);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool QuickSortMutate<T>(in Span<T> slice, in int pidx, out int lt_idx,
      out int gt_idx)
      where T : unmanaged, IComparable<T> {
    int length = slice.Length;
    T pivot    = slice[pidx];

    lt_idx = 0;
    gt_idx = length;

    bool saw_greater = false;
    bool partitioned = true;

    for (int i = 0; i < gt_idx;) {
      switch (slice[i].CompareTo(pivot)) {
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
}
}
