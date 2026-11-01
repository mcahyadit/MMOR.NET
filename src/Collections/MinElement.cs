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
  public static ulong MinElement(this ReadOnlySpan<ulong> self) {
    int length = self.Length;

    int start;
    ulong result;
    if (Vector.IsHardwareAccelerated && length > 2 * Vector<ulong>.Count) {
      start = length & -Vector<ulong>.Count;

      ReadOnlySpan<Vector<ulong>> self_epi =
          MemoryMarshal.Cast<ulong, Vector<ulong>>(self[..start]);
      Vector<ulong> result_epi = self_epi[0];
      int length_epi           = self_epi.Length;
      for (int i = 1; i < length_epi; ++i) {
        result_epi = Vector.Min(result_epi, self_epi[i]);
      }
      result = result_epi[0];
      for (int i = 1; i < Vector<ulong>.Count; ++i) {
        result = Math.Min(result, result_epi[i]);
      }
    } else {
      result = self[0];
      start  = 1;
    }
    for (int i = start; i < length; ++i) {
      result = Math.Min(result, self[i]);
    }
    return result;
  }
}
}
