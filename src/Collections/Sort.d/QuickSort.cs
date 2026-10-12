using System;
using MMOR.NET.Random;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  public static void QuickSort<T>(this Span<T> slice)
      where T : unmanaged, IComparable<T> {
    int length = slice.Length;
    if (length <= 1)
      return;

    int pivot_idx = Xoroshiro.global.NextInt(0, length);
    QuickSelectMutate(slice, pivot_idx, out int lt_idx, out int gt_idx);

    QuickSort(slice[..lt_idx]);
    QuickSort(slice[gt_idx..]);
  }

  public static bool QuickSelectMutate<T>(Span<T> slice, int pidx, out int lt_idx, out int gt_idx)
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

  public static void QuickSort<TKey, TValue>(Span<TKey> keys, Span<TValue> values)
      where TKey : unmanaged, IComparable<TKey>
      where TValue : unmanaged {
    int length = keys.Length;
    if (length <= 1)
      return;

    int pivot_idx = Xoroshiro.global.NextInt(0, length);
    QuickSelectMutate(keys, values, pivot_idx, out int lt_idx, out int gt_idx);

    QuickSort(keys[..lt_idx], values[..lt_idx]);
    QuickSort(keys[gt_idx..], values[gt_idx..]);
  }

  public static bool QuickSelectMutate<TKey, TValue>(Span<TKey> keys, Span<TValue> values, int pidx,
      out int lt_idx, out int gt_idx)
      where TKey : unmanaged, IComparable<TKey>
      where TValue : unmanaged {
    int length = keys.Length;
    TKey pivot = keys[pidx];

    lt_idx = 0;
    gt_idx = length;

    bool saw_greater = false;
    bool partitioned = true;

    for (int i = 0; i < gt_idx;) {
      switch (keys[i].CompareTo(pivot)) {
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
}
}
