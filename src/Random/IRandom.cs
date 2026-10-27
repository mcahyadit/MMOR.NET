using System;

namespace MMOR.NET.Random {
public abstract class IRandom<T> : IRandom
    where T : IRandom<T>, new() {
  /**
   * <summary>
   * A lazyily instantiated <see langword="static"/> instance of <typeparamref name="T"/>.
   * </summary>
   */
  public static T Shared                 => shared_.Value;
  public static readonly Lazy<T> shared_  = new(() => Activator.CreateInstance<T>());

  public ulong Seed { get; set; }
  public ulong StateCount { get; set; }

  public override string ToString() => $"{typeof(T).Name}-{this.GetSeedStr()}";

  public abstract uint NextUInt();

  /**
   * <summary>
   * A lazyily instantiated <see langword="static"/> instance of <typeparamref name="T"/>.
   * </summary>
   */
  [Obsolete("Use Shared instead.")]
  public static T global => shared_.Value;
}

/**
 * <summary>
 * <strong>Generic Pseudo Random Number Generator abstraction.</strong>
 * <br/> - For easily swapping out different algorithm when needed.
 * </summary>
 */
public interface IRandom {
  /**
   * <summary>
   * The number which the current PRNG is seeded with.
   * </summary>
   */
  public ulong Seed { get; }

  /**
   * <summary>Generates a random 32-bit unsigned integer.</summary>
   * <returns>Random 32-bit unsigned integer between.</returns>
   */
  public uint NextUInt();  // Primary Result

  /**
   * <summary>
   * <br/> Tracks the number of times the PRNG has been called.
   * <br/> Mostly used alongside <see cref="RandomExtensions.JumpTo{T}(T, ulong)"/> to replicate the
   * state of a PRNG.
   * </summary>
   */
  public ulong StateCount { get; }
}

public interface IRandomDoubleSeed : IRandom {
  public bool IsDoubleSeeded { get; }
  public ulong Seed2 { get; }
}

public interface IRandomJumpable : IRandom {
  /**
   * <summary>
   *  <br/> Jump aheads the PRNG as if it has called <see cref="IRandom.NextUInt()"/>
   *    <paramref name="steps"/> times.
   *  <br/> Efficiency depends purely on the PRNG algorithm, with <see cref="MT19937"/> and
   *    <see cref="Xoroshiro"/> being pure linear loop.
   * </summary>
   * <param name="steps">Length of the jump ahead.</param>
   */
  public void Jump(ulong steps);
}

public interface IRandom64 : IRandom {
  public ulong NextUInt64();
}

/**
 * <summary>
 * A <see cref="IRandom"/> wrapper for the default <see cref="System.Random"/>.
 * </summary>
 */
public class NETdefault : IRandom<NETdefault> {
  public readonly System.Random random;
  public override string ToString() => $".NET-0x{Seed:X}";

  public NETdefault() : this(RandUtils.GenericU32RandomSeed()) {}

  public NETdefault(ulong seed) {
    Seed = seed;

    int seed_i32 = (int)seed;
    Seed         = (ulong)seed_i32;
    random       = new System.Random(seed_i32);
  }

  public override uint NextUInt() {
    ++StateCount;
    var first  = (uint)random.Next();
    var second = (uint)random.Next();
    return (second << 16) | (first & 0xFFFF);
  }
}
}
