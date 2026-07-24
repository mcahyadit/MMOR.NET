using System;
using System.Numerics;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  public static ulong Sum(this ReadOnlySpan<ulong> self) {
    int alen = self.Length;
    int vlen = Vector<ulong>.Count;
    int rem  = alen - vlen;

    Vector<ulong> acc_v = Vector<ulong>.Zero;

    int i = 0;
    for (; i <= rem; i += vlen) {
      Vector<ulong> self_v = self.Slice(i, vlen).ToVector();
      acc_v += self_v;
    }
    ulong result = Vector.Dot(acc_v, Vector<ulong>.One);
    for (; i < alen; ++i) {
      result += self[i];
    }
    return result;
  }

  public static int Sum(this ReadOnlySpan<int> self) {
    int alen = self.Length;
    int vlen = Vector<int>.Count;
    int rem  = alen - vlen;

    Vector<int> acc_v = Vector<int>.Zero;

    int i = 0;
    for (; i <= rem; i += vlen) {
      Vector<int> self_v = self.Slice(i, vlen).ToVector();
      acc_v += self_v;
    }
    int result = Vector.Dot(acc_v, Vector<int>.One);
    for (; i < alen; ++i) {
      result += self[i];
    }
    return result;
  }

  public static double Sum(this ReadOnlySpan<double> self) {
    int alen = self.Length;
    int vlen = Vector<double>.Count;
    int rem  = alen - vlen;

    Vector<double> acc_v = Vector<double>.Zero;

    int i = 0;
    for (; i <= rem; i += vlen) {
      Vector<double> self_v = self.Slice(i, vlen).ToVector();
      acc_v += self_v;
    }
    double result = Vector.Dot(acc_v, Vector<double>.One);
    for (; i < alen; ++i) {
      result += self[i];
    }
    return result;
  }
}
}
