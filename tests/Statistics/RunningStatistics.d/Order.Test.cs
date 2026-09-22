using System;
using Xunit;

namespace MMOR.NET.Statistics {
using Random = System.Random;
public partial class RunningStatisticsTest {
  public struct OrderParam {
    public string identifier { get; init; } = "Generic Test";
    public required double[] values { get; init; }
    public ulong[] counts { get; init; }         = [];
    public int seed_a { get; init; }             = 12345;
    public int seed_b { get; init; }             = 67890;
    public int precision { get; init; }          = 13;
    public int variance_precision { get; init; } = 13;
    public int skewness_precision { get; init; } = 13;
    public int kurtosis_precision { get; init; } = 12;
    public double tolerance { get; init; }       = 1e-9;
    public bool relative { get; init; }          = false;
    public OrderParam() {}
    public override string ToString() => identifier;

    public int[] Permutation(int seed) {
      int[] perm = new int[values.Length];
      for (int i = 0; i < perm.Length; ++i) {
        perm[i] = i;
      }
      Random rng = new(seed);
      for (int i = perm.Length - 1; i > 0; --i) {
        int j              = rng.Next(i + 1);
        (perm[i], perm[j]) = (perm[j], perm[i]);
      }
      return perm;
    }
  }

  [Theory]
  [MemberData(nameof(kOrderTestParams))]
  public void OrderTest(OrderParam p) {
    if (p.counts.Length > 0 && p.counts.Length != p.values.Length)
      throw new ArgumentException(string.Format("[ERROR]: values.Length: {0} != counts.Length: {1}",
          p.values.Length, p.counts.Length));

    RunningStatisticsAdvanced seq = new();
    for (int i = 0; i < p.values.Length; ++i) {
      PushAt(seq, p.values, p.counts, i);
    }

    RunningStatisticsAdvanced rev = new();
    for (int i = p.values.Length - 1; i >= 0; --i) {
      PushAt(rev, p.values, p.counts, i);
    }

    RunningStatisticsAdvanced shuf_a = new();
    RunningStatisticsAdvanced shuf_b = new();
    int[] perm_a                     = p.Permutation(p.seed_a);
    int[] perm_b                     = p.Permutation(p.seed_b);
    for (int i = 0; i < p.values.Length; ++i) {
      PushAt(shuf_a, p.values, p.counts, perm_a[i]);
      PushAt(shuf_b, p.values, p.counts, perm_b[i]);
    }

    try {
      AssertEquivalent(seq, rev, p);
      AssertEquivalent(seq, shuf_a, p);
      AssertEquivalent(seq, shuf_b, p);
    } catch {
      Console.Error.WriteLine(p.identifier);
      throw;
    }
  }

  static void PushAt(RunningStatisticsAdvanced s, double[] values, ulong[] counts, int i) {
    if (counts.Length > 0) {
      s.Push(values[i], counts[i]);
      return;
    }
    s.Push(values[i]);
  }

  void AssertEquivalent(RunningStatisticsAdvanced a, RunningStatisticsAdvanced b, OrderParam p) {
    AssertStat(a.Count, b.Count, p, p.precision);
    AssertStat(a.Mean, b.Mean, p, p.precision);
    AssertStat(a.Variance, b.Variance, p, p.variance_precision);
    AssertStat(a.Skewness, b.Skewness, p, p.skewness_precision);
    AssertStat(a.Kurtosis, b.Kurtosis, p, p.kurtosis_precision);
    AssertStat(a.GeometricMean, b.GeometricMean, p, p.precision);
    AssertStat(a.HarmonicMean, b.HarmonicMean, p, p.precision);
    AssertStat(a.RootMeanSquare, b.RootMeanSquare, p, p.precision);
    AssertStat(a.Minimum, b.Minimum, p, p.precision);
    AssertStat(a.Maximum, b.Maximum, p, p.precision);
  }

  static void AssertStat(double expected, double actual, OrderParam p, int precision) {
    if (!p.relative) {
      TestUtils.AssertApproximately(expected, actual, precision);
      return;
    }
    TestUtils.AssertApproximately(expected, actual, p.tolerance);
  }
}
}