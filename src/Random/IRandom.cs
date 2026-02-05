using System;
using System.Threading;
using MMOR.NET.Mathematics;

namespace MMOR.NET.Random {
  public abstract class IRandom<T> : IRandom
      where T : IRandom {
    private static T? global_;
    /**
     * <summary>
     * A lazyily instantiated <see langword="static"/> instance of <typeparamref name="T"/>.
     * </summary>
     * */
    public static T global => global_ ??= Activator.CreateInstance<T>();
  }

  /**
   * <summary>
   * <strong>Generic Pseudo Random Number Generator abstraction.</strong>
   * <br/> - For easily swapping out different algorithm when needed.
   * </summary>
   * */
  public abstract class IRandom {
    /**
     * <summary>
     * The number which the current PRNG is seeded with.
     * </summary>
     * */
    public ulong Seed { get; protected set; }

    protected static ulong DefaultReSeed() {
      var seed = (ulong)(Environment.TickCount ^ Thread.CurrentThread.ManagedThreadId ^
                         Guid.NewGuid().GetHashCode());
      seed     = (seed >> 32) ^ seed;
      return (uint)seed;
    }

    /**
     * <summary>Generates a random 32-bit unsigned integer.</summary>
     * <returns>Random 32-bit unsigned integer between.</returns>
     * */
    public abstract uint NextUInt();  // Primary Result

    /**
     * <summary>
     *  Generates a random 32-bit unsigned integer between <paramref name="min_inclusive"/> and
     *  <paramref name="max_exclusive"/>.
     *  <br/> Respecting modulo bias.
     *  <br/> In turn, <see cref="StateCount"/> might be incremented more than once.
     * </summary>
     * <returns>
     *  Random 32-bit unsigned integer between <paramref name="min_inclusive"/> and
     *  <paramref name="max_exclusive"/>.
     * </returns>
     * <exception cref="ArgumentException">
     *  Thrown when <paramref name="max_exclusive"/> is less than or equal to
     *  <paramref name="min_inclusive"/>.
     * </exception>
     * */
    public uint NextUInt(uint min_inclusive, uint max_exclusive) {
      if (min_inclusive >= max_exclusive)
        throw new ArgumentException(
            $"max_exclusive {max_exclusive} must be greater than min_inclusive {min_inclusive}");

      uint range = max_exclusive - min_inclusive;
      uint limit = uint.MaxValue - uint.MaxValue % range;
      uint result;

      do {
        result = NextUInt();
      } while (result >= limit);

      return min_inclusive + result % range;
    }

    /**
     * <summary>
     *  Generates a random 32-bit signed integer between <paramref name="min_inclusive"/> and
     *  <paramref name="max_exclusive"/>.
     *  <br/> Respecting modulo bias.
     *  <br/> In turn, <see cref="StateCount"/> might be incremented more than once.
     * </summary>
     * <returns>
     *  Random 32-bit signed integer between <paramref name="min_inclusive"/> and
     *  <paramref name="max_exclusive"/>.
     * </returns>
     * <exception cref="ArgumentException">
     *  Thrown when <paramref name="max_exclusive"/> is less than or equal to
     *  <paramref name="min_inclusive"/>.
     * </exception>
     * */
    public int NextInt(int min_inclusive, int max_exclusive) {
      if (min_inclusive >= max_exclusive)
        throw new ArgumentException(
            $"max_exclusive {max_exclusive} must be greater than min_inclusive {min_inclusive}");
      var range = (uint)(max_exclusive - min_inclusive);
      return min_inclusive + (int)NextUInt(0, range);
    }

