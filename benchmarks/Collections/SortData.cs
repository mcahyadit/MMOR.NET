using System;

namespace MMOR.NET.Benchmarks.Collections {
public enum Distribution {
  Random,
  Sorted,
  Reverse,
  OrganPipe,
  FewUnique,
  NearlySorted,
  Sawtooth,
}

/**
 * <summary>
 *  Rank (order-statistic) fractions for the selection benchmark, resolved against
 *  <see cref="SortData.N" />. Extremes (1 and <c>n</c>) are deliberately excluded: they
 *  short-circuit to <c>MinElement</c>/<c>MaxElement</c> and measure nothing distinctive.
 * </summary>
 */
public enum KFraction {
  Small,
  Quarter,
  Median,
  ThreeQuarter,
}

/**
 * <summary>
 *  Shared dataset sources and argument sets for the sort and selection benchmarks.
 * </summary>
 */
public static class SortData {
  // public static int[] N { get; } = [1024, 65536, 262144];
  public static int[] N { get; } = [65536];

  public static Distribution[] Distributions { get; } = [
    Distribution.Random,
    Distribution.Sorted,
    Distribution.Reverse,
    Distribution.OrganPipe,
    Distribution.FewUnique,
    Distribution.NearlySorted,
    Distribution.Sawtooth,
  ];

  public static KFraction[] KFractions { get; } = [
    KFraction.Small,
    KFraction.Quarter,
    KFraction.Median,
    KFraction.ThreeQuarter,
  ];

  /**
   * <summary>
   *  Resolve a (1-indexed) rank for <paramref name="fraction" /> against <paramref name="n" />.
   * </summary>
   */
  public static int ResolveK(int n, KFraction fraction) => fraction switch {
    KFraction.Small        => Math.Max(1, n / 16),
    KFraction.Quarter      => Math.Max(1, n / 4),
    KFraction.Median       => Math.Max(1, n / 2),
    KFraction.ThreeQuarter => Math.Max(1, 3 * n / 4),
    _                      => throw new ArgumentOutOfRangeException(nameof(fraction)),
  };

  /**
   * <summary>
   *  Build a fresh <paramref name="n" />-element sample following the requested
   *  <paramref name="distribution" />.
   * </summary>
   */
  public static int[] Build(int n, Distribution distribution) {
    int[] values = new int[n];
    var rng      = new System.Random(42);
    switch (distribution) {
      case Distribution.Random:
        for (int i = 0; i < n; ++i) values[i] = rng.Next();
        break;
      case Distribution.Sorted:
        for (int i = 0; i < n; ++i) values[i] = i;
        break;
      case Distribution.Reverse:
        for (int i = 0; i < n; ++i) values[i] = n - i;
        break;
      case Distribution.OrganPipe:
        int half = n / 2;
        for (int i = 0; i < n; ++i) values[i] = i < half ? i : n - i;
        break;
      case Distribution.FewUnique:
        for (int i = 0; i < n; ++i) values[i] = rng.Next(16);
        break;
      case Distribution.NearlySorted: {
        for (int i = 0; i < n; ++i) values[i] = i;
        int swaps = Math.Max(1, n / 100);
        for (int s = 0; s < swaps; ++s) {
          int a                  = rng.Next(n);
          int b                  = rng.Next(n);
          (values[a], values[b]) = (values[b], values[a]);
        }
        break;
      }
      case Distribution.Sawtooth:
        for (int i = 0; i < n; ++i) values[i] = i % 32;
        break;
    }
    return values;
  }
}
}
