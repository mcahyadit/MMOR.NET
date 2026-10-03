using System;
using Xunit;

namespace MMOR.NET.Statistics {
public partial class RunningStatisticsTest {
  /**
   * <summary>
   *  One case for the span overload. Either <see cref="values" /> supplies the sample directly, or
   *  a positive <see cref="length" /> asks for a generated fixture.
   * </summary>
   * <remarks>
   *  The reference is always the scalar Welford push, never a second SIMD evaluation, so the two
   *  are genuinely independent: Welford updates the central moments from deltas and never forms
   *  the raw power sums that the vector reduction has to cancel its way back out of.
   * </remarks>
   */
  public struct SimdParam {
    public string identifier { get; init; } = "Generic Test";
    public double[] values { get; init; }   = [];
    public int length { get; init; }        = 0;
    public ulong seed { get; init; }        = 0x5EED5EED5EED5EED;
    public double offset { get; init; }
    public double? first_sample { get; init; }

    /// <summary>Placeholder precisions, overridden below when the fixture is generated.</summary>
    public int mean_precision { get; init; }               = 15;
    public int standard_deviation_precision { get; init; } = 15;
    public int skewness_precision { get; init; }           = 15;
    public int kurtosis_precision { get; init; }           = 15;
    public int min_value_precision { get; init; }          = 15;
    public int max_value_precision { get; init; }          = 15;
    public int min_count_precision { get; init; }          = 15;
    public int max_count_precision { get; init; }          = 15;

    /**
     * <summary>
     *  Relative tolerance, used instead of the decimal precisions once a fixture is generated. A
     *  fixed number of decimal places is not a meaningful bar once the sample is displaced: the
     *  mean then occupies the leading digits of every sample and its share of the significand is
     *  what is left for the deviations, so the reachable accuracy falls as the displacement grows.
     *  Scaling the bar with the magnitude keeps the comparison honest at every displacement.
     * </summary>
     */
    public double tolerance { get; init; } = 1e-12;

    public SimdParam() {}
    public override string ToString() => identifier;
  }

  [Theory]
  [MemberData(nameof(kSimdTestParams))]
  [MemberData(nameof(kSimdOffsetParams))]
  [MemberData(nameof(kSimdOutlierParams))]
  public void SimdTest(SimdParam p) {
    double[] values = p.length > 0 ? BuildSamples(p) : p.values;

    RunningStatistics s1 = new();
    RunningStatistics s2 = new();

    for (int i = 0; i < values.Length; ++i) {
      s1.Push(values[i]);
    }

    s2.Push(values.AsSpan());

    try {
      Assert.Equal(s1.Count, s2.Count);
      Assert.Equal(s1.MinCount, s2.MinCount);
      Assert.Equal(s1.MaxCount, s2.MaxCount);

      AssertSame(p, s1.MinValue, s2.MinValue, p.min_value_precision, "MinValue");
      AssertSame(p, s1.MaxValue, s2.MaxValue, p.max_value_precision, "MaxValue");

      AssertSame(p, s1.Mean, s2.Mean, p.mean_precision, "Mean");
      AssertSame(p, s1.StandardDeviation, s2.StandardDeviation, p.standard_deviation_precision,
          "StandardDeviation");
      AssertSame(p, s1.Skewness, s2.Skewness, p.skewness_precision, "Skewness");
      AssertSame(p, s1.Kurtosis, s2.Kurtosis, p.kurtosis_precision, "Kurtosis");
    } catch {
      Console.Error.WriteLine(p.identifier);
      throw;
    }
  }

  static double RelativeError(double expected, double actual) {
    double scale = Math.Max(Math.Abs(expected), Math.Abs(actual));
    if (scale <= 0)
      return Math.Abs(expected - actual);

    return Math.Abs(expected - actual) / scale;
  }

  /**
   * <summary>
   *  Generated fixtures are held to a relative bar that widens with the displacement. Decimal
   *  places stop meaning anything once the sample is moved far enough to exhaust the significand,
   *  and the normalized moments divide that error down a second time: the reachable accuracy of
   *  skewness and kurtosis falls in proportion to |displacement| / sigma. Scaling the bar by the
   *  displacement keeps the comparison meaningful at every magnitude, and still has teeth, because
   *  the failure this guards against lands near unity in relative terms however far out it is.
   * </summary>
   */
  static void AssertSame(SimdParam p, double expected, double actual, int precision, string name) {
    if (p.length <= 0) {
      TestUtils.AssertApproximately(expected, actual, precision);
      return;
    }

    double displacement = Math.Max(Math.Abs(p.offset), Math.Abs(p.first_sample ?? 0.0));
    double bar          = p.tolerance * Math.Max(1.0, displacement);
    double error        = RelativeError(expected, actual);

    Assert.True(error <= bar,
        $"{p.identifier}: {name} relative error {error:E3} exceeds {bar:E3} " +
            $"(expected {expected:R}, actual {actual:R})");
  }
}
}
