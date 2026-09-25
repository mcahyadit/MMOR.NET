using System;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Statistics {
public partial class RunningStatisticsV2 {
  public ulong Count      = 0;
  public double CountF64 => Count;
  public double Mean      = 0;
  public double Sum {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => Mean * Count;
  }

  /// <summary> WARNING: Modify at your own risk </summary>
  public double moment_2nd_ = 0;
  /// <summary> WARNING: Modify at your own risk </summary>
  public double moment_3rd_ = 0;
  /// <summary> WARNING: Modify at your own risk </summary>
  public double moment_4th_ = 0;

  public double MinValue = double.MaxValue;
  public ulong MinCount  = 0;
  public double MaxValue = double.MinValue;
  public ulong MaxCount  = 0;

  public double Variance {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (Count < 2)
        return double.NaN;
      return moment_2nd_ / (Count - 1);
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
  public double CoefficientOfVariation {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => StandardDeviation / Mean;
  }

  public double Skewness {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (Count < 3)
        return double.NaN;
      return Count * moment_3rd_ * Math.Sqrt(moment_2nd_ / (Count - 1)) /
             (moment_2nd_ * moment_2nd_ * (Count - 2)) * (Count - 1);
    }
  }

  public double Kurtosis {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (Count < 4)
        return double.NaN;
      return (CountF64 * Count - 1) / ((Count - 2) * (Count - 3)) *
             (Count * moment_4th_ / (moment_2nd_ * moment_2nd_) - 3 + 6.0 / (Count + 1));
    }
  }
}
}
