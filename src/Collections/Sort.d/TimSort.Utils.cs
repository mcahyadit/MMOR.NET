using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using MMOR.NET.Bits;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int ComputeMinRun(in int length) {
    // Bit Manip for getting Power Of 2 until [32, 64)
    int log2    = BitOperations.Log2((uint)length);
    int idx     = Math.Max(0, log2 - 5);
    int min_run = (length >> idx) & 63;
    int add     = BitOps.Bzhi32((uint)length, idx) != 0 ? 1 : 0;
    return Math.Max(32, min_run + add);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int CountRun<T, TComparer>(in ReadOnlySpan<T> slice, in TComparer cmp)
      where TComparer : IComparer<T> {
    int length = slice.Length;
    if (length <= 1)
      return length;

    int run = 2;
    if (cmp.Compare(slice[1], slice[0]) >= 0) {
      while (run < length && cmp.Compare(slice[run], slice[run - 1]) >= 0) ++run;
      return run;
    } else {
      while (run < length && cmp.Compare(slice[run], slice[run - 1]) < 0) ++run;
      return -run;
    }
  }
  //
  // [MethodImpl(MethodImplOptions.AggressiveInlining)]
  // public static int CountRun<T>(in ReadOnlySpan<T> slice)
  //     where T : IComparable<T> => CountRun(slice, Comparer<T>.Default);
}
}
