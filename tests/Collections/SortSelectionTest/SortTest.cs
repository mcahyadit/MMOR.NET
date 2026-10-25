using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Sdk;

namespace MMOR.NET.Collections {
/**
 * <summary>
 *  Scalar sort algorithms checked against <see cref="Array.Sort" />. Each kind runs both its
 *  single-span entry point and its two-span key/value overload (which keeps a parallel payload in
 *  lock-step), and the stable kinds additionally assert that equal keys keep their original
 *  relative order. Every case is repeated with a reverse comparer to exercise the IComparer
 *  overloads rather than the default <see cref="IComparable{T}" /> path.
 * </summary>
 */
public class SortTest {
  static readonly IComparer<int> kReverse = Comparer<int>.Create((x, y) => y.CompareTo(x));

  [Theory]
  [MemberData(nameof(SortSelectionData.Params), MemberType = typeof(SortSelectionData))]
  public void InsertionSort_MatchesSortedOracle(SortSelectionParam p) {
    int[] expected = SortSelectionData.Sorted(p.values);

    int[] span = [..p.values];
    span.AsSpan().InsertionSort();
    AssertSorted("InsertionSort/single-span", span, expected);

    int[] keys    = [..p.values];
    int[] payload = [..p.payload];
    CollectionUtils.InsertionSort(keys.AsSpan(), payload.AsSpan());
    AssertKeyValueOracle("InsertionSort/key-value", p.values, keys, payload, expected,
        assert_stable: true);

    int[] reverse_expected = [..expected];
    Array.Reverse(reverse_expected);

    span = [..p.values];
    span.AsSpan().InsertionSort(kReverse);
    AssertSorted("InsertionSort/single-span/reverse", span, reverse_expected);

    keys    = [..p.values];
    payload = [..p.payload];
    CollectionUtils.InsertionSort(keys.AsSpan(), payload.AsSpan(), kReverse);
    AssertKeyValueOracle("InsertionSort/key-value/reverse", p.values, keys, payload,
        reverse_expected, assert_stable: true);
  }

  [Theory]
  [MemberData(nameof(SortSelectionData.Params), MemberType = typeof(SortSelectionData))]
  public void BinaryInsertionSort_MatchesSortedOracle(SortSelectionParam p) {
    int[] expected = SortSelectionData.Sorted(p.values);

    int[] span = [..p.values];
    span.AsSpan().BinaryInsertionSort();
    AssertSorted("BinaryInsertionSort/single-span", span, expected);

    int[] keys    = [..p.values];
    int[] payload = [..p.payload];
    CollectionUtils.BinaryInsertionSort(keys.AsSpan(), payload.AsSpan());
    AssertKeyValueOracle("BinaryInsertionSort/key-value", p.values, keys, payload, expected,
        assert_stable: true);

    int[] reverse_expected = [..expected];
    Array.Reverse(reverse_expected);

    span = [..p.values];
    span.AsSpan().BinaryInsertionSort(kReverse);
    AssertSorted("BinaryInsertionSort/single-span/reverse", span, reverse_expected);

    keys    = [..p.values];
    payload = [..p.payload];
    CollectionUtils.BinaryInsertionSort(keys.AsSpan(), payload.AsSpan(), kReverse);
    AssertKeyValueOracle("BinaryInsertionSort/key-value/reverse", p.values, keys, payload,
        reverse_expected, assert_stable: true);
  }

  [Theory]
  [MemberData(nameof(SortSelectionData.Params), MemberType = typeof(SortSelectionData))]
  public void MergeSort_MatchesSortedOracle(SortSelectionParam p) {
    int[] expected = SortSelectionData.Sorted(p.values);

    int[] span = [..p.values];
    span.AsSpan().MergeSort();
    AssertSorted("MergeSort/single-span", span, expected);

    int[] keys    = [..p.values];
    int[] payload = [..p.payload];
    CollectionUtils.MergeSort(keys.AsSpan(), payload.AsSpan());
    AssertKeyValueOracle("MergeSort/key-value", p.values, keys, payload, expected,
        assert_stable: true);

    int[] reverse_expected = [..expected];
    Array.Reverse(reverse_expected);

    span = [..p.values];
    span.AsSpan().MergeSort(kReverse);
    AssertSorted("MergeSort/single-span/reverse", span, reverse_expected);

    keys    = [..p.values];
    payload = [..p.payload];
    CollectionUtils.MergeSort(keys.AsSpan(), payload.AsSpan(), kReverse);
    AssertKeyValueOracle("MergeSort/key-value/reverse", p.values, keys, payload, reverse_expected,
        assert_stable: true);
  }

