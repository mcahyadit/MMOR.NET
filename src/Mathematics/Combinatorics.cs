using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MMOR.NET.Mathematics {
public static partial class Combinatorics {
  public static int MixedRadixIndices(Span<int> buffer, ReadOnlySpan<int> radices, int length) {
    Debug.Assert(radices.Length == length);
    int combins = 1;
    for (int i = 0; i < length; ++i) {
      int radix = radices[i];
      Debug.Assert(radix > 0);
      combins *= radix;
    }

    Span<int> counter = stackalloc int[length];
    Debug.Assert(buffer.Length >= combins * length);
    for (int ci = 0; ci < combins; ++ci) {
      Span<int> vals = buffer.Slice(ci * length, length);
      counter.CopyTo(vals);
      for (int i = 0; i < length; ++i) {
        if (++counter[i] < radices[i]) {
          break;
        }
        counter[i] = 0;
      }
    }
    return combins;
  }

  public static int MixedRadixIndices(Span<int> buffer, int radix, int length) {
    Debug.Assert(radix > 0);
    int combins = 1;
    for (int i = 0; i < length; ++i) {
      combins *= radix;
    }

    Span<int> counter = stackalloc int[length];
    Debug.Assert(buffer.Length >= combins * length);
    for (int ci = 0; ci < combins; ++ci) {
      Span<int> vals = buffer.Slice(ci * length, length);
      counter.CopyTo(vals);
      for (int i = 0; i < length; ++i) {
        if (++counter[i] < radix) {
          break;
        }
        counter[i] = 0;
      }
    }
    return combins;
  }

  public static int CartesianProduct<T>(Span<T> buffer, ReadOnlySpan<T> values, int length)
      where T : unmanaged {
    Debug.Assert(!values.IsEmpty);
    Span<int> index_buffer = stackalloc int[buffer.Length];
    int len                = MixedRadixIndices(index_buffer, values.Length, length);
    for (int i = 0; i < len; ++i) {
      buffer[i] = values[index_buffer[i]];
    }
    return len;
  }
}
}
