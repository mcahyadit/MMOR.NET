using System;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Random {
public abstract class MersenneTwister32Base<T> : IRandom<T>
    where T : MersenneTwister32Base<T>, new() {
  public readonly uint[] state_;
  public int s_idx_;

  public MersenneTwister32Base(int size) {
    state_ = new uint[size];
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void MersenneTwist32(Span<uint> state, in int middle, in int mask_base,
      in uint twist_matrix) {
    uint upper_mask = (uint)(~0 << mask_base);
    uint lower_mask = ~upper_mask;
    int length      = state.Length;
    unchecked {
      for (int idx = 0; idx < length - middle; ++idx) {
        uint y     = (state[idx] & upper_mask) | (state[idx + 1] & lower_mask);
        state[idx] = state[idx + middle] ^ (y >> 1) ^ ((0u - (y & 1)) & twist_matrix);
      }
      for (int idx = length - middle; idx < length - 1; ++idx) {
        uint y     = (state[idx] & upper_mask) | (state[idx + 1] & lower_mask);
        state[idx] = state[idx + (middle - length)] ^ (y >> 1) ^ ((0u - (y & 1)) & twist_matrix);
      }
      uint ylast        = (state[length - 1] & upper_mask) | (state[0] & lower_mask);
      state[length - 1] = state[middle - 1] ^ (ylast >> 1) ^ ((0u - (ylast & 1)) & twist_matrix);
    }
  }
}

/**
 * <summary>
 * <strong><u>M</u>ersenne <u>T</u>wister 19937</strong>
 * <br/> - <b>19937</b> indicates the length.
 * <br/> - Meaning the RNG will exhaust its randomness in 2 ^ 19937 - 1 iterations.
 * </summary>
 */
public class MT19937 : MersenneTwister32Base<MT19937> {
  public override uint NextUInt() {
    ++StateCount;
    if (s_idx_ >= state_.Length) {
      MersenneTwist32(state_, 397, 31, 0x9908B0DFu);
      s_idx_ = 0;
    }

    uint number = state_[s_idx_++];

    // Tempering
    number ^= number >> 11;
    number ^= (number << 7) & 0x9D2C5680;
    number ^= (number << 15) & 0xEFC60000;
    number ^= number >> 18;

    return number;
  }

  public MT19937() : this(RandUtils.GenericU32RandomSeed()) {}

  public MT19937(ulong seed) : base(624) {
    Seed = seed & 0xFFFFFFFFu;

    state_[0] = (uint)Seed;
    for (int idx = 1; idx < state_.Length; ++idx) {
      state_[idx] = 1812433253u * (state_[idx - 1] ^ (state_[idx - 1] >> 30)) + (uint)idx;
    }
    s_idx_ = state_.Length;
  }
}

/**
 * <remarks>
 *  Only slightly lower length, but more cache friendly compared to MT19937
 * </remarks>
 */
public class MT11213b : MersenneTwister32Base<MT11213b> {
  public override uint NextUInt() {
    ++StateCount;
    if (s_idx_ >= state_.Length) {
      MersenneTwist32(state_, 175, 19, 0xCCAB8EE7);
      s_idx_ = 0;
    }

    uint number = state_[s_idx_++];

    // Tempering
    number ^= number >> 11;
    number ^= (number << 7) & 0x31B6AB00;
    number ^= (number << 15) & 0xFFE50000;
    number ^= number >> 17;

    return number;
  }

  public MT11213b() : this(RandUtils.GenericU32RandomSeed()) {}

  public MT11213b(ulong seed) : base(351) {
    Seed = seed & 0xFFFFFFFFu;

    state_[0] = (uint)Seed;
    for (int idx = 1; idx < state_.Length; ++idx) {
      state_[idx] = 1812433253u * (state_[idx - 1] ^ (state_[idx - 1] >> 30)) + (uint)idx;
    }
    s_idx_ = state_.Length;
  }
}
}
