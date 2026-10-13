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
    if (self.IsEmpty)
      throw new ArgumentException("[ERROR]: passed span is empty.");
    int alen = self.Length;
    int vlen = Vector<ulong>.Count;

    int i;
    ulong result;
    if (Vector.IsHardwareAccelerated && alen > 2 * vlen) {
      int rem             = alen - vlen;
      Vector<ulong> acc_v = self[..vlen].ToVector();
      i                   = vlen;
      for (; i <= rem; i += vlen) {
        acc_v = Vector.Max(acc_v, self.Slice(i, vlen).ToVector());
      }
      result = acc_v[0];
      for (int j = 1; j < vlen; ++j) {
        result = Math.Max(result, acc_v[j]);
      }
    } else {
      result = self[0];
      i      = 1;
    }
    for (; i < alen; ++i) {
      result = Math.Max(result, self[i]);
    }
    return result;
  }
}
}
