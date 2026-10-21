using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using MMOR.NET.Collections;

namespace MMOR.NET.Bits {
public partial class BitOpsTest {
  public static ulong MmMovEpiMaskReference<T>(Vector<T> vector)
      where T : unmanaged, IConvertible {
    long result = 0;
    int size    = Unsafe.SizeOf<T>() * 8;
    for (int i = 0; i < Vector<T>.Count; ++i) {
      result |= (vector[i].ToInt64(null) >> (size - 1)) << i;
    }
    return (ulong)result;
  }

  public static Vector<T> MmMovMaskEpiReference<T>(ulong mask)
      where T : unmanaged, IConvertible {
    Span<T> buffer = stackalloc T[Vector<T>.Count];
    for (int i = 0; i < Vector<T>.Count; ++i) {
      ulong bit_i = (mask >> i) & 1;
      buffer[i]   = bit_i == 1 ? Vector<T>.AllBitsSet[0] : Vector<T>.Zero[0];
    }
    return buffer.ToVector();
  }

  private static ulong ReferenceMovemask64(Vector<ulong> v) {
    ulong result = 0;
    for (int i = 0; i < Vector<ulong>.Count; ++i) {
      result |= (v[i] >> 63) << i;
    }
    return result;
  }

  private static ulong ReferenceMovemask32(Vector<uint> v) {
    ulong result = 0;
    for (int i = 0; i < Vector<uint>.Count; ++i) {
      result |= (ulong)(v[i] >> 31) << i;
    }
    return result;
  }
}
}
