using System.Runtime.CompilerServices;

namespace MMOR.NET.Random
{
  /**
   * <summary>
   * Based on
   * <see href="https://gist.github.com/orlp/32f5d1b631ab092608b1" />
   * </summary>
   * */
    protected uint[] block = new uint[16];
    protected ulong blockDex;

    protected uint[] constants = { 0x61707865, 0x3320646e, 0x79622d32, 0x6b206574 };
    protected ulong ctr;
    protected uint[] input = new uint[16];
    protected uint[] keysetup = new uint[8];
    protected uint stream;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void GenerateBlock()
    {
      for (var i = 0; i < 4; ++i)
        input[i] = constants[i];
      for (var i = 0; i < 8; ++i)
        input[4 + i] = keysetup[i];
      input[12] = (uint)(blockDex & 0xffffffffu);
      input[13] = (uint)(blockDex >> 32);
      input[14] = input[15] = 0xdeadbeef; // Could use 128-bit counter.

      for (var i = 0; i < 16; ++i)
        block[i] = input[i];
      ChaChaCore();
      for (var i = 0; i < 16; ++i)
        block[i] += input[i];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static uint ROTL(uint v, int n) => (v << n) | (v >> (32 - n));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static void QuarterRound(uint[] x, int a, int b, int c, int d)
    {
      x[a] += x[b];
      x[d] ^= x[a];
      x[d] = ROTL(x[d], 16);
      x[c] += x[d];
      x[b] ^= x[c];
      x[b] = ROTL(x[b], 12);
      x[a] += x[b];
      x[d] ^= x[a];
      x[d] = ROTL(x[d], 8);
      x[c] += x[d];
      x[b] ^= x[c];
      x[b] = ROTL(x[b], 7);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void ChaChaCore()
    {
      for (var i = 0; i < 10; i += 2)
      {
        QuarterRound(block, 0, 4, 8, 12);
        QuarterRound(block, 1, 5, 9, 13);
        QuarterRound(block, 2, 6, 10, 14);
        QuarterRound(block, 3, 7, 11, 15);
        QuarterRound(block, 0, 5, 10, 15);
        QuarterRound(block, 1, 6, 11, 12);
        QuarterRound(block, 2, 7, 8, 13);
        QuarterRound(block, 3, 4, 9, 14);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override uint NextUInt()
    {
      ulong nextBlockDex = ctr / 16;
      ulong dexInBlock = ctr % 16;
      if (nextBlockDex != blockDex)
      {
        blockDex = nextBlockDex;
        GenerateBlock();
      }

      ++ctr;

      return block[dexInBlock];
    }

    //-+-+-+-+-+-+-+-+
    // Initialization
    //-+-+-+-+-+-+-+-+
    #region Initialization
    public ChaCha20()
      : this(DefaultReSeed()) { }

    public ChaCha20(ulong seed, ulong? stream = null)
    {
      Seed = seed;

      ctr = 0;
      blockDex = ulong.MaxValue;

      uint seedVal;
      if (stream == null)
      {
        SplitMix64 sm = new(seed);
        seedVal = (uint)sm.Next();
        this.stream = (uint)sm.Next();

        Seed = seedVal;
      }
      else
      {
        seedVal = (uint)seed;
        this.stream = (uint)stream;
      }

      keysetup[0] = seedVal & 0xffffffffu;
      keysetup[1] = (uint)(seed >> 32);
      keysetup[2] = keysetup[3] = 0xdeadbeef;
      keysetup[4] = this.stream & 0xffffffffu;
      keysetup[5] = (uint)(seed >> 32);
      keysetup[6] = keysetup[7] = 0xdeadbeef;
    }

    public override string ToString() => $"ChaCha-0x{Seed:X}-0x{stream:X}";

    //-+-+-+-+-+-+-+-+
    #endregion
  }
}
