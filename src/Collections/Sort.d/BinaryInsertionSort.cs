using System;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  /**
   * <summary>
   *  Stable Sort <paramref name="slice"/> in ascending order using binary insertion sort. <br/>
   *  <i>O(n log n)</i> comparisons. <i>O(n^2)</i> moves worst case. <br/>
   * </summary>
   */
  public static void BinaryInsertionSort<T>(this Span<T> slice)
      where T : unmanaged, IComparable<T> {
    int length = slice.Length;
    for (int c_idx = 1; c_idx < length; ++c_idx) {
      T pivot = slice[c_idx];

      int i_idx;
      if (pivot.CompareTo(slice[c_idx - 1]) >= 0)
        continue;
      i_idx = TailBinarySearch(slice[..c_idx], pivot);

      slice[i_idx..c_idx].CopyTo(slice.Slice(i_idx + 1, c_idx - i_idx));
      slice[i_idx] = pivot;
    }
  }

  /// <inheritdoc cref="BinaryInsertionSort{T}(Span{T})"/>
  public static void BinaryInsertionSort<TKey, TValue>(Span<TKey> keys, Span<TValue> values)
      where TKey : unmanaged, IComparable<TKey>
      where TValue : unmanaged {
    int length = keys.Length;
    for (int c_idx = 1; c_idx < length; ++c_idx) {
      TKey pivot   = keys[c_idx];
      TValue p_val = values[c_idx];

      int i_idx;
      if (pivot.CompareTo(keys[c_idx - 1]) >= 0)
        continue;
      i_idx = TailBinarySearch(keys[..c_idx], pivot);

      keys[i_idx..c_idx].CopyTo(keys.Slice(i_idx + 1, c_idx - i_idx));
      values[i_idx..c_idx].CopyTo(values.Slice(i_idx + 1, c_idx - i_idx));
      keys[i_idx]   = pivot;
      values[i_idx] = p_val;
    }
  }
}
}