  [Theory]
  [MemberData(nameof(SortSelectionData.Params), MemberType = typeof(SortSelectionData))]
  public void QuickSort_MatchesSortedOracle(SortSelectionParam p) {
    int[] expected = SortSelectionData.Sorted(p.values);

    int[] span = [..p.values];
    span.AsSpan().QuickSort();
    AssertSorted("QuickSort/single-span", span, expected);

    int[] keys    = [..p.values];
    int[] payload = [..p.payload];
    CollectionUtils.QuickSort(keys.AsSpan(), payload.AsSpan());
    AssertKeyValueOracle("QuickSort/key-value", p.values, keys, payload, expected,
        assert_stable: false);

    int[] reverse_expected = [..expected];
    Array.Reverse(reverse_expected);

    span = [..p.values];
    span.AsSpan().QuickSort(kReverse);
    AssertSorted("QuickSort/single-span/reverse", span, reverse_expected);

    keys    = [..p.values];
    payload = [..p.payload];
    CollectionUtils.QuickSort(keys.AsSpan(), payload.AsSpan(), kReverse);
    AssertKeyValueOracle("QuickSort/key-value/reverse", p.values, keys, payload, reverse_expected,
        assert_stable: false);
  }

  [Theory]
  [MemberData(nameof(SortSelectionData.Params), MemberType = typeof(SortSelectionData))]
  public void HeapSort_MatchesSortedOracle(SortSelectionParam p) {
    int[] expected = SortSelectionData.Sorted(p.values);

    int[] span = [..p.values];
    span.AsSpan().HeapSort();
    AssertSorted("HeapSort/single-span", span, expected);

    int[] keys    = [..p.values];
    int[] payload = [..p.payload];
    CollectionUtils.HeapSort(keys.AsSpan(), payload.AsSpan());
    AssertKeyValueOracle("HeapSort/key-value", p.values, keys, payload, expected,
        assert_stable: false);

    int[] reverse_expected = [..expected];
    Array.Reverse(reverse_expected);

    span = [..p.values];
    span.AsSpan().HeapSort(kReverse);
    AssertSorted("HeapSort/single-span/reverse", span, reverse_expected);

    keys    = [..p.values];
    payload = [..p.payload];
    CollectionUtils.HeapSort(keys.AsSpan(), payload.AsSpan(), kReverse);
    AssertKeyValueOracle("HeapSort/key-value/reverse", p.values, keys, payload, reverse_expected,
        assert_stable: false);
  }

  [Theory]
  [MemberData(nameof(SortSelectionData.Params), MemberType = typeof(SortSelectionData))]
  public void IntroSort_MatchesSortedOracle(SortSelectionParam p) {
    int[] expected = SortSelectionData.Sorted(p.values);

    int[] span = [..p.values];
    span.AsSpan().IntroSort();
    AssertSorted("IntroSort/single-span", span, expected);

    int[] keys    = [..p.values];
    int[] payload = [..p.payload];
    CollectionUtils.IntroSort(keys.AsSpan(), payload.AsSpan());
    AssertKeyValueOracle("IntroSort/key-value", p.values, keys, payload, expected,
        assert_stable: false);

    int[] reverse_expected = [..expected];
    Array.Reverse(reverse_expected);

    span = [..p.values];
    span.AsSpan().IntroSort(kReverse);
    AssertSorted("IntroSort/single-span/reverse", span, reverse_expected);

    keys    = [..p.values];
    payload = [..p.payload];
    CollectionUtils.IntroSort(keys.AsSpan(), payload.AsSpan(), kReverse);
    AssertKeyValueOracle("IntroSort/key-value/reverse", p.values, keys, payload, reverse_expected,
        assert_stable: false);
  }

