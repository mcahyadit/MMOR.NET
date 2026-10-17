using System.Collections.Frozen;
using System.Collections.Generic;

namespace MMOR.NET.Statistics {
public enum CommonQuantile {
  /// <summary> 2.15 Percentile, Approximately 2nd. </summary>
  SevenLowOuter,
  /// <summary> 8.87 Percentile, Approximately 8th. </summary>
  SevenLowInner,
  Percentile5th,
  Percentile10th,
  Octile1st,
  Quartile1st,
  Median,
  Quartile3rd,
  Octile7th,
  Percentile90th,
  Percentile95th,
  /// <summary> 91.13 Percentile, Approximately 92nd. </summary>
  SevenHighInner,
  /// <summary> 97.85 Percentile, Approximately 98th. </summary>
  SevenHighOuter,
}
public static partial class TotalStatisticsExtension {
  /**
   * <summary>
   *  R-7 tau of every <see cref="CommonQuantile"/>.
   * </summary>
   */
  public static readonly FrozenDictionary<CommonQuantile, double> kCommonQuantiles =
      FrozenDictionary.ToFrozenDictionary(new Dictionary<CommonQuantile, double> {
        [CommonQuantile.SevenLowOuter]  = 0.0215,
        [CommonQuantile.Percentile5th]  = 0.05,
        [CommonQuantile.SevenLowInner]  = 0.0887,
        [CommonQuantile.Percentile10th] = 0.10,
        [CommonQuantile.Octile1st]      = 0.125,
        [CommonQuantile.Quartile1st]    = 0.25,
        [CommonQuantile.Median]         = 0.5,
        [CommonQuantile.Quartile3rd]    = 0.75,
        [CommonQuantile.Octile7th]      = 0.875,
        [CommonQuantile.Percentile90th] = 0.90,
        [CommonQuantile.SevenHighInner] = 0.9113,
        [CommonQuantile.Percentile95th] = 0.95,
        [CommonQuantile.SevenHighOuter] = 0.9785,
      });

  public const int kHeapThreshold = 128;

  // Internal State Tracker
  // .. for First Pass Mini-TimSorting
  public enum TimState : byte {
    HitMinrun    = 1 << 1,
    NeedsSorting = 1 << 2,
    NeedsReverse = 1 << 3,
    QueueMerge   = 1 << 4,
  }

  public enum QuantileType {
    R1,
    R2,
    R3,
    R4,
    R5,
    R6,
    R7,
    R8,
    R9,
    // Names
    InvertedCDF            = R1,
    AveragedInvertedCDF    = R2,
    ClosestObservation     = R3,
    InterpolatedInverseCDF = R4,
    Hazen                  = R5,
    Weibull                = R6,
    Linear                 = R7,
    Gumbel                 = R7,
    MedianUnbiased         = R8,
    Blom                   = R9,
    NormalUnbiased         = R9,
  }
}
}
