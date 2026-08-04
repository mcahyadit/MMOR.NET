using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using System.Runtime.InteropServices;
using MMOR.NET.Bits;
using MMOR.Roslyn;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(List<>), typeof(CollectionsMarshal),
      nameof(CollectionsMarshal.AsSpan))]
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(ImmutableArray<>), typeof(ImmutableArray<>),
      "AsSpan()")]
  public static ulong ConditionalSum(this ReadOnlySpan<ulong> self, ulong bitmask) {
    int alen = self.Length;
    if (alen > 64)
      throw new ArgumentException(
          $"[ERROR]: Trying to use with bitmask, but array length is {alen} ");
    int vlen = Vector<ulong>.Count;
    int rem  = alen - vlen;

    Vector<ulong> acc_v = Vector<ulong>.Zero;

    int i = 0;
    for (; i <= rem; i += vlen) {
      Vector<ulong> slice  = BitOps.ToVectorU64(bitmask, i);
      Vector<ulong> self_v = self.Slice(i, vlen).ToVector();
      acc_v += Vector.ConditionalSelect(slice, self_v, Vector<ulong>.Zero);
    }
    ulong result = Vector.Dot(acc_v, Vector<ulong>.One);
    for (; i < alen; ++i) {
      if (((bitmask >> i) & 1) == 1)
        result += self[i];
    }
    return result;
  }

  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(List<>), typeof(CollectionsMarshal),
      nameof(CollectionsMarshal.AsSpan))]
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(ImmutableArray<>), typeof(ImmutableArray<>),
      "AsSpan()")]
  public static ulong ConditionalSum(this ReadOnlySpan<ulong> self, ReadOnlySpan<bool> conditions) {
    int alen = self.Length;
    if (alen != conditions.Length)
      throw new ArgumentException(
          string.Format("[ERROR]: self.Length: {0} != conditions.Length: {1}",  //
              self.Length, conditions.Length));
    int vlen = Vector<ulong>.Count;
    int rem  = alen - vlen;

    Vector<ulong> acc_v = Vector<ulong>.Zero;

    int i = 0;
    for (; i <= rem; i += vlen) {
      Vector<ulong> slice  = BitOps.ToVectorU64(conditions.Slice(i, vlen).ToBitmask(), 0);
      Vector<ulong> self_v = self.Slice(i, vlen).ToVector();
      acc_v += Vector.ConditionalSelect(slice, self_v, Vector<ulong>.Zero);
    }
    ulong result = Vector.Dot(acc_v, Vector<ulong>.One);
    for (; i < alen; ++i) {
      if (conditions[i])
        result += self[i];
    }
    return result;
  }
}
}
