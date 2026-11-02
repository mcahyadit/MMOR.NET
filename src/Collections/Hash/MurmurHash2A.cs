using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MMOR.NET.Collections.Hash {
public static partial class HashUtils {
  public static ulong MurmurHash2A(ReadOnlySpan<byte> value, ulong seed) {
    ulong m = 0xC6A4A7935BD1E995;
    byte r  = 47;

    int len   = value.Length;
    ulong h   = seed ^ ((ulong)len * m);
    int block = len & -sizeof(ulong);

    ReadOnlySpan<ulong> value_u64 = MemoryMarshal.Cast<byte, ulong>(value[..block]);

    for (int i = 0; i < value_u64.Length; ++i) {
      ulong k = value_u64[i];
      k *= m;
      k ^= k >> r;
      k *= m;

      h ^= k;
      h *= m;
    }

    switch (len & 7) {
      case 7: h ^= (ulong)value[6] << 48; goto case 6;
      case 6: h ^= (ulong)value[5] << 40; goto case 5;
      case 5: h ^= (ulong)value[4] << 32; goto case 4;
      case 4: h ^= (ulong)value[3] << 24; goto case 3;
      case 3: h ^= (ulong)value[2] << 16; goto case 2;
      case 2: h ^= (ulong)value[1] << 8; goto case 1;
      case 1: h ^= value[0]; goto default;
      default: h *= m; break;
    }

    h ^= h >> r;
    h *= m;
    h ^= h >> r;

    return h;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong MurmurHash2A<T>(ReadOnlySpan<T> value, ulong seed)
      where T : unmanaged => MurmurHash2A(MemoryMarshal.Cast<T, byte>(value), seed);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong MurmurHash2A(string value, ulong seed) => MurmurHash2A(
      MemoryMarshal.Cast<char, byte>(value), seed);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong MurmurHash2A<T>(T value, ulong seed)
      where T : unmanaged {
    return MurmurHash2A(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref value, 1)),
        seed);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong MurmurHash2A(ulong value, in ulong seed) {
    ulong m = 0xC6A4A7935BD1E995;
    byte r  = 47;

    ulong h = seed ^ (sizeof(ulong) * m);

    value *= m;
    value ^= value >> r;
    value *= m;

    h ^= value;
    h *= m;

    h ^= h >> r;
    h *= m;
    h ^= h >> r;

    return h;
  }
}
}
