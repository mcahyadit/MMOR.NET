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

  public readonly double MinValue { get; init; }
  public readonly ulong MinCount { get; init; }
  public readonly double MaxValue { get; init; }
  public readonly ulong MaxCount { get; init; }
  public readonly double ModeValue { get; init; }
  public readonly ulong ModeCount { get; init; }

  public static readonly TotalStatisticsV2 kDefault = new TotalStatisticsV2 {
    Count           = 0,
    MinValue        = double.NaN,
    MaxValue        = double.NaN,
    ModeValue       = double.NaN,
    CommonQuantiles = default!,

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
  };
}
}
