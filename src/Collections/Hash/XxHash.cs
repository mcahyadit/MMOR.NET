using System;
using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MMOR.NET.Mathematics;

namespace MMOR.NET.Collections.Hash {
public static partial class HashUtils {
  // https://codeberg.org/ziglang/zig/src/commit/8e8cf3835a96a5c1ab72c1638e82f7a1f5d6032d/lib/std/hash/xxhash.zig
  public static readonly ImmutableArray<byte> kXxHash3Secret = ImmutableArray.Create(
      stackalloc byte[192] { 0xb8, 0xfe, 0x6c, 0x39, 0x23, 0xa4, 0x4b, 0xbe, 0x7c, 0x01, 0x81, 0x2c,
        0xf7, 0x21, 0xad, 0x1c, 0xde, 0xd4, 0x6d, 0xe9, 0x83, 0x90, 0x97, 0xdb, 0x72, 0x40, 0xa4,
        0xa4, 0xb7, 0xb3, 0x67, 0x1f, 0xcb, 0x79, 0xe6, 0x4e, 0xcc, 0xc0, 0xe5, 0x78, 0x82, 0x5a,
        0xd0, 0x7d, 0xcc, 0xff, 0x72, 0x21, 0xb8, 0x08, 0x46, 0x74, 0xf7, 0x43, 0x24, 0x8e, 0xe0,
        0x35, 0x90, 0xe6, 0x81, 0x3a, 0x26, 0x4c, 0x3c, 0x28, 0x52, 0xbb, 0x91, 0xc3, 0x00, 0xcb,
        0x88, 0xd0, 0x65, 0x8b, 0x1b, 0x53, 0x2e, 0xa3, 0x71, 0x64, 0x48, 0x97, 0xa2, 0x0d, 0xf9,
        0x4e, 0x38, 0x19, 0xef, 0x46, 0xa9, 0xde, 0xac, 0xd8, 0xa8, 0xfa, 0x76, 0x3f, 0xe3, 0x9c,
        0x34, 0x3f, 0xf9, 0xdc, 0xbb, 0xc7, 0xc7, 0x0b, 0x4f, 0x1d, 0x8a, 0x51, 0xe0, 0x4b, 0xcd,
        0xb4, 0x59, 0x31, 0xc8, 0x9f, 0x7e, 0xc9, 0xd9, 0x78, 0x73, 0x64, 0xea, 0xc5, 0xac, 0x83,
        0x34, 0xd3, 0xeb, 0xc3, 0xc5, 0x81, 0xa0, 0xff, 0xfa, 0x13, 0x63, 0xeb, 0x17, 0x0d, 0xdd,
        0x51, 0xb7, 0xf0, 0xda, 0x49, 0xd3, 0x16, 0x55, 0x26, 0x29, 0xd4, 0x68, 0x9e, 0x2b, 0x16,
        0xbe, 0x58, 0x7d, 0x47, 0xa1, 0xfc, 0x8f, 0xf8, 0xb8, 0xd1, 0x7a, 0xd0, 0x31, 0xce, 0x45,
        0xcb, 0x3a, 0x8f, 0x95, 0x16, 0x04, 0x28, 0xaf, 0xd7, 0xfb, 0xca, 0xbb, 0x4b, 0x40, 0x7e });

  public const ulong kXxHash3Prime1 = 0x165667919E3779F9;
  public const ulong kXxHash3Prime2 = 0x9FB21C651E98DF25;

  public const ulong kXxHash64Prime1 = 0x9E3779B185EBCA87;
  public const ulong kXxHash64Prime2 = 0xC2B2AE3D27D4EB4F;
  public const ulong kXxHash64Prime3 = 0x165667B19E3779F9;
  public const ulong kXxHash64Prime4 = 0x85EBCA77C2B2AE63;
  public const ulong kXxHash64Prime5 = 0x27D4EB2F165667C5;

  public static ulong XxHash3(string value, ulong seed = 0) => XxHash3(
      MemoryMarshal.Cast<char, byte>(value), seed);

  public static ulong XxHash3<T>(ReadOnlySpan<T> value, ulong seed = 0)
      where T : unmanaged => XxHash3(MemoryMarshal.Cast<T, byte>(value), seed);

