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
  public static int IndexOf<T>(this ReadOnlySpan<T> self, T item)
      where T : unmanaged {
    if (self.IsEmpty) return -1;
    int length = self.Length;

    int start = 0;
    if (typeof(T).IsEnum) {
      switch (Unsafe.SizeOf<T>()) {
        case 1: return IndexOf(MemoryMarshal.Cast<T, byte>(self), BitOps.BitCast<T, byte>(item));
        case 2:
          return IndexOf(MemoryMarshal.Cast<T, ushort>(self), BitOps.BitCast<T, ushort>(item));
        case 4: return IndexOf(MemoryMarshal.Cast<T, uint>(self), BitOps.BitCast<T, uint>(item));
        case 8: return IndexOf(MemoryMarshal.Cast<T, ulong>(self), BitOps.BitCast<T, ulong>(item));
        default: goto scalar_tail;
      }
    }

    if (Vector.IsHardwareAccelerated) {
      if (typeof(T) == typeof(double) && double.IsNaN(BitOps.BitCast<T, double>(item)))
        goto scalar_tail;
      if (typeof(T) == typeof(float) && float.IsNaN(BitOps.BitCast<T, float>(item)))
        goto scalar_tail;
      if (VectorSupported<T>()) {
        int vlen = Vector<T>.Count;
        if (length <= 2 * vlen) goto scalar_tail;

        start = length & -vlen;

        ReadOnlySpan<Vector<T>> self_epi = MemoryMarshal.Cast<T, Vector<T>>(self[..start]);

        Vector<T> comp = new(item);

        int length_epi = self_epi.Length;
        for (int i = 0; i < length_epi; ++i) {
          Vector<T> value_epi = self_epi[i];

          if (Vector.EqualsAny(value_epi, comp)) {
            Vector<T> cmp_eq = Vector.Equals(value_epi, comp);
            ulong movm       = BitOps.MmMovEpiMask(cmp_eq);
            return i * vlen + BitOperations.TrailingZeroCount(movm);
          }
        }
      } else if (typeof(T) == typeof(char)) {
        int vlen = Vector<ushort>.Count;
        if (length <= 2 * vlen) goto scalar_tail;

        start = length & -vlen;

        ReadOnlySpan<Vector<ushort>> self_epi =
            MemoryMarshal.Cast<T, Vector<ushort>>(self[..start]);

        Vector<ushort> comp = new(BitOps.BitCast<T, ushort>(item));

        int length_epi = self_epi.Length;
        for (int i = 0; i < length_epi; ++i) {
          Vector<ushort> value_epi = self_epi[i];

          if (Vector.EqualsAny(value_epi, comp)) {
            Vector<ushort> cmp_eq = Vector.Equals(value_epi, comp);
            ulong movm            = BitOps.MmMovEpiMask(cmp_eq);
            return i * vlen + BitOperations.TrailingZeroCount(movm);
          }
        }
      }
    }

  scalar_tail:
    for (int i = start; i < length; ++i) {
      if (EqualityComparer<T>.Default.Equals(self[i], item)) return i;
    }
    return -1;
  }
}
}
