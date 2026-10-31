using System;
using System.Numerics;
using System.Runtime.InteropServices;

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
    int length = self.Length;
    int start  = 0;

    if (Vector.IsHardwareAccelerated) {
      start = length - (length % Vector<ulong>.Count);
      if (start == 0)
        goto scalar_tail;

      Vector<ulong> curr_epi       = kUInt64IotaBase;
      Span<Vector<ulong>> self_epi = MemoryMarshal.Cast<ulong, Vector<ulong>>(self[..start]);
      int length_epi               = self_epi.Length;
      for (int i = 0; i < length_epi; ++i) {
        self_epi[i] = curr_epi;
        curr_epi += kUInt64IotaBase;
      }
    }
  scalar_tail:
    for (int i = start; i < length; ++i) {
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
    int length = self.Length;
    int start  = 0;

    if (Vector.IsHardwareAccelerated) {
      start = length - (length % Vector<int>.Count);
      if (start == 0)
        goto scalar_tail;

      Vector<int> curr_epi       = kInt32IotaBase;
      Span<Vector<int>> self_epi = MemoryMarshal.Cast<int, Vector<int>>(self[..start]);
      int length_epi             = self_epi.Length;
      for (int i = 0; i < length_epi; ++i) {
        self_epi[i] = curr_epi;
        curr_epi += kInt32IotaBase;
      }
    }
  scalar_tail:
    for (int i = start; i < length; ++i) {
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
    int length = self.Length;
    int start  = 0;

    if (Vector.IsHardwareAccelerated) {
      start = length - (length % Vector<double>.Count);
      if (start == 0)
        goto scalar_tail;

      Vector<double> curr_epi       = kDoubleIotaBase;
      Span<Vector<double>> self_epi = MemoryMarshal.Cast<double, Vector<double>>(self[..start]);
      int length_epi                = self_epi.Length;
      for (int i = 0; i < length_epi; ++i) {
        self_epi[i] = curr_epi;
        curr_epi += kDoubleIotaBase;
      }
    }
  scalar_tail:
    for (int i = start; i < length; ++i) {
      self[i] = i;
    }
  }
}
}
