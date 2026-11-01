using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Bits {
public static class EnumOps {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TTo As<TFrom, TTo>(TFrom value)
      where TFrom : unmanaged
      where TTo : unmanaged {
#if NETSTANDARD
    return Unsafe.As<TFrom, TTo>(ref value);
#else
    return Unsafe.BitCast<TFrom, TTo>(value);
#endif
  }

  [MethodImpl(MethodImplOptions.NoInlining)]
  private static T ThrowUnsupported<T>() => throw new NotSupportedException(
      $"Enum {typeof(T)} has an unsupported size ({Unsafe.SizeOf<T>()} bytes).");
  [MethodImpl(MethodImplOptions.NoInlining)]
  private static bool ThrowUnsupported2<T>() => throw new NotSupportedException(
      $"Enum {typeof(T)} has an unsupported size ({Unsafe.SizeOf<T>()} bytes).");
  [MethodImpl(MethodImplOptions.NoInlining)]
  private static int ThrowUnsupported3<T>() => throw new NotSupportedException(
      $"Enum {typeof(T)} has an unsupported size ({Unsafe.SizeOf<T>()} bytes).");

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T BitwiseOr<T>(T left, T right)
      where T : unmanaged, Enum {
    return Unsafe.SizeOf<T>() switch {
      sizeof(byte)   => As<byte, T>((byte)(As<T, byte>(left) | As<T, byte>(right))),
      sizeof(ushort) => As<ushort, T>((ushort)(As<T, ushort>(left) | As<T, ushort>(right))),
      sizeof(uint)   => As<uint, T>(As<T, uint>(left) | As<T, uint>(right)),
      sizeof(ulong)  => As<ulong, T>(As<T, ulong>(left) | As<T, ulong>(right)),
      _              => ThrowUnsupported<T>(),
    };
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T BitwiseAnd<T>(T left, T right)
      where T : unmanaged, Enum {
    return Unsafe.SizeOf<T>() switch {
      sizeof(byte)   => As<byte, T>((byte)(As<T, byte>(left) & As<T, byte>(right))),
      sizeof(ushort) => As<ushort, T>((ushort)(As<T, ushort>(left) & As<T, ushort>(right))),
      sizeof(uint)   => As<uint, T>(As<T, uint>(left) & As<T, uint>(right)),
      sizeof(ulong)  => As<ulong, T>(As<T, ulong>(left) & As<T, ulong>(right)),
      _              => ThrowUnsupported<T>(),
    };
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T BitwiseXor<T>(T left, T right)
      where T : unmanaged, Enum {
    return Unsafe.SizeOf<T>() switch {
      sizeof(byte)   => As<byte, T>((byte)(As<T, byte>(left) ^ As<T, byte>(right))),
      sizeof(ushort) => As<ushort, T>((ushort)(As<T, ushort>(left) ^ As<T, ushort>(right))),
      sizeof(uint)   => As<uint, T>(As<T, uint>(left) ^ As<T, uint>(right)),
      sizeof(ulong)  => As<ulong, T>(As<T, ulong>(left) ^ As<T, ulong>(right)),
      _              => ThrowUnsupported<T>(),
    };
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T BitwiseNot<T>(T value)
      where T : unmanaged, Enum {
    return Unsafe.SizeOf<T>() switch {
      sizeof(byte)   => As<byte, T>((byte)~As<T, byte>(value)),
      sizeof(ushort) => As<ushort, T>((ushort)~As<T, ushort>(value)),
      sizeof(uint)   => As<uint, T>(~As<T, uint>(value)),
      sizeof(ulong)  => As<ulong, T>(~As<T, ulong>(value)),
      _              => ThrowUnsupported<T>(),
    };
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool HasFlag<T>(this T value, T flag)
      where T : unmanaged, Enum {
    return Unsafe.SizeOf<T>() switch {
      sizeof(byte)   => (As<T, byte>(value) & As<T, byte>(flag)) != 0,
      sizeof(ushort) => (As<T, ushort>(value) & As<T, ushort>(flag)) != 0,
      sizeof(uint)   => (As<T, uint>(value) & As<T, uint>(flag)) != 0,
      sizeof(ulong)  => (As<T, ulong>(value) & As<T, ulong>(flag)) != 0,
      _              => ThrowUnsupported2<T>(),
    };
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int PopCnt<T>(T value)
      where T : unmanaged, Enum {
    return Unsafe.SizeOf<T>() switch {
      sizeof(byte)   => BitOperations.PopCount(As<T, byte>(value)),
      sizeof(ushort) => BitOperations.PopCount(As<T, ushort>(value)),
      sizeof(uint)   => BitOperations.PopCount(As<T, uint>(value)),
      sizeof(ulong)  => BitOperations.PopCount(As<T, ulong>(value)),
      _              => ThrowUnsupported3<T>(),
    };
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int TzCnt<T>(T value)
      where T : unmanaged, Enum {
    return Unsafe.SizeOf<T>() switch {
      sizeof(byte)   => BitOperations.TrailingZeroCount(As<T, byte>(value)),
      sizeof(ushort) => BitOperations.TrailingZeroCount(As<T, ushort>(value)),
      sizeof(uint)   => BitOperations.TrailingZeroCount(As<T, uint>(value)),
      sizeof(ulong)  => BitOperations.TrailingZeroCount(As<T, ulong>(value)),
      _              => ThrowUnsupported3<T>(),
    };
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int LzCnt<T>(T value)
      where T : unmanaged, Enum {
    return Unsafe.SizeOf<T>() switch {
      sizeof(byte)   => BitOperations.LeadingZeroCount(As<T, byte>(value)) - (32 - 8),
      sizeof(ushort) => BitOperations.LeadingZeroCount(As<T, ushort>(value)) - (32 - 16),
      sizeof(uint)   => BitOperations.LeadingZeroCount(As<T, uint>(value)),
      sizeof(ulong)  => BitOperations.LeadingZeroCount(As<T, ulong>(value)),
      _              => ThrowUnsupported3<T>(),
    };
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int Log2<T>(T value)
      where T : unmanaged, Enum {
    return Unsafe.SizeOf<T>() switch {
      sizeof(byte)   => BitOperations.Log2(As<T, byte>(value)),
      sizeof(ushort) => BitOperations.Log2(As<T, ushort>(value)),
      sizeof(uint)   => BitOperations.Log2(As<T, uint>(value)),
      sizeof(ulong)  => BitOperations.Log2(As<T, ulong>(value)),
      _              => ThrowUnsupported3<T>(),
    };
  }
}
}
