using System;
using System.Numerics;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  public readonly struct PdqSortConfig {
    public readonly int insertion_sort_threshold { get; init; }
    public readonly int ninther_threshold { get; init; }
    public readonly int partial_insertion_sort_limit { get; init; }
  }

  public static readonly PdqSortConfig kDefaultPdqSortConfig = new() {
    insertion_sort_threshold     = 24,
    ninther_threshold            = 128,
    partial_insertion_sort_limit = 8,
  };

  /**
   * <summary>
   *  Internal function for <see cref="PdqSort{T}(Span{T}, PdqSortConfig?, int?)"/>. <br/>
   *  Use at your own risk.
   * </summary>
   */
  public static void OrlpNintherSortUnsafe<T>(Span<T> slice)
      where T : unmanaged, IComparable<T> {
    int length = slice.Length;
    // Cheat from orlp definition
    // From: ABC...BAC...CBA
    //   To: ABC...ABC...CBA
    int midx = length / 2 - 1;
    for (int i = 0; i < 3; ++i) {
      Span<T> group = slice[i..(length - i)];
      // We don't do `midx + i` because the Slice already does it
      Sort3Unsafe(group, midx);
    }
    Sort3Unsafe(slice.Slice(midx, 3), 1);
  }

  public static bool PartialInsertionSort<T>(Span<T> slice, int limit)
      where T : unmanaged, IComparable<T> {
    int length = slice.Length;
    for (int c_idx = 1; c_idx < length; ++c_idx) {
      T pivot   = slice[c_idx];
      int s_idx = c_idx - 1;

      while (s_idx >= 0 && slice[s_idx].CompareTo(pivot) > 0) {
        slice[s_idx + 1] = slice[s_idx];
        --s_idx;
      }
      slice[s_idx + 1] = pivot;

      limit -= c_idx - s_idx - 1;
      if (limit <= 0)
        return false;
    }
    return true;
  }

  public static void BreakPatterns<T>(Span<T> slice, int t1, int t2) {
    if (slice.Length >= t1) {
      int l4                  = slice.Length / 4;
      (slice[0], slice[l4])   = (slice[l4], slice[0]);
      (slice[^1], slice[^l4]) = (slice[^l4], slice[^1]);

      if (slice.Length > t2) {
        (slice[1], slice[l4 + 1])     = (slice[l4 + 1], slice[1]);
        (slice[2], slice[l4 + 2])     = (slice[l4 + 2], slice[2]);
        (slice[^2], slice[^(l4 + 1)]) = (slice[^(l4 + 1)], slice[^2]);
        (slice[^3], slice[^(l4 + 2)]) = (slice[^(l4 + 2)], slice[^3]);
      }
    }
  }

  public static void PdqSort<T>(this Span<T> slice, PdqSortConfig? config = null,
      int? depth_limit = null)
      where T : unmanaged, IComparable<T> {
    PdqSortConfig cfg = config ?? kDefaultPdqSortConfig;
    int length        = slice.Length;
    if (length <= 1)
      return;

    if (length < cfg.insertion_sort_threshold) {
      InsertionSort(slice);
      return;
    }

    int pivot_idx = length / 2;
    if (length > cfg.ninther_threshold) {
      OrlpNintherSortUnsafe(slice);
    } else {
      Sort3Unsafe(slice, pivot_idx);
    }
    bool partitioned = QuickSortMutate(slice, pivot_idx, out int lt_idx, out int gt_idx);

    int l8                 = length / 8;
    Span<T> lt_span        = slice[..lt_idx];
    Span<T> gt_span        = slice[gt_idx..];
    bool highly_unbalanced = lt_span.Length < l8 || gt_span.Length < l8;

    bool lt_sort = false;
    bool gt_sort = false;
    if (highly_unbalanced) {
      depth_limit ??= BitOperations.Log2((uint)length);
      if (--depth_limit == 0) {
        HeapSort(slice);
        return;
      }

      BreakPatterns(lt_span, cfg.insertion_sort_threshold, cfg.ninther_threshold);
      BreakPatterns(gt_span, cfg.insertion_sort_threshold, cfg.ninther_threshold);
    } else if (partitioned) {
      lt_sort = PartialInsertionSort(slice[..lt_idx], cfg.partial_insertion_sort_limit);
      gt_sort = PartialInsertionSort(slice[gt_idx..], cfg.partial_insertion_sort_limit);
    }

    if (!lt_sort)
      PdqSort(slice[..lt_idx], cfg, depth_limit);
    if (!gt_sort)
      PdqSort(slice[gt_idx..], cfg, depth_limit);
  }

  /// <inheritdoc cref="OrlpNintherSortUnsafe{T}(Span{T})"/>
  public static void NintherSortUnsafe<TKey, TValue>(Span<TKey> keys, Span<TValue> values)
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

  public static bool PartialInsertionSort<TKey, TValue>(Span<TKey> keys, Span<TValue> values,
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

  public static void BreakPatterns<TKey, TValue>(Span<TKey> keys, Span<TValue> values, int t1,
      int t2)
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

  public static void PdqSort<TKey, TValue>(Span<TKey> keys, Span<TValue> values,
      PdqSortConfig? config = null, int? depth_limit = null)
      where TKey : unmanaged, IComparable<TKey>
      where TValue : unmanaged {
    PdqSortConfig cfg = config ?? kDefaultPdqSortConfig;
    int length        = keys.Length;
    if (length <= 1)
      return;

    if (length < cfg.insertion_sort_threshold) {
      InsertionSort(keys, values);
      return;
    }

    int pivot_idx = length / 2;
    if (length > cfg.ninther_threshold) {
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

      BreakPatterns(keys[..lt_idx], values[..lt_idx], cfg.insertion_sort_threshold,
          cfg.ninther_threshold);
      BreakPatterns(keys[gt_idx..], values[gt_idx..], cfg.insertion_sort_threshold,
          cfg.ninther_threshold);
    } else if (partitioned) {
      lt_sort = PartialInsertionSort(keys[..lt_idx], values[..lt_idx],  //
          cfg.partial_insertion_sort_limit);
      gt_sort = PartialInsertionSort(keys[gt_idx..], values[gt_idx..],  //
          cfg.partial_insertion_sort_limit);
    }

    if (!lt_sort)
      PdqSort(keys[..lt_idx], values[..lt_idx], cfg, depth_limit);
    if (!gt_sort)
      PdqSort(keys[gt_idx..], values[gt_idx..], cfg, depth_limit);
  }
}
}
