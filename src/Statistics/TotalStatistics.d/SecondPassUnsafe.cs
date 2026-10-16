using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using MMOR.NET.Collections;
using MMOR.NET.Mathematics;

namespace MMOR.NET.Statistics {
public static partial class TotalStatisticsExtension {
  public static TotalStatisticsV2 SecondPassUnsafe(Span<double> values, Span<ulong> counts,
      RunningStatistics first_pass, ulong count_positive, ulong count_negative, ulong count_0) {
    double sum      = first_pass.Sum;
    double geom_sum = 0;
    double iq_sum   = 0;

    double mode_val = 0;
    ulong mode_cnt  = 0;

    ulong re_cnt       = 0;
    double c_val       = 0;
    double gini_index  = 0;
    double entropy     = 0;
    double hh_index    = 0;
    double theil_index = 0;
    double simp_index  = 0;

    double q1_point = first_pass.Count * 0.25;
    double q3_point = first_pass.Count * 0.75;

    //=============
    // Second Pass
    //=============
    int length = values.Length;

    Dictionary<double, double> quantiles = new(kCommonQuantiles.Length);

    double last_value = values[0];
    int idx           = 0;
    int unique        = 0;

    for (; idx < length; ++unique) {
      //=====================
      // Collapse Duplicates
      //=====================
      double value = values[idx];

      int run_idx = 1;
      while (idx + run_idx < length) {
        if (values[idx + run_idx] != value) {
          break;
        }
        run_idx = Math.Min(length - idx, run_idx << 1);
      }
      int dupes   = CollectionUtils.TailBinarySearch(values.Slice(idx, run_idx), value);
      ulong count = dupes > 1 ? CollectionUtils.Sum(counts.Slice(idx, dupes)) : counts[idx];

      values[unique] = value;
      counts[unique] = count;
      idx += dupes;
      //=====================

      if (mode_cnt < count) {
        mode_val = value;
        mode_cnt = count;
      }

      re_cnt += count;
      iq_sum += value * Math.Max(0, Math.Min(re_cnt, q3_point)  //
                                        - Math.Max(re_cnt - count, q1_point));

      foreach (double tau in kCommonQuantiles) {
        // NOTE: Use type R-7 Quantile
        double point = tau * (first_pass.Count - 1);
        if (re_cnt < Math.Ceiling(point) + 1)
          break;

        if (quantiles.ContainsKey(tau))
          continue;

        double lo_rank = Math.Floor(point);
        double frac    = point - lo_rank;
        double lo_val  = (lo_rank + 1.0) > (re_cnt - count) ? value : last_value;
        quantiles.Add(tau, value * frac + lo_val * (1 - frac));
      }
      last_value = value;

      geom_sum += value > 0 ? Math.Log(value) * count : 0;

      double p_cnt = count / first_pass.CountF64;
      if (sum != 0) {
        double p_val = value / sum * count;
        c_val += p_val;
        gini_index += 2.0 * p_cnt * c_val - p_cnt * p_val;
        entropy += p_cnt * MathExt.Log2(p_cnt);
        hh_index += p_val * p_val;
      }

      double ratio = first_pass.Mean == 0 ? 0 : value / first_pass.Mean;
      theil_index += ratio <= 0 ? 0 : count * ratio * Math.Log(ratio);
      simp_index += p_cnt * (count - 1.0) / (first_pass.CountF64 - 1);
    }
    entropy          = sum == 0 ? double.NaN : -entropy;
    gini_index       = sum == 0 ? double.NaN : 1.0 - gini_index;
    theil_index      = count_positive == 0 ? double.NaN : theil_index / count_positive;
    hh_index         = sum == 0 ? double.NaN : hh_index;
    simp_index       = first_pass.Count < 2 ? double.NaN : 1 - simp_index;
    double iq_mean   = 2 * iq_sum / first_pass.Count;
    double geom_mean = count_positive == 0 ? double.NaN : Math.Exp(geom_sum / count_positive);

    //=============
    // Third Pass
    //=============
    double iqr   = quantiles[0.75] - quantiles[0.25];
    double lower = Math.Max(quantiles[0.25] - 1.5 * iqr, first_pass.MinValue);
    double upper = Math.Min(quantiles[0.75] + 1.5 * iqr, first_pass.MaxValue);

    ulong outliers   = 0;
    double m_abs_dev = 0;
    double u_abs_dev = 0;
    double harm_sum  = 0;

    int vlen = Vector<double>.Count;
    int vend = 0;
    if (Vector.IsHardwareAccelerated) {
      vend                      = unique - (unique % vlen);
      Vector<double> mean_epi   = new(first_pass.Mean);
      Vector<double> median_epi = new(quantiles[0.5]);
      Vector<double> lower_epi  = new(lower);
      Vector<double> upper_epi  = new(upper);

      Vector<ulong> outliers_epi   = Vector<ulong>.Zero;
      Vector<double> m_abs_dev_epi = Vector<double>.Zero;
      Vector<double> u_abs_dev_epi = Vector<double>.Zero;
      Vector<double> harm_sum_epi  = Vector<double>.Zero;
      for (int i = 0; i < vend; i += vlen) {
        Vector<double> value = values.Slice(i, vlen).ToVector();
        Vector<ulong> count  = counts.Slice(i, vlen).ToVector();

        Vector<double> count_f64 = Vector.ConvertToDouble(count);

        m_abs_dev_epi += Vector.Abs(value - median_epi) * count_f64;
        u_abs_dev_epi += Vector.Abs(value - mean_epi) * count_f64;
        harm_sum_epi += Vector.AndNot(count_f64 / value,  //
            Vector.Equals<double>(value, Vector<double>.Zero));

        Vector<long> cmp_lt      = Vector.LessThan(value, lower_epi);
        Vector<long> cmp_gt      = Vector.GreaterThan(value, upper_epi);
        Vector<ulong> is_outlier = Vector.AsVectorUInt64(Vector.BitwiseOr(cmp_lt, cmp_gt));
        outliers_epi += Vector.BitwiseAnd(is_outlier, count);
      }
      outliers += outliers_epi.SumElements();
      m_abs_dev += m_abs_dev_epi.SumElements();
      u_abs_dev += u_abs_dev_epi.SumElements();
      harm_sum += harm_sum_epi.SumElements();
    }
    for (int i = vend; i < unique; ++i) {
      ulong count  = counts[i];
      double value = values[i];

      m_abs_dev += Math.Abs(value - quantiles[0.5]) * count;
      u_abs_dev += Math.Abs(value - first_pass.Mean) * count;
      harm_sum += value != 0 ? count / value : 0;

      if (value < lower || value > upper)
        outliers += count;
    }
    m_abs_dev /= first_pass.CountF64;
    u_abs_dev /= first_pass.CountF64;
    double harm_mean = (first_pass.Count == count_0 || harm_sum == 0)
                           ? double.NaN
                           : (first_pass.Count - count_0) / harm_sum;

    return new TotalStatisticsV2 {
      Count         = first_pass.Count,
      Count0        = count_0,
      CountNegative = count_negative,
      CountPositive = count_positive,

      Mean             = first_pass.Mean,
      CentralMoment2nd = first_pass.moments_.Central2nd,
      CentralMoment3rd = first_pass.moments_.Central3rd,
      CentralMoment4th = first_pass.moments_.Central4th,

      MinValue  = first_pass.MinValue,
      MinCount  = first_pass.MinCount,
      MaxValue  = first_pass.MaxValue,
      MaxCount  = first_pass.MaxCount,
      ModeValue = mode_val,
      ModeCount = mode_cnt,

      GeometricMean = geom_mean,
      HarmonicMean  = harm_mean,

      Entropy                  = entropy,
      GiniCoefficient          = gini_index,
      HerfindahlHirschmanIndex = hh_index,
      TheilIndex               = theil_index,
      SimpsonDiversityIndex    = simp_index,

      MeanAbsoluteDeviation   = u_abs_dev,
      MedianAbsoluteDeviation = m_abs_dev,

      CommonQuantiles = quantiles.ToImmutableDictionary(),

      InterQuartileMean = iq_mean,
      OutlierCount      = outliers,
    };
  }
}
}
