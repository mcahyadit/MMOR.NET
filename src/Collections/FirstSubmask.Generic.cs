using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MMOR.NET.Bits;
using MMOR.Roslyn;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  /**
   * <summary>
   *  Returns the index of the first element i <paramref name="self"/> that is
   *  a submask of <paramref name="mask"/>.
   * </summary>
   * <exception cref="NotSupportedException">
   *  Thrown when generic type <typeparamref name="T"/> is over 64-bit.
   * </exception>
   */
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(List<>), typeof(CollectionsMarshal),
      nameof(CollectionsMarshal.AsSpan))]
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(ImmutableArray<>), typeof(ImmutableArray<>),
      "AsSpan()")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int FirstSubmask<T>(this ReadOnlySpan<T> self, T mask)
      where T : unmanaged {
    return Unsafe.SizeOf<T>() switch {
      1 => FirstSubmask(MemoryMarshal.Cast<T, byte>(self), BitOps.BitCast<T, byte>(mask)),
      2 => FirstSubmask(MemoryMarshal.Cast<T, ushort>(self), BitOps.BitCast<T, ushort>(mask)),
      4 => FirstSubmask(MemoryMarshal.Cast<T, uint>(self), BitOps.BitCast<T, uint>(mask)),
      8 => FirstSubmask(MemoryMarshal.Cast<T, ulong>(self), BitOps.BitCast<T, ulong>(mask)),
      _ => throw new NotSupportedException(
          $"[ERROR]: {typeof(T)} is unsupported for {nameof(FirstSubmask)}"),
    };
  }

  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(List<>), typeof(CollectionsMarshal),
      nameof(CollectionsMarshal.AsSpan))]
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(ImmutableArray<>), typeof(ImmutableArray<>),
      "AsSpan()")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int IndexOfOrFirstSubmask<T>(this ReadOnlySpan<T> self, T mask)
      where T : unmanaged {
    return Unsafe.SizeOf<T>() switch {
      1 => IndexOfOrFirstSubmask(MemoryMarshal.Cast<T, byte>(self), BitOps.BitCast<T, byte>(mask)),
      2 => IndexOfOrFirstSubmask(MemoryMarshal.Cast<T, ushort>(self),
          BitOps.BitCast<T, ushort>(mask)),
      4 => IndexOfOrFirstSubmask(MemoryMarshal.Cast<T, uint>(self), BitOps.BitCast<T, uint>(mask)),
      8 =>
          IndexOfOrFirstSubmask(MemoryMarshal.Cast<T, ulong>(self), BitOps.BitCast<T, ulong>(mask)),
      _ => throw new NotSupportedException(
          $"[ERROR]: {typeof(T)} is unsupported for {nameof(IndexOfOrFirstSubmask)}"),
    };
  }
}
}
