using BenchmarkDotNet.Attributes;

namespace MMOR.NET.Random {
[MemoryDiagnoser]
public class RandomEngineBench {
  [Params(1024, 65536, 1048576)]
  public int N;

  public uint RandN<T>(T rng)
      where T : IRandom {
    uint result = ~0u;
    for (int i = 0; i < N; ++i) {
      result ^= rng.NextUInt();
    }
    return result;
  }

  public ulong RandN64<T>(T rng)
      where T : IRandom64 {
    ulong result = ~0ul;
    for (int i = 0; i < N; ++i) {
      result ^= rng.NextUInt64();
    }
    return result;
  }

  [Benchmark(Baseline = true)]
  public uint MT19937() {
    return RandN(new MT19937());
  }

  [Benchmark]
  public uint MT11213b() {
    return RandN(new MT11213b());
  }

  [Benchmark]
  public ulong MT19937u64() {
    return RandN64(new MT19937u64());
  }

  [Benchmark]
  public ulong Xoroshiro128p() {
    return RandN64(new Xoroshiro128p());
  }

  [Benchmark]
  public ulong Xoroshiro128pp() {
    return RandN64(new Xoroshiro());
  }

  [Benchmark]
  public uint Xoroshiro64ss() {
    return RandN(new Xoroshiro64s());
  }

  [Benchmark]
  public ulong RomuTrio() {
    return RandN64(new RomuTrio());
  }

  [Benchmark]
  public uint PCG() {
    return RandN(new PCG());
  }

  [Benchmark]
  public uint ChaCha20() {
    return RandN(new ChaCha20());
  }
}
}
