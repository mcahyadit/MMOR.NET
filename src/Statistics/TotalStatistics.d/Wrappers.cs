using System;
using System.Buffers;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using MMOR.NET.Bits;
using MMOR.NET.Collections;

namespace MMOR.NET.Statistics {
public static partial class TotalStatisticsExtension {
  [MethodImpl(MethodImplOptions.NoInlining)]
  private static void ThrowIfNonFinite(double value) {
    if (!double.IsFinite(value))
      throw new NotSupportedException("[ERROR]: TotalStatistics cannot process non Finite number.");
  }

  public static TotalStatisticsV2 CalculateStatistics(this ReadOnlySpan<double> values) {
    if (values.IsEmpty)
      return TotalStatisticsV2.kDefault;

    int length = values.Length;

    double[] sort_tmp        = ArrayPool<double>.Shared.Rent(length);
    TotalStatisticsV2 result = TotalStatisticsV2.kDefault;
    try {
      values.CopyTo(sort_tmp);
      Array.Sort(sort_tmp, 0, length);

      using RentedList<double> val = new();
      using RentedList<ulong> cnt  = new();

      int vlen = Vector<double>.Count;
      int rem  = length - vlen;

      int i = 0;
      val.Add(sort_tmp[0]);
      cnt.Add(0);
      for (; i <= rem;) {
        Vector<double> value_epi = sort_tmp.AsSpan().Slice(i, vlen).ToVector();

        int count = 0;
        while (count == 0) {
          ThrowIfNonFinite(val[^1]);

          Vector<double> latest = new(val[^1]);
          Vector<long> cmp_eq   = Vector.Equals(value_epi, latest);

          ulong mask = BitOps.MmMovEpi64Mask(Vector.AsVectorUInt64(cmp_eq));
          count      = BitOperations.PopCount(mask);

          if (count == 0) {
            val.Add(value_epi[0]);
            cnt.Add(0);
          }
        }
        cnt[^1] += (ulong)count;
        i += count;
      }
      for (; i < length; ++i) {
        double value = sort_tmp[i];
        ThrowIfNonFinite(value);

        if (val[^1] != value) {
          val.Add(value);
          cnt.Add(0);
        }
        ++cnt[^1];
      }

      result = CalculateCoreUnsafe(val, cnt);
    } finally {
      ArrayPool<double>.Shared.Return(sort_tmp, true);
    }
    return result;
  }

  public static TotalStatisticsV2 CalculateStatistics<TVal, TCnt>(
      this IReadOnlyDictionary<TVal, TCnt> histogram)
      where TVal : struct, IConvertible
      where TCnt : struct, IConvertible {
    if (histogram.Count == 0)
      return TotalStatisticsV2.kDefault;
    int length               = histogram.Count;
    double[] values          = ArrayPool<double>.Shared.Rent(length);
    ulong[] counts           = ArrayPool<ulong>.Shared.Rent(length);
    TotalStatisticsV2 result = TotalStatisticsV2.kDefault;
    try {
      int unique = 0;
      foreach ((TVal key, TCnt val) in histogram) {
        double value = key.ToDouble(null);
        ulong count  = val.ToUInt64(null);

        ThrowIfNonFinite(value);
        if (count == 0)
          continue;

        values[unique] = value;
        counts[unique] = count;
        ++unique;
      }
      if (histogram is not SortedDictionary<TVal, TCnt>) {
        Array.Sort(values, counts, 0, unique);
      }

      result = CalculateCoreUnsafe(values.AsSpan(0, unique), counts.AsSpan(0, unique));
    } finally {
      ArrayPool<double>.Shared.Return(values, true);
      ArrayPool<ulong>.Shared.Return(counts, true);
    }
    return result;
  }

  public static TotalStatisticsV2 CalculateStatistics<T>(this IEnumerable<T> enumerable)
      where T : struct, IConvertible {
    using RentedList<double> values = new();
    foreach (T enumerator in enumerable) {
      double value = enumerator.ToDouble(null);
      ThrowIfNonFinite(value);
      values.Add(value);
    }
    return CalculateStatistics(values.AsSpan());
  }
}
}
