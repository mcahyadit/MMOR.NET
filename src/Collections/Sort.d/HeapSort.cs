using System;
using System.Collections.Generic;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  /**
   * <summary>
   *  Unstable Sort <paramref name="slice"/> in ascending order using heap sort. <br/>
   *  <i>O(n log n)</i> all cases. <br/>
   * </summary>
   */
  public static void HeapSort<T>(this Span<T> slice)
      where T : unmanaged, IComparable<T> {
    int length = slice.Length;
    if (length <= 1)
      return;

    int last_parent = length / 2 - 1;
    for (int i = last_parent; i >= 0; --i) {
      HeapSortMutate(slice, i);
    }

    // Slice Down
    for (int i = length - 1; i > 0; --i) {
      (slice[i], slice[0]) = (slice[0], slice[i]);
      HeapSortMutate(slice[..i], 0);
    }
  }

  public static void HeapSortMutate<T>(Span<T> slice, int m_idx)
      where T : unmanaged, IComparable<T> {
    int length = slice.Length;
    int x_idx  = m_idx;

    int l_idx = 2 * m_idx + 1;
    int r_idx = l_idx + 1;

    if (l_idx < length && Comparer<T>.Default.Compare(slice[l_idx], slice[x_idx]) > 0) {
      x_idx = l_idx;
    }
    if (r_idx < length && Comparer<T>.Default.Compare(slice[r_idx], slice[x_idx]) > 0) {
      x_idx = r_idx;
    }

    if (x_idx != m_idx) {
      (slice[m_idx], slice[x_idx]) = (slice[x_idx], slice[m_idx]);
      HeapSortMutate(slice, x_idx);
    }
  }

  /// <inheritdoc cref="HeapSort{T}(Span{T})"/>
  public static void HeapSort<TKey, TValue>(Span<TKey> keys, Span<TValue> values)
      where TKey : unmanaged, IComparable<TKey>
      where TValue : unmanaged {
    int length = keys.Length;
    if (length <= 1)
      return;

    int last_parent = length / 2 - 1;
    for (int i = last_parent; i >= 0; --i) {
      HeapSortMutate(keys, values, i);
    }

    // Slice Down
    for (int i = length - 1; i > 0; --i) {
      (keys[i], keys[0])     = (keys[0], keys[i]);
      (values[i], values[0]) = (values[0], values[i]);
      HeapSortMutate(keys[..i], values[..i], 0);
    }
  }

  public static void HeapSortMutate<TKey, TValue>(Span<TKey> keys, Span<TValue> values, int m_idx)
      where TKey : unmanaged, IComparable<TKey>
      where TValue : unmanaged {
    int length = keys.Length;
    int x_idx  = m_idx;

    int l_idx = 2 * m_idx + 1;
    int r_idx = l_idx + 1;

    if (l_idx < length && Comparer<TKey>.Default.Compare(keys[l_idx], keys[x_idx]) > 0) {
      x_idx = l_idx;
    }
    if (r_idx < length && Comparer<TKey>.Default.Compare(keys[r_idx], keys[x_idx]) > 0) {
      x_idx = r_idx;
    }

    if (x_idx != m_idx) {
      (keys[m_idx], keys[x_idx])     = (keys[x_idx], keys[m_idx]);
      (values[m_idx], values[x_idx]) = (values[x_idx], values[m_idx]);
      HeapSortMutate(keys, values, x_idx);
    }
  }
}
}
