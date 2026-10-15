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
   *  Duplicate-heavy samples (equal keys everywhere) used by the stability tests; the
   *  payload of each element is its original index, so any swap of equal keys is visible.
   * </summary>
   */
  public static readonly TheoryData<SortSelectionParam> StabilityParams = BuildStabilityParams();

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

  static TheoryData<SortSelectionParam> BuildStabilityParams() {
    TheoryData<SortSelectionParam> data = [];

    System.Random rng = new(7);

    for (int n = 1; n <= 40; ++n) {
      int[] values = new int[n];
      for (int i = 0; i < n; ++i) values[i] = rng.Next(0, 3);
      data.Add(new SortSelectionParam {
        identifier = string.Format("stability-dup3-len{0}", n),
        values     = values,
        ks         = [],
      });
    }

    foreach (int n in new[] { 2, 31, 32, 33, 64, 65, 127, 128, 129, 200, 1000 }) {
      int[] alleq = new int[n];
      Array.Fill(alleq, 7);
      data.Add(new SortSelectionParam {
        identifier = string.Format("stability-alleq-len{0}", n),
        values     = alleq,
        ks         = [],
      });

      int[] asc = new int[n];
      for (int i = 0; i < n; ++i) asc[i] = i / 4;
      data.Add(new SortSelectionParam {
        identifier = string.Format("stability-asc-plateau-len{0}", n),
        values     = asc,
        ks         = [],
      });

      int[] desc = new int[n];
      for (int i = 0; i < n; ++i) desc[i] = (n - 1 - i) / 4;
      data.Add(new SortSelectionParam {
        identifier = string.Format("stability-desc-plateau-len{0}", n),
        values     = desc,
        ks         = [],
      });

      int[] sawtooth = new int[n];
      for (int i = 0; i < n; ++i) sawtooth[i] = i % 16;
      data.Add(new SortSelectionParam {
        identifier = string.Format("stability-sawtooth-len{0}", n),
        values     = sawtooth,
        ks         = [],
      });

      int[] dup = new int[n];
      for (int i = 0; i < n; ++i) dup[i] = rng.Next(0, n / 8 + 1);
      data.Add(new SortSelectionParam {
        identifier = string.Format("stability-dupn-len{0}", n),
        values     = dup,
        ks         = [],
      });
    }

    return data;
  }
}
}
