namespace MMOR.NET.Random {
  /**
   * <summary>
   * <strong><u>M</u>ersenne <u>T</u>wister 19937</strong>
   * <br/> - <b>19937</b> indicates the length.
   * <br/> - Meaning the RNG will exhaust its randomness in 2 ^ 19937 - 1 iterations.
   * </summary>
   * */
  public class MT19937 : IRandom<MT19937> {
    protected const int N             = 624;
    protected const int M             = 397;
    protected const uint matrix_a_    = 0x9908b0dfU;
    protected const uint upper_mask_  = 0x80000000U;
    protected const uint lower_mask_  = 0x7fffffffU;
    protected readonly uint[] mt_val_ = new uint[N];
    protected int mt_dex_             = N + 1;

    public override uint NextUInt() {
      if (mt_dex_ >= N) {
        for (var k = 0; k < N - M; k++) {
          uint y     = (mt_val_[k] & upper_mask_) | (mt_val_[k + 1] & lower_mask_);
          mt_val_[k] = mt_val_[k + M] ^ (y >> 1) ^ ((y & 0x1) * matrix_a_);
        }

        for (int k = N - M; k < N - 1; k++) {
          uint y     = (mt_val_[k] & upper_mask_) | (mt_val_[k + 1] & lower_mask_);
          mt_val_[k] = mt_val_[k + (M - N)] ^ (y >> 1) ^ ((y & 0x1) * matrix_a_);
        }

        uint last_y     = (mt_val_[N - 1] & upper_mask_) | (mt_val_[0] & lower_mask_);
        mt_val_[N - 1] = mt_val_[M - 1] ^ (last_y >> 1) ^ ((last_y & 0x1) * matrix_a_);

        mt_dex_ = 0;
      }

      uint number = mt_val_[mt_dex_++];

      // Tempering
      number ^= number >> 11;
      number ^= (number << 7) & 0x9d2c5680;
      number ^= (number << 15) & 0xefc60000;
      number ^= number >> 18;

      return number;
    }

    public MT19937() : this(DefaultReSeed()) {}

    public MT19937(ulong seed) {
      Seed       = seed;
      mt_val_[0] = (uint)seed;
      for (mt_dex_ = 1; mt_dex_ < N; mt_dex_++)
        mt_val_[mt_dex_] =
            1812433253u * (mt_val_[mt_dex_ - 1] ^ (mt_val_[mt_dex_ - 1] >> 30)) + (uint)mt_dex_;
    }

    public override string ToString() => $"MT19937-0x{Seed:X}";
  }
}