    /**
     * <summary>
     *  Generates a random <see cref="float"/> between <paramref name="min_inclusive"/> and
     *  <paramref name="max_inclusive"/>.
     *  <br/> Respecting modulo bias.
     *  <br/> In turn, <see cref="StateCount"/> might be incremented more than once.
     * </summary>
     * <returns>
     *  Random <see cref="float"/> between <paramref name="min_inclusive"/> and
     *  <paramref name="max_inclusive"/>.
     * </returns>
     * <inception cref="ArgumentException">
     *  Thrown when <paramref name="max_inclusive"/> is less than or equal to
     *  <paramref name="min_inclusive"/>.
     * </exception>
     * */
    public float NextFloat(float min_inclusive = 0, float max_inclusive = 1) {
      if (MathExt.Approximately(min_inclusive, max_inclusive))
        return min_inclusive;
      if (min_inclusive > max_inclusive)
        throw new ArgumentException(
            $"max_exclusive {max_inclusive} must be greater than min_inclusive {min_inclusive}");
      return min_inclusive + (float)NextUInt() / uint.MaxValue * (max_inclusive - min_inclusive);
    }

    /**
     * <summary>
     *  Generates a random <see cref="float"/> between <paramref name="min_inclusive"/> and
     *  <paramref name="max_inclusive"/>.
     *  <br/> Respecting modulo bias.
     *  <br/> In turn, <see cref="StateCount"/> might be incremented more than once.
     * </summary>
     * <returns>
     *  Random <see cref="float"/> between <paramref name="min_inclusive"/> and
     *  <paramref name="max_inclusive"/>.
     * </returns>
     * <inception cref="ArgumentException">
     *  Thrown when <paramref name="max_inclusive"/> is less than or equal to
     *  <paramref name="min_inclusive"/>.
     * </exception>
     * */
    public double NextDouble(double min_inclusive = 0, double max_inclusive = 1) {
      if (MathExt.Approximately(min_inclusive, max_inclusive))
        return min_inclusive;
      if (min_inclusive > max_inclusive)
        throw new ArgumentException(
            $"max_exclusive {max_inclusive} must be greater than min_inclusive {min_inclusive}");
      return min_inclusive + (double)NextUInt() / uint.MaxValue * (max_inclusive - min_inclusive);
    }

    /**
     * <summary>
     * <br/> Tracks the number of times the PRNG has been called.
     * <br/> Mostly used alongside <see cref="JumpTo(ulong)"/> to replicate the state of a PRNG.
     * </summary>
     * */
    public ulong StateCount { get; protected set; }

    /**
     * <summary>
     *  <br/> Jump aheads the PRNG as if it has called <see cref="NextUInt()"/>
     *    <paramref name="steps"/> times.
     *  <br/> Efficiency depends purely on the PRNG algorithm, with <see cref="MT19937"/> and
     *    <see cref="Xoroshiro"/> being pure linear loop.
     * </summary>
     * <param name="steps">Length of the jump ahead.</param>
     * */
    public virtual void Jump(ulong steps) {
      for (ulong i = 0; i < steps; i++) NextUInt();
    }

    /**
     * <summary>
     *  Calls <see cref="Jump(ulong)"/> in respect to the current <see cref="StateCount"/>.
     * </summary>
     * <param name="state_count_target">The desired final <see cref="StateCount"/> value.</param>
     * <exception cref="InvalidOperationException">
     *  Thrown when <paramref name="state_count_target"/> is lower than the current PRNG's
     *  <see cref="StateCount"/>. As jumping backwards is unsupported.
     * </exception>
     * */
    public void JumpTo(ulong state_count_target) {
      if (state_count_target < StateCount)
        throw new InvalidOperationException(
            $"Jumping Backwards is unsupported. Current state is {StateCount}, while the target state is {state_count_target}");

      Jump(state_count_target - StateCount);
    }
  }

  /**
   * <summary>
   * A <see cref="IRandom"/> wrapper for the default <see cref="System.Random"/>.
   * </summary>
   * */
  public class NETdefault : IRandom<NETdefault> {
    private readonly System.Random random;

    public NETdefault() : this(DefaultReSeed()) {}

    public NETdefault(ulong seed) {
      Seed = seed;

      var castedSeed = (int)seed;
      Seed           = (ulong)castedSeed;
      random         = new System.Random(castedSeed);
    }

    public override string ToString() => $".NET-0x{Seed:X}";

    public override uint NextUInt() {
      var first  = (uint)random.Next();
      var second = (uint)random.Next();
      return (second << 16) | (first & 0xFFFF);
    }
  }
}
