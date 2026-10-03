using System;
using System.Numerics;
using Xunit;

namespace MMOR.NET.Statistics {
public partial class RunningStatisticsTest {
  public static readonly TheoryData<ZeroCountParam> kZeroCountTestParams = BuildZeroCountParams();

  static TheoryData<ZeroCountParam> BuildZeroCountParams() {
    TheoryData<ZeroCountParam> data = [];

    data.Add(new ZeroCountParam {
      identifier = "zeros-all-8",
      values     = [1.0, 2.0, -3.5, 0.25, 7.5, -9.0, 4.5, -1.25],
      counts     = [0, 0, 0, 0, 0, 0, 0, 0],
    });

    data.Add(new ZeroCountParam {
      identifier = "zeros-all-32",
      values     = Linear(32, -8.0, 0.5),
      counts     = Fill(32, 0),
    });

    data.Add(new ZeroCountParam {
      identifier = "zeros-lead-16",
      values =
          [
            1.5, -2.5, 3.5, -4.5, 5.5, -6.5, 7.5, -8.5, 9.5, -10.5, 11.5, -12.5, 13.5, -14.5, 15.5,
            -16.5
          ],
      counts   = [0, 0, 0, 0, 1, 2, 3, 1, 4, 0, 2, 1, 3, 1, 5, 2],
      relative = true,
    });

    data.Add(new ZeroCountParam {
      identifier = "zeros-blocks-32",
      values     = MixedMagnitudes(32),
      counts     = LeadingZeros(CycleCounts(32, 7), 8),
      relative   = true,
    });

    data.Add(new ZeroCountParam {
      identifier = "zeros-head-tail-16",
      values =
          [
            2.0, 4.0, 6.0, 8.0, 10.0, 12.0, 14.0, 16.0, 18.0, 20.0, 22.0, 24.0, 26.0, 28.0, 30.0,
            32.0
          ],
      counts   = [0, 1, 2, 3, 4, 5, 6, 7, 8, 2, 3, 4, 5, 6, 7, 0],
      relative = true,
    });

    data.Add(new ZeroCountParam {
      identifier = "zeros-interleave-32",
      values     = Linear(32, -4.0, 0.25),
      counts     = EvenIndexZero(CycleCounts(32, 9)),
    });

    data.Add(new ZeroCountParam {
      identifier = "zeros-dup-minmax-12",
      values     = [1.0, 1.0, 5.0, 5.0, 1.0, 5.0, 3.0, 3.0, 1.0, 5.0, 1.0, 5.0],
      counts     = [2, 0, 3, 0, 0, 4, 1, 0, 5, 0, 0, 6],
    });

    data.Add(new ZeroCountParam {
      identifier = "zeros-single-nonzero-16",
      values     = Linear(16, -6.0, 0.75),
      counts     = FillOne(16, 0, 9, 3),
    });

    for (int len = 1; len <= 9; ++len) {
      data.Add(new ZeroCountParam {
        identifier = string.Format("zeros-lead-len{0}", len),
        values     = Linear(len, 1.0, 1.0),
        counts     = FillOne(len, 1, 0, 0),
      });
      data.Add(new ZeroCountParam {
        identifier = string.Format("zeros-tail-len{0}", len),
        values     = Linear(len, 1.0, 1.0),
        counts     = FillOne(len, 1, len - 1, 0),
      });
    }

    int vlen = Vector<double>.Count;
    foreach (int zero_prefix in new[] { vlen - 1, vlen, vlen + 1, 2 * vlen - 1, 2 * vlen }) {
      int alen = 4 * vlen + 4;
      data.Add(new ZeroCountParam {
        identifier = string.Format("zeros-lead-vlen{0}-prefix{1}", vlen, zero_prefix),
        values     = Linear(alen, -8.0, 16.0 / alen),
        counts     = LeadingZeros(CycleCounts(alen, 7), zero_prefix),
      });
    }

    data.Add(new ZeroCountParam {
      identifier = string.Format("zeros-blocks-zero-tail-vlen{0}", vlen),
      values     = Linear(3 * vlen, -8.0, 16.0 / (3.0 * vlen)),
      counts     = LeadingZeros(CycleCounts(3 * vlen, 5), 2 * vlen),
    });

    data.Add(new ZeroCountParam {
      identifier = "zeros-mixed-64",
      values     = MixedMagnitudes(64),
      counts     = EveryThirdZero(CycleCounts(64, 10)),
      relative   = true,
    });

    data.Add(new ZeroCountParam {
      identifier = "zeros-scaled-32",
      values     = LogSpaced(1e-9, 1e9, 32),
      counts     = EveryThirdZero(ScaledCounts(32, 1000)),
      relative   = true,
    });

    data.Add(new ZeroCountParam {
      identifier = "zeros-wide-64",
      values     = LogSpaced(1e-12, 1e12, 64),
      counts     = EveryThirdZero(PowerOfTwoCounts(64)),
      relative   = true,
    });

    foreach (CountParam p in kCountTestParams) {
      data.Add(new ZeroCountParam {
        identifier         = string.Format("{0}-zerocount", p.identifier),
        values             = p.values,
        counts             = p.counts,
        precision          = p.precision,
        variance_precision = p.precision,
        skewness_precision = p.precision - 1,
        kurtosis_precision = p.precision - 1,
        tolerance          = p.tolerance,
        relative           = p.relative,
      });
    }

    return data;
  }

  /**
   * <summary>
   *  Copy of <paramref name="counts"/> with every third entry (index % 3) zeroed.
   * </summary>
   */
  static ulong[] EveryThirdZero(ulong[] counts) {
    ulong[] a = new ulong[counts.Length];
    for (int i = 0; i < counts.Length; ++i) {
      a[i] = i % 3 == 0 ? 0 : counts[i];
    }
    return a;
  }

  static ulong[] EvenIndexZero(ulong[] counts) {
    ulong[] a = new ulong[counts.Length];
    for (int i = 0; i < counts.Length; ++i) {
      a[i] = i % 2 == 0 ? 0 : counts[i];
    }
    return a;
  }

  static ulong[] LeadingZeros(ulong[] counts, int zero_prefix) {
    ulong[] a = new ulong[counts.Length];
    for (int i = 0; i < counts.Length; ++i) {
      a[i] = i < zero_prefix ? 0 : counts[i];
    }
    return a;
  }

  static ulong[] FillOne(int n, ulong value, int index, ulong index_count) {
    ulong[] a = new ulong[n];
    for (int i = 0; i < n; ++i) {
      a[i] = i == index ? index_count : value;
    }
    return a;
  }
}
}