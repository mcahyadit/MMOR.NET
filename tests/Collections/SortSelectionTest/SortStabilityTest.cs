using System;
using Xunit;
using Xunit.Sdk;

namespace MMOR.NET.Collections {
/**
 * <summary>
 *  Stability checks for the algorithms documented as stable: elements comparing equal
 *  must keep their original relative order after sorting. Each key is paired with its
 *  original index so any swap of equal keys is observable.
 * </summary>
 */
public class SortStabilityTest {
  public enum StableSortKind {
    Insertion,
    BinaryInsertion,
    Merge,
    Tim,
  }

  [Theory]
  [MemberData(nameof(SortSelectionData.StabilityParams), MemberType = typeof(SortSelectionData))]
  public void StableSorts_PreserveEqualKeyOrder(SortSelectionParam p) {
    foreach (StableSortKind kind in Enum.GetValues<StableSortKind>()) AssertStable(kind, p.values);
  }

  readonly struct Pair(int key, int index) : IComparable<Pair> {
    public readonly int key   = key;
    public readonly int index = index;

    public int CompareTo(Pair other)  => key.CompareTo(other.key);
    public override string ToString() => string.Format("{0}#{1}", key, index);
  }

  static void RunStableSort(Span<Pair> span, StableSortKind kind) {
    switch (kind) {
      case StableSortKind.Insertion:
        span.InsertionSort();
        break;
      case StableSortKind.BinaryInsertion:
        span.BinaryInsertionSort();
        break;
      case StableSortKind.Merge:
        span.MergeSort();
        break;
      default:
        span.TimSort();
        break;
    }
  }

  static void AssertStable(StableSortKind kind, int[] keys) {
    Pair[] buffer = new Pair[keys.Length];
    for (int i = 0; i < keys.Length; ++i) buffer[i] = new Pair(keys[i], i);

    try {
      RunStableSort(buffer.AsSpan(), kind);
    } catch (Exception e) {
      throw new XunitException(string.Format("{0} n={1} threw {2}: {3}", kind, keys.Length,
          e.GetType().Name, e.Message));
    }

    for (int i = 1; i < buffer.Length; ++i) {
      if (buffer[i - 1].key > buffer[i].key)
        throw new XunitException(string.Format("{0} n={1}: unsorted at {2}: {3}", kind, keys.Length,
            i, SortSelectionData.Dump(buffer)));
      if (buffer[i - 1].key == buffer[i].key && buffer[i - 1].index >= buffer[i].index)
        throw new XunitException(string.Format("{0} n={1}: unstable at {2}: {3}", kind, keys.Length,
            i, SortSelectionData.Dump(buffer)));
    }
  }
}
}
