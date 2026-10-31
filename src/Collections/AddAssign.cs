using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using System.Runtime.InteropServices;
using MMOR.Roslyn;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  [TypeMarshalOverload(typeof(Span<>), typeof(List<>), typeof(CollectionsMarshal),
      nameof(CollectionsMarshal.AsSpan))]
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(List<>), typeof(CollectionsMarshal),
      nameof(CollectionsMarshal.AsSpan))]
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(ImmutableArray<>), typeof(ImmutableArray<>),
      "AsSpan()")]
  public static void AddAssign(this Span<ulong> self, ReadOnlySpan<ulong> other) {
    int length = self.Length;
    int start  = 0;

    if (Vector.IsHardwareAccelerated) {
      start = length - (length % Vector<ulong>.Count);
      if (start == 0)
        goto scalar_tail;

      Span<Vector<ulong>> self_epi = MemoryMarshal.Cast<ulong, Vector<ulong>>(self[..start]);
      ReadOnlySpan<Vector<ulong>> other_epi =
          MemoryMarshal.Cast<ulong, Vector<ulong>>(other[..start]);
      int length_epi = self_epi.Length;
      for (int i = 0; i < length_epi; ++i) {
        self_epi[i] += other_epi[i];
      }
    }
  scalar_tail:
    for (int i = start; i < length; ++i) {
      self[i] += other[i];
    }
  }

  [TypeMarshalOverload(typeof(Span<>), typeof(List<>), typeof(CollectionsMarshal),
      nameof(CollectionsMarshal.AsSpan))]
  public static void AddAssign(this Span<ulong> self, ulong value) {
    int length = self.Length;
    int start  = 0;

    if (Vector.IsHardwareAccelerated) {
      start = length - (length % Vector<ulong>.Count);
      if (start == 0)
        goto scalar_tail;

      Span<Vector<ulong>> self_epi = MemoryMarshal.Cast<ulong, Vector<ulong>>(self[..start]);
      Vector<ulong> value_epi      = new(value);
      int length_epi               = self_epi.Length;
      for (int i = 0; i < length_epi; ++i) {
        self_epi[i] += value_epi;
      }
    }
  scalar_tail:
    for (int i = start; i < length; ++i) {
      self[i] += value;
    }
  }
}
}
