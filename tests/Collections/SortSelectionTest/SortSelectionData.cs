using System;
using Xunit;

namespace MMOR.NET.Collections {
/**
 * <summary>
 *  One shared scenario for the sort and selection tests. <see cref="values" /> is the key sample,
 *  <see cref="payload" /> the parallel array whose element <c>i</c> records the original index of
 *  key <c>i</c> (so any reordering of equal keys is observable), and <see cref="ks" /> the ranks
 *  (1-based) probed by the selection tests.
 * </summary>
 */
public struct SortSelectionParam {
  public string identifier { get; init; } = "Generic Test";
  public required int[] values { get; init; }
  public required int[] payload { get; init; }
  public required int[] ks { get; init; }
  public SortSelectionParam() {}
  public override string ToString() => identifier;
}

/**
 * <summary>
 *  Shared dataset source and oracle helpers for the sort and selection tests. Every sample carries
 *  its own payload, so the stable sorts can assert order preservation on all of them.
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

    // Small random / duplicate-heavy samples: probe every rank.
    for (int n = 1; n <= 40; ++n) {
      Add(data, string.Format("distinct-len{0}", n), RandomValues(n, rng), AllK(n));
      Add(data, string.Format("duplicates-len{0}", n), RandomValues(n, rng, 4), AllK(n));
      Add(data, string.Format("stability-dup3-len{0}", n), RandomValues(n, rng, 3), AllK(n));
    }

    for (int n = 1; n <= 16; ++n)
      Add(data, string.Format("alleq-len{0}", n), Repeat(n, 42), AllK(n));

    foreach (int n in new[] { 129, 200, 1000 })
      Add(data, string.Format("large-len{0}", n), RandomValues(n, rng, n / 2 + 1), SampleK(n));

    // TimSort only leaves its binary-insertion fast path once n exceeds ComputeMinRun (`min_run`
    // lives in [32, 64)); probe both sides of that cliff and the ArrayPool threshold (128).
    foreach (int n in new[] { 32, 33, 63, 64, 65, 127, 128 })
      Add(data, string.Format("boundary-len{0}", n), RandomValues(n, rng), SampleK(n));

    // Monotone inputs: TimSort finds (and reverses) a single run, quicksorts must survive their
    // classic sorted/reverse-sorted degenerate pivots.
    foreach (int n in new[] { 24, 25, 32, 48, 64, 96, 128, 129, 256, 1024 }) {
      Add(data, string.Format("ascending-len{0}", n), Ascending(n), SampleK(n));
      Add(data, string.Format("descending-len{0}", n), Descending(n), SampleK(n));
    }

    // Two runs that make one merge side win every comparison: the samples the old suite lacked,
    // which is what let the broken key/value TimMerge ship.
    foreach (int n in new[] { 128, 256, 1024 }) {
      Add(data, string.Format("tim-gallop-left-len{0}", n), GallopLeft(n), SampleK(n));
      Add(data, string.Format("tim-gallop-right-len{0}", n), GallopRight(n), SampleK(n));
    }

    // Musser's median-of-three killer. IntroSort and IntroSelect pick their pivot / pivot
    // estimate as the median of first/middle/last, which this permutation of 1..n defeats, so
    // they burn through their depth limit and land on the heapsort / heap-select fallback.
    // PdqSort and PdqSelect are median-of-three based too, but their BreakPatterns shuffle
    // (and the partial-insertion fast path) scrambles the pattern before the depth limit trips.
    foreach (int n in new[] { 32, 100, 128, 256, 1000 })
      Add(data, string.Format("median3-killer-len{0}", n), MedianOfThreeKiller(n), SampleK(n));

    // Bitonic / sawtooth shapes: heavy duplicates plus many short runs, exercising run
    // extension, plateau merges and duplicate gallops.
    foreach (int n in new[] { 64, 128, 129, 256, 1000 }) {
      Add(data, string.Format("organ-pipe-len{0}", n), OrganPipe(n), SampleK(n));
      Add(data, string.Format("sawtooth-p16-len{0}", n), Sawtooth(n, 16), SampleK(n));
      Add(data, string.Format("sawtooth-p32-len{0}", n), Sawtooth(n, 32), SampleK(n));
    }

    foreach (int n in new[] { 128, 1000 })
      Add(data, string.Format("fewdistinct-len{0}", n), RandomValues(n, rng, 4), SampleK(n));

    // Duplicate-heavy stability shapes: many equal keys, so a stable sort's payload ordering is
    // what proves it kept the original relative order.
    foreach (int n in new[] { 2, 31, 32, 33, 64, 65, 127, 128, 129, 200, 1000 }) {
      Add(data, string.Format("stability-alleq-len{0}", n), Repeat(n, 7), SampleK(n));
      Add(data, string.Format("stability-asc-plateau-len{0}", n), Plateau(n, 4, false), SampleK(n));
      Add(data, string.Format("stability-desc-plateau-len{0}", n), Plateau(n, 4, true), SampleK(n));
      Add(data, string.Format("stability-sawtooth-len{0}", n), Sawtooth(n, 16), SampleK(n));
      Add(data, string.Format("stability-dupn-len{0}", n), RandomValues(n, rng, n / 8 + 1),
          SampleK(n));
    }

    return data;
  }

  static void Add(TheoryData<SortSelectionParam> data, string id, int[] values,
      int[] ks) => data.Add(new SortSelectionParam {
    identifier = id,
    values     = values,
    payload    = Payload(values.Length),
    ks         = ks,
  });

  static int[] Payload(int n) {
    int[] payload = new int[n];
    for (int i = 0; i < n; ++i) payload[i] = i;
    return payload;
  }

  static int[] RandomValues(int n, System.Random rng, int max_exclusive = 0) {
    int[] values = new int[n];
    for (int i = 0; i < n; ++i)
      values[i] = max_exclusive > 0 ? rng.Next(max_exclusive) : rng.Next();
    return values;
  }

  static int[] Repeat(int n, int value) {
    int[] values = new int[n];
    Array.Fill(values, value);
    return values;
  }

  static int[] AllK(int n) {
    int[] ks = new int[n];
    for (int i = 0; i < n; ++i) ks[i] = i + 1;
    return ks;
  }

  /**
   * <summary>
   *  The ranks to probe against the oracle: every rank for tiny samples, otherwise a sparse
   *  spread covering the ends and the middle.
   * </summary>
   */
  static int[] SampleK(int n) => n <= 8 ? AllK(n) : [1, n / 4, n / 2, n - 1, n];

