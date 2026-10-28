using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MMOR.NET.Bits;
using MMOR.Roslyn;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(List<>), typeof(CollectionsMarshal),
      nameof(CollectionsMarshal.AsSpan))]
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(ImmutableArray<>), typeof(ImmutableArray<>),
      "AsSpan()")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int FirstSubmask(this ReadOnlySpan<uint> self, uint mask) {
    if (mask == 0)
      return -1;

    int alen = self.Length;
    int vlen = Vector<int>.Count;
    int rem  = alen - vlen;

    Vector<uint> item_epi = new(mask);

    int i = 0;
    if (Vector.IsHardwareAccelerated) {
      for (; i <= rem; i += vlen) {
        Vector<uint> val_epi = self.Slice(i, vlen).ToVector();
        Vector<uint> bitand  = Vector.BitwiseAnd(val_epi, item_epi);
        if (Vector.EqualsAny(bitand, val_epi)) {
          Vector<uint> cmp_eq = Vector.Equals(bitand, val_epi);
          ulong mov2m         = BitOps.MmMovEpiMask(cmp_eq);
          return i + BitOperations.TrailingZeroCount(mov2m);
        }
      }
    }
    for (; i < alen; ++i) {
      uint mask_i = self[i];
      if ((mask_i & mask) == mask_i)
        return i;
    }
    return -1;
  }
}
}
