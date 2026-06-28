# Examples

## Gacha Rates

```cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MMOR.NET.MultiThreadSimulation;
using MMOR.NET.Random;

public class GachaSimulator : ISimulationObject<GachaSimulator> {
  public SimulationMetadata sim_meta { get; } = new();

  // Consts
  public const int kTiers = 2;

  public static readonly IReadOnlyList<double> kBaseRates = [
    0.006,
    0.051,
  ];

  public static readonly IReadOnlyList<int> kHardPity = [
    89,
    9,
  ];

  public static readonly IReadOnlyList<Func<int, double>> kSoftPity = [
    x => Math.Max(0, x - 73) * 0.06,
    x => x == 8 ? 0.51 : 0,
  ];

  // State
  // .. not reset when Clear
  IRandom rng_;
  private int[] pity_counts_ = new int[kTiers + 1];

  // Data
  // .. results, reset on Clear
  public ulong[] tiers_hits = new ulong[kTiers + 1];

  public GachaSimulator(IRandom rng) {
    rng_ = rng;
  }

  public void Clear() {
    for (int i = 0; i <= kTiers; ++i) {
      tiers_hits[i] = 0;
    }
  }

  public void Combine(GachaSimulator add_data) {
    for (int i = 0; i <= kTiers; ++i) {
      tiers_hits[i] += add_data.tiers_hits[i];
    }
  }

  public void SingleSim() {
    double roll = rng_.NextDouble();
    int gotten  = kTiers;

    for (int i = 0; i < kTiers; ++i) {
      int pity_count = pity_counts_[i];
      bool get_this  = pity_count >= kHardPity[i];

      double rate = kBaseRates[i] + kSoftPity[i].Invoke(pity_count);

      get_this |= roll <= rate;
      roll -= rate;

      if (get_this) {
        gotten = i;
        break;
      }
    }

    for (int i = 0; i <= kTiers; ++i) {
      ++pity_counts_[i];
    }
    ++tiers_hits[gotten];
    pity_counts_[gotten] = 0;
  }
}

public static class Program {
  public static void Main() {
    TestHarness<GachaSimulator> test_harness    = new();
    SimulationConfig<GachaSimulator> sim_config = new() {
      sim_obj_ctor = x => new GachaSimulator(x),
      target_iteration  = 5_000_000_000,
      thread_count      = 8,
    };

    test_harness.OnExceptionCatch += (ex, ctx) => {
      Console.Error.WriteLine($"Error in TestHarness:");
      Console.Error.WriteLine(ctx);
      Console.Error.WriteLine(ex.ToString());
      Console.Error.WriteLine();
      throw ex;
    };

    test_harness.OnStart += () => Console.WriteLine("Starting Simulation");

    test_harness.OnReport = (sim_data, metadata) => {
      TextWriter writer = metadata.currently_testing ? Console.Error : Console.Out;
      try {
        writer.WriteLine(string.Format("Progress {0:P2}",
            (double)metadata.total_iterations / metadata.target_iterations));
        writer.WriteLine("Collective Rates:");
        writer.WriteLine("=================");
        for (int i = 0; i <= GachaSimulator.kTiers; ++i) {
          ulong hits  = sim_data.tiers_hits[i];
          double perc = (double)hits / metadata.total_iterations;
          writer.WriteLine(string.Format("{0}*: {1,14:N0} ({2,20:P16})", 5 - i, hits, perc));
        }
        writer.WriteLine();
        writer.WriteLine();
        return Task.CompletedTask;
      } catch (Exception ex) {
        Console.Error.WriteLine(ex.ToString());
        throw;
      }
    };

    TaskCompletionSource<bool> tcs  = new();
    test_harness.OnFinish += ()    => tcs.TrySetResult(true);

    _ = test_harness.RunTest(sim_config);
    test_harness.PokeReport();

    tcs.Task.GetAwaiter().GetResult();
  }
}
```
