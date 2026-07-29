using System;
using System.Numerics;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  /**
   * <summary>
   *  Returns the minimum element of an array, excluding <c>0</c>. <br/>
   *  Will still return <c>0</c> if array only have <c>0</c>.
   * </summary>
   */
  public static ulong MinElementNonZero(this ReadOnlySpan<ulong> self) {
    if (self.IsEmpty)
      throw new ArgumentException("[ERROR]: passed span is empty.");
    int alen = self.Length;
    int vlen = Vector<ulong>.Count;

    int i;
    ulong result;
    if (Vector.IsHardwareAccelerated && alen > 2 * vlen) {
      int rem             = alen - vlen;
      Vector<ulong> acc_v = self[..vlen].ToVector() - Vector<ulong>.One;
      i                   = vlen;
      for (; i <= rem; i += vlen) {
        acc_v = Vector.Min(acc_v, self.Slice(i, vlen).ToVector() - Vector<ulong>.One);
      }
      result = acc_v[0];
      for (int j = 1; j < vlen; ++j) {
        result = Math.Min(result, acc_v[j]);
      }
    } else {
      result = self[0] - 1;
      i      = 1;
    }
    for (; i < alen; ++i) {
      result = Math.Min(result, self[i] - 1);
    }
    return result + 1;
  }
}
}
