#if !NETSTANDARD
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace MMOR.NET.Random {
public abstract partial class DjbChaChaBase<T> {
  // https://codeberg.org/ziglang/zig/src/commit/4dd65e63f23569782f1560d40217cc74aae8b6cb/lib/std/crypto/chacha20.zig
  Span<Vector128<uint>> mm128_block_epi_ => MemoryMarshal.Cast<uint, Vector128<uint>>(block_);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<uint> RotateLeft(in Vector128<uint> x, int offset) {
    return x << offset | x >> (32 - offset);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Mm128DoubleRound() {
    mm128_block_epi_[0] += mm128_block_epi_[1];
    mm128_block_epi_[3] ^= mm128_block_epi_[0];
    mm128_block_epi_[3] = RotateLeft(mm128_block_epi_[3], 16);

    mm128_block_epi_[2] += mm128_block_epi_[3];
    mm128_block_epi_[1] ^= mm128_block_epi_[2];
    mm128_block_epi_[1] = RotateLeft(mm128_block_epi_[1], 12);

    mm128_block_epi_[0] += mm128_block_epi_[1];
    mm128_block_epi_[3] ^= mm128_block_epi_[0];
    mm128_block_epi_[0] = Vector128.Shuffle(mm128_block_epi_[0], Vector128.Create(3, 0u, 1, 2));
    mm128_block_epi_[3] = RotateLeft(mm128_block_epi_[3], 8);

    mm128_block_epi_[2] += mm128_block_epi_[3];
    mm128_block_epi_[3] = Vector128.Shuffle(mm128_block_epi_[3], Vector128.Create(2, 3, 0u, 1));
    mm128_block_epi_[1] ^= mm128_block_epi_[2];
    mm128_block_epi_[2] = Vector128.Shuffle(mm128_block_epi_[2], Vector128.Create(1, 2, 3, 0u));
    mm128_block_epi_[1] = RotateLeft(mm128_block_epi_[1], 7);

    mm128_block_epi_[0] += mm128_block_epi_[1];
    mm128_block_epi_[3] ^= mm128_block_epi_[0];
    mm128_block_epi_[3] = RotateLeft(mm128_block_epi_[3], 16);

    mm128_block_epi_[2] += mm128_block_epi_[3];
    mm128_block_epi_[1] ^= mm128_block_epi_[2];
    mm128_block_epi_[1] = RotateLeft(mm128_block_epi_[1], 12);

    mm128_block_epi_[0] += mm128_block_epi_[1];
    mm128_block_epi_[3] ^= mm128_block_epi_[0];
    mm128_block_epi_[0] = Vector128.Shuffle(mm128_block_epi_[0], Vector128.Create(1, 2, 3, 0u));
    mm128_block_epi_[3] = RotateLeft(mm128_block_epi_[3], 8);

    mm128_block_epi_[2] += mm128_block_epi_[3];
    mm128_block_epi_[3] = Vector128.Shuffle(mm128_block_epi_[3], Vector128.Create(2, 3, 0u, 1));
    mm128_block_epi_[1] ^= mm128_block_epi_[2];
    mm128_block_epi_[2] = Vector128.Shuffle(mm128_block_epi_[2], Vector128.Create(3, 0u, 1, 2));
    mm128_block_epi_[1] = RotateLeft(mm128_block_epi_[1], 7);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<uint> RotateLeft(in Vector256<uint> x, int offset) {
    return x << offset | x >> (32 - offset);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Mm256DoubleRound() {
    Vector256<uint> a = Vector256.Create(mm128_block_epi_[0], mm128_block_epi_[0]);
    Vector256<uint> b = Vector256.Create(mm128_block_epi_[1], mm128_block_epi_[1]);
    Vector256<uint> c = Vector256.Create(mm128_block_epi_[2], mm128_block_epi_[2]);
    Vector256<uint> d = Vector256.Create(mm128_block_epi_[3],  //
        mm128_block_epi_[3] + Vector128.Create(1u, 0, 0, 0));

    a += b;
    d ^= a;
    d = RotateLeft(d, 16);

    c += d;
    b ^= c;
    b = RotateLeft(b, 12);

    a += b;
    d ^= a;
    a = Vector256.Shuffle(a, Vector256.Create(3, 0u, 1, 2, 7, 4, 5, 6));
    d = RotateLeft(d, 8);

    c += d;
    d = Vector256.Shuffle(d, Vector256.Create(2, 3, 0u, 1, 6, 7, 4, 5));
    b ^= c;
    c = Vector256.Shuffle(c, Vector256.Create(1, 2, 3, 0u, 5, 6, 7, 4));
    b = RotateLeft(b, 7);

    a += b;
    d ^= a;
    d = RotateLeft(d, 16);

    c += d;
    b ^= c;
    b = RotateLeft(b, 12);

    a += b;
    d ^= a;
    a = Vector256.Shuffle(a, Vector256.Create(1, 2, 3, 0u, 5, 6, 7, 4));
    d = RotateLeft(d, 8);

    c += d;
    d = Vector256.Shuffle(d, Vector256.Create(2, 3, 0u, 1, 6, 7, 4, 5));
    b ^= c;
    c = Vector256.Shuffle(c, Vector256.Create(3, 0u, 1, 2, 7, 4, 5, 6));
    b = RotateLeft(b, 7);

    mm128_block_epi_[0] = a.GetLower();
    mm128_block_epi_[1] = b.GetLower();
    mm128_block_epi_[2] = c.GetLower();
    mm128_block_epi_[3] = d.GetLower();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Mm512DoubleRound() {
    var a = Vector512.Create(Vector256.Create(mm128_block_epi_[0], mm128_block_epi_[0]),
        Vector256.Create(mm128_block_epi_[0], mm128_block_epi_[0]));
    var b = Vector512.Create(Vector256.Create(mm128_block_epi_[1], mm128_block_epi_[1]),
        Vector256.Create(mm128_block_epi_[1], mm128_block_epi_[1]));
    var c = Vector512.Create(Vector256.Create(mm128_block_epi_[2], mm128_block_epi_[2]),
        Vector256.Create(mm128_block_epi_[2], mm128_block_epi_[2]));
    var d = Vector512.Create(
        Vector256.Create(mm128_block_epi_[3], mm128_block_epi_[3] + Vector128.Create(1u, 0, 0, 0)),
        Vector256.Create(mm128_block_epi_[3] + Vector128.Create(2u, 0, 0, 0),
            mm128_block_epi_[3] + Vector128.Create(3u, 0, 0, 0)));

    a += b;
    d ^= a;
    d = Avx512F.RotateLeft(d, 16);

    c += d;
    b ^= c;
    b = Avx512F.RotateLeft(b, 12);

    a += b;
    d ^= a;
    a = Vector512.Shuffle(a,
        Vector512.Create(3, 0u, 1, 2, 7, 4, 5, 6, 11, 8, 9, 10, 15, 12, 13, 14));
    d = Avx512F.RotateLeft(d, 8);

    c += d;
    d = Vector512.Shuffle(d,
        Vector512.Create(2, 3, 0u, 1, 6, 7, 4, 5, 10, 11, 8, 9, 14, 15, 12, 13));
    b ^= c;
    c = Vector512.Shuffle(c,
        Vector512.Create(1, 2, 3, 0u, 5, 6, 7, 4, 9, 10, 11, 8, 13, 14, 15, 12));
    b = Avx512F.RotateLeft(b, 7);

    a += b;
    d ^= a;
    d = Avx512F.RotateLeft(d, 16);

    c += d;
    b ^= c;
    b = Avx512F.RotateLeft(b, 12);

    a += b;
    d ^= a;
    a = Vector512.Shuffle(a,
        Vector512.Create(1, 2, 3, 0u, 5, 6, 7, 4, 9, 10, 11, 8, 13, 14, 15, 12));
    d = Avx512F.RotateLeft(d, 8);

    c += d;
    d = Vector512.Shuffle(d,
        Vector512.Create(2, 3, 0u, 1, 6, 7, 4, 5, 10, 11, 8, 9, 14, 15, 12, 13));
    b ^= c;
    c = Vector512.Shuffle(c,
        Vector512.Create(3, 0u, 1, 2, 7, 4, 5, 6, 11, 8, 9, 10, 15, 12, 13, 14));
    b = Avx512F.RotateLeft(b, 7);

    mm128_block_epi_[0] = a.GetLower().GetLower();
    mm128_block_epi_[1] = b.GetLower().GetLower();
    mm128_block_epi_[2] = c.GetLower().GetLower();
    mm128_block_epi_[3] = d.GetLower().GetLower();
  }
}
}
#endif
