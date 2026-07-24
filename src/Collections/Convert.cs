using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> ToVector<T>(this Span<T> span)
      where T : unmanaged {
    if (span.Length < Vector<T>.Count)
      throw new ArgumentException(
          string.Format("[ERROR]: target buffer size: {0}, is less then Vector size: {1}.",
              span.Length, Vector<T>.Count),
          nameof(span));
    return MemoryMarshal.Read<Vector<T>>(MemoryMarshal.AsBytes(span));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> ToVector<T>(this ReadOnlySpan<T> span)
      where T : unmanaged {
    if (span.Length < Vector<T>.Count)
      throw new ArgumentException(
          string.Format("[ERROR]: target buffer size: {0}, is less then Vector size: {1}.",
              span.Length, Vector<T>.Count),
          nameof(span));
    return MemoryMarshal.Read<Vector<T>>(MemoryMarshal.AsBytes(span));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void CopyTo<T>(this in Vector<T> source, Span<T> destination)
      where T : unmanaged {
    Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(destination)),
        source);
  }
}
}
