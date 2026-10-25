using System;
using System.Collections.Generic;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  public static int IndexMedian3<T, TComparer>(in ReadOnlySpan<T> slice, in TComparer cmp)
      where TComparer : IComparer<T> {
    ref readonly T a = ref slice[0];
    ref readonly T b = ref slice[slice.Length / 2];
    ref readonly T c = ref slice[^1];

    bool ab = cmp.Compare(a, b) < 0;
    bool ac = cmp.Compare(a, c) < 0;

    if (ab == ac) {
      bool bc = cmp.Compare(b, c) < 0;
      return bc == ab ? (slice.Length / 2) : (slice.Length - 1);
    }

    return 0;
  }
}
}
