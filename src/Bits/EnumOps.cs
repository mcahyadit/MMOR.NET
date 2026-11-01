using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Bits {
using static BitOps;
public static class EnumOps {
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
      sizeof(byte) => BitCast<byte, T>((byte)(BitCast<T, byte>(left) | BitCast<T, byte>(right))),
      sizeof(ushort) =>
          BitCast<ushort, T>((ushort)(BitCast<T, ushort>(left) | BitCast<T, ushort>(right))),
      sizeof(uint)  => BitCast<uint, T>(BitCast<T, uint>(left) | BitCast<T, uint>(right)),
      sizeof(ulong) => BitCast<ulong, T>(BitCast<T, ulong>(left) | BitCast<T, ulong>(right)),
      _             => ThrowUnsupported<T>(),
    };
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T BitwiseAnd<T>(T left, T right)
      where T : unmanaged, Enum {
    return Unsafe.SizeOf<T>() switch {
      sizeof(byte) => BitCast<byte, T>((byte)(BitCast<T, byte>(left) & BitCast<T, byte>(right))),
      sizeof(ushort) =>
          BitCast<ushort, T>((ushort)(BitCast<T, ushort>(left) & BitCast<T, ushort>(right))),
      sizeof(uint)  => BitCast<uint, T>(BitCast<T, uint>(left) & BitCast<T, uint>(right)),
      sizeof(ulong) => BitCast<ulong, T>(BitCast<T, ulong>(left) & BitCast<T, ulong>(right)),
      _             => ThrowUnsupported<T>(),
    };
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T BitwiseXor<T>(T left, T right)
      where T : unmanaged, Enum {
    return Unsafe.SizeOf<T>() switch {
      sizeof(byte) => BitCast<byte, T>((byte)(BitCast<T, byte>(left) ^ BitCast<T, byte>(right))),
      sizeof(ushort) =>
          BitCast<ushort, T>((ushort)(BitCast<T, ushort>(left) ^ BitCast<T, ushort>(right))),
      sizeof(uint)  => BitCast<uint, T>(BitCast<T, uint>(left) ^ BitCast<T, uint>(right)),
      sizeof(ulong) => BitCast<ulong, T>(BitCast<T, ulong>(left) ^ BitCast<T, ulong>(right)),
      _             => ThrowUnsupported<T>(),
    };
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T BitwiseNot<T>(T value)
      where T : unmanaged, Enum {
    return Unsafe.SizeOf<T>() switch {
      sizeof(byte)   => BitCast<byte, T>((byte)~BitCast<T, byte>(value)),
      sizeof(ushort) => BitCast<ushort, T>((ushort)~BitCast<T, ushort>(value)),
      sizeof(uint)   => BitCast<uint, T>(~BitCast<T, uint>(value)),
      sizeof(ulong)  => BitCast<ulong, T>(~BitCast<T, ulong>(value)),
      _              => ThrowUnsupported<T>(),
    };
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool HasFlag<T>(this T value, T flag)
      where T : unmanaged, Enum {
    return Unsafe.SizeOf<T>() switch {
      sizeof(byte)   => (BitCast<T, byte>(value) & BitCast<T, byte>(flag)) != 0,
      sizeof(ushort) => (BitCast<T, ushort>(value) & BitCast<T, ushort>(flag)) != 0,
      sizeof(uint)   => (BitCast<T, uint>(value) & BitCast<T, uint>(flag)) != 0,
      sizeof(ulong)  => (BitCast<T, ulong>(value) & BitCast<T, ulong>(flag)) != 0,
      _              => ThrowUnsupported2<T>(),
    };
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int PopCnt<T>(T value)
      where T : unmanaged, Enum {
    return Unsafe.SizeOf<T>() switch {
      sizeof(byte)   => BitOperations.PopCount(BitCast<T, byte>(value)),
      sizeof(ushort) => BitOperations.PopCount(BitCast<T, ushort>(value)),
      sizeof(uint)   => BitOperations.PopCount(BitCast<T, uint>(value)),
      sizeof(ulong)  => BitOperations.PopCount(BitCast<T, ulong>(value)),
      _              => ThrowUnsupported3<T>(),
    };
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int TzCnt<T>(T value)
      where T : unmanaged, Enum {
    return Unsafe.SizeOf<T>() switch {
      sizeof(byte)   => BitOperations.TrailingZeroCount(BitCast<T, byte>(value)),
      sizeof(ushort) => BitOperations.TrailingZeroCount(BitCast<T, ushort>(value)),
      sizeof(uint)   => BitOperations.TrailingZeroCount(BitCast<T, uint>(value)),
      sizeof(ulong)  => BitOperations.TrailingZeroCount(BitCast<T, ulong>(value)),
      _              => ThrowUnsupported3<T>(),
    };
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int LzCnt<T>(T value)
      where T : unmanaged, Enum {
    return Unsafe.SizeOf<T>() switch {
      sizeof(byte)   => BitOperations.LeadingZeroCount(BitCast<T, byte>(value)) - (32 - 8),
      sizeof(ushort) => BitOperations.LeadingZeroCount(BitCast<T, ushort>(value)) - (32 - 16),
      sizeof(uint)   => BitOperations.LeadingZeroCount(BitCast<T, uint>(value)),
      sizeof(ulong)  => BitOperations.LeadingZeroCount(BitCast<T, ulong>(value)),
      _              => ThrowUnsupported3<T>(),
    };
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int Log2<T>(T value)
      where T : unmanaged, Enum {
    return Unsafe.SizeOf<T>() switch {
      sizeof(byte)   => BitOperations.Log2(BitCast<T, byte>(value)),
      sizeof(ushort) => BitOperations.Log2(BitCast<T, ushort>(value)),
      sizeof(uint)   => BitOperations.Log2(BitCast<T, uint>(value)),
      sizeof(ulong)  => BitOperations.Log2(BitCast<T, ulong>(value)),
      _              => ThrowUnsupported3<T>(),
    };
  }
}
}
