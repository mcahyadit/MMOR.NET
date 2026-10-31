using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using System.Runtime.InteropServices;
using MMOR.Roslyn;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(List<>), typeof(CollectionsMarshal),
      nameof(CollectionsMarshal.AsSpan))]
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(ImmutableArray<>), typeof(ImmutableArray<>),
      "AsSpan()")]
  public static ulong MaxElement(this ReadOnlySpan<ulong> self) {
    int length = self.Length;
    int start  = 0;

    ulong result;
    if (Vector.IsHardwareAccelerated && length > 2 * Vector<ulong>.Count) {
      start = length - (length % Vector<ulong>.Count);

      Vector<ulong> result_epi = Vector<ulong>.Zero;
      ReadOnlySpan<Vector<ulong>> self_epi =
          MemoryMarshal.Cast<ulong, Vector<ulong>>(self[..start]);
      int length_epi = self_epi.Length;
      for (int i = 0; i < length_epi; ++i) {
        result_epi = Vector.Max(result_epi, self_epi[i]);
      }
      result = result_epi[0];
      for (int i = 1; i < Vector<ulong>.Count; ++i) {
        result = Math.Max(result, result_epi[i]);
      }
    } else {
      result = self[0];
      start  = 1;
    }
    for (int i = start; i < length; ++i) {
      result = Math.Max(result, self[i]);
    }
    return result;
  }
}
}