  static int[] Ascending(int n) {
    int[] values = new int[n];
    for (int i = 0; i < n; ++i) values[i] = i;
    return values;
  }

  static int[] Descending(int n) {
    int[] values = new int[n];
    for (int i = 0; i < n; ++i) values[i] = n - 1 - i;
    return values;
  }

  static int[] Plateau(int n, int width, bool descending) {
    int[] values = new int[n];
    for (int i = 0; i < n; ++i) values[i] = (descending ? n - 1 - i : i) / width;
    return values;
  }

  /**
   * <summary>
   *  A descending block followed by a larger ascending block. TimSort reverses the first block
   *  into the smaller run, so the left run wins every merge comparison and drives the left
   *  gallop (with its <c>TailBinarySearch</c> bulk copy).
   * </summary>
   */
  static int[] GallopLeft(int n) {
    int m        = n / 2;
    int[] values = new int[n];
    for (int i = 0; i < m; ++i) values[i] = m - 1 - i;
    for (int i = m; i < n; ++i) values[i] = i;
    return values;
  }

  /**
   * <summary>
   *  The mirror of <see cref="GallopLeft(int)"/>: an ascending block followed by a descending
   *  one, so the right run is the smaller and drives the right gallop (<c>HeadBinarySearch</c>).
   * </summary>
   */
  static int[] GallopRight(int n) {
    int m        = n / 2;
    int[] values = new int[n];
    for (int i = 0; i < m; ++i) values[i] = m + i;
    for (int i = m; i < n; ++i) values[i] = n - 1 - i;
    return values;
  }

  /**
   * <summary>
   *  Musser's median-of-three killer sequence (a permutation of 1..n). Every median of
   *  first/middle/last is the minimum of its slice, so a median-of-three partition advances by
   *  only O(1) elements per level until the depth limit trips the heapsort/heap-select fallback.
   * </summary>
   * <remarks>
   *  Reference: M. D. McIlroy, "A Killer Adversary for Quicksort" (SP&amp;E 1999), and the
   *  construction popularised by <c>programmingpraxis.com</c>.
   * </remarks>
   */
  static int[] MedianOfThreeKiller(int n) {
    int[] values = new int[n];
    int len      = n;
    if (len % 2 != 0) {
      values[len - 1] = len;
      --len;
    }

    int half = len / 2;
    for (int i = 0; i < half; ++i) {
      bool odd         = half % 2 != 0;
      values[i]        = i % 2 == 0 ? i + 1 : half + i + (odd ? 1 : 0);
      values[half + i] = (i + 1) * 2;
    }
    return values;
  }

  static int[] OrganPipe(int n) {
    int[] values = new int[n];
    int half     = n / 2;
    for (int i = 0; i <= half && i < n; ++i) values[i] = i;
    for (int i = half + 1; i < n; ++i) values[i] = n - 1 - i;
    return values;
  }

  static int[] Sawtooth(int n, int period) {
    int[] values = new int[n];
    for (int i = 0; i < n; ++i) values[i] = i % period;
    return values;
  }
}
}
