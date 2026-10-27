using System;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Random {
public abstract class MersenneTwister64Base<T> : IRandom<T>, IRandom64
    where T : MersenneTwister64Base<T>, new() {
  public readonly ulong[] state_;
  public int s_idx_;

  public MersenneTwister64Base(int size) {
    state_ = new ulong[size];
  }

  public abstract ulong NextUInt64();
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override uint NextUInt() => (uint)this.NextUInt64(0, uint.MaxValue);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void MersenneTwist64(Span<ulong> state, in int middle, in int mask_base,
      in ulong twist_matrix) {
    ulong upper_mask = (ulong)(~0 << mask_base);
    ulong lower_mask = ~upper_mask;
    int length       = state.Length;
    unchecked {
      for (int idx = 0; idx < length - middle; ++idx) {
        ulong y    = (state[idx] & upper_mask) | (state[idx + 1] & lower_mask);
        state[idx] = state[idx + middle] ^ (y >> 1) ^ ((0u - (y & 1)) & twist_matrix);
      }
      for (int idx = length - middle; idx < length - 1; ++idx) {
        ulong y    = (state[idx] & upper_mask) | (state[idx + 1] & lower_mask);
        state[idx] = state[idx + (middle - length)] ^ (y >> 1) ^ ((0u - (y & 1)) & twist_matrix);
      }
      ulong ylast       = (state[length - 1] & upper_mask) | (state[0] & lower_mask);
      state[length - 1] = state[middle - 1] ^ (ylast >> 1) ^ ((0u - (ylast & 1)) & twist_matrix);
    }
  }
}

/// <inheritdoc cref="MT19937"/>
/// <remarks> 64-bit version. </remarks>
public class MT19937u64 : MersenneTwister64Base<MT19937u64> {
  public override ulong NextUInt64() {
    ++StateCount;
    if (s_idx_ >= state_.Length) {
      MersenneTwist64(state_, 156, 31, 0xB5026F5AA96619E9ul);
      s_idx_ = 0;
    }

    ulong number = state_[s_idx_++];

    // Tempering
    number ^= (number >> 29) & 0x5555555555555555ul;
    number ^= (number << 17) & 0x71D67FFFEDA60000ul;
    number ^= (number << 37) & 0xFFF7EEE000000000ul;
    number ^= number >> 43;

    return number;
  }

  public MT19937u64() : this(RandUtils.GenericU32RandomSeed()) {}

  public MT19937u64(ulong seed) : base(312) {
    Seed = seed;

    state_[0]        = Seed;
    const ulong mult = 6364136223846793005;
    for (int idx = 1; idx < state_.Length; ++idx) {
      state_[idx] = mult * (state_[idx - 1] ^ (state_[idx - 1] >> 62)) + (ulong)idx;
    }
    s_idx_ = state_.Length;
  }
}
}
