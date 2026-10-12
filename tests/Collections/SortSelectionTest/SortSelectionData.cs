using System;
using Xunit;

namespace MMOR.NET.Collections {
/**
 * <summary>
 *  One shared scenario for the sort and selection tests: <see cref="values" /> is the sample and
 *  <see cref="ks" /> the ranks (1-based) to probe against the sorted oracle.
 * </summary>
 */
public struct SortSelectionParam {
  public string identifier { get; init; } = "Generic Test";
  public required int[] values { get; init; }
  public required int[] ks { get; init; }
  public SortSelectionParam() {}
  public override string ToString() => identifier;
}

/**
 * <summary>
 *  Shared dataset sources and oracle helpers for the sort and selection tests.
 * </summary>
 */
public static class SortSelectionData {
  public static readonly TheoryData<SortSelectionParam> Params = BuildParams();

  /**
   * <summary>
   *  Ascending copy of <paramref name="data" /> built with <see cref="Array.Sort" /> as the oracle.
   * </summary>
   */
  public static T[] Sorted<T>(T[] data)
      where T : unmanaged, IComparable<T> {
    T[] copy = (T[])data.Clone();
    Array.Sort(copy);
    return copy;
  }

  public static string Dump<T>(T[] data) => "[" + string.Join(",", data) + "]";

  static TheoryData<SortSelectionParam> BuildParams() {
    TheoryData<SortSelectionParam> data = [];

    System.Random rng = new(1);

    for (int n = 1; n <= 40; ++n) {
      int[] values = new int[n];
      for (int i = 0; i < n; ++i) values[i] = rng.Next();
      data.Add(new SortSelectionParam {
        identifier = string.Format("distinct-len{0}", n),
        values     = values,
        ks         = AllK(n),
      });
    }

    for (int n = 1; n <= 40; ++n) {
      int[] values = new int[n];
      for (int i = 0; i < n; ++i) values[i] = rng.Next(0, 4);
      data.Add(new SortSelectionParam {
        identifier = string.Format("duplicates-len{0}", n),
        values     = values,
        ks         = AllK(n),
      });
    }

    for (int n = 1; n <= 16; ++n) {
      int[] values = new int[n];
      Array.Fill(values, 42);
      data.Add(new SortSelectionParam {
        identifier = string.Format("alleq-len{0}", n),
        values     = values,
        ks         = AllK(n),
      });
    }

    foreach (int n in new[] { 129, 200, 1000 }) {
      int[] values = new int[n];
      for (int i = 0; i < n; ++i) values[i] = rng.Next(0, n / 2 + 1);
      data.Add(new SortSelectionParam {
        identifier = string.Format("large-len{0}", n),
        values     = values,
        ks         = [1, n / 4, n / 2, n - 1, n],
      });
    }

    return data;
  }

  static int[] AllK(int n) {
    int[] ks = new int[n];
    for (int i = 0; i < n; ++i) ks[i] = i + 1;
    return ks;
  }
}
}
