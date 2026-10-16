using System;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Statistics {
public readonly partial struct TotalStatisticsV2 {
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
