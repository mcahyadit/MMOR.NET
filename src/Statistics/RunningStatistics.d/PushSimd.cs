using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MMOR.NET.Collections;
using MMOR.NET.Mathematics;
using MMOR.Roslyn;

namespace MMOR.NET.Statistics {
public static partial class RunningStatisticsV2Extensions {
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(List<>), typeof(CollectionsMarshal),
      nameof(CollectionsMarshal.AsSpan))]
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(ImmutableArray<>), typeof(ImmutableArray<>),
      "AsSpan()")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Push(this RunningStatisticsV2 self, ReadOnlySpan<double> values) {
    Push(self, values, (ReadOnlySpan<ulong>)default);
  }

  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(List<>), typeof(CollectionsMarshal),
      nameof(CollectionsMarshal.AsSpan))]
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(ImmutableArray<>), typeof(ImmutableArray<>),
      "AsSpan()")]
  public static void Push(this RunningStatisticsV2 self, ReadOnlySpan<double> values,
      ReadOnlySpan<ulong> counts) {
    if (!counts.IsEmpty && values.Length != counts.Length)
      throw new ArgumentException(
          "[ERROR]: Length mismatch.\n" +
          $"values.Length: {values.Length} != counts.Count: {counts.Length}");

    int vlen = Vector<double>.Count;
    int alen = values.Length;
    int rem  = alen - vlen;

    Vector<double> min_val = new(self.min_value_);
    Vector<ulong> min_cnt  = Vector<ulong>.Zero;
    Vector<double> max_val = new(self.max_value_);
    Vector<ulong> max_cnt  = Vector<ulong>.Zero;

    int i                   = 0;
    RunningStatisticsV2 tmp = new();
    for (; i <= rem; i += vlen) {
      Vector<double> value = values.Slice(i, vlen).ToVector();
      if (counts.IsEmpty) {
        double mean           = value.SumElements() / vlen;
        Vector<double> delta  = value - new Vector<double>(mean);
        Vector<double> delta2 = delta * delta;
        Vector<double> delta3 = delta2 * delta;
        Vector<double> delta4 = delta2 * delta2;
        double moment_2nd     = delta2.SumElements();
        double moment_3rd     = delta3.SumElements();
        double moment_4th     = delta4.SumElements();

        tmp.Count       = (ulong)vlen;
        tmp.moment_1st_ = mean;
        tmp.moment_2nd_ = moment_2nd;
        tmp.moment_3rd_ = moment_3rd;
        tmp.moment_4th_ = moment_4th;

        min_cnt = Vector.ConditionalSelect(Vector.AsVectorUInt64(Vector.LessThan(value, min_val)),
            Vector<ulong>.One, min_cnt);
        min_cnt = Vector.ConditionalSelect(Vector.AsVectorUInt64(Vector.Equals(value, min_val)),
            min_cnt + Vector<ulong>.One, min_cnt);
        max_cnt = Vector.ConditionalSelect(
            Vector.AsVectorUInt64(Vector.GreaterThan(value, max_val)), Vector<ulong>.One, max_cnt);
        max_cnt = Vector.ConditionalSelect(Vector.AsVectorUInt64(Vector.Equals(value, max_val)),
            max_cnt + Vector<ulong>.One, max_cnt);
      } else {
        Vector<ulong> count = counts.Slice(i, vlen).ToVector();
        ulong count_u64     = count.SumElements();

        if (count_u64 == 0)
          continue;
        Vector<double> count_f64 = Vector.ConvertToDouble(count);

        double mean           = Vector.Dot(value, count_f64) / count_u64;
        Vector<double> delta  = value - new Vector<double>(mean);
        Vector<double> delta2 = delta * delta;
        Vector<double> delta3 = delta2 * delta;
        Vector<double> delta4 = delta2 * delta2;
        double moment_2nd     = Vector.Dot(delta2, count_f64);
        double moment_3rd     = Vector.Dot(delta3, count_f64);
        double moment_4th     = Vector.Dot(delta4, count_f64);

        tmp.Count       = count_u64;
        tmp.moment_1st_ = mean;
        tmp.moment_2nd_ = moment_2nd;
        tmp.moment_3rd_ = moment_3rd;
        tmp.moment_4th_ = moment_4th;

        min_cnt = Vector.ConditionalSelect(Vector.AsVectorUInt64(Vector.LessThan(value, min_val)),
            count, min_cnt);
        min_cnt = Vector.ConditionalSelect(Vector.AsVectorUInt64(Vector.Equals(value, min_val)),
            min_cnt + count, min_cnt);
        max_cnt = Vector.ConditionalSelect(
            Vector.AsVectorUInt64(Vector.GreaterThan(value, max_val)), count, max_cnt);
        max_cnt = Vector.ConditionalSelect(Vector.AsVectorUInt64(Vector.Equals(value, max_val)),
            max_cnt + count, max_cnt);
      }
      Push(self, tmp);
      min_val = Vector.Min(min_val, value);
      max_val = Vector.Max(max_val, value);
    }

    for (int k = 0; k < vlen; ++k) {
      if (min_val[k] < self.min_value_) {
        self.min_value_ = min_val[k];
        self.min_count_ = min_cnt[k];
      } else if (min_val[k] == self.min_value_) {
        self.min_count_ += min_cnt[k];
      }

      if (max_val[k] > self.max_value_) {
        self.max_value_ = max_val[k];
        self.max_count_ = max_cnt[k];
      } else if (max_val[k] == self.max_value_) {
        self.max_count_ += max_cnt[k];
      }
    }

    for (; i < alen; ++i) {
      Push(self, values[i]);
    }
  }
}
}
