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
  /// <inheritdoc cref="FirstSubmask{T}(ReadOnlySpan{T}, T)"/>
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(List<>), typeof(CollectionsMarshal),
      nameof(CollectionsMarshal.AsSpan))]
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(ImmutableArray<>), typeof(ImmutableArray<>),
      "AsSpan()")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int FirstSubmask(this ReadOnlySpan<uint> self, uint mask) {
    int length = self.Length;
    int start  = 0;

    if (Vector.IsHardwareAccelerated) {
      start = length - (length % Vector<uint>.Count);
      if (start == 0)
        goto scalar_tail;

      ReadOnlySpan<Vector<uint>> self_epi = MemoryMarshal.Cast<uint, Vector<uint>>(self[..start]);
      Vector<uint> item_epi               = new(mask);
      int length_epi                      = self_epi.Length;
      for (int i = 0; i < length_epi; ++i) {
        Vector<uint> val_epi = self_epi[i];
        Vector<uint> bitand  = val_epi & item_epi;
        if (Vector.EqualsAny(bitand, val_epi)) {
          Vector<uint> cmp_eq = Vector.Equals(bitand, val_epi);
          ulong mov2m         = BitOps.MmMovEpiMask(cmp_eq);
          return i * Vector<uint>.Count + BitOperations.TrailingZeroCount(mov2m);
        }
      }
    }
  scalar_tail:
    for (int i = start; i < length; ++i) {
      uint mask_i = self[i];
      if ((mask_i & mask) == mask_i)
        return i;
    }
    return -1;
  }
}
}
