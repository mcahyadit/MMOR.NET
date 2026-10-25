using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  /**
   * <summary>
   *  Stable Sort <paramref name="slice"/> in ascending order using binary insertion sort. <br/>
   *  <i>O(n log n)</i> comparisons. <i>O(n^2)</i> moves worst case. <br/>
   * </summary>
   */
  public static void BinaryInsertionSort<T, TComparer>(this Span<T> slice, in TComparer cmp)
      where T : unmanaged
      where TComparer : IComparer<T> {
    int length = slice.Length;
    for (int c_idx = 1; c_idx < length; ++c_idx) {
      T pivot = slice[c_idx];

      int i_idx;
      if (cmp.Compare(pivot, slice[c_idx - 1]) >= 0)
        continue;
      i_idx = TailBinarySearch(slice[..c_idx], pivot, cmp);

      slice[i_idx..c_idx].CopyTo(slice.Slice(i_idx + 1, c_idx - i_idx));
      slice[i_idx] = pivot;
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void BinaryInsertionSort<T>(this Span<T> slice)
      where T : unmanaged, IComparable<T> => BinaryInsertionSort(slice, Comparer<T>.Default);

  /// <inheritdoc cref="BinaryInsertionSort{T}(Span{T})"/>
  public static void BinaryInsertionSort<TKey, TValue, TComparer>(Span<TKey> keys,
      Span<TValue> values, in TComparer cmp)
      where TKey : unmanaged
      where TValue : unmanaged
      where TComparer : IComparer<TKey> {
    int length = keys.Length;
    for (int c_idx = 1; c_idx < length; ++c_idx) {
      TKey pivot   = keys[c_idx];
      TValue p_val = values[c_idx];

      int i_idx;
      if (cmp.Compare(pivot, keys[c_idx - 1]) >= 0)
        continue;
      i_idx = TailBinarySearch(keys[..c_idx], pivot, cmp);

      keys[i_idx..c_idx].CopyTo(keys.Slice(i_idx + 1, c_idx - i_idx));
      values[i_idx..c_idx].CopyTo(values.Slice(i_idx + 1, c_idx - i_idx));
      keys[i_idx]   = pivot;
      values[i_idx] = p_val;
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void BinaryInsertionSort<TKey, TValue>(Span<TKey> keys, Span<TValue> values)
      where TKey : unmanaged
      where TValue : unmanaged => BinaryInsertionSort(keys, values, Comparer<TKey>.Default);
}
}
