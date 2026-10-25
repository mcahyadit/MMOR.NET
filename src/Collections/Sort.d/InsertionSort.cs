using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  /**
   * <summary>
   *  Stable Sort <paramref name="slice"/> in ascending order using insertion sort. <br/>
   *  <i>O(n)</i> best case. <i>O(n^2)</i> average &amp; worst case. <br/>
   * </summary>
   */
  public static void InsertionSort<T, TComparer>(this Span<T> slice, in TComparer cmp)
      where T : unmanaged
      where TComparer : IComparer<T> {
    int length = slice.Length;
    for (int i = 1; i < length; ++i) {
      T pivot = slice[i];
      int j   = i - 1;

      while (j >= 0 && cmp.Compare(slice[j], pivot) > 0) {
        slice[j + 1] = slice[j];
        --j;
      }
      slice[j + 1] = pivot;
    }
  }

  /// <inheritdoc cref="InsertionSort{T}(Span{T})"/>
  public static void InsertionSort<TKey, TValue, TComparer>(Span<TKey> keys, Span<TValue> values,
      in TComparer cmp)
      where TKey : unmanaged
      where TValue : unmanaged
      where TComparer : IComparer<TKey> {
    int length = keys.Length;
    for (int i = 1; i < length; ++i) {
      TKey pivot   = keys[i];
      TValue p_val = values[i];
      int j        = i - 1;

      while (j >= 0 && cmp.Compare(keys[j], pivot) > 0) {
        keys[j + 1]   = keys[j];
        values[j + 1] = values[j];
        --j;
      }
      keys[j + 1]   = pivot;
      values[j + 1] = p_val;
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void InsertionSort<T>(this Span<T> slice)
      where T : unmanaged, IComparable<T> => InsertionSort(slice, Comparer<T>.Default);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void InsertionSort<TKey, TValue>(Span<TKey> keys, Span<TValue> values)
      where TKey : unmanaged
      where TValue : unmanaged => InsertionSort(keys, values, Comparer<TKey>.Default);
}
}
