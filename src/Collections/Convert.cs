using System;
using System.Numerics;
using System.Runtime.CompilerServices;
#if NETSTANDARD
using System.Runtime.InteropServices;
#endif

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> ToVector<T>(this Span<T> span)
      where T : unmanaged {
#if !NETSTANDARD
    return new Vector<T>(span);
#else
    if (span.Length < Vector<T>.Count)
      throw new ArgumentException(
          string.Format("[ERROR]: target buffer size: {0}, is less then Vector size: {1}.",
              span.Length, Vector<T>.Count),
          nameof(span));
    return MemoryMarshal.Read<Vector<T> >(MemoryMarshal.AsBytes(span));
#endif
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> ToVector<T>(this ReadOnlySpan<T> span)
      where T : unmanaged {
#if !NETSTANDARD
    return new Vector<T>(span);
#else
    if (span.Length < Vector<T>.Count)
      throw new ArgumentException(
          string.Format("[ERROR]: target buffer size: {0}, is less then Vector size: {1}.",
              span.Length, Vector<T>.Count),
          nameof(span));
    return MemoryMarshal.Read<Vector<T> >(MemoryMarshal.AsBytes(span));
#endif
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void CopyTo<T>(this in Vector<T> source, Span<T> destination)
      where T : unmanaged {
#if !NETSTANDARD
    source.CopyTo(destination);
#else
    Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(destination)),
        source);
#endif
  }
}
}
