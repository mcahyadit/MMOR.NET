using System;
using System.Buffers;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  public static void MergeSort<T>(this Span<T> slice)
      where T : unmanaged, IComparable<T> {
    int length = slice.Length;
    if (length <= 1)
      return;

    int midx = length / 2;
    MergeSort(slice[..midx]);
    MergeSort(slice[midx..]);
    MergeSortMutate(slice, midx);
  }

  public static void MergeSortMutate<T>(Span<T> slice, int midx)
      where T : unmanaged, IComparable<T> {
    Span<T> l_slice = slice[..midx];
    Span<T> r_slice = slice[midx..];

    int length = slice.Length;
    int l_len  = midx;
    int r_len  = length - midx;
    T[]? l_mem = length > kHeapThreshold ? ArrayPool<T>.Shared.Rent(l_len) : null;
    T[]? r_mem = length > kHeapThreshold ? ArrayPool<T>.Shared.Rent(r_len) : null;
    try {
      Span<T> l_buffer = l_mem is null ? stackalloc T[l_len] : l_mem.AsSpan(0, l_len);
      Span<T> r_buffer = r_mem is null ? stackalloc T[r_len] : r_mem.AsSpan(0, r_len);
      l_slice.CopyTo(l_buffer);
      r_slice.CopyTo(r_buffer);

      int i    = 0;
      int lidx = 0;
      int ridx = 0;
      for (; lidx < l_len && ridx < r_len; ++i) {
        int cmp = l_buffer[lidx].CompareTo(r_buffer[ridx]);
        if (cmp <= 0) {
          slice[i] = l_buffer[lidx++];
        } else {
          slice[i] = r_buffer[ridx++];
        }
      }
      for (; lidx < l_len; ++i, ++lidx) {
        slice[i] = l_buffer[lidx];
      }
      for (; ridx < r_len; ++i, ++ridx) {
        slice[i] = r_buffer[ridx];
      }

    } finally {
      if (l_mem is not null) {
        ArrayPool<T>.Shared.Return(l_mem, true);
        ArrayPool<T>.Shared.Return(r_mem!, true);
      }
    }
  }

  public static void MergeSort<TKey, TValue>(Span<TKey> keys, Span<TValue> values)
      where TKey : unmanaged, IComparable<TKey>
      where TValue : unmanaged {
    int length = keys.Length;
    if (length <= 1)
      return;

    int midx = length / 2;
    MergeSort(keys[..midx], values[..midx]);
    MergeSort(keys[midx..], values[midx..]);
    MergeSortMutate(keys, values, midx);
  }

  public static void MergeSortMutate<TKey, TValue>(Span<TKey> keys, Span<TValue> values, int midx)
      where TKey : unmanaged, IComparable<TKey>
      where TValue : unmanaged {
    Span<TKey> l_slice = keys[..midx];
    Span<TKey> r_slice = keys[midx..];

    int length    = keys.Length;
    int l_len     = midx;
    int r_len     = length - midx;
    TKey[]? l_mem = length > kHeapThreshold ? ArrayPool<TKey>.Shared.Rent(l_len) : null;
    TKey[]? r_mem = length > kHeapThreshold ? ArrayPool<TKey>.Shared.Rent(r_len) : null;
    try {
      Span<TKey> l_buffer = l_mem is null ? stackalloc TKey[l_len] : l_mem.AsSpan(0, l_len);
      Span<TKey> r_buffer = r_mem is null ? stackalloc TKey[r_len] : r_mem.AsSpan(0, r_len);
      l_slice.CopyTo(l_buffer);
      r_slice.CopyTo(r_buffer);

      int i    = 0;
      int lidx = 0;
      int ridx = 0;
      for (; lidx < l_len && ridx < r_len; ++i) {
        int cmp = l_buffer[lidx].CompareTo(r_buffer[ridx]);
        if (cmp <= 0) {
          keys[i] = l_buffer[lidx++];
        } else {
          keys[i] = r_buffer[ridx++];
        }
      }
      for (; lidx < l_len; ++i, ++lidx) {
        keys[i] = l_buffer[lidx];
      }
      for (; ridx < r_len; ++i, ++ridx) {
        keys[i] = r_buffer[ridx];
      }

    } finally {
      if (l_mem is not null) {
        ArrayPool<TKey>.Shared.Return(l_mem, true);
        ArrayPool<TKey>.Shared.Return(r_mem!, true);
      }
    }
  }
}
}