  public static ulong XxHash3<T>(T value, ulong seed = 0)
      where T : unmanaged {
    return XxHash3(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref value, 1)), seed);
  }

  public static ulong XxHash3(ReadOnlySpan<byte> value, ulong seed = 0) {
    if (value.IsEmpty) {
      ReadOnlySpan<ulong> flip =
          MemoryMarshal.Cast<byte, ulong>(kXxHash3Secret.AsSpan(56, 2 * sizeof(ulong)));
      ulong key = flip[0] ^ flip[1];
      return AvalancheH64(seed ^ key);
    }

    int length = value.Length;
    if (length < sizeof(uint)) return XxHash3_Hash3(value, seed);
    if (length <= sizeof(ulong)) return XxHash3_Hash8(value, seed);
    if (length <= 16) return XxHash3_Hash16(value, seed);
    if (length <= 128) return XxHash3_Hash128(value, seed);
    return 0;
  }

  public static ulong XxHash3_Hash3(ReadOnlySpan<byte> value, ulong seed) {
    ReadOnlySpan<uint> flip = MemoryMarshal.Cast<byte, uint>(kXxHash3Secret.AsSpan());

    uint blk = value[^1]                  //
               | (uint)value.Length << 8  //
               | (uint)value[0] << 16     //
               | (uint)value[value.Length >> 1] << 24;
    ulong key = unchecked((flip[0] ^ flip[1]) + seed);
    return AvalancheH64(key ^ blk);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong AvalancheH3(in ulong x0) {
    unchecked {
      ulong x1 = (x0 ^ (x0 >> 37)) * kXxHash3Prime1;
      return x1 ^ (x1 >> 32);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong AvalancheH64(in ulong x0) {
    unchecked {
      ulong x1 = (x0 ^ (x0 >> 33)) * kXxHash64Prime2;
      ulong x2 = (x1 ^ (x1 >> 29)) * kXxHash64Prime3;
      return x2 ^ (x2 >> 32);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong AvalancheRrmxmx(in ulong x0, in uint len) {
    unchecked {
      ulong x1 = (x0 ^ BitOperations.RotateLeft(x0, 49) ^ BitOperations.RotateLeft(x0, 24)) *
                 kXxHash3Prime2;
      ulong x2 = (x1 ^ ((x1 >> 35) + len)) * kXxHash3Prime2;
      return x2 ^ (x2 >> 28);
    }
  }

  public static ulong XxHash3_Hash8(ReadOnlySpan<byte> value, ulong seed) {
    ReadOnlySpan<ulong> flip =
        MemoryMarshal.Cast<byte, ulong>(kXxHash3Secret.AsSpan(8, 2 * sizeof(ulong)));

    uint blk0 = BinaryPrimitives.ReadUInt32LittleEndian(value);
    uint blk1 = BinaryPrimitives.ReadUInt32LittleEndian(value[^sizeof(uint)..]);

    unchecked {
      ulong mixed    = seed ^ ((ulong)BinaryPrimitives.ReverseEndianness((uint)seed) << 32);
      ulong key      = (flip[0] ^ flip[1]) - mixed;
      ulong combined = ((ulong)blk0 << 32) + blk1;
      return AvalancheRrmxmx(key ^ combined, (uint)value.Length);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong MulXor(in ulong a, in ulong b) {
    MathExt.BigMulU64(a, b, out ulong hi, out ulong lo);
    return hi ^ lo;
  }

  public static ulong XxHash3_Hash16(ReadOnlySpan<byte> value, ulong seed) {
    ReadOnlySpan<ulong> flip =
        MemoryMarshal.Cast<byte, ulong>(kXxHash3Secret.AsSpan(24, 4 * sizeof(ulong)));

    ulong blk0 = BinaryPrimitives.ReadUInt64LittleEndian(value);
    ulong blk1 = BinaryPrimitives.ReadUInt64LittleEndian(value[^sizeof(ulong)..]);

    unchecked {
      ulong lo = blk0 ^ ((flip[0] ^ flip[1]) + seed);
      ulong hi = blk1 ^ ((flip[2] ^ flip[3]) - seed);
      ulong combined =
          (ulong)value.Length + BinaryPrimitives.ReverseEndianness(lo) + hi + MulXor(lo, hi);
      return AvalancheH3(combined);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong Mix16(ulong seed, ReadOnlySpan<byte> value, ReadOnlySpan<byte> secret) {
    ReadOnlySpan<ulong> blk        = MemoryMarshal.Cast<byte, ulong>(value);
    ReadOnlySpan<ulong> secret_u64 = MemoryMarshal.Cast<byte, ulong>(secret);
    return MulXor(blk[0] ^ unchecked(secret_u64[0] + seed),
        blk[1] ^ unchecked(secret_u64[1] - seed));
  }

  public static ulong XxHash3_Hash128(ReadOnlySpan<byte> value, ulong seed) {
    unchecked {
      ulong acc = kXxHash64Prime1 * (ulong)value.Length;
      for (int i = 0; i < 4; ++i) {
        int in_offset   = 48 - (i * 16);
        int scrt_offset = 96 - (i * 32);
        if (value.Length > scrt_offset) {
          acc += Mix16(seed, value[in_offset..], kXxHash3Secret.AsSpan(scrt_offset, 2));
          acc += Mix16(seed, value[in_offset..], kXxHash3Secret.AsSpan(scrt_offset + 16, 2));
        }
      }
      return AvalancheH3(acc);
    }
  }
}
}
