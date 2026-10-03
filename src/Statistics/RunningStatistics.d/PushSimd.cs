using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MMOR.NET.Bits;
using MMOR.NET.Collections;
using MMOR.NET.Mathematics;
using MMOR.Roslyn;

namespace MMOR.NET.Statistics {
public static partial class RunningStatisticsExtensions {
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(List<>), typeof(CollectionsMarshal),
      nameof(CollectionsMarshal.AsSpan))]
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(ImmutableArray<>), typeof(ImmutableArray<>),
      "AsSpan()")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Push(this RunningStatistics self, ReadOnlySpan<double> values) {
    Push(self, values, (ReadOnlySpan<ulong>)default);
  }

  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(List<>), typeof(CollectionsMarshal),
      nameof(CollectionsMarshal.AsSpan))]
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(ImmutableArray<>), typeof(ImmutableArray<>),
      "AsSpan()")]
  public static void Push(this RunningStatistics self, ReadOnlySpan<double> values,
      ReadOnlySpan<ulong> counts) {
    if (!counts.IsEmpty && values.Length != counts.Length)
      throw new ArgumentException(
          "[ERROR]: Length mismatch.\n" +
          $"values.Length: {values.Length} != counts.Count: {counts.Length}");

    if (values.IsEmpty)
      return;

    int alen = values.Length;
    int vlen = Vector<double>.Count;

    int rem = alen - vlen;

    int i = 0;
    if (self.Count == 0) {
      if (counts.IsEmpty) {
        Push(self, values[i]);
      } else {
        int j   = 0;
        ulong m = 0;
        for (; j <= rem; j += vlen) {
          Vector<ulong> v_epi  = counts.Slice(j, vlen).ToVector();
          Vector<ulong> cmp_gt = Vector.GreaterThan(v_epi, Vector<ulong>.Zero);

          m = BitOps.MmMovemaskEpi64(cmp_gt);
          if (m != 0)
            break;
        }
        if (m != 0) {
          i = j + BitOperations.TrailingZeroCount(m);
        } else {
          i = j;
          while (counts[i] == 0) {
            if (++i == alen)
              return;
          }
        }

        Push(self, values[i], counts[i]);
      }
      ++i;
    }

    if (alen >= vlen + i) {
      double shift             = self.moments_.Raw1st;
      Vector<double> shift_epi = new(shift);

      Vector<double> sum1_epi = Vector<double>.Zero;
      Vector<double> sum2_epi = Vector<double>.Zero;
      Vector<double> sum3_epi = Vector<double>.Zero;
      Vector<double> sum4_epi = Vector<double>.Zero;
      Vector<ulong> cnt       = Vector<ulong>.Zero;

      Vector<double> min_val = new(double.MaxValue);
      Vector<ulong> min_cnt  = Vector<ulong>.Zero;
      Vector<double> max_val = new(double.MinValue);
      Vector<ulong> max_cnt  = Vector<ulong>.Zero;

      for (; i <= rem; i += vlen) {
        Vector<double> value_epi = values.Slice(i, vlen).ToVector();

        // Shift for numerical stability
        Vector<double> delta  = value_epi - shift_epi;
        Vector<double> delta2 = delta * delta;

        Vector<ulong> count_epi;
        if (!counts.IsEmpty) {
          count_epi = counts.Slice(i, vlen).ToVector();

          Vector<double> count_f64_epi = Vector.ConvertToDouble(count_epi);

          sum1_epi += delta * count_f64_epi;
          sum2_epi += delta2 * count_f64_epi;
          sum3_epi += delta2 * delta * count_f64_epi;
          sum4_epi += delta2 * delta2 * count_f64_epi;
          cnt += count_epi;
        } else {
          count_epi = Vector<ulong>.One;
          sum1_epi += delta;
          sum2_epi += delta2;
          sum3_epi += delta2 * delta;
          sum4_epi += delta2 * delta2;
          cnt += Vector<ulong>.One;
        }

        Vector<long> cnt_nz =
            Vector.AsVectorInt64(Vector.GreaterThan(count_epi, Vector<ulong>.Zero));

        min_cnt = Vector.ConditionalSelect(Vector.AsVectorUInt64(Vector.BitwiseAnd(cnt_nz,  //
                                               Vector.LessThan(value_epi, min_val))),
            count_epi, min_cnt);
        min_cnt = Vector.ConditionalSelect(Vector.AsVectorUInt64(Vector.BitwiseAnd(cnt_nz,  //
                                               Vector.Equals(value_epi, min_val))),
            min_cnt + count_epi, min_cnt);
        max_cnt = Vector.ConditionalSelect(Vector.AsVectorUInt64(Vector.BitwiseAnd(cnt_nz,  //
                                               Vector.GreaterThan(value_epi, max_val))),
            count_epi, max_cnt);
        max_cnt = Vector.ConditionalSelect(Vector.AsVectorUInt64(Vector.BitwiseAnd(cnt_nz,  //
                                               Vector.Equals(value_epi, max_val))),
            max_cnt + count_epi, max_cnt);
        min_val = Vector.ConditionalSelect(cnt_nz, Vector.Min(min_val, value_epi), min_val);
        max_val = Vector.ConditionalSelect(cnt_nz, Vector.Max(max_val, value_epi), max_val);
      }  // End of SIMD Loop

      // Reduce Moments
      ulong count = cnt.SumElements();
      double sum1 = sum1_epi.SumElements();
      double sum2 = sum2_epi.SumElements();
      double sum3 = sum3_epi.SumElements();
      double sum4 = sum4_epi.SumElements();

      double pivot = sum1 / count;
      double mean  = shift + pivot;

      self.PushMoments(
          new MomentsRecord {
            Raw1st     = mean,
            Central2nd = sum2 - sum1 * pivot,
            Central3rd = sum3 - 3 * pivot * sum2 + 2 * pivot * pivot * sum1,
            Central4th = sum4 - 4 * pivot * sum3 + 6 * pivot * pivot * sum2 -
                         3 * pivot * pivot * pivot * sum1,
          },
          count);

      for (int k = 0; k < vlen; ++k) {
        self.PushMinMax(new MinMaxRecord {
          MinValue = min_val[k],
          MinCount = min_cnt[k],
          MaxValue = max_val[k],
          MaxCount = max_cnt[k],
        });
      }
    }

    for (; i < alen; ++i) {
      if (counts.IsEmpty)
        Push(self, values[i]);
      else
        Push(self, values[i], counts[i]);
    }
  }
}
}
