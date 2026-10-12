using System;

namespace MMOR.NET.Benchmarks.Collections {
public enum Distribution {
  Random,
  Sorted,
  Reverse,
  OrganPipe,
  FewUnique,
  AllEqual,
}

/**
 * <summary>
 *  Shared dataset sources and argument sets for the sort and selection benchmarks.
 * </summary>
 */
public static class SortData {
  public static int[] N { get; } = [1024, 65536, 262144];

  public static Distribution[] Distributions { get; } = [
    Distribution.Random,
    Distribution.Sorted,
    Distribution.Reverse,
    Distribution.OrganPipe,
    Distribution.FewUnique,
    Distribution.AllEqual,
  ];

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
      case Distribution.AllEqual:
        Array.Fill(values, 0);
        break;
    }
    return values;
  }
}
}
