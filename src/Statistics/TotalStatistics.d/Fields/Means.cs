using System;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Statistics {
public readonly partial struct TotalStatisticsV2 {
  /**
   * <summary>
   *  <br/>
   *  Ignores 0 and negative values. <br/>
   *  Averaged by <see cref="CountPositive"/>.
   * </summary>
   */
  public readonly double GeometricMean { get; init; }
  /**
   * <summary>
   *  <br/>
   *  Ignores 0. <br/>
   *  Averaged by <see cref="CountA"/>.
   * </summary>
   */
  public readonly double HarmonicMean { get; init; }
  public readonly double InterQuartileMean { get; init; }

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
}
}
