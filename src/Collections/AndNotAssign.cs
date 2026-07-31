using System;
using System.Numerics;
using System.Collections.Generic;
using System.Collections.Immutable;
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
  public static void AndNotAssign(this Span<ulong> self, ReadOnlySpan<ulong> other) {
    int alen = self.Length;
    if (alen != other.Length)
      throw new ArgumentException(string.Format("[ERROR]: self.Length: {0} != other.Length: {1}",
          self.Length, other.Length));
    int vlen = Vector<ulong>.Count;
    int rem  = alen - vlen;
    int i    = 0;
    for (; i <= rem; i += vlen) {
      Vector<ulong> self_v  = self.Slice(i, vlen).ToVector();
      Vector<ulong> other_v = other.Slice(i, vlen).ToVector();
      Vector.AndNot(self_v, other_v).CopyTo(self.Slice(i, vlen));
    }
    for (; i < alen; ++i) {
      self[i] &= ~other[i];
    }
  }
}
}
