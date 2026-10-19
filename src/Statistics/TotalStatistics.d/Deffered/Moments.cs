using System;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Statistics {
public readonly partial struct TotalStatistics {
  public double Sum {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => Mean * Count;
  }

  public double Variance {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (Count < 2)
        return double.NaN;
      return CentralMoment2nd / (Count - 1);
    }
  }

  public double Skewness {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (Count < 3)
        return double.NaN;
      if (Variance == 0)
        return 0;
      return Count * CentralMoment3rd * Math.Sqrt(CentralMoment2nd / (Count - 1)) /
             (CentralMoment2nd * CentralMoment2nd * (Count - 2)) * (Count - 1);
    }
  }

  public double Kurtosis {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (Count < 4)
        return double.NaN;
      if (CentralMoment2nd == 0)
        return 0;
      return (CountF64 * Count - 1) / ((CountF64 - 2) * (Count - 3)) *
             (Count * CentralMoment4th / (CentralMoment2nd * CentralMoment2nd) - 3 +
                 6.0 / (Count + 1));
    }
  }

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

  public double RootMeanSquare {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => Math.Sqrt((CentralMoment2nd + Count * Mean * Mean) / Count);
  }

  public double CubicMean {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      double sum3 = CentralMoment3rd + 3 * CentralMoment2nd * Mean + Count * Mean * Mean * Mean;
      return Math.Sign(sum3) * Math.Pow(Math.Abs(sum3) / Count, 1.0 / 3.0);
    }
  }

  public double ContraHarmonicMean {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (Mean == 0 || Count == 0)
        return double.NaN;
      return (CentralMoment2nd + Count * Mean * Mean) / (Mean * Count);
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
