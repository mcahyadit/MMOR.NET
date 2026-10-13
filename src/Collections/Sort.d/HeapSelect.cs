using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  public static void HeapSelectMutate<T>(Span<T> slice, int m_idx)
      where T : unmanaged, IComparable<T> {
    int length = slice.Length;
    int x_idx  = m_idx;

    int l_idx = 2 * m_idx + 1;
    int r_idx = l_idx + 1;

    if (l_idx < length && slice[l_idx].CompareTo(slice[x_idx]) < 0) {
      x_idx = l_idx;
    }
    if (r_idx < length && slice[r_idx].CompareTo(slice[x_idx]) < 0) {
      x_idx = r_idx;
    }

    if (x_idx != m_idx) {
      (slice[m_idx], slice[x_idx]) = (slice[x_idx], slice[m_idx]);
      HeapSelectMutate(slice, x_idx);
    }
  }

  /**
   * <summary>
   *  Return the <paramref name="k"/>th (1-Indexed) smallest value
   *  in unordered <paramref name="slice"/>. <br/>
   *  <i>O(n)</i> average case. <i>O(n^2)</i> worst case. <br/>
   * </summary>
   */
  [Pure]
  public static T HeapSelect<T>(this ReadOnlySpan<T> slice, int k)
      where T : unmanaged, IComparable<T> {
    int length = slice.Length;
    if ((uint)(k - 1) >= length)
      throw new ArgumentOutOfRangeException(
          $"[ERROR]: Trying to HeapSelect {k}th element in an array of Length: {length}");
    T[]? mut_data = length > kHeapThreshold ? ArrayPool<T>.Shared.Rent(length) : null;
    try {
      Span<T> mut_buffer = mut_data is null ? stackalloc T[length] : mut_data.AsSpan(0, length);
      slice.CopyTo(mut_buffer);
      return HeapSelectMutable(mut_buffer, k);
    } finally {
      if (mut_data is not null) {
        ArrayPool<T>.Shared.Return(mut_data, true);
      }
    }
  }

  public static T HeapSelectMutable<T>(this Span<T> slice, int k)
      where T : unmanaged, IComparable<T> {
    int length = slice.Length;
    if ((uint)(k - 1) >= length)
      throw new ArgumentOutOfRangeException(
          $"[ERROR]: Trying to HeapSelect {k}th element in an array of Length: {length}");
    if (length == 1)
      return slice[0];
    if (k == 1)
      return MinElement<T>(slice);
    if (length == k)
      return MaxElement<T>(slice);

    int last_parent = length / 2 - 1;
    for (int i = last_parent; i >= 0; --i) {
      HeapSelectMutate(slice, i);
    }

    // Slice Down
    for (int i = k; i > 0; --i) {
      (slice[^i], slice[0]) = (slice[0], slice[^i]);
      HeapSelectMutate(slice[..^ i], 0);
    }

    return slice[length - k];
  }

  /// <inheritdoc cref="HeapSelect{T}(ReadOnlySpan{T}, int)"/>
  [Pure]
  public static T HeapSelect<T>(this ICollection<T> collection, int k)
      where T : unmanaged, IComparable<T> {
    int length = collection.Count;
    if ((uint)(k - 1) >= length)
      throw new ArgumentOutOfRangeException(
          $"[ERROR]: Trying to HeapSelect {k}th element in an array of Length: {length}");
    T[] mut_data = ArrayPool<T>.Shared.Rent(length);
    try {
      collection.CopyTo(mut_data, 0);
      T result = HeapSelectMutable(mut_data.AsSpan(0, length), k);
      return result;
    } finally {
      ArrayPool<T>.Shared.Return(mut_data, true);
    }
  }
}
}
