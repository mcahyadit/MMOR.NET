using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  /// <inheritdoc cref="QuickSort{T}(Span{T})"/>
  public static void QuickSort<TKey, TValue, TComparer>(Span<TKey> keys, Span<TValue> values,
      in TComparer cmp)
      where TKey : unmanaged
      where TValue : unmanaged
      where TComparer : IComparer<TKey> {
    int length = keys.Length;
    if (length <= 1)
      return;

    int pivot_idx = IndexMedian3<TKey, TComparer>(keys, cmp);
    QuickSelectMutate(keys, values, cmp, pivot_idx, out int lt_idx, out int gt_idx);

    QuickSort(keys[..lt_idx], values[..lt_idx], cmp);
    QuickSort(keys[gt_idx..], values[gt_idx..], cmp);
  }

  public static bool QuickSelectMutate<TKey, TValue, TComparer>(in Span<TKey> keys,
      in Span<TValue> values, in TComparer cmp, in int pidx, out int lt_idx, out int gt_idx)
      where TKey : unmanaged
      where TValue : unmanaged
      where TComparer : IComparer<TKey> {
    int length = keys.Length;
    TKey pivot = keys[pidx];

    lt_idx = 0;
    gt_idx = length;

    bool saw_greater = false;
    bool partitioned = true;

    for (int i = 0; i < gt_idx;) {
      switch (cmp.Compare(keys[i], pivot)) {
        case < 0:
          (keys[lt_idx], keys[i])     = (keys[i], keys[lt_idx]);
          (values[lt_idx], values[i]) = (values[i], values[lt_idx]);
          ++lt_idx;
          ++i;

          partitioned = !saw_greater && partitioned;
          break;
        case > 0:
          --gt_idx;
          (keys[gt_idx], keys[i])     = (keys[i], keys[gt_idx]);
          (values[gt_idx], values[i]) = (values[i], values[gt_idx]);

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
  public static void QuickSort<TKey, TValue>(Span<TKey> keys, Span<TValue> values)
      where TKey : unmanaged
      where TValue : unmanaged => QuickSort(keys, values, Comparer<TKey>.Default);
}
}
