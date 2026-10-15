using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  /// <inheritdoc cref="MergeSort{T}(Span{T})"/>
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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void MergeSortMutate<TKey, TValue>(in Span<TKey> keys, in Span<TValue> values,
      in int midx)
      where TKey : unmanaged, IComparable<TKey>
      where TValue : unmanaged {
    int length      = keys.Length;
    int l_len       = midx;
    TKey[]? l_mem   = length > kHeapThreshold ? ArrayPool<TKey>.Shared.Rent(l_len) : null;
    TValue[]? v_mem = length > kHeapThreshold ? ArrayPool<TValue>.Shared.Rent(l_len) : null;
    try {
      Span<TKey> l_buff   = l_mem is null ? stackalloc TKey[l_len] : l_mem.AsSpan(0, l_len);
      Span<TValue> v_buff = v_mem is null ? stackalloc TValue[l_len] : v_mem.AsSpan(0, l_len);
      keys[..midx].CopyTo(l_buff);
      values[..midx].CopyTo(v_buff);

      int idx   = 0;
      int l_idx = 0;
      int r_idx = midx;
      for (; l_idx < l_len && r_idx < length; ++idx) {
        if (l_buff[l_idx].CompareTo(keys[r_idx]) <= 0) {
          keys[idx]   = l_buff[l_idx];
          values[idx] = v_buff[l_idx];
          l_idx++;
        } else {
          keys[idx]   = keys[r_idx];
          values[idx] = values[r_idx];
          r_idx++;
        }
      }
      for (; l_idx < l_len; ++l_idx, ++idx) {
        keys[idx]   = l_buff[l_idx];
        values[idx] = v_buff[l_idx];
      }
    } finally {
      if (l_mem is not null) {
        ArrayPool<TKey>.Shared.Return(l_mem, true);
        ArrayPool<TValue>.Shared.Return(v_mem!, true);
      }
    }
  }
}
}
