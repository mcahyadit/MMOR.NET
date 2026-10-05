using System;
using System.Buffers.Binary;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;

#if !NETSTANDARD
using System.Runtime.Intrinsics.Arm;
#endif

namespace MMOR.NET.Bits {
public static partial class BitOps {
  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte ReverseBits(byte value) {
#if !NETSTANDARD
    if (ArmBase.IsSupported) {
      return (byte)(ArmBase.ReverseElementBits((uint)value) >> 24);
    }
#endif
    return (byte)((value * 0x0202020202ul & 0x010884422010ul) % 1023);
  }

  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint ReverseBits(uint value) {
#if !NETSTANDARD
    if (ArmBase.IsSupported) {
      return ArmBase.ReverseElementBits(value);
    }
#endif

    value = ((value >> 1) & 0x55555555) | ((value & 0x55555555) << 1);
    value = ((value >> 2) & 0x33333333) | ((value & 0x33333333) << 2);
    value = ((value >> 4) & 0x0F0F0F0F) | ((value & 0x0F0F0F0F) << 4);
    return BinaryPrimitives.ReverseEndianness(value);
  }

  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint ReverseBits(uint value, int length) {
    if (length == 32)
      return ReverseBits(value);
    if (length == 0)
      return value;
    if ((uint)length > 32)
      throw new ArgumentOutOfRangeException(nameof(length), length, "[ERROR]: length: [0, 32]");

    uint mask    = Bzhi32(~0u, length);
    uint outside = value & ~mask;
    uint inside  = value & mask;
    return outside | (ReverseBits(inside) >> (32 - length));
  }

  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint ReverseBits(uint value, int start, int length) {
    if ((uint)start > 31)
      throw new ArgumentOutOfRangeException(nameof(start), start, "[ERROR]: start: [0, 31]");
    if ((uint)length > 32 - start) {
      throw new ArgumentOutOfRangeException(nameof(length), length,
          $"[ERROR]: length: [0, {32 - start}]");
    }
    if (length <= 1)
      return value;

    uint mask     = Bzhi32(~0u, length) << start;
    uint reversed = BitOperations.RotateLeft(ReverseBits(value), 2 * start + length);
    return (value & ~mask) | (reversed & mask);
  }

  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong ReverseBits(ulong value) {
#if !NETSTANDARD
    if (ArmBase.Arm64.IsSupported) {
      return ArmBase.Arm64.ReverseElementBits(value);
    }
#endif

    value = ((value >> 1) & 0x5555555555555555) | ((value & 0x5555555555555555) << 1);
    value = ((value >> 2) & 0x3333333333333333) | ((value & 0x3333333333333333) << 2);
    value = ((value >> 4) & 0x0F0F0F0F0F0F0F0F) | ((value & 0x0F0F0F0F0F0F0F0F) << 4);
    return BinaryPrimitives.ReverseEndianness(value);
  }

  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong ReverseBits(ulong value, int length) {
    if (length == 64)
      return ReverseBits(value);
    if (length == 0)
      return value;
    if ((uint)length > 64)
      throw new ArgumentOutOfRangeException(nameof(length), length, "[ERROR]: length: [0, 64]");

    ulong mask    = Bzhi64(~0ul, length);
    ulong outside = value & ~mask;
    ulong inside  = value & mask;
    return outside | (ReverseBits(inside) >> (64 - length));
  }

  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong ReverseBits(ulong value, int start, int length) {
    if ((uint)start > 63)
      throw new ArgumentOutOfRangeException(nameof(start), start, "[ERROR]: start: [0, 63]");
    if ((uint)length > 64 - start) {
      throw new ArgumentOutOfRangeException(nameof(length), length,
          $"[ERROR]: length: [0, {64 - start}]");
    }
    if (length <= 1)
      return value;

    ulong mask     = Bzhi64(~0ul, length) << start;
    ulong reversed = BitOperations.RotateLeft(ReverseBits(value), 2 * start + length);
    return (value & ~mask) | (reversed & mask);
  }
}
}
