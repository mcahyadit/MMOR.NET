using System;
using System.Numerics;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  public static readonly Vector<ulong> kUInt64IotaBase = ((Func<Vector<ulong>>)(() => {
    int len        = Vector<ulong>.Count;
    ulong[] result = new ulong[len];
    for (int i = 0; i < len; ++i) {
      result[i] = (ulong)i;
    }
    return new Vector<ulong>(result);
  })).Invoke();
  public static readonly Vector<ulong> kUInt64IotaInc  = new((ulong)Vector<ulong>.Count);
  public static void IotaFill(this Span<ulong> self) {
    int alen = self.Length;
    int vlen = Vector<ulong>.Count;
    int rem  = alen - vlen;

    Vector<ulong> acc_v = kUInt64IotaBase;

    int i = 0;
    for (; i <= rem; i += vlen) {
      acc_v.CopyTo(self.Slice(i, vlen));
      acc_v += kUInt64IotaInc;
    }
    for (; i < alen; ++i) {
      self[i] = (ulong)i;
    }
  }
  public static readonly Vector<int> kInt32IotaBase = ((Func<Vector<int>>)(() => {
    int len      = Vector<int>.Count;
    int[] result = new int[len];
    for (int i = 0; i < len; ++i) {
      result[i] = i;
    }
    return new Vector<int>(result);
  })).Invoke();
  public static readonly Vector<int> kInt32IotaInc  = new(Vector<int>.Count);
  public static void IotaFill(this Span<int> self) {
    int alen = self.Length;
    int vlen = Vector<int>.Count;
    int rem  = alen - vlen;

    Vector<int> acc_v = kInt32IotaBase;

    int i = 0;
    for (; i <= rem; i += vlen) {
      acc_v.CopyTo(self.Slice(i, vlen));
      acc_v += kInt32IotaInc;
    }
    for (; i < alen; ++i) {
      self[i] = i;
    }
  }

  public static readonly Vector<double> kDoubleIotaBase = ((Func<Vector<double>>)(() => {
    int len         = Vector<double>.Count;
    double[] result = new double[len];
    for (int i = 0; i < len; ++i) {
      result[i] = i;
    }
    return new Vector<double>(result);
  })).Invoke();
  public static readonly Vector<double> kDoubleIotaInc  = new(Vector<double>.Count);
  public static void IotaFill(this Span<double> self) {
    int alen = self.Length;
    int vlen = Vector<double>.Count;
    int rem  = alen - vlen;

    Vector<double> acc_v = kDoubleIotaBase;

    int i = 0;
    for (; i <= rem; i += vlen) {
      acc_v.CopyTo(self.Slice(i, vlen));
      acc_v += kDoubleIotaInc;
    }
    for (; i < alen; ++i) {
      self[i] = i;
    }
  }
}
}
