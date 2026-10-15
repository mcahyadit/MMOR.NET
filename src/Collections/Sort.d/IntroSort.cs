using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  /**
   * <summary>
   *  Internal function for <see cref="IntroSort{T}(Span{T}, in int, int?)"/>. <br/>
   *  Use at your own risk.
   * </summary>
   */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Sort3Unsafe<T>(in Span<T> slice, in int midx)
      where T : unmanaged, IComparable<T> {
    ref T a = ref slice[0];
    ref T b = ref slice[midx];
    ref T c = ref slice[^1];

    if (a.CompareTo(b) > 0) {
      (a, b) = (b, a);
    }
    if (a.CompareTo(c) > 0) {
      (a, c) = (c, a);
    }
    if (b.CompareTo(c) > 0) {
      (b, c) = (c, b);
    }
  }

  /**
   * <summary>
   *  Unstable Sort <paramref name="slice"/> in ascending order using introsort. <br/>
   *  <i>O(n log n)</i> all cases. <br/>
   * </summary>
   */
  public static void IntroSort<T>(this Span<T> slice, in int insertion_sort_threshold = 24,
      int? depth_limit = null)
      where T : unmanaged, IComparable<T> {
    int length = slice.Length;
    if (length <= 1)
      return;

    depth_limit ??= 2 * BitOperations.Log2((uint)length);

    if (length <= insertion_sort_threshold) {
      InsertionSort(slice);
    } else if (depth_limit <= 0) {
      HeapSort(slice);
    } else {
      int pivot_idx = IndexMedian3<T>(slice);
      QuickSortMutate(slice, pivot_idx, out int lt_idx, out int gt_idx);
      IntroSort(slice[..lt_idx], insertion_sort_threshold, depth_limit - 1);
      IntroSort(slice[gt_idx..], insertion_sort_threshold, depth_limit - 1);
    }
  }
}
}
