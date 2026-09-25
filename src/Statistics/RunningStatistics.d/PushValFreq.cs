
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using MMOR.NET.Collections;
using MMOR.NET.Mathematics;

namespace MMOR.NET.Statistics {
public static partial class RunningStatisticsV2Extensions {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Push(this RunningStatisticsV2 self, double value, ulong count) {
    if (count == 0)
      return;
    if (count == 1) {
      self.Push(value);
    } else {
      Push(self, new RunningStatisticsV2 {
        Mean  = value,
        Count = count,

        MinValue = value,
        MinCount = count,
        MaxValue = value,
        MaxCount = count,
      });
    }
  }

  public static void Push(this RunningStatisticsV2 self,
      IEnumerable<KeyValuePair<double, ulong>> values) {
    foreach ((double value, ulong count) in values) {
      Push(self, value, count);
    }
  }

  public static void Push<TValue, TCount>(this RunningStatisticsV2 self,
      IEnumerable<KeyValuePair<TValue, TCount>> values)
      where TValue : struct, IConvertible
      where TCount : struct, IConvertible {
    foreach ((TValue value, TCount count) in values) {
      Push(self, value.ToDouble(null), count.ToUInt64(null));
    }
  }

  public static void Push(this RunningStatisticsV2 self, IEnumerable<(double, ulong)> values) {
    foreach ((double value, ulong count) in values) {
      Push(self, value, count);
    }
  }

  public static void Push<TValue, TCount>(this RunningStatisticsV2 self,
      IEnumerable<(TValue, TCount)> values)
      where TValue : struct, IConvertible
      where TCount : struct, IConvertible {
    foreach ((TValue value, TCount count) in values) {
      Push(self, value.ToDouble(null), count.ToUInt64(null));
    }
  }

  public static void Push(this RunningStatisticsV2 self, ReadOnlySpan<double> values,
      ReadOnlySpan<ulong> counts) {
    if (values.Length != counts.Length)
      throw new ArgumentException(
          "[ERROR]: Length mismatch.\n" +
          $"values.Length: {values.Length} != counts.Count: {counts.Length}");

    int vlen = Vector<double>.Count;
    int alen = values.Length;
    int rem  = alen - vlen;

    Vector<double> min_val = new(self.MinValue);
    Vector<ulong> min_cnt  = Vector<ulong>.Zero;
    Vector<double> max_val = new(self.MaxValue);
    Vector<ulong> max_cnt  = Vector<ulong>.Zero;

    int i = 0;
    for (; i <= rem; i += vlen) {
      Vector<double> value     = values.Slice(i, vlen).ToVector();
      Vector<ulong> count      = counts.Slice(i, vlen).ToVector();
      Vector<double> count_f64 = Vector.ConvertToDouble(count);
      ulong count_u64          = count.SumElements();

      if (count_u64 == 0)
        continue;

      double mean           = Vector.Dot(value, count_f64) / count_u64;
      Vector<double> delta  = value - new Vector<double>(mean);
      Vector<double> delta2 = delta * delta;
      Vector<double> delta3 = delta2 * delta;
      Vector<double> delta4 = delta2 * delta2;
      double moment_2nd     = Vector.Dot(delta2, count_f64);
      double moment_3rd     = Vector.Dot(delta3, count_f64);
      double moment_4th     = Vector.Dot(delta4, count_f64);

      Push(self, new RunningStatisticsV2 {
        Mean  = mean,
        Count = count_u64,

        moment_2nd_ = moment_2nd,
        moment_3rd_ = moment_3rd,
        moment_4th_ = moment_4th,
      });

      min_cnt = Vector.ConditionalSelect(Vector.AsVectorUInt64(Vector.LessThan(value, min_val)),
          count, min_cnt);
      min_cnt = Vector.ConditionalSelect(Vector.AsVectorUInt64(Vector.Equals(value, min_val)),
          min_cnt + count, min_cnt);
      max_cnt = Vector.ConditionalSelect(Vector.AsVectorUInt64(Vector.GreaterThan(value, max_val)),
          count, max_cnt);
      max_cnt = Vector.ConditionalSelect(Vector.AsVectorUInt64(Vector.Equals(value, max_val)),
          max_cnt + count, max_cnt);
      min_val = Vector.Min(min_val, value);
      max_val = Vector.Max(max_val, value);
    }

    for (int k = 0; k < vlen; ++k) {
      if (min_val[k] < self.MinValue) {
        self.MinValue = min_val[k];
        self.MinCount = min_cnt[k];
      } else if (min_val[k] == self.MinValue) {
        self.MinCount += min_cnt[k];
      }

      if (max_val[k] > self.MaxValue) {
        self.MaxValue = max_val[k];
        self.MaxCount = max_cnt[k];
      } else if (max_val[k] == self.MaxValue) {
        self.MaxCount += max_cnt[k];
      }
    }

    for (; i < alen; ++i) {
      Push(self, values[i], counts[i]);
    }
  }
}
}
