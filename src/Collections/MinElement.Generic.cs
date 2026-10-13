using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MMOR.Roslyn;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(List<>), typeof(CollectionsMarshal),
      nameof(CollectionsMarshal.AsSpan))]
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(ImmutableArray<>), typeof(ImmutableArray<>),
      "AsSpan()")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T MinElement<T>(this ReadOnlySpan<T> self)
      where T : unmanaged, IComparable<T> {
    if (self.IsEmpty)
      throw new ArgumentException("[ERROR]: passed span is empty.");
    int alen = self.Length;

    int i    = 1;
    T result = self[0];
    if (Vector.IsHardwareAccelerated) {
      if (VectorSupported<T>()) {
        int vlen = Vector<T>.Count;
        if (alen <= 2 * vlen)
          goto scalar_tail;

        int rem         = alen - vlen;
        Vector<T> acc_v = self[..vlen].ToVector();
        i               = vlen;
        for (; i <= rem; i += vlen) {
          acc_v = Vector.Min(acc_v, self.Slice(i, vlen).ToVector());
        }
        result = acc_v[0];
        for (int j = 1; j < vlen; ++j) {
          if (result.CompareTo(acc_v[j]) > 0)
            result = acc_v[j];
        }
      } else if (typeof(T) == typeof(char)) {
        ReadOnlySpan<ushort> cast = MemoryMarshal.Cast<T, ushort>(self);
        int vlen                  = Vector<ushort>.Count;
        if (alen <= 2 * vlen)
          goto scalar_tail;

        int rem              = alen - vlen;
        Vector<ushort> acc_v = cast[..vlen].ToVector();
        i                    = vlen;
        for (; i <= rem; i += vlen) {
          acc_v = Vector.Min(acc_v, cast.Slice(i, vlen).ToVector());
        }
        ushort cres = acc_v[0];
        for (int j = 1; j < vlen; ++j) {
          if (cres.CompareTo(acc_v[j]) > 0)
            cres = acc_v[j];
        }
        result = Unsafe.As<ushort, T>(ref cres);
      }
    }

  scalar_tail:
    for (; i < alen; ++i) {
      if (result.CompareTo(self[i]) > 0)
        result = self[i];
    }
    return result;
  }
}
}
