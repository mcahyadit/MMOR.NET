using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  /**
   * <summary>
   *  Unstable Sort <paramref name="slice"/> in ascending order using heap sort. <br/>
   *  <i>O(n log n)</i> all cases. <br/>
   * </summary>
   */
  public static void HeapSort<T, TComparer>(this Span<T> slice, in TComparer cmp)
      where T : unmanaged
      where TComparer : IComparer<T> {
    int length = slice.Length;
    if (length <= 1)
      return;

    int last_parent = length / 2 - 1;
    for (int i = last_parent; i >= 0; --i) {
      HeapSortMutate(slice, cmp, i);
    }

    // Slice Down
    for (int i = length - 1; i > 0; --i) {
      (slice[i], slice[0]) = (slice[0], slice[i]);
      HeapSortMutate(slice[..i], cmp, 0);
    }
  }

  public static void HeapSortMutate<T, TComparer>(Span<T> slice, in TComparer cmp, int m_idx)
      where T : unmanaged
      where TComparer : IComparer<T> {
    int length = slice.Length;
    int x_idx  = m_idx;

    int l_idx = 2 * m_idx + 1;
    int r_idx = l_idx + 1;

    if (l_idx < length && cmp.Compare(slice[l_idx], slice[x_idx]) > 0) {
      x_idx = l_idx;
    }
    if (r_idx < length && cmp.Compare(slice[r_idx], slice[x_idx]) > 0) {
      x_idx = r_idx;
    }

    if (x_idx != m_idx) {
      (slice[m_idx], slice[x_idx]) = (slice[x_idx], slice[m_idx]);
      HeapSortMutate(slice, cmp, x_idx);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void HeapSort<T>(this Span<T> slice)
      where T : unmanaged, IComparable<T> => HeapSort(slice, Comparer<T>.Default);
}
}