  [Theory]
  [MemberData(nameof(SortSelectionData.Params), MemberType = typeof(SortSelectionData))]
  public void PdqSort_MatchesSortedOracle(SortSelectionParam p) {
    int[] expected = SortSelectionData.Sorted(p.values);

    int[] span = [..p.values];
    span.AsSpan().PdqSort();
    AssertSorted("PdqSort/single-span", span, expected);

    int[] keys    = [..p.values];
    int[] payload = [..p.payload];
    CollectionUtils.PdqSort(keys.AsSpan(), payload.AsSpan());
    AssertKeyValueOracle("PdqSort/key-value", p.values, keys, payload, expected,
        assert_stable: false);

    int[] reverse_expected = [..expected];
    Array.Reverse(reverse_expected);

    span = [..p.values];
    span.AsSpan().PdqSort(kReverse);
    AssertSorted("PdqSort/single-span/reverse", span, reverse_expected);

    keys    = [..p.values];
    payload = [..p.payload];
    CollectionUtils.PdqSort(keys.AsSpan(), payload.AsSpan(), kReverse);
    AssertKeyValueOracle("PdqSort/key-value/reverse", p.values, keys, payload, reverse_expected,
        assert_stable: false);
  }

  [Theory]
  [MemberData(nameof(SortSelectionData.Params), MemberType = typeof(SortSelectionData))]
  public void TimSort_MatchesSortedOracle(SortSelectionParam p) {
    int[] expected = SortSelectionData.Sorted(p.values);

    int[] span = [..p.values];
    span.AsSpan().TimSort();
    AssertSorted("TimSort/single-span", span, expected);

    int[] keys    = [..p.values];
    int[] payload = [..p.payload];
    CollectionUtils.TimSort(keys.AsSpan(), payload.AsSpan());
    AssertKeyValueOracle("TimSort/key-value", p.values, keys, payload, expected,
        assert_stable: true);

    int[] reverse_expected = [..expected];
    Array.Reverse(reverse_expected);

    span = [..p.values];
    span.AsSpan().TimSort(kReverse);
    AssertSorted("TimSort/single-span/reverse", span, reverse_expected);

    keys    = [..p.values];
    payload = [..p.payload];
    CollectionUtils.TimSort(keys.AsSpan(), payload.AsSpan(), kReverse);
    AssertKeyValueOracle("TimSort/key-value/reverse", p.values, keys, payload, reverse_expected,
        assert_stable: true);
  }

  static void AssertSorted<TKey>(string label, TKey[] got, TKey[] expected)
      where TKey : unmanaged, IComparable<TKey> {
    if (!got.AsSpan().SequenceEqual(expected))
      throw new XunitException(string.Format("{0} n={1}: expected {2}, got {3}", label, got.Length,
          SortSelectionData.Dump(expected), SortSelectionData.Dump(got)));
  }

  /**
   * <summary>
   *  Full key/value oracle. The payload started as each key's original index, so after sorting it
   *  must still be a permutation of <c>0..n-1</c> and every key must sit next to the payload slot
   *  it came from: <c>keys[payload[i]] == sorted_keys[i]</c>. This is what catches a merge that
   *  copies a key but not its value (or the reverse). When <paramref name="assert_stable" /> the
   * equal keys must also keep their original relative order.
   * </summary>
   */
  static void AssertKeyValueOracle<TKey>(string label, TKey[] keys, TKey[] sorted_keys,
      int[] sorted_payload, TKey[] expected, bool assert_stable)
      where TKey : unmanaged, IComparable<TKey> {
    AssertSorted(label, sorted_keys, expected);

    int n       = keys.Length;
    bool[] seen = new bool[n];
    for (int i = 0; i < n; ++i) {
      int j = sorted_payload[i];
      if (j < 0 || j >= n || seen[j])
        throw new XunitException(string.Format("{0} n={1}: payload not a permutation at {2}: {3}",
            label, n, i, SortSelectionData.Dump(sorted_payload)));
      seen[j] = true;
      if (keys[j].CompareTo(sorted_keys[i]) != 0)
        throw new XunitException(string.Format("{0} n={1}: key/payload mismatch at {2}: {3} vs {4}",
            label, n, i, keys[j], sorted_keys[i]));
    }

    if (!assert_stable)
      return;
    for (int i = 1; i < n; ++i)
      if (sorted_keys[i - 1].CompareTo(sorted_keys[i]) == 0  //
          && sorted_payload[i - 1] >= sorted_payload[i])
        throw new XunitException(string.Format("{0} n={1}: unstable at {2}: {3}", label, n, i,
            SortSelectionData.Dump(sorted_payload)));
  }
}
}
