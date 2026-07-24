using System;
using System.Numerics;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  public static int Product(this ReadOnlySpan<int> self) {
    int alen = self.Length;
    int vlen = Vector<int>.Count;
    int rem  = alen - vlen;

    Vector<int> acc_v = Vector<int>.One;

    int i = 0;
    for (; i <= rem; i += vlen) {
      Vector<int> self_v = self.Slice(i, vlen).ToVector();
      acc_v *= self_v;
    }
    int result = 1;
    for (; i < alen; ++i) {
      result *= self[i];
    }
    for (i = 0; i < vlen; ++i) {
      result *= acc_v[i];
    }
    return result;
  }

  public static double Product(this ReadOnlySpan<double> self) {
    int alen = self.Length;
    int vlen = Vector<double>.Count;
    int rem  = alen - vlen;

    Vector<double> acc_v = Vector<double>.One;

    int i = 0;
    for (; i <= rem; i += vlen) {
      Vector<double> self_v = self.Slice(i, vlen).ToVector();
      acc_v *= self_v;
    }
    double result = 1;
    for (; i < alen; ++i) {
      result *= self[i];
    }
    for (i = 0; i < vlen; ++i) {
      result *= acc_v[i];
    }
    return result;
  }
}
}
