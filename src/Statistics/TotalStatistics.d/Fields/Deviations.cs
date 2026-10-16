using System;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Statistics {
public readonly partial struct TotalStatisticsV2 {
  /**
   * <summary>
   *  Average of Absolute of the Differences between each element to the <see cref="Mean"/>
   * </summary>
   */
  public readonly double MeanAbsoluteDeviation { get; init; }
  /**
   * <summary>
   *  Average of Absolute of the Differences between each element to the <see cref="Median"/>
   * </summary>
   */
  public readonly double MedianAbsoluteDeviation { get; init; }

  public readonly double Entropy { get; init; }
  public readonly double GiniCoefficient { get; init; }
  public readonly double HerfindahlHirschmanIndex { get; init; }
  /**
   * <summary>
   *  Economic Redundancy Metric for measure of <i>equality</i>. <br/>
   *  <br/>
   *  Averaged by <see cref="CountPositive"/>. <br/>
   *  Measurement ignores zeroes and negative values.
   * </summary>
   */
  public readonly double TheilIndex { get; init; }
  public readonly double SimpsonDiversityIndex { get; init; }

  public double StandardDeviation {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (Count < 2)
        return double.NaN;
      return Math.Sqrt(Variance);
    }
  }

  public double StandardError {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (Count < 2)
        return double.NaN;
      return Math.Sqrt(Variance / Count);
    }
  }

  /**
   * <summary>
   *  Ratio of <see cref="StandardDeviation"/> to <see cref="Mean"/>.
   * </summary>
   */
  public double CoefficientOfVariation {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (Count < 2 || Mean == 0)
        return double.NaN;
      return StandardDeviation / Mean;
    }
  }

  public double HerfindahlHirschmanIndexNormalized {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (Count < 2)
        return double.NaN;
      double rec = 1 / CountF64;
      return (HerfindahlHirschmanIndex - rec) / (1.0 - rec);
    }
  }
}
}
