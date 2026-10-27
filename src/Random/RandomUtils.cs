using System;
using System.Numerics;
using System.Threading;

#if !NETSTANDARD
using System.Runtime.Intrinsics.X86;
#endif

namespace MMOR.NET.Random {
public static class RandUtils {
  public static uint GenericU32RandomSeed() {
    var seed = (ulong)(Environment.TickCount ^ Thread.CurrentThread.ManagedThreadId ^
                       Guid.NewGuid().GetHashCode());
    seed     = (seed >> 32) ^ seed;
    return (uint)seed;
  }

  public static string GetSeedStr<T>(this T self)
      where T : IRandom {
    static int WriteSeed(ulong seed, Span<char> dest) {
      dest[0] = '0';
      dest[1] = 'x';
      seed.TryFormat(dest[2..], out int written, seed > 0xFFFFFFFFF ? "X16" : "X8");
      return written + 2;
    }

    Span<char> buffer = stackalloc char[2 + 16 + 1 + 2 + 16];
    int len           = WriteSeed(self.Seed, buffer);
    if (self is IRandomDoubleSeed { IsDoubleSeeded : true } doub) {
      buffer[len++] = '-';
      len += WriteSeed(doub.Seed2, buffer[len..]);
    }
    return buffer[..len].ToString();
  }

  public static ulong SplitMix64(ulong seed, out ulong next) {
    seed += 0x9e3779b97f4a7c15;
    ulong z = seed;
    z       = (z ^ (z >> 30)) * 0xbf58476d1ce4e5b9;
    z       = (z ^ (z >> 27)) * 0x94d049bb133111eb;
    next    = z ^ (z >> 31);
    return seed;
  }

  public static uint SplitMix32(uint seed, out uint next) {
    seed   = 1664525u * (seed + 314159265u);
    uint z = seed;
    z      = (z ^ (z >> 15)) * 0x5ce4e5b9u;
    z      = (z ^ (z >> 13)) * 0x1331c1ebu;
    next   = z ^ (z >> 15);
    return seed;
  }

  public static void ShuffleArray<T, TRandom>(this in Span<T> arr, TRandom rng)
      where TRandom : IRandom {
    int length = arr.Length;
    for (int c_idx = length - 1; c_idx >= 1; --c_idx) {
      int r_idx = rng.NextInt(0, c_idx + 1);

      (arr[c_idx], arr[r_idx]) = (arr[r_idx], arr[c_idx]);
    }
  }

  /**
   * <summary>
   *  Randomly selects an active bit in a bitmask and returns the position.
   * </summary>
   * <param name="bitmask">The bitmask to select from.</param>
   * <param name="rng">The random engine.</param>
   * <returns>
   *  <c>-1</c> if <paramref name="bitmask"/> is <c>0</c>,
   *  otherwise the index of the randomly selected active bit.
   * </returns>
   */
  public static int GetRandomActiveBit<T>(ulong bitmask, T rng)
      where T : IRandom {
    if (bitmask == 0)
      return -1;

    int count  = BitOperations.PopCount(bitmask);
    int target = rng.NextInt(0, count);
#if !NETSTANDARD
    if (Bmi2.X64.IsSupported) {
      ulong mask = Bmi2.X64.ParallelBitDeposit(1ul << target, bitmask);
      return BitOperations.TrailingZeroCount(mask);
    }
#endif
    for (int i = 0; i < target; ++i) {
      bitmask &= bitmask - 1;
    }

    return BitOperations.TrailingZeroCount(bitmask);
  }
}
}
