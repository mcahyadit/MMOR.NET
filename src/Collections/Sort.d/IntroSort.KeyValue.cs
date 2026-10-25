using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  /// <inheritdoc cref="Sort3Unsafe{T, TComparer}(in Span{T}, in TComparer, in int)"/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Sort3Unsafe<TKey, TValue, TComparer>(Span<TKey> keys, Span<TValue> values,
      in TComparer cmp, int midx)
      where TKey : unmanaged
      where TValue : unmanaged
      where TComparer : IComparer<TKey> {
    ref TKey a = ref keys[0];
    ref TKey b = ref keys[midx];
    ref TKey c = ref keys[^1];

    if (cmp.Compare(a, b) > 0) {
      (a, b)                    = (b, a);
      (values[0], values[midx]) = (values[midx], values[0]);
    }
    if (cmp.Compare(a, c) > 0) {
      (a, c)                  = (c, a);
      (values[0], values[^1]) = (values[^1], values[0]);
    }
    if (cmp.Compare(b, c) > 0) {
      (b, c)                     = (c, b);
      (values[midx], values[^1]) = (values[^1], values[midx]);
    }
  }

  /// <inheritdoc cref="IntroSort{T, TComparer}(Span{T}, in TComparer, in int, int?)"/>
  public static void IntroSort<TKey, TValue, TComparer>(Span<TKey> keys, Span<TValue> values,
      in TComparer cmp, int insertion_sort_threshold = 24, int? depth_limit = null)
      where TKey : unmanaged
      where TValue : unmanaged
      where TComparer : IComparer<TKey> {
    int length = keys.Length;
    if (length <= 1)
      return;

    depth_limit ??= 2 * BitOperations.Log2((uint)length);

    if (length <= insertion_sort_threshold) {
      InsertionSort(keys, values, cmp);
    } else if (depth_limit <= 0) {
      HeapSort(keys, values, cmp);
    } else {
      int pivot_idx = IndexMedian3<TKey, TComparer>(keys, cmp);
      QuickSelectMutate(keys, values, cmp, pivot_idx, out int lt_idx, out int gt_idx);
      IntroSort(keys[..lt_idx], values[..lt_idx], cmp, insertion_sort_threshold, depth_limit - 1);
      IntroSort(keys[gt_idx..], values[gt_idx..], cmp, insertion_sort_threshold, depth_limit - 1);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void IntroSort<TKey, TValue>(Span<TKey> keys, Span<TValue> values,
      int insertion_sort_threshold = 24, int? depth_limit = null)
      where TKey : unmanaged
      where TValue : unmanaged => IntroSort(keys,
                     values,
                     Comparer<TKey>.Default,
                     insertion_sort_threshold,
                     depth_limit);
}
}
