using System;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  public static int IndexMedian3<T>(in ReadOnlySpan<T> slice)
      where T : IComparable<T> {
    ref readonly T a = ref slice[0];
    ref readonly T b = ref slice[slice.Length / 2];
    ref readonly T c = ref slice[^1];

    bool ab = a.CompareTo(b) < 0;
    bool ac = a.CompareTo(c) < 0;

    if (ab == ac) {
      bool bc = b.CompareTo(c) < 0;
      return bc == ab ? (slice.Length / 2) : (slice.Length - 1);
    }

    return 0;
  }
}
}
