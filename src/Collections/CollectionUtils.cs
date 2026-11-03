using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MMOR.Roslyn;

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
    return MemoryMarshal.Read<Vector<T>>(MemoryMarshal.AsBytes(span));
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
    return MemoryMarshal.Read<Vector<T>>(MemoryMarshal.AsBytes(span));
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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool VectorSupported<T>() {
#if !NETSTANDARD
    return Vector<T>.IsSupported;
#else
    return typeof(T) == typeof(byte) || typeof(T) == typeof(sbyte) || typeof(T) == typeof(short) ||
           typeof(T) == typeof(ushort) || typeof(T) == typeof(int) || typeof(T) == typeof(uint) ||
           typeof(T) == typeof(long) || typeof(T) == typeof(ulong) || typeof(T) == typeof(float) ||
           typeof(T) == typeof(double);
#endif
  }

  [TypeMarshalOverload(typeof(Span<>), typeof(List<>), typeof(CollectionsMarshal),
      nameof(CollectionsMarshal.AsSpan))]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Span<Vector<T>> AsVectorSpan<T>(this in Span<T> source)
      where T : unmanaged {
    return MemoryMarshal.Cast<T, Vector<T>>(source);
  }

  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(List<>), typeof(CollectionsMarshal),
      nameof(CollectionsMarshal.AsSpan))]
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(ImmutableArray<>), typeof(ImmutableArray<>),
      "AsSpan()")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ReadOnlySpan<Vector<T>> AsVectorSpan<T>(this in ReadOnlySpan<T> source)
      where T : unmanaged {
    return MemoryMarshal.Cast<T, Vector<T>>(source);
  }
}
}
