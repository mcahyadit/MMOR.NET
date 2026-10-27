namespace MMOR.NET.Random {
/**
 * <summary>
 * <strong><u>P</u>ermuted <u>C</u>ongruential <u>G</u>enerator</strong>
 * <br/> - Developed by Dr. M.E. O'Neill (2014).
 * <br/> - A statistical improvement over Linear Congruential Generator using permutation.
 * <br/> - Have period of 2^64.
 * </summary>
 */
public class PCG : IRandom<PCG>, IRandomDoubleSeed, IRandomJumpable {
  public ulong Seed2 { get; }
  public bool IsDoubleSeeded { get; }

  protected const ulong kDefaultSequence = 721347520444481703ul;
  protected const ulong kMultiplier      = 6364136223846793005ul;
  protected ulong inc_;

  protected ulong state_;

  public override uint NextUInt() {
    ++StateCount;
    unchecked {
      ulong p_state  = state_;
      state_         = p_state * kMultiplier + (inc_ | 1);
      uint xor_shift = (uint)(((p_state >> 18) ^ p_state) >> 27);
      int rotate     = (int)(p_state >> 59);

      return (xor_shift >> rotate) | (xor_shift << (-rotate & 31));
    }
  }

  public PCG() : this(RandUtils.GenericU32RandomSeed()) {}

  public PCG(ulong seed, ulong? inc = null) {
    Seed = seed;

    ulong sequence;
    if (inc is null) {
      RandUtils.SplitMix64(RandUtils.SplitMix64(seed, out seed), out sequence);
    } else {
      IsDoubleSeeded = true;
      Seed2          = inc.Value;
      sequence       = inc.Value;
    }
    state_ = 0ul;
    inc_   = (seed << 1) | 1;  // PCG Needs inc to be always odd

    unchecked {
      state_ = state_ * kMultiplier + inc_;
      state_ += sequence;
      state_ = state_ * kMultiplier + inc_;
    }
  }

  public void Jump(ulong steps) {
    if (steps == 0)
      return;

    ulong curr_mult = kMultiplier;
    ulong curr_plus = inc_;
    ulong acc_mult  = 1;
    ulong acc_plus  = 0;

    for (ulong d = steps; d > 0; d >>= 1) {
      if ((d & 1) != 0) {
        acc_mult = unchecked(acc_mult * curr_mult);
        acc_plus = unchecked(acc_plus * curr_mult + curr_plus);
      }
      curr_plus = unchecked(curr_plus * (curr_mult + 1));
      curr_mult = unchecked(curr_mult * curr_mult);
    }

    state_ = unchecked(acc_mult * state_ + acc_plus);
    StateCount += steps;
  }
}
}
