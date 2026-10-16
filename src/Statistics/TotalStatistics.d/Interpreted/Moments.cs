using System;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Statistics {
public readonly partial struct TotalStatisticsV2 {
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
}
}
