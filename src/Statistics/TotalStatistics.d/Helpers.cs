using System.Collections.Immutable;

namespace MMOR.NET.Statistics {
public static partial class TotalStatisticsExtension {
  public const int kHeapThreshold = 128;

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

  public static readonly ImmutableArray<double> kCommonQuantiles =
      ImmutableArray.Create(stackalloc double[] {
        0.0215,  // 7 Number Summary
        0.0887,  // 7 Number Summary
        0.10,    // Bowley Summary
        0.125,   // Tunkey Summary
        0.25,    // Q1
        0.5,     // Median
        0.75,    // Q3
        0.875,   // Tunkey Summary
        0.90,    // Bowley Summary
        0.9113,  // 7 Number Summary
        0.9785,  // 7 Number Summary
      });
}
}
