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
    if (self.IsEmpty)
      return -1;
    int alen = self.Length;

    int i = 0;
    if (typeof(T).IsEnum) {
      switch (Unsafe.SizeOf<T>()) {
#if NETSTANDARD
        case 1:
          return IndexOf(MemoryMarshal.Cast<T, byte>(self), Unsafe.As<T, byte>(ref item));
        case 2:
          return IndexOf(MemoryMarshal.Cast<T, ushort>(self), Unsafe.As<T, ushort>(ref item));
        case 4:
          return IndexOf(MemoryMarshal.Cast<T, uint>(self), Unsafe.As<T, uint>(ref item));
        case 8:
          return IndexOf(MemoryMarshal.Cast<T, ulong>(self), Unsafe.As<T, ulong>(ref item));
#else
        case 1:
          return IndexOf(MemoryMarshal.Cast<T, byte>(self), Unsafe.BitCast<T, byte>(item));
        case 2:
          return IndexOf(MemoryMarshal.Cast<T, ushort>(self), Unsafe.BitCast<T, ushort>(item));
        case 4:
          return IndexOf(MemoryMarshal.Cast<T, uint>(self), Unsafe.BitCast<T, uint>(item));
        case 8:
          return IndexOf(MemoryMarshal.Cast<T, ulong>(self), Unsafe.BitCast<T, ulong>(item));
#endif
        default:
          goto scalar_tail;
      }
    }

    if (Vector.IsHardwareAccelerated) {
      if (typeof(T) == typeof(double) && double.IsNaN(Unsafe.As<T, double>(ref item)))
        goto scalar_tail;
      if (typeof(T) == typeof(float) && float.IsNaN(Unsafe.As<T, float>(ref item)))
        goto scalar_tail;
      if (VectorSupported<T>()) {
        int vlen = Vector<T>.Count;
        int rem  = alen - vlen;

        Vector<T> comp = new(item);

        for (; i <= rem; i += vlen) {
          Vector<T> value_epi = self.Slice(i, vlen).ToVector();

          if (Vector.EqualsAny(value_epi, comp)) {
            Vector<T> cmp_eq = Vector.Equals(value_epi, comp);
            ulong movm       = BitOps.MmMovEpiMask(cmp_eq);
            return i + BitOperations.TrailingZeroCount(movm);
          }
        }
      } else if (typeof(T) == typeof(char)) {
        ReadOnlySpan<ushort> cast = MemoryMarshal.Cast<T, ushort>(self);
        int vlen                  = Vector<ushort>.Count;
        int rem                   = alen - vlen;

#if NETSTANDARD
        Vector<ushort> comp = new(Unsafe.As<T, ushort>(ref item));
#else
        Vector<ushort> comp = new(Unsafe.BitCast<T, ushort>(item));
#endif

        for (; i <= rem; i += vlen) {
          Vector<ushort> value_epi = cast.Slice(i, vlen).ToVector();

          if (Vector.EqualsAny(value_epi, comp)) {
            Vector<ushort> cmp_eq = Vector.Equals(value_epi, comp);
            ulong movm            = BitOps.MmMovEpiMask(cmp_eq);
            return i + BitOperations.TrailingZeroCount(movm);
          }
        }
      }
    }

  scalar_tail:
    for (; i < alen; ++i) {
      if (EqualityComparer<T>.Default.Equals(self[i], item))
        return i;
    }
    return -1;
  }
}
}
