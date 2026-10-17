using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MMOR.NET.Statistics {
[StructLayout(LayoutKind.Auto)]
public readonly partial struct TotalStatisticsV2 {
  public readonly ulong Count { get; init; }
  public double CountF64 => Count;
  public readonly ulong Count0 { get; init; }
  public readonly ulong CountNegative { get; init; }
  public readonly ulong CountPositive { get; init; }
  public ulong CountA {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => Count - Count0;
  }

  public readonly double Mean { get; init; }
  public readonly double CentralMoment2nd { get; init; }
  public readonly double CentralMoment3rd { get; init; }
  public readonly double CentralMoment4th { get; init; }

  public readonly double MinValue { get; init; }
  public readonly ulong MinCount { get; init; }
  public readonly double MaxValue { get; init; }
  public readonly ulong MaxCount { get; init; }
  public readonly double ModeValue { get; init; }
  public readonly ulong ModeCount { get; init; }

  public readonly FrozenDictionary<CommonQuantile, double> CommonQuantiles { get; init; }
  public readonly ulong OutlierCount { get; init; }

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

  public static readonly TotalStatisticsV2 kDefault = new TotalStatisticsV2 {
    Count     = 0,
    MinValue  = double.NaN,
    MaxValue  = double.NaN,
    ModeValue = double.NaN,

    Mean              = double.NaN,
    CentralMoment2nd  = double.NaN,
    CentralMoment3rd  = double.NaN,
    CentralMoment4th  = double.NaN,
    GeometricMean     = double.NaN,
    HarmonicMean      = double.NaN,
    InterQuartileMean = double.NaN,

    MedianAbsoluteDeviation  = double.NaN,
    MeanAbsoluteDeviation    = double.NaN,
    Entropy                  = double.NaN,
    GiniCoefficient          = double.NaN,
    HerfindahlHirschmanIndex = double.NaN,
    TheilIndex               = double.NaN,
    SimpsonDiversityIndex    = double.NaN,

    CommonQuantiles = ((Func<FrozenDictionary<CommonQuantile, double>>)(() => {
      FrozenDictionary<CommonQuantile, double> cmn = TotalStatisticsExtension.kCommonQuantiles;
      Dictionary<CommonQuantile, double> result    = new(cmn.Count);
      foreach ((var name, _) in cmn) {
        result.Add(name, double.NaN);
      }
      return FrozenDictionary.ToFrozenDictionary(result);
    })).Invoke(),
  };
}
}
