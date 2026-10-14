using System;
using Xunit;
using Xunit.Sdk;

namespace MMOR.NET.Collections {
/**
 * <summary>
 *  Top-level scalar sort algorithms checked against <see cref="Array.Sort" />.
 * </summary>
 */
public class SortTest {
  public enum SortKind {
    Insertion,
    Merge,
    Quick,
    Heap,
    Intro,
    Pdq,
    Tim,
  }

  [Theory]
  [MemberData(nameof(SortSelectionData.Params), MemberType = typeof(SortSelectionData))]
  public void InsertionSort_MatchesSortedOracle(
      SortSelectionParam p) => AssertSort(SortKind.Insertion, p.values);

  [Theory]
  [MemberData(nameof(SortSelectionData.Params), MemberType = typeof(SortSelectionData))]
  public void MergeSort_MatchesSortedOracle(
      SortSelectionParam p) => AssertSort(SortKind.Merge, p.values);

  [Theory]
  [MemberData(nameof(SortSelectionData.Params), MemberType = typeof(SortSelectionData))]
  public void QuickSort_MatchesSortedOracle(
      SortSelectionParam p) => AssertSort(SortKind.Quick, p.values);

  [Theory]
  [MemberData(nameof(SortSelectionData.Params), MemberType = typeof(SortSelectionData))]
  public void HeapSort_MatchesSortedOracle(
      SortSelectionParam p) => AssertSort(SortKind.Heap, p.values);

  [Theory]
  [MemberData(nameof(SortSelectionData.Params), MemberType = typeof(SortSelectionData))]
  public void PdqSort_MatchesSortedOracle(
      SortSelectionParam p) => AssertSort(SortKind.Pdq, p.values);

  [Theory]
  [MemberData(nameof(SortSelectionData.Params), MemberType = typeof(SortSelectionData))]
  public void IntroSort_MatchesSortedOracle(
      SortSelectionParam p) => AssertSort(SortKind.Intro, p.values);

  [Fact]
  public void Sort_Double_Ascending() {
    System.Random rng = new(4);
    double[] data     = new double[100];
    for (int i = 0; i < data.Length; ++i) data[i] = rng.NextDouble();

    foreach (SortKind kind in Enum.GetValues<SortKind>()) AssertSort(kind, data);
  }

  [Fact]
  public void Sort_Char_Ascending() {
    char[] data = "zyxwvutsrqponmlkjihgfedcba".ToCharArray();

    foreach (SortKind kind in Enum.GetValues<SortKind>()) AssertSort(kind, data);
  }

  static void RunSort<T>(Span<T> span, SortKind kind)
      where T : unmanaged, IComparable<T> {
    switch (kind) {
      case SortKind.Insertion:
        span.InsertionSort();
        break;
      case SortKind.Merge:
        span.MergeSort();
        break;
      case SortKind.Heap:
        span.HeapSort();
        break;
      case SortKind.Intro:
        span.IntroSort();
        break;
      case SortKind.Pdq:
        span.PdqSort();
        break;
      case SortKind.Tim:
        span.TimSort();
        break;
      default:
        span.QuickSort();
        break;
    }
  }

  static void AssertSort<T>(SortKind kind, T[] data)
      where T : unmanaged, IComparable<T> {
    T[] expected = SortSelectionData.Sorted(data);
    T[] buffer   = (T[])data.Clone();
    try {
      RunSort(buffer.AsSpan(), kind);
    } catch (Exception e) {
      throw new XunitException(string.Format("{0} n={1} threw {2}: {3}", kind, data.Length,
          e.GetType().Name, e.Message));
    }
    if (!buffer.AsSpan().SequenceEqual(expected))
      throw new XunitException(string.Format("{0} n={1}: expected {2}, got {3}", kind, data.Length,
          SortSelectionData.Dump(expected), SortSelectionData.Dump(buffer)));
  }
}
}
