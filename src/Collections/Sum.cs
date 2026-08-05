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
    int alen = self.Length;
    int vlen = Vector<ulong>.Count;
    int rem  = alen - vlen;

    Vector<ulong> acc_v = Vector<ulong>.Zero;

    int i = 0;
    for (; i <= rem; i += vlen) {
      Vector<ulong> self_v = self.Slice(i, vlen).ToVector();
      acc_v += self_v;
    }
    ulong result = MathExt.SumElements(acc_v);
    for (; i < alen; ++i) {
      result += self[i];
    }
    return result;
  }
}
}
