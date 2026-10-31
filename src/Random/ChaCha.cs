using System;
using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if !NETSTANDARD
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif

namespace MMOR.NET.Random {
public abstract partial class DjbChaChaBase<T> : IRandom<T>, IRandomJumpable, IRandomDoubleSeed
    where T : IRandom<T>, new() {
  [DoesNotReturn]
  public static void ThrowKeyLength<TKeys>(ReadOnlySpan<TKeys> keys)
      where TKeys : unmanaged {
    throw new ArgumentException(
        "[ERROR]: ChaCha keys length must be exactly 32 or 16 bytes.\n" +
        $"Passed keys have {keys.Length} {typeof(TKeys)}" +
        (typeof(TKeys) == typeof(byte) ? $" ({keys.Length * Unsafe.SizeOf<TKeys>()} bytes)" : ""));
  }

  [DoesNotReturn]
  public static void ThrowRounds(int rounds) {
    throw new ArgumentException(
        "[ERROR]: ChaCha rounds needs to be a multiple of 2 and at least 2.\n" +
        $"Passed rounds is {rounds}");
  }

  public ulong Seed2 { get; }
  public bool IsDoubleSeeded { get; }

  public static readonly ImmutableArray<uint> kConstants32 =
      ImmutableArray.Create(stackalloc uint[4] {
        0x61707865,  //  "expa"
        0x3320646E,  //  "nd 3"
        0x79622D32,  //  "2-by"
        0x6b206574,  //  "te k"
      });
  public static readonly ImmutableArray<uint> kConstants16 =
      ImmutableArray.Create(stackalloc uint[4] {
        0x61707865,  //  "expa"
        0x3120646E,  //  "nd 1"
        0x79622D36,  //  "6-by"
        0x6b206574,  //  "te k"
      });

  public const int kBlockWords              = 16;  // 512-bit
  public static readonly int kBlockWordsEpi = kBlockWords / Vector<uint>.Count;
  public uint[] block_                      = new uint[kBlockWords];
  public ulong block_index_                 = ulong.MaxValue;

  public readonly ImmutableArray<uint> seed_;
  public readonly ulong kNonce = 0ul;
  public readonly int kRounds  = 0;

  public void GenerateBlock(ulong counter) {
    unchecked {
      Span<uint> state = stackalloc uint[kBlockWords];
      if (seed_.Length == 8) {
        kConstants32.CopyTo(state);
        seed_.CopyTo(state[4..]);
      } else {
        kConstants16.CopyTo(state);
        seed_.CopyTo(state[4..]);
        seed_.CopyTo(state[8..]);
      }
      state[12] = (uint)(counter & ~0u);
      state[13] = (uint)(counter >> 32);
      state[14] = (uint)(kNonce & ~0u);
      state[15] = (uint)(kNonce >> 32);

      state.CopyTo(block_);
      for (int i = 0; i < kRounds; i += 2) {
#if !NETSTANDARD
        if (Avx512F.IsSupported && Vector512.IsHardwareAccelerated && Vector512<uint>.IsSupported) {
          Mm256DoubleRound();
          continue;
        }
        if (Vector256.IsHardwareAccelerated && Vector256<uint>.IsSupported) {
          Mm256DoubleRound();
          continue;
        }
        if (Vector128.IsHardwareAccelerated && Vector128<uint>.IsSupported) {
          Mm128DoubleRound();
          continue;
        }
#endif
        // NOTE: Read this column-major
        QuarterRound(0, 4, 8, 12);
        QuarterRound(1, 5, 9, 13);
        QuarterRound(2, 6, 10, 14);
        QuarterRound(3, 7, 11, 15);
        // 4 QuarterRound => 1 Round
        QuarterRound(0, 5, 10, 15);
        QuarterRound(1, 6, 11, 12);
        QuarterRound(2, 7, 8, 13);
        QuarterRound(3, 4, 9, 14);
        // 8 QuarterRound => 2 Rounds
      }

      if (Vector.IsHardwareAccelerated) {
        int lane_cnt                 = Vector<uint>.Count;
        int length_epi               = kBlockWords / lane_cnt;
        Span<Vector<uint>> block_epi = MemoryMarshal.Cast<uint, Vector<uint>>(block_);
        Span<Vector<uint>> state_epi = MemoryMarshal.Cast<uint, Vector<uint>>(state);
        for (int i = 0; i < kBlockWordsEpi; ++i) {
          block_epi[i] += state_epi[i];
        }
      } else {
        for (int i = 0; i < kBlockWords; ++i) {
          block_[i] += state[i];
        }
      }
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void QuarterRound(int a, int b, int c, int d) {
    unchecked {
      block_[a] += block_[b];
      block_[d] = BitOperations.RotateLeft(block_[d] ^ block_[a], 16);
      block_[c] += block_[d];
      block_[b] = BitOperations.RotateLeft(block_[b] ^ block_[c], 12);
      block_[a] += block_[b];
      block_[d] = BitOperations.RotateLeft(block_[d] ^ block_[a], 8);
      block_[c] += block_[d];
      block_[b] = BitOperations.RotateLeft(block_[b] ^ block_[c], 7);
    }
  }

  public override uint NextUInt() {
    unchecked {
      ulong block = StateCount / kBlockWords;
      if (block != block_index_) {
        GenerateBlock(block);
        block_index_ = block;
      }
      return block_[(int)(StateCount++ % kBlockWords)];
    }
  }

  public DjbChaChaBase(int rounds, ulong seed, ulong nonce = 0) {
    Seed = seed;
    if ((rounds % 2 != 0) || rounds < 2)
      ThrowRounds(rounds);
    kRounds = rounds;

    Span<uint> seeds = stackalloc uint[8];
    for (int i = 0; i < 8; i += 2) {
      ulong mix    = RandUtils.SplitMix64(ref seed);
      seeds[i]     = (uint)(mix & ~0u);
      seeds[i + 1] = (uint)(mix >> 32);
    }
    seed_ = ImmutableArray.Create(seeds);

    if (nonce != 0) {
      IsDoubleSeeded = true;
      Seed2          = nonce;
      kNonce         = nonce;
    }
  }

  public DjbChaChaBase(int rounds, ReadOnlySpan<uint> keys, ulong nonce = 0) {
    // NOTE: Consider changing the API of IRandom
    Seed = 0;
    if ((rounds % 2 != 0) || rounds < 2)
      ThrowRounds(rounds);
    kRounds = rounds;

    if (keys.Length == 8 || keys.Length == 4) {
      seed_ = ImmutableArray.Create(keys);
    } else {
      ThrowKeyLength(keys);
    }
    if (nonce != 0) {
      IsDoubleSeeded = true;
      Seed2          = nonce;
      kNonce         = nonce;
    }
  }

  public DjbChaChaBase(int rounds, ReadOnlySpan<byte> keys, ulong nonce = 0) {
    // NOTE: Consider changing the API of IRandom
    Seed = 0;
    if ((rounds % 2 != 0) || rounds < 2)
      ThrowRounds(rounds);
    kRounds = rounds;

    if (keys.Length == 32) {
      Span<uint> seeds = stackalloc uint[8];
      for (int i = 0; i < 8; ++i) {
        seeds[i] = BinaryPrimitives.ReadUInt32LittleEndian(keys);
        keys     = keys[sizeof(uint)..];
      }
      seed_ = ImmutableArray.Create(seeds);
    } else if (keys.Length == 16) {
      Span<uint> seeds = stackalloc uint[4];
      for (int i = 0; i < 4; ++i) {
        uint seed = BinaryPrimitives.ReadUInt32LittleEndian(keys);
        seeds[i]  = seed;
        keys      = keys[sizeof(uint)..];
      }

      seed_ = ImmutableArray.Create(seeds);
    } else {
      ThrowKeyLength(keys);
    }

    if (nonce != 0) {
      IsDoubleSeeded = true;
      Seed2          = nonce;
      kNonce         = nonce;
    }
  }

  public void Jump(ulong delta) {
    StateCount += delta;
  }
}

public class ChaCha20 : DjbChaChaBase<ChaCha20> {
  public ChaCha20() : base(20, RandUtils.GenericU32RandomSeed()) {}
  public ChaCha20(ulong seed, ulong nonce = 0) : base(20, seed, nonce) {}
  public ChaCha20(ReadOnlySpan<uint> seeds, ulong nonce = 0) : base(20, seeds, nonce) {}
  public ChaCha20(ReadOnlySpan<byte> seeds, ulong nonce = 0) : base(20, seeds, nonce) {}
}

public class ChaCha12 : DjbChaChaBase<ChaCha12> {
  public ChaCha12() : base(12, RandUtils.GenericU32RandomSeed()) {}
  public ChaCha12(ulong seed, ulong nonce = 0) : base(12, seed, nonce) {}
  public ChaCha12(ReadOnlySpan<uint> seeds, ulong nonce = 0) : base(12, seeds, nonce) {}
  public ChaCha12(ReadOnlySpan<byte> seeds, ulong nonce = 0) : base(12, seeds, nonce) {}
}

public class ChaCha8 : DjbChaChaBase<ChaCha8> {
  public ChaCha8() : base(8, RandUtils.GenericU32RandomSeed()) {}
  public ChaCha8(ulong seed, ulong nonce = 0) : base(8, seed, nonce) {}
  public ChaCha8(ReadOnlySpan<uint> seeds, ulong nonce = 0) : base(8, seeds, nonce) {}
  public ChaCha8(ReadOnlySpan<byte> seeds, ulong nonce = 0) : base(8, seeds, nonce) {}
}
}
