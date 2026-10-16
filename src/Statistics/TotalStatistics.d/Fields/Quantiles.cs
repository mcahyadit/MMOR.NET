using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Statistics {
public readonly partial struct TotalStatisticsV2 {
  public readonly ImmutableDictionary<double, double> CommonQuantiles { get; init; }
  public readonly ulong OutlierCount { get; init; }

  public double Median {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => CommonQuantiles is null || CommonQuantiles.IsEmpty ? double.NaN : CommonQuantiles[0.5];
  }
  public readonly double Quartile1st {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => CommonQuantiles is null || CommonQuantiles.IsEmpty ? double.NaN : CommonQuantiles[0.25];
  }
  public readonly double Quartile3rd {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => CommonQuantiles is null || CommonQuantiles.IsEmpty ? double.NaN : CommonQuantiles[0.75];
  }

  public double InterQuartileRange {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get =>
        CommonQuantiles is null || CommonQuantiles.IsEmpty ? double.NaN : Quartile3rd - Quartile1st;
  }
  public double LowerFence {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => CommonQuantiles is null || CommonQuantiles.IsEmpty
               ? double.NaN
               : Math.Max(MinValue, Quartile1st - 1.5 * InterQuartileRange);
  }
  public readonly double UpperFence {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => CommonQuantiles is null || CommonQuantiles.IsEmpty
               ? double.NaN
               : Math.Min(MaxValue, Quartile3rd + 1.5 * InterQuartileRange);
  }

  /**
   * <summary>
   *  Central Tendency defined by weighted average of Quartiles.
   * </summary>
   */
  public double TriMean {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => CommonQuantiles is null || CommonQuantiles.IsEmpty
               ? double.NaN
               : (Quartile1st + 2 * Median + Quartile3rd) / 4;
  }

  /**
   * <summary>
   *  Average between the 1st and 3rd Quartiles.
   * </summary>
   */
  public double MidHinge {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => CommonQuantiles is null || CommonQuantiles.IsEmpty  //
               ? double.NaN
               : (Quartile1st + Quartile3rd) / 2;
  }

  public double QuartileCoefficientOfDispersion {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (CommonQuantiles is null || CommonQuantiles.IsEmpty || Quartile1st + Quartile3rd == 0)
        return double.NaN;
      return InterQuartileRange / (Quartile1st + Quartile3rd);
    }
  }

  public double PearsonModeSkewness {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (Count < 2)
        return double.NaN;
      return (Mean - ModeValue) / StandardDeviation;
    }
  }

  public double PearsonMedianSkewness {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (CommonQuantiles is null || CommonQuantiles.IsEmpty || Count < 2)
        return double.NaN;
      return 3 * (Mean - Median) / StandardDeviation;
    }
  }

  /**
   * <summary>
   *  Quantile-based <see cref="Skewness"/> metric. <br/>
   *  Based on 3rd and 1st Quartile.
   * </summary>
   */
  public double YuleCoefficient {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (!double.IsFinite(InterQuartileRange) || InterQuartileRange == 0)
        return double.NaN;
      return (Quartile3rd + Quartile1st - 2 * Median) / InterQuartileRange;
    }
  }

  /**
   * <summary>
   *  Quantile-based <see cref="Skewness"/> metric. <br/>
   *  Based on 90th and 10th Percentile.
   * </summary>
   */
  public double KellySkewness {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (CommonQuantiles is null || CommonQuantiles.IsEmpty)
        return double.NaN;

      double q90th = CommonQuantiles[0.9];
      double q10th = CommonQuantiles[0.1];
      if (q90th - q10th == 0)
        return double.NaN;
      return (q90th + q10th - 2 * Median) / (q90th - q10th);
    }
  }

  public double GroeneveldMeedenCoefficient {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (CommonQuantiles is null || CommonQuantiles.IsEmpty || MedianAbsoluteDeviation == 0)
        return double.NaN;
      return (Mean - Median) / MedianAbsoluteDeviation;
    }
  }

  /**
   * <summary>
   *  Ratio of <see cref="MedianAbsoluteDeviation"/> to <see cref="Median"/>.
   * </summary>
   */
  public double IndexOfDispersion {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (CommonQuantiles is null || CommonQuantiles.IsEmpty || Median == 0)
        return double.NaN;
      return MedianAbsoluteDeviation / Median;
    }
  }
}
}
