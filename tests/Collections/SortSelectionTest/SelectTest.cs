using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Sdk;

namespace MMOR.NET.Collections {
/**
 * <summary>
 *  Top-level scalar selection algorithms checked against the sorted oracle.
 * </summary>
 */
public class SelectTest {
  public enum SelectKind { QuickSelect, MedianOfMedians, IntroSelect, PdqSelect }

  public static readonly TheoryData<int> kInvalidKLengths = [1, 5, 10];

  [Theory]
  [MemberData(nameof(SortSelectionData.Params), MemberType = typeof(SortSelectionData))]
  public void QuickSelect_MatchesSortedOracle(SortSelectionParam p) {
    foreach (int k in p.ks) AssertOne(SelectKind.QuickSelect, p.values, k);
  }

  [Theory]
  [MemberData(nameof(SortSelectionData.Params), MemberType = typeof(SortSelectionData))]
  public void MedianOfMedians_MatchesSortedOracle(SortSelectionParam p) {
    foreach (int k in p.ks) AssertOne(SelectKind.MedianOfMedians, p.values, k);
  }

  [Theory]
  [MemberData(nameof(SortSelectionData.Params), MemberType = typeof(SortSelectionData))]
  public void IntroSelect_MatchesSortedOracle(SortSelectionParam p) {
    foreach (int k in p.ks) AssertOne(SelectKind.IntroSelect, p.values, k);
  }

  [Theory]
  [MemberData(nameof(SortSelectionData.Params), MemberType = typeof(SortSelectionData))]
  public void PdqSelect_MatchesSortedOracle(SortSelectionParam p) {
    foreach (int k in p.ks) AssertOne(SelectKind.PdqSelect, p.values, k);
  }

  [Theory]
  [MemberData(nameof(kInvalidKLengths))]
  public void Select_InvalidK_Throws(int n) {
    int[] data = Enumerable.Range(0, n).ToArray();
    foreach (SelectKind kind in Enum.GetValues<SelectKind>()) {
      Assert.Throws<ArgumentOutOfRangeException>(() => RunSelect<int>(kind, data, 0));
      Assert.Throws<ArgumentOutOfRangeException>(() => RunSelect<int>(kind, data, n + 1));
    }
  }

  [Fact]
  public void Select_Empty_Throws() {
    foreach (SelectKind kind in Enum.GetValues<SelectKind>())
      Assert.Throws<ArgumentOutOfRangeException>(() => RunSelect<int>(kind, [], 1));
  }

  [Fact]
  public void Select_Double() {
    System.Random rng = new(4);
    for (int n = 1; n <= 32; ++n) {
      double[] data = new double[n];
      for (int i = 0; i < n; ++i) data[i] = rng.NextDouble();
      foreach (SelectKind kind in Enum.GetValues<SelectKind>())
        for (int k = 1; k <= n; ++k) AssertOne(kind, data, k);
    }
  }

  [Fact]
  public void Select_Char() {
    char[] data = "zyxwvutsrqponmlkjihgfedcba".ToCharArray();
    foreach (SelectKind kind in Enum.GetValues<SelectKind>())
      for (int k = 1; k <= data.Length; ++k) AssertOne(kind, data, k);
  }

  static void AssertOne<T>(SelectKind kind, T[] values, int k)
      where T : unmanaged, IComparable<T> {
    T expected = SortSelectionData.Sorted(values)[k - 1];
    T got;
    try {
      got = RunSelect<T>(kind, values, k);
    } catch (Exception e) {
      throw new XunitException(string.Format("{0} n={1} k={2} threw {3}: {4}", kind, values.Length,
          k, e.GetType().Name, e.Message));
    }
    if (!EqualityComparer<T>.Default.Equals(got, expected))
      throw new XunitException(string.Format("{0} n={1} k={2}: expected {3}, got {4}", kind,
          values.Length, k, expected, got));
  }

  static T RunSelect<T>(SelectKind kind, ReadOnlySpan<T> data, int k)
      where T : unmanaged, IComparable<T> {
    switch (kind) {
      case SelectKind.QuickSelect:
        return data.QuickSelect(k);
      case SelectKind.MedianOfMedians:
        return data.MedianOfMedians(k);
      case SelectKind.IntroSelect:
        return data.IntroSelect(k);
      default:
        return data.PdqSelect(k);
    }
  }
}
}
