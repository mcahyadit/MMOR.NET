using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  public static int Product(this ReadOnlySpan<int> self) {
    int length = self.Length;
    int start;

    int result;
    if (Vector.IsHardwareAccelerated && length > 2 * Vector<ulong>.Count) {
      start = length - (length % Vector<int>.Count);

      Vector<int> result_epi             = Vector<int>.One;
      ReadOnlySpan<Vector<int>> self_epi = MemoryMarshal.Cast<int, Vector<int>>(self[..start]);
      int length_epi                     = self_epi.Length;
      for (int i = 0; i < length_epi; ++i) {
        result_epi *= self_epi[i];
      }
      result = result_epi[0];
      for (int i = 1; i < Vector<ulong>.Count; ++i) {
        result *= result_epi[i];
      }
    } else {
      start  = 1;
      result = self[0];
    }
    for (int i = start; i < length; ++i) {
      result *= self[i];
    }
    return result;
  }

  public static double Product(this ReadOnlySpan<double> self) {
    int length = self.Length;
    int start;

    double result;
    if (Vector.IsHardwareAccelerated && length > 2 * Vector<ulong>.Count) {
      start = length - (length % Vector<double>.Count);

      Vector<double> result_epi = Vector<double>.One;
      ReadOnlySpan<Vector<double>> self_epi =
          MemoryMarshal.Cast<double, Vector<double>>(self[..start]);
      int length_epi = self_epi.Length;
      for (int i = 0; i < length_epi; ++i) {
        result_epi *= self_epi[i];
      }
      result = result_epi[0];
      for (int i = 1; i < Vector<ulong>.Count; ++i) {
        result *= result_epi[i];
      }
    } else {
      start  = 1;
      result = self[0];
    }
    for (int i = start; i < length; ++i) {
      result *= self[i];
    }
    return result;
  }
}
}
