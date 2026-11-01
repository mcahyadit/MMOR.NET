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
    switch (Unsafe.SizeOf<T>()) {
      case 1:
        return FirstSubmask(MemoryMarshal.Cast<T, byte>(self), EnumOps.As<T, byte>(mask));
      case 2:
        return FirstSubmask(MemoryMarshal.Cast<T, ushort>(self), EnumOps.As<T, ushort>(mask));
      case 4:
        return FirstSubmask(MemoryMarshal.Cast<T, uint>(self), EnumOps.As<T, uint>(mask));
      case 8:
        return FirstSubmask(MemoryMarshal.Cast<T, ulong>(self), EnumOps.As<T, ulong>(mask));
      default:
        throw new NotSupportedException(
            $"[ERROR]: {typeof(T)} is unsupported for {nameof(FirstSubmask)}");
    }
  }

  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(List<>), typeof(CollectionsMarshal),
      nameof(CollectionsMarshal.AsSpan))]
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(ImmutableArray<>), typeof(ImmutableArray<>),
      "AsSpan()")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int IndexOfOrFirstSubmask<T>(this ReadOnlySpan<T> self, T mask)
      where T : unmanaged {
    switch (Unsafe.SizeOf<T>()) {
      case 1:
        return IndexOfOrFirstSubmask(MemoryMarshal.Cast<T, byte>(self), EnumOps.As<T, byte>(mask));
      case 2:
        return IndexOfOrFirstSubmask(MemoryMarshal.Cast<T, ushort>(self),
            EnumOps.As<T, ushort>(mask));
      case 4:
        return IndexOfOrFirstSubmask(MemoryMarshal.Cast<T, uint>(self), EnumOps.As<T, uint>(mask));
      case 8:
        return IndexOfOrFirstSubmask(MemoryMarshal.Cast<T, ulong>(self),
            EnumOps.As<T, ulong>(mask));
      default:
        throw new NotSupportedException(
            $"[ERROR]: {typeof(T)} is unsupported for {nameof(IndexOfOrFirstSubmask)}");
    }
  }
}
}
