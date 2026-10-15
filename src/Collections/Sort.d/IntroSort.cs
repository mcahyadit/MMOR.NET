using System;
using System.Numerics;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  /**
   * <summary>
   *  Internal function for <see cref="IntroSort{T}(Span{T}, int, int?)"/>. <br/>
   *  Use at your own risk.
   * </summary>
   */
  public static void Sort3Unsafe<T>(Span<T> slice, int midx)
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
  public static void IntroSort<T>(this Span<T> slice, int insertion_sort_threshold = 16,
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

  /// <inheritdoc cref="Sort3Unsafe{T}(Span{T}, int)"/>
  public static void Sort3Unsafe<TKey, TValue>(Span<TKey> keys, Span<TValue> values, int midx)
      where TKey : unmanaged, IComparable<TKey>
      where TValue : unmanaged {
    ref TKey a = ref keys[0];
    ref TKey b = ref keys[midx];
    ref TKey c = ref keys[^1];

    if (a.CompareTo(b) > 0) {
      (a, b)                    = (b, a);
      (values[0], values[midx]) = (values[midx], values[0]);
    }
    if (a.CompareTo(c) > 0) {
      (a, c)                  = (c, a);
      (values[0], values[^1]) = (values[^1], values[0]);
    }
    if (b.CompareTo(c) > 0) {
      (b, c)                     = (c, b);
      (values[midx], values[^1]) = (values[^1], values[midx]);
    }
  }

  /// <inheritdoc cref="IntroSort{T}(Span{T}, int, int?)"/>
  public static void IntroSort<TKey, TValue>(this Span<TKey> keys, Span<TValue> values,
      int insertion_sort_threshold = 16, int? depth_limit = null)
      where TKey : unmanaged, IComparable<TKey>
      where TValue : unmanaged {
    int length = keys.Length;
    if (length <= 1)
      return;

    depth_limit ??= 2 * BitOperations.Log2((uint)length);

    if (length <= insertion_sort_threshold) {
      InsertionSort(keys, values);
    } else if (depth_limit <= 0) {
      HeapSort(keys, values);
    } else {
      int pivot_idx = IndexMedian3<TKey>(keys);
      QuickSelectMutate(keys, values, pivot_idx, out int lt_idx, out int gt_idx);
      IntroSort(keys[..lt_idx], values[..lt_idx], insertion_sort_threshold, depth_limit - 1);
      IntroSort(keys[gt_idx..], values[gt_idx..], insertion_sort_threshold, depth_limit - 1);
    }
  }
}
}
