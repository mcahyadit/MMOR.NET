using System;
using System.Collections.Generic;
using System.Numerics;
using MMOR.NET.Collections;
using MMOR.NET.Mathematics;

namespace MMOR.NET.Statistics {
public static partial class RunningStatisticsV2Extensions {
  public static void Push(this RunningStatisticsV2 self, double value) {
    ++self.Count;
    double delta = value - self.Mean;
    double s     = delta / self.Count;
    double s2    = s * s;
    double t     = delta * s * (self.Count - 1);

    double moment_1st = self.Mean + s;
    self.Mean         = moment_1st;
    self.moment_4th_ += t * s2 * (self.Count * self.Count - 3 * self.Count + 3) +
                        6 * s2 * self.moment_2nd_ - 4 * s * self.moment_3rd_;
    self.moment_3rd_ += t * s * (self.Count - 2) - 3 * s * self.moment_2nd_;
    self.moment_2nd_ += t;

    if (value < self.MinValue) {
      self.MinValue = value;
      self.MinCount = 1;
    } else if (value == self.MinValue) {
      ++self.MinCount;
    }

    if (value > self.MaxValue) {
      self.MaxValue = value;
      self.MaxCount = 1;
    } else if (value == self.MaxValue) {
      ++self.MaxCount;
    }
  }

  public static void Push(this RunningStatisticsV2 self, IEnumerable<double> values) {
    foreach (double value in values) {
      Push(self, value);
    }
  }

  public static void Push<T>(this RunningStatisticsV2 self, IEnumerable<T> values)
      where T : struct, IConvertible {
    foreach (T value in values) {
      Push(self, value.ToDouble(null));
    }
  }

  public static void Push(this RunningStatisticsV2 self, ReadOnlySpan<double> values) {
    int vlen = Vector<double>.Count;
    int alen = values.Length;
    int rem  = alen - vlen;

    Vector<double> min_val = new(self.MinValue);
    Vector<ulong> min_cnt  = Vector<ulong>.Zero;
    Vector<double> max_val = new(self.MaxValue);
    Vector<ulong> max_cnt  = Vector<ulong>.Zero;

    int i = 0;
    for (; i <= rem; i += vlen) {
      Vector<double> value      = values.Slice(i, vlen).ToVector();
      double mean               = value.SumElements() / vlen;
      Vector<double> delta_epi  = value - new Vector<double>(mean);
      Vector<double> delta2_epi = delta_epi * delta_epi;
      Vector<double> delta3_epi = delta2_epi * delta_epi;
      Vector<double> delta4_epi = delta2_epi * delta2_epi;
      double moment_2nd_epi     = delta2_epi.SumElements();
      double moment_3rd_epi     = delta3_epi.SumElements();
      double moment_4th_epi     = delta4_epi.SumElements();

      ulong total_count = self.Count + (ulong)vlen;
      double delta      = mean - self.Mean;
      double delta2     = delta * delta;
      double delta3     = delta2 * delta;
      double delta4     = delta2 * delta2;

      double count_cross   = self.CountF64 * vlen;
      double self_count_2  = self.CountF64 * self.Count;
      double other_count_2 = vlen * vlen;
      double total_count_2 = (double)total_count * total_count;
      double total_count_3 = total_count_2 * total_count;

      double moment_1st = self.Mean + delta * vlen / total_count;
      double moment_2nd = self.moment_2nd_ + moment_2nd_epi  //
                          + delta2 * self.Count * vlen / total_count;
      double moment_3rd = self.moment_3rd_ + moment_3rd_epi                           //
                          + delta3 *                                                  //
                                count_cross * (self.CountF64 - vlen) / total_count_2  //
                          +
                          3 * delta *  //
                              (self.Count * moment_2nd_epi - vlen * self.moment_2nd_) / total_count;
      double moment_4th = self.moment_4th_ + moment_4th_epi  //
                          +
                          delta4 * count_cross * (self_count_2 - count_cross + other_count_2) /  //
                              total_count_3                                                      //
                          + 6 * delta2 *                                                         //
                                (self_count_2 * moment_2nd_epi + other_count_2 * self.moment_2nd_) /
                                total_count_2                                              //
                          + 4 * delta *                                                    //
                                (self.Count * moment_3rd_epi - vlen * self.moment_3rd_) /  //
                                total_count;

      self.Count       = total_count;
      self.Mean        = moment_1st;
      self.moment_2nd_ = moment_2nd;
      self.moment_3rd_ = moment_3rd;
      self.moment_4th_ = moment_4th;

      min_cnt = Vector.ConditionalSelect(Vector.AsVectorUInt64(Vector.LessThan(value, min_val)),
          Vector<ulong>.One, min_cnt);
      min_cnt = Vector.ConditionalSelect(Vector.AsVectorUInt64(Vector.Equals(value, min_val)),
          min_cnt + Vector<ulong>.One, min_cnt);
      max_cnt = Vector.ConditionalSelect(Vector.AsVectorUInt64(Vector.GreaterThan(value, max_val)),
          Vector<ulong>.One, max_cnt);
      max_cnt = Vector.ConditionalSelect(Vector.AsVectorUInt64(Vector.Equals(value, max_val)),
          max_cnt + Vector<ulong>.One, max_cnt);
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
      Push(self, values[i]);
    }
  }
}
}
