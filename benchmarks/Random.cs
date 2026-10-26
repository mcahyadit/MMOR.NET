using BenchmarkDotNet.Attributes;
using MMOR.NET.Random;

namespace MMOR.NET.Benchmarks.Collections {
[MemoryDiagnoser]
public class RandomBench {
  [Params(1024, 65536, 1048576)]
  public int N;

  public uint[] RandFill<T>(uint[] work, T rng)
      where T : IRandom {
    for (int i = 0; i < N; ++i) {
      work[i] = rng.NextUInt();
    }
    return work;
  }

  public ulong[] RandFill64<T>(ulong[] work, T rng)
      where T : IRandom64 {
    for (int i = 0; i < N; ++i) {
      work[i] = rng.NextUInt64();
    }
    return work;
  }

  [Benchmark(Baseline = true)]
  public uint[] MT19937() {
    return RandFill(new uint[N], new MT19937());
  }

  [Benchmark]
  public uint[] MT11213b() {
    return RandFill(new uint[N], new MT11213b());
  }

  [Benchmark]
  public ulong[] MT19937u64() {
    return RandFill64(new ulong[N], new MT19937u64());
  }

  [Benchmark]
  public uint[] Xoroshiro128pp() {
    return RandFill(new uint[N], new Xoroshiro());
  }

  [Benchmark]
  public uint[] PCG() {
    return RandFill(new uint[N], new PCG());
  }

  [Benchmark]
  public uint[] ChaCha20() {
    return RandFill(new uint[N], new ChaCha20());
  }
}
}
