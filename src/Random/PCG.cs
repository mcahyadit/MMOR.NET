using System.Runtime.CompilerServices;

namespace MMOR.NET.Random {
  /**
   * <summary>
   * <strong><u>P</u>ermuted <u>C</u>ongruential <u>G</u>enerator</strong>
   * <br/> - Developed by Dr. M.E. O'Neill (2014).
   * <br/> - A statistical improvement over Linear Congruential Generator using permutation.
   * <br/> - Have period of 2^64.
   * </summary>
   * */
  public class PCG : IRandom<PCG> {
    protected const ulong kDefaultSequence = 721347520444481703ul;
    protected const ulong kMultiplier      = 6364136223846793005ul;
    protected ulong inc_;

    protected ulong state_;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override uint NextUInt() {
      ulong state_prev = state_;
      state_           = unchecked(state_prev * kMultiplier + inc_);
      var xor_shift    = (uint)(((state_prev >> 18) ^ state_prev) >> 27);
      var rotate       = (int)(state_prev >> 59);

      return (xor_shift >> rotate) | (xor_shift << (-rotate & 31));
    }

    public PCG() : this(DefaultReSeed()) {}

    public PCG(ulong seed, ulong? inc = null) {
      Seed = seed;

      state_         = 0ul;
      ulong sequence = inc ?? kDefaultSequence;
      this.inc_      = (sequence << 1) | 1;  // PCG Needs inc to be always odd

      NextUInt();
      state_ += seed;
      NextUInt();
    }

    public override string ToString() => $"PCG-0x{Seed:X}-0x{inc_:X}";
  }
}
