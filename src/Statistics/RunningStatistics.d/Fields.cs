using System;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Statistics {
public partial class RunningStatistics {
  public ulong Count      = 0;
  public double CountF64 => Count;
  public MomentsRecord moments_;
  public MinMaxRecord min_max_;

  public double Mean {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (Count == 0)
        return double.NaN;
      return moments_.Raw1st;
    }
  }
  public double Sum {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => moments_.Raw1st * Count;
  }

  public double MinValue {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (Count == 0)
        return double.NaN;
      return min_max_.MinValue;
    }
  }
  public ulong MinCount {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (Count == 0)
        return 0;
      return min_max_.MinCount;
    }
  }

  public double MaxValue {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (Count == 0)
        return double.NaN;
      return min_max_.MaxValue;
    }
  }
  public ulong MaxCount {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (Count == 0)
        return 0;
      return min_max_.MaxCount;
    }
  }

  public double Variance {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (Count < 2)
        return double.NaN;
      return moments_.Central2nd / (Count - 1);
    }
  }

  public double StandardDeviation {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (Count < 2)
        return double.NaN;
      return Math.Sqrt(moments_.Central2nd / (Count - 1));
    }
  }
  public double StandardError {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (Count < 2)
        return double.NaN;
      return Math.Sqrt(moments_.Central2nd / (CountF64 * (Count - 1)));
    }
  }
  public double CoefficientOfVariation {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (Count < 2)
        return double.NaN;
      return Math.Sqrt(moments_.Central2nd / (Count - 1)) / moments_.Raw1st;
    }
  }

  public double Skewness {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (Count < 3)
        return double.NaN;
      if (moments_.Central2nd == 0)
        return 0;
      return Count * moments_.Central3rd * Math.Sqrt(moments_.Central2nd / (Count - 1)) /
             (moments_.Central2nd * moments_.Central2nd * (Count - 2)) * (Count - 1);
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
      if (moments_.Central2nd == 0)
        return 0;
      return (CountF64 * Count - 1) / ((CountF64 - 2) * (Count - 3)) *
             (Count * moments_.Central4th / (moments_.Central2nd * moments_.Central2nd) - 3 +
                 6.0 / (Count + 1));
    }
  }
}
}
