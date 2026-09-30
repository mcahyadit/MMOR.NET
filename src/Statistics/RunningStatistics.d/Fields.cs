using System;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Statistics {
public partial class RunningStatisticsV2 {
  public ulong Count      = 0;
  public double CountF64 => Count;

  public double Mean {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (Count == 0)
        return double.NaN;
      return moment_1st_;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set {
      moment_1st_ = value;
    }
  }
  public double Sum {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => moment_1st_ * Count;
  }

  public double moment_1st_ = 0;
  /// <summary> WARNING: Modify at your own risk </summary>
  public double moment_2nd_ = 0;
  /// <summary> WARNING: Modify at your own risk </summary>
  public double moment_3rd_ = 0;
  /// <summary> WARNING: Modify at your own risk </summary>
  public double moment_4th_ = 0;

  public double min_value_ = double.MaxValue;
  public double MinValue {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (Count == 0)
        return double.NaN;
      return min_value_;
    }
  }
  public ulong min_count_ = 0;
  public double MinCount {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (Count == 0)
        return double.NaN;
      return min_count_;
    }
  }

  public double max_value_ = double.MinValue;
  public double MaxValue {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (Count == 0)
        return double.NaN;
      return max_value_;
    }
  }
  public ulong max_count_ = 0;
  public double MaxCount {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (Count == 0)
        return double.NaN;
      return max_count_;
    }
  }

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
      return Math.Sqrt(moment_2nd_ / (Count - 1));
    }
  }
  public double StandardError {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (Count < 2)
        return double.NaN;
      return Math.Sqrt(moment_2nd_ / (CountF64 * (Count - 1)));
    }
  }
  public double CoefficientOfVariation {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => StandardDeviation / moment_1st_;
  }

  public double Skewness {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (Count < 3)
        return double.NaN;
      if (moment_2nd_ == 0)
        return 0;
      return Count * moment_3rd_ * Math.Sqrt(moment_2nd_ / (Count - 1)) /
             (moment_2nd_ * moment_2nd_ * (Count - 2)) * (Count - 1);
    }
  }

  /**
   * <summary>
   *  Bessel Corrected Excess Kurtosis.
   * </summary>
   */
  public double Kurtosis {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (Count < 4)
        return double.NaN;
      if (moment_2nd_ == 0)
        return 0;
      return (CountF64 * Count - 1) / ((Count - 2) * (Count - 3)) *
             (Count * moment_4th_ / (moment_2nd_ * moment_2nd_) - 3 + 6.0 / (Count + 1));
    }
  }
}
}
