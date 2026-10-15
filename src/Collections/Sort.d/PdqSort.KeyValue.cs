using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  /// <inheritdoc cref="OrlpNintherSortUnsafe{T}(in Span{T})"/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void NintherSortUnsafe<TKey, TValue>(in Span<TKey> keys, in Span<TValue> values)
      where TKey : unmanaged, IComparable<TKey>
      where TValue : unmanaged {
    int length = keys.Length;
    // Cheat from orlp definition
    // From: ABC...BAC...CBA
    //   To: ABC...ABC...CBA
    int midx = length / 2 - 1;
    for (int i = 0; i < 3; ++i) {
      Span<TKey> k_group   = keys[i..(length - i)];
      Span<TValue> v_group = values[i..(length - i)];
      // We don't do `midx + i` because the Slice already does it
      Sort3Unsafe(k_group, v_group, midx);
    }
    Sort3Unsafe(keys.Slice(midx, 3), values.Slice(midx, 3), 1);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool PartialInsertionSort<TKey, TValue>(in Span<TKey> keys, in Span<TValue> values,
      int limit)
      where TKey : unmanaged, IComparable<TKey>
      where TValue : unmanaged {
    int length = keys.Length;
    for (int c_idx = 1; c_idx < length; ++c_idx) {
      TKey pivot   = keys[c_idx];
      TValue p_val = values[c_idx];
      int s_idx    = c_idx - 1;

      while (s_idx >= 0 && keys[s_idx].CompareTo(pivot) > 0) {
        keys[s_idx + 1]   = keys[s_idx];
        values[s_idx + 1] = values[s_idx];
        --s_idx;
      }
      keys[s_idx + 1]   = pivot;
      values[s_idx + 1] = p_val;

      limit -= c_idx - s_idx - 1;
      if (limit <= 0)
        return false;
    }
    return true;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void BreakPatterns<TKey, TValue>(in Span<TKey> keys, in Span<TValue> values, int t1,
      in int t2)
      where TKey : unmanaged, IComparable<TKey>
      where TValue : unmanaged {
    if (keys.Length >= t1) {
      int l4                    = keys.Length / 4;
      (keys[0], keys[l4])       = (keys[l4], keys[0]);
      (values[0], values[l4])   = (values[l4], values[0]);
      (keys[^1], keys[^l4])     = (keys[^l4], keys[^1]);
      (values[^1], values[^l4]) = (values[^l4], values[^1]);

      if (keys.Length > t2) {
        (keys[1], keys[l4 + 1])         = (keys[l4 + 1], keys[1]);
        (values[1], values[l4 + 1])     = (values[l4 + 1], values[1]);
        (keys[2], keys[l4 + 2])         = (keys[l4 + 2], keys[2]);
        (values[2], values[l4 + 2])     = (values[l4 + 2], values[2]);
        (keys[^2], keys[^(l4 + 1)])     = (keys[^(l4 + 1)], keys[^2]);
        (values[^2], values[^(l4 + 1)]) = (values[^(l4 + 1)], values[^2]);
        (keys[^3], keys[^(l4 + 2)])     = (keys[^(l4 + 2)], keys[^3]);
        (values[^3], values[^(l4 + 2)]) = (values[^(l4 + 2)], values[^3]);
      }
    }
  }

  /// <inheritdoc cref="PdqSort{T}"/>
  public static void PdqSort<TKey, TValue>(Span<TKey> keys, Span<TValue> values,
      in int insertion_sort_threshold = 24, in int ninther_threshold = 128,
      in int partial_insertion_sort_limit = 8, int? depth_limit = null)
      where TKey : unmanaged, IComparable<TKey>
      where TValue : unmanaged {
    int length = keys.Length;
    if (length <= 1)
      return;

    if (length < insertion_sort_threshold) {
      InsertionSort(keys, values);
      return;
    }

    int pivot_idx = length / 2;
    if (length > ninther_threshold) {
      NintherSortUnsafe(keys, values);
    } else {
      Sort3Unsafe(keys, values, pivot_idx);
    }
    bool partitioned = QuickSelectMutate(keys, values, pivot_idx, out int lt_idx, out int gt_idx);

    int l8                 = length / 8;
    bool highly_unbalanced = lt_idx < l8 || length - gt_idx < l8;

    bool lt_sort = false;
    bool gt_sort = false;
    if (highly_unbalanced) {
      depth_limit ??= BitOperations.Log2((uint)length);
      if (--depth_limit == 0) {
        HeapSort(keys, values);
        return;
      }

      BreakPatterns(keys[..lt_idx], values[..lt_idx], insertion_sort_threshold, ninther_threshold);
      BreakPatterns(keys[gt_idx..], values[gt_idx..], insertion_sort_threshold, ninther_threshold);
    } else if (partitioned) {
      lt_sort = PartialInsertionSort(keys[..lt_idx], values[..lt_idx],  //
          partial_insertion_sort_limit);
      gt_sort = PartialInsertionSort(keys[gt_idx..], values[gt_idx..],  //
          partial_insertion_sort_limit);
    }

    if (!lt_sort)
      PdqSort(keys[..lt_idx], values[..lt_idx], insertion_sort_threshold, ninther_threshold,
          partial_insertion_sort_limit, depth_limit);
    if (!gt_sort)
      PdqSort(keys[gt_idx..], values[gt_idx..], insertion_sort_threshold, ninther_threshold,
          partial_insertion_sort_limit, depth_limit);
  }
}
}
