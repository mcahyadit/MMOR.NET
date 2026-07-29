using System;
using System.Runtime.InteropServices;
using MMOR.Roslyn;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Buffers.Binary;
using System.Diagnostics.Contracts;

#if !NETSTANDARD
using System.Runtime.Intrinsics.X86;
#endif

namespace MMOR.NET.Bits {
public static partial class BitOps {
  /**
   * <summary>
   *  Converts a list of booleans into its bitmask representation.
   * </summary>
   * <exception cref="ArgumentOutOfRangeException">
   *  Thrown when the size of <paramref name="bools"/> is greater than 64;
   * </exception>
   */
  [Pure]
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(List<>),
      "System.Runtime.InteropServices.CollectionsMarshal.AsSpan(@)")]
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(ImmutableArray<>), "@.AsSpan()")]
  public static ulong ToBitmask(this ReadOnlySpan<bool> bools) {
    int len = bools.Length;
    if (len > 64) {
      throw new ArgumentOutOfRangeException(nameof(bools),
          $"Tried to convert a List of boolean that is too big (Count: {len}) for UInt64.");
    }
    ulong result = 0;
    int i        = 0;
    int rem      = bools.Length - 8;
    for (; i <= rem; i += 8) {
      ReadOnlySpan<ulong> cast = MemoryMarshal.Cast<bool, ulong>(bools.Slice(i, 8));

      ulong chunk = BitConverter.IsLittleEndian  //
                        ? cast[0]
                        : BinaryPrimitives.ReverseEndianness(cast[0]);
#if !NETSTANDARD
      if (Bmi2.X64.IsSupported) {
        const ulong mask = 0x0101010101010101ul;
        result |= Bmi2.X64.ParallelBitExtract(chunk, mask) << i;
        continue;
      }
#endif
      const ulong magic = 0x0102040810204080ul;
      result |= (magic * chunk >> 56) << i;
    }
    for (; i < len; ++i) {
      if (bools[i])
        result |= 1ul << i;
    }
    return result;
  }

  /**
   * <summary>
   *  Formats a bitmask as a boolean array.
   * </summary>
   * <exception cref="ArgumentException">
   *  Thrown when the size of <paramref name="buffer"/> is empty;
   * </exception>
   * <remarks>
   *
   *  FromBitmask will not check the buffer's length. <br/>
   *  It will only write up to the provided length.
   * </remarks>
   */
  [TypeMarshalOverload(typeof(Span<>), typeof(List<>),
      "System.Runtime.InteropServices.CollectionsMarshal.AsSpan(@)")]
  public static void WriteToBoolArray(this ulong bitmask, Span<bool> buffer) {
    if (buffer.IsEmpty)
      throw new ArgumentException(nameof(buffer), "[ERROR]: passed buffer is empty.");
    int alen   = Math.Min(buffer.Length, 64);
    int i      = 0;
    int rem    = alen - 8;
    ulong mask = 0x0101010101010101ul;
    for (; i <= rem; i += 8) {
      Span<ulong> cast = MemoryMarshal.Cast<bool, ulong>(buffer.Slice(i, 8));
      ulong chunk;
#if !NETSTANDARD
      if (Bmi2.X64.IsSupported) {
        ulong bitslice = Bmi2.X64.ZeroHighBits(bitmask >> i, 8);
        chunk          = Bmi2.X64.ParallelBitDeposit(bitslice, mask);
        goto assign;
      }
#endif
      ulong slice       = (bitmask >> i) & 0xFF;
      const ulong magic = 0x0102040810204080ul;
      chunk             = ((magic * slice) & mask) >> 7;
#if !NETSTANDARD
    assign:
#endif
      cast[0] = BitConverter.IsLittleEndian ? chunk : BinaryPrimitives.ReverseEndianness(chunk);
    }
    for (; i < alen; ++i) {
      buffer[i] = ((bitmask >> i) & 1) == 1;
    }
  }
}
}
