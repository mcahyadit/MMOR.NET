using System.Runtime.CompilerServices;

namespace MMOR.NET.Random
{
  /// <summary>
  ///     <strong><u>P</u>ermuted <u>C</u>ongruential <u>G</u>enerator</strong>
  ///     <br />
  ///     -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
  ///     <br /> - Developed by David Blackman and Sebastiano Vigna (2018)
  ///     <br /> - Statistical improvement from the basic xorshift algorithm
  ///     <br /> - Have period of 2^64.
  ///     <br /> - Implements generic abstract class with other rng for interchangibility.
  ///     <br />
  ///     -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
  /// </summary>
  public class PCG : IRandom<PCG>
  {
    protected const ulong _defaultSequence = 721347520444481703ul;
    protected const ulong Multiplier = 6364136223846793005ul;
    protected ulong inc;

    protected ulong state;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override uint NextUInt()
    {
      ulong statePrev = state;
      state = unchecked(statePrev * Multiplier + inc);
      var xorShift = (uint)(((statePrev >> 18) ^ statePrev) >> 27);
      var rotate = (int)(statePrev >> 59);

      return (xorShift >> rotate) | (xorShift << (-rotate & 31));
    }

    //-+-+-+-+-+-+-+-+
    // Initialization
    //-+-+-+-+-+-+-+-+

    #region Initialization
    public PCG()
      : this(DefaultReSeed()) { }

    public PCG(ulong seed, ulong? inc = null)
    {
      Seed = seed;

      state = 0ul;
      ulong sequence = inc ?? _defaultSequence;
      this.inc = (sequence << 1) | 1; // PCG Needs inc to be always odd

      NextUInt();
      state += seed;
      NextUInt();
    }

    public override string ToString() => $"PCG-0x{Seed:X}-0x{inc:X}";

    //-+-+-+-+-+-+-+-+
    #endregion
  }
}
