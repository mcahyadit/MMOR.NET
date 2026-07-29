using System;
using System.Numerics;
using MMOR.NET.Mathematics;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  public static int CountEqual(this ReadOnlySpan<ulong> self, ulong value) {
    int alen = self.Length;
    int vlen = Vector<ulong>.Count;
    int rem  = alen - vlen;

    Vector<ulong> com_v = new(value);
    Vector<ulong> acc_v = Vector<ulong>.Zero;

    int i = 0;
    for (; i <= rem; i += vlen) {
      Vector<ulong> self_v = self.Slice(i, vlen).ToVector();
      Vector<ulong> eq_vec = Vector.Equals(self_v, com_v);
      Vector<ulong> norm_v = Vector.BitwiseAnd(eq_vec, Vector<ulong>.One);
      acc_v += norm_v;
    }
    int result = (int)Vector.Dot(acc_v, Vector<ulong>.One);
    for (; i < alen; ++i) {
      if (self[i] == value)
        ++result;
    }
    return result;
  }

  public static int CountEqual(this ReadOnlySpan<int> self, int value) {
    int alen = self.Length;
    int vlen = Vector<int>.Count;
    int rem  = alen - vlen;

    Vector<int> com_v = new(value);
    Vector<int> acc_v = Vector<int>.Zero;

    int i = 0;
    for (; i <= rem; i += vlen) {
      Vector<int> self_v = self.Slice(i, vlen).ToVector();
      Vector<int> eq_vec = Vector.Equals(self_v, com_v);
      Vector<int> norm_v = Vector.BitwiseAnd(eq_vec, Vector<int>.One);
      acc_v += norm_v;
    }
    int result = Vector.Dot(acc_v, Vector<int>.One);
    for (; i < alen; ++i) {
      if (self[i] == value)
        ++result;
    }
    return result;
  }

  public static Vector<double> kAbsToleranceDV = new(MathExt.kAbsToleranceD);
  public static int CountEqual(this ReadOnlySpan<double> self, double value) {
    int alen = self.Length;
    int vlen = Vector<double>.Count;
    int rem  = alen - vlen;

    Vector<double> com_v = new(value);
    Vector<long> acc_v   = Vector<long>.Zero;

    int i = 0;
    for (; i <= rem; i += vlen) {
      Vector<double> self_v = self.Slice(i, vlen).ToVector();
      Vector<double> abs_df = Vector.Abs(self_v - com_v);
      Vector<long> eq_vec   = Vector.LessThan(abs_df, kAbsToleranceDV);
      Vector<long> norm_v   = Vector.BitwiseAnd(eq_vec, Vector<long>.One);
      acc_v += norm_v;
    }
    int result = (int)Vector.Dot(acc_v, Vector<long>.One);
    for (; i < alen; ++i) {
      if (MathExt.Approximately(self[i], value))
        ++result;
    }
    return result;
  }
}
}
