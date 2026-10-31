using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using System.Runtime.InteropServices;
using MMOR.NET.Mathematics;
using MMOR.Roslyn;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(List<>), typeof(CollectionsMarshal),
      nameof(CollectionsMarshal.AsSpan))]
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(ImmutableArray<>), typeof(ImmutableArray<>),
      "AsSpan()")]
  public static ulong Sum(this ReadOnlySpan<ulong> self) {
    int length = self.Length;
    int start  = 0;

    ulong result = 0;
    if (Vector.IsHardwareAccelerated) {
      start = length - (length % Vector<ulong>.Count);
      if (start == 0)
        goto scalar_tail;

      Vector<ulong> result_epi = Vector<ulong>.Zero;
      ReadOnlySpan<Vector<ulong>> self_epi =
          MemoryMarshal.Cast<ulong, Vector<ulong>>(self[..start]);
      int length_epi = self_epi.Length;
      for (int i = 0; i < length_epi; ++i) {
        result_epi += self_epi[i];
      }
      result = MathExt.SumElements(result_epi);
    }
  scalar_tail:
    for (int i = start; i < length; ++i) {
      result += self[i];
    }
    return result;
  }
}
}
