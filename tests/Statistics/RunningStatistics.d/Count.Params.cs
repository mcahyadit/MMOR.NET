using System;
using Xunit;

namespace MMOR.NET.Statistics {
public partial class RunningStatisticsTest {
  public static readonly TheoryData<CountParam> kCountTestParams = BuildCountParams();

  static TheoryData<CountParam> BuildCountParams() {
    TheoryData<CountParam> data = [];

    data.Add(new CountParam {
      identifier = "unique10",
      values     = [1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0],
      counts     = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1],
    });

    data.Add(new CountParam {
      identifier = "dup10",
      values     = [1.0, 2.0, 3.0, 4.0, 5.0],
      counts     = [2, 3, 1, 5, 7],
    });

    data.Add(new CountParam {
      identifier = "lognorm20_dup",
      values =
          [
            0.9399765058618492, 1.0364096510349152, 1.2112884129025059, 0.6872902888021615,
            0.8007734256180844, 1.2730012634440684, 0.7620223150600683, 0.8872228477449238,
            1.4598327026902647, 1.4547767743452336, 0.7510745046968204, 0.8889790666309475,
            0.6426018241541233, 0.8575711350259936, 1.2352016830308057, 0.9446108483080732,
            1.6665316891723445, 0.9333630819614659, 0.7534769829202982, 0.9930239736550892
          ],
      counts = [5, 0, 2, 4, 1, 0, 3, 6, 2, 1, 4, 5, 0, 2, 3, 1, 7, 2, 0, 4],
    });

    data.Add(new CountParam {
      identifier = "with_zero",
      values     = [0.0, 1.0, 2.0, 3.0, 4.0],
      counts     = [3, 2, 1, 4, 5],
    });

    data.Add(new CountParam {
      identifier = "wide-range-64",
      values     = LogSpaced(1e-12, 1e12, 64),
      counts     = PowerOfTwoCounts(64),
      relative   = true,
    });

    data.Add(new CountParam {
      identifier = "tiny-50",
      values     = LogSpaced(1e-9, 1e-4, 50),
      counts     = ScaledCounts(50, 1000),
      relative   = true,
    });

    data.Add(new CountParam {
      identifier = "huge-50",
      values     = LogSpaced(1e14, 1e16, 50),
      counts     = ScaledCounts(50, 1000),
      relative   = true,
    });

    data.Add(new CountParam {
      identifier = "mixed-200",
      values     = MixedMagnitudes(200),
      counts     = CycleCounts(200, 10),
      relative   = true,
    });

    data.Add(new CountParam {
      identifier = "count-sweep-40",
      values     = Linear(40, 1.0, 1.0),
      counts     = Fill(40, 100_000),
      relative   = true,
    });

    data.Add(new CountParam {
      identifier = "extreme-count-16",
      values     = LogSpaced(0.5, 8.0, 16),
      counts     = Descending(16, 400_000, 25_000),
      relative   = true,
    });

    data.Add(new CountParam {
      identifier = "huge-scale-huge-count-32",
      values     = LogSpaced(1e13, 1e18, 32),
      counts     = Fill(32, 50_000),
      relative   = true,
    });

    data.Add(new CountParam {
      identifier = "heavy-tail-count-10k",
      values     = kHeavyTailValues,
      counts     = kHeavyTailCounts,
      relative   = true,
    });

    return data;
  }

  static double[] Linear(int n, double start, double step) {
    double[] a = new double[n];
    for (int i = 0; i < n; ++i) {
      a[i] = start + step * i;
    }
    return a;
  }

  static double[] LogSpaced(double min, double max, int n) {
    double[] a       = new double[n];
    double log_min   = Math.Log(min);
    double log_range = Math.Log(max) - log_min;
    for (int i = 0; i < n; ++i) {
      a[i] = Math.Exp(log_min + log_range * i / (n - 1));
    }
    return a;
  }

  static double[] MixedMagnitudes(int n) {
    double[] magnitudes = [1e-12, 1e-6, 1.0, 1e6, 1e12];
    double[] scales     = [1.0, 1.5, 2.0, 2.5, 3.0, 3.5, 4.0];
    double[] a          = new double[n];
    for (int i = 0; i < n; ++i) {
      a[i] = magnitudes[i % magnitudes.Length] * scales[i % scales.Length];
    }
    return a;
  }

  static ulong[] Fill(int n, ulong value) {
    ulong[] a = new ulong[n];
    for (int i = 0; i < n; ++i) {
      a[i] = value;
    }
    return a;
  }

  static ulong[] PowerOfTwoCounts(int n) {
    ulong[] a = new ulong[n];
    for (int i = 0; i < n; ++i) {
      a[i] = 1UL << (i % 10);
    }
    return a;
  }

  static ulong[] ScaledCounts(int n, ulong scale) {
    ulong[] a = new ulong[n];
    for (int i = 0; i < n; ++i) {
      a[i] = (1UL + (ulong)(i % 5)) * scale;
    }
    return a;
  }

  static ulong[] CycleCounts(int n, ulong max) {
    ulong[] a = new ulong[n];
    for (int i = 0; i < n; ++i) {
      a[i] = 1 + ((ulong)i % max);
    }
    return a;
  }

  static ulong[] Descending(int n, ulong first, ulong step) {
    ulong[] a = new ulong[n];
    for (int i = 0; i < n; ++i) {
      a[i] = first - step * (ulong)i;
    }
    return a;
  }
}
}