namespace MMOR.NET.Random {
  /**
   * <summary>
   * Based on
   * <see href="https://gist.github.com/orlp/32f5d1b631ab092608b1" />
   * </summary>
   * */
  public class ChaCha20 : IRandom<ChaCha20> {
    protected uint[] block_ = new uint[16];
    protected ulong block_dex_;

    protected uint[] constants_ = { 0x61707865, 0x3320646e, 0x79622d32, 0x6b206574 };
    protected ulong ctr_;
    protected uint[] input_    = new uint[16];
    protected uint[] keysetup_ = new uint[8];
    protected uint stream_;

    protected void GenerateBlock() {
      for (var i = 0; i < 4; ++i) input_[i] = constants_[i];
      for (var i = 0; i < 8; ++i) input_[4 + i] = keysetup_[i];
      input_[12] = (uint)(block_dex_ & 0xffffffffu);
      input_[13] = (uint)(block_dex_ >> 32);
      input_[14] = input_[15] = 0xdeadbeef;  // Could use 128-bit counter.

      for (var i = 0; i < 16; ++i) block_[i] = input_[i];
      ChaChaCore();
      for (var i = 0; i < 16; ++i) block_[i] += input_[i];
    }

    protected static uint ROTL(uint v, int n) => (v << n) | (v >> (32 - n));

    protected static void QuarterRound(uint[] x, int a, int b, int c, int d) {
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

    protected void ChaChaCore() {
      for (var i = 0; i < 10; i += 2) {
        QuarterRound(block_, 0, 4, 8, 12);
        QuarterRound(block_, 1, 5, 9, 13);
        QuarterRound(block_, 2, 6, 10, 14);
        QuarterRound(block_, 3, 7, 11, 15);
        QuarterRound(block_, 0, 5, 10, 15);
        QuarterRound(block_, 1, 6, 11, 12);
        QuarterRound(block_, 2, 7, 8, 13);
        QuarterRound(block_, 3, 4, 9, 14);
      }
    }

    public override uint NextUInt() {
      ulong next_block_dex = ctr_ / 16;
      ulong dex_in_block   = ctr_ % 16;
      if (next_block_dex != block_dex_) {
        block_dex_ = next_block_dex;
        GenerateBlock();
      }

      ++ctr_;

      return block_[dex_in_block];
    }

//-+-+-+-+-+-+-+-+
// Initialization
//-+-+-+-+-+-+-+-+
#region Initialization
    public ChaCha20() : this(DefaultReSeed()) {}

    public ChaCha20(ulong seed, ulong? stream = null) {
      Seed = seed;

      ctr_       = 0;
      block_dex_ = ulong.MaxValue;

      uint seed_val;
      if (stream == null) {
        SplitMix64 sm = new(seed);
        seed_val      = (uint)sm.Next();
        stream_       = (uint)sm.Next();

        Seed = seed_val;
      } else {
        seed_val = (uint)seed;
        stream_  = (uint)stream;
      }

      keysetup_[0] = seed_val & 0xffffffffu;
      keysetup_[1] = (uint)(seed >> 32);
      keysetup_[2] = 0xdeadbeef;
      keysetup_[3] = 0xdeadbeef;
      keysetup_[4] = stream_ & 0xffffffffu;
      keysetup_[5] = (uint)(seed >> 32);
      keysetup_[6] = 0xdeadbeef;
      keysetup_[7] = 0xdeadbeef;
    }

    public override string ToString() => $"ChaCha-0x{Seed:X}-0x{stream_:X}";

//-+-+-+-+-+-+-+-+
#endregion
  }
}
