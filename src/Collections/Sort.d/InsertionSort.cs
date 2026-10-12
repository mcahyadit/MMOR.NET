using System;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  public static void InsertionSort<T>(this Span<T> slice)
      where T : unmanaged, IComparable<T> {
    int length = slice.Length;
    for (int i = 1; i < length; ++i) {
      T pivot = slice[i];
      int j   = i - 1;

      while (j >= 0 && slice[j].CompareTo(pivot) > 0) {
        slice[j + 1] = slice[j];
        --j;
      }
      slice[j + 1] = pivot;
    }
  }

  public static void InsertionSort<TKey, TValue>(Span<TKey> keys, Span<TValue> values)
      where TKey : unmanaged, IComparable<TKey>
      where TValue : unmanaged {
    int length = keys.Length;
    for (int i = 1; i < length; ++i) {
      TKey pivot   = keys[i];
      TValue p_val = values[i];
      int j        = i - 1;

      while (j >= 0 && keys[j].CompareTo(pivot) > 0) {
        keys[j + 1]   = keys[j];
        values[j + 1] = values[j];
        --j;
      }
      keys[j + 1]   = pivot;
      values[j + 1] = p_val;
    }
  }
}
}
