using BenchmarkDotNet.Attributes;

namespace MMOR.NET.Random {
[MemoryDiagnoser]
public class RandomDistributionBench {
  public const int N = 65536;

  [Params(3, 256, 283, 100_000, 1_000_000_000, int.MaxValue, uint.MaxValue)]
  public uint Range;

  MT19937 rng = null!;
  uint[] work = null!;

  [GlobalSetup]
  public void Setup() {
    work = new uint[N];
    rng  = new();
  }

  [Benchmark(Baseline = true)]
  public uint[] Basic32() {
    for (int i = 0; i < N; ++i) {
      if (Range == uint.MaxValue)
        work[i] = rng.NextUInt();
      else
        work[i] = rng.NextUInt(0, Range);
    }
    return work;
  }

  [Benchmark]
  public uint[] Lemire32() {
    for (int i = 0; i < N; ++i) {
      work[i] = rng.NextUInt32Lemire(0, Range);
    }
    return work;
  }

  [Benchmark]
  public uint[] Boost32() {
    for (int i = 0; i < N; ++i) {
      work[i] = rng.NextUInt32Boost(0, Range);
    }
    return work;
  }
}
}
