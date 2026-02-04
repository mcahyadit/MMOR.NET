using System;
using System.Threading;
using MMOR.NET.Mathematics;

namespace MMOR.NET.Random {
  public abstract class IRandom<T> : IRandom
      where T : IRandom {
    private static T? _global;
    public static T global => _global ??= Activator.CreateInstance<T>();
  }

  /**
   * <summary>
   * <strong>Generic Pseudo Random Number Generator abstraction.</strong>
   * <br/> - For easily swapping out different algorithm when needed.
   * </summary>
   * */
  public abstract class IRandom {
    public ulong Seed { get; protected set; }

    protected static ulong DefaultReSeed() {
      var seed = (ulong)(Environment.TickCount ^ Thread.CurrentThread.ManagedThreadId ^
                         Guid.NewGuid().GetHashCode());
      seed     = (seed >> 32) ^ seed;
      return (uint)seed;
    }

    public abstract uint NextUInt();  // Primary Result
    public uint NextUInt(uint min_inclusive, uint max_exclusive) {
      if (min_inclusive >= max_exclusive)
        throw new Exception(
            $"max_exclusive {max_exclusive} must be greater than min_inclusive {min_inclusive}");

      uint range = max_exclusive - min_inclusive;
      uint limit = uint.MaxValue - uint.MaxValue % range;
      uint result;

      do {
        result = NextUInt();
      } while (result >= limit);

      return min_inclusive + result % range;
    }

    public int NextInt(int min_inclusive, int max_exclusive) {
      if (min_inclusive >= max_exclusive)
        throw new Exception(
            $"max_exclusive {max_exclusive} must be greater than min_inclusive {min_inclusive}");
      var range = (uint)(max_exclusive - min_inclusive);
      return min_inclusive + (int)NextUInt(0, range);
    }

    public float NextFloat(float min_inclusive = 0, float max_exclusive = 1) {
      if (MathExt.Approximately(min_inclusive, max_exclusive))
        return min_inclusive;
      if (min_inclusive > max_exclusive)
        throw new Exception(
            $"max_exclusive {max_exclusive} must be greater than min_inclusive {min_inclusive}");
      return min_inclusive + (float)NextUInt() / uint.MaxValue * (max_exclusive - min_inclusive);
    }

    public double NextDouble(double min_inclusive = 0, double max_exclusive = 1) {
      if (MathExt.Approximately(min_inclusive, max_exclusive))
        return min_inclusive;
      if (min_inclusive > max_exclusive)
        throw new Exception(
            $"max_exclusive {max_exclusive} must be greater than min_inclusive {min_inclusive}");
      return min_inclusive + (double)NextUInt() / uint.MaxValue * (max_exclusive - min_inclusive);
    }
  }

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
