using System;
using System.Runtime.CompilerServices;
using System.Threading;
using MMOR.NET.Mathematics;

namespace MMOR.NET.Random
{
  public abstract class IRandom<T> : IRandom
    where T : IRandom
  {
    private static T? _global;
    public static T global => _global ??= Activator.CreateInstance<T>();
  }

  /// <summary>
  ///     <strong>abstract vRandom</strong>
  ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
  ///     <br /> - Abstract for RNG Algorithms.
  ///     <br /> - Allows easy switching.
  ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
  /// </summary>
  public abstract class IRandom
  {
    public ulong Seed { get; protected set; }

    protected static ulong DefaultReSeed()
    {
      var seed = (ulong)(
        Environment.TickCount ^ Thread.CurrentThread.ManagedThreadId ^ Guid.NewGuid().GetHashCode()
      );
      seed = (seed >> 32) ^ seed;
      return (uint)seed;
    }

    public abstract uint NextUInt(); // Primary Result
#if !DEBUG && !UNITY_EDITOR
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public uint NextUInt(uint minInclusive, uint maxExclusive)
    {
      if (minInclusive >= maxExclusive)
        throw new Exception(
          $"maxExclusive {maxExclusive} must be greater than minInclusive {minInclusive}"
        );

      uint range = maxExclusive - minInclusive;
      uint limit = uint.MaxValue - uint.MaxValue % range;
      uint result;

      do
      {
        result = NextUInt();
      } while (result >= limit);

      return minInclusive + result % range;
    }

#if !DEBUG && !UNITY_EDITOR
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public int NextInt(int minInclusive, int maxExclusive)
    {
      if (minInclusive >= maxExclusive)
        throw new Exception(
          $"maxExclusive {maxExclusive} must be greater than minInclusive {minInclusive}"
        );
      var range = (uint)(maxExclusive - minInclusive);
      return minInclusive + (int)NextUInt(0, range);
    }

#if !DEBUG && !UNITY_EDITOR
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public float NextFloat(float minInclusive = 0, float maxExclusive = 1)
    {
      if (MathExt.Approximately(minInclusive, maxExclusive))
        return minInclusive;
      if (minInclusive > maxExclusive)
        throw new Exception(
          $"maxExclusive {maxExclusive} must be greater than minInclusive {minInclusive}"
        );
      return minInclusive + (float)NextUInt() / uint.MaxValue * (maxExclusive - minInclusive);
    }

#if !DEBUG && !UNITY_EDITOR
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public double NextDouble(double minInclusive = 0, double maxExclusive = 1)
    {
      if (MathExt.Approximately(minInclusive, maxExclusive))
        return minInclusive;
      if (minInclusive > maxExclusive)
        throw new Exception(
          $"maxExclusive {maxExclusive} must be greater than minInclusive {minInclusive}"
        );
      return minInclusive + (double)NextUInt() / uint.MaxValue * (maxExclusive - minInclusive);
    }
  }

  public class NETdefault : IRandom<NETdefault>
  {
    private readonly System.Random random;

    public NETdefault()
      : this(DefaultReSeed()) { }

    public NETdefault(ulong seed)
    {
      Seed = seed;

      var castedSeed = (int)seed;
      Seed = (ulong)castedSeed;
      random = new System.Random(castedSeed);
    }

    public override string ToString() => $".NET-0x{Seed:X}";

#if !DEBUG && !UNITY_EDITOR
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public override uint NextUInt()
    {
      var first = (uint)random.Next();
      var second = (uint)random.Next();
      return (second << 16) | (first & 0xFFFF);
    }
  }
}
