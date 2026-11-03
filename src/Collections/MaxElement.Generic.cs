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
  public static T MaxElement<T>(this ReadOnlySpan<T> self)
      where T : unmanaged, IComparable<T> {
    if (self.IsEmpty) throw new ArgumentException("[ERROR]: passed span is empty.");
    int length = self.Length;

    int start = 1;
    T result  = self[0];
    if (Vector.IsHardwareAccelerated) {
      if (VectorSupported<T>()) {
        int vlen = Vector<T>.Count;
        if (length <= 2 * vlen) goto scalar_tail;

        start = length & -vlen;

        ReadOnlySpan<Vector<T>> self_epi = self.AsVectorSpan();

        Vector<T> result_epi = self_epi[0];
        int length_epi       = self_epi.Length;
        for (int i = 1; i < length_epi; ++i) {
          result_epi = Vector.Max(result_epi, self_epi[i]);
        }
        result = result_epi[0];
        for (int i = 1; i < vlen; ++i) {
          if (result.CompareTo(result_epi[i]) < 0) result = result_epi[i];
        }
      } else if (typeof(T) == typeof(char)) {
        int vlen = Vector<ushort>.Count;
        if (length <= 2 * vlen) goto scalar_tail;

        start = length & -vlen;

        ReadOnlySpan<Vector<ushort>> self_epi =
            MemoryMarshal.Cast<T, Vector<ushort>>(self[..start]);

        Vector<ushort> result_epi = self_epi[0];
        int length_epi            = self_epi.Length;
        for (int i = 1; i < length_epi; ++i) {
          result_epi = Vector.Max(result_epi, self_epi[i]);
        }

        ReadOnlySpan<T> char_epi = MemoryMarshal.Cast<Vector<ushort>, T>(
            MemoryMarshal.CreateReadOnlySpan(ref result_epi, 1));
        result = char_epi[0];
        for (int i = 1; i < vlen; ++i) {
          if (result.CompareTo(char_epi[i]) < 0) result = char_epi[i];
        }
      }
    }

  scalar_tail:
    for (int i = start; i < length; ++i) {
      if (result.CompareTo(self[i]) < 0) result = self[i];
    }
    return result;
  }
}
}
