using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using System.Runtime.InteropServices;
using MMOR.NET.Collections;
using MMOR.NET.Mathematics;
using MMOR.Roslyn;

namespace MMOR.NET.Statistics {
/** <summary>
 * <br/> Extended version of <see cref="RunningStatistics"/>.
 * <br/> Additionally includes:
 * <list type="bullet">
 *  <item><see cref="Skewness">Skewness</see>,</item>
 *  <item><see cref="Kurtosis">Kurtosis</see>,</item>
 *  <item><see cref="GeometricMean">Geometric Mean</see>,</item>
 *  <item><see cref="HarmonicMean">Harmonic Mean</see>,</item>
 *  <item><see cref="RootMeanSquare">Root Mean Square</see></item>
 * </list>
 * </summary>
 * */
public partial class RunningStatisticsAdvanced : RunningStatistics {
  //====================================
  // █ █▄░█ ▀█▀ █▀▀ █▀█ █▀▀ ▄▀█ █▀▀ █▀▀
  // █ █░▀█ ░█░ ██▄ █▀▄ █▀░ █▀█ █▄▄ ██▄
  // MARK: Interface
  //====================================
  /// <inheritdoc cref="TotalStatistics.Skewness" />
  public double Skewness {
    get {
      if (count_ < 3)
        return double.NaN;
      return count_ * moment_3_ * Math.Sqrt(moment_2_ / (count_ - 1)) /
             (moment_2_ * moment_2_ * (count_ - 2)) * (count_ - 1);
    }
  }

  /// <inheritdoc cref="TotalStatistics.Kurtosis" />
  public double Kurtosis {
    get {
      if (count_ < 4)
        return double.NaN;
      return ((double)count_ * count_ - 1) / ((count_ - 2) * (count_ - 3)) *
             (count_ * moment_4_ / (moment_2_ * moment_2_) - 3 + 6.0 / (count_ + 1));
    }
  }

  /// <inheritdoc cref="StreamingStatistics.GeometricMean"/>
  public double GeometricMean => count_ < 1 ? double.NaN : Math.Exp(mean_geometric_ / count_);

  /// <inheritdoc cref="StreamingStatistics.HarmonicMean"/>
  public double HarmonicMean => count_ < 1 ? double.NaN : count_ / mean_harmonics_;

  /// <inheritdoc cref="StreamingStatistics.RootMeanSquare"/>
  public double RootMeanSquare => count_ < 1 ? double.NaN : Math.Sqrt(mean_rms_);
  //=================================
  // █ █▄░█ ▀█▀ █▀▀ █▀█ █▄░█ ▄▀█ █░░ 
  // █ █░▀█ ░█░ ██▄ █▀▄ █░▀█ █▀█ █▄▄ 
  // MARK: Internal
  //=================================
  protected double moment_3_ = 0;
  protected double moment_4_ = 0;

  protected double mean_geometric_ = 0;
  protected double mean_harmonics_ = 0;
  protected double mean_rms_       = 0;

  //=======================
  // █ █▄░█ █▀█ █░█ ▀█▀ █▀
  // █ █░▀█ █▀▀ █▄█ ░█░ ▄█
  // MARK: Inputs
  //=======================
  public override void Push(double value, ulong count = 1) {
    if (count == 0)
      return;

    double old_count = count_;
    if (value == 0)
      count_0_ += count;
    count_ += count;
    double d  = value - mean_;
    double s  = d / count_ * count;
    double s2 = s * s / count;
    double t  = d * s * old_count;

    mean_ += s;
    moment_4_ += t * s2 * (old_count * old_count - old_count * count + count * count) / count +
                 6 * s2 * count * moment_2_ - 4 * s * moment_3_;
    moment_3_ += t * s * (old_count - count) / count - 3 * s * moment_2_;
    moment_2_ += t;

    mean_harmonics_ += value == 0 ? value : 1.0 / value * count;
    mean_geometric_ += Math.Log(value) * count;
    mean_rms_ += (value * value - mean_rms_) * count / count_;

    //================
    // Update MinMax
    //================
    min_val_ = Math.Min(min_val_, value);
    EvaluateMax(value, count);
  }

  public void Push(RunningStatisticsAdvanced stats) {
    if (stats.count_ == 0) {
      return;
    } else if (count_ == 0) {
      count_   = stats.count_;
      count_0_ = stats.count_0_;

      min_val_   = stats.min_val_;
      max_val_   = stats.max_val_;
      count_max_ = stats.count_max_;

      mean_     = stats.mean_;
      moment_2_ = stats.moment_2_;
      moment_3_ = stats.moment_3_;
      moment_4_ = stats.moment_4_;

      mean_geometric_ = stats.mean_geometric_;
      mean_harmonics_ = stats.mean_harmonics_;
      mean_rms_       = stats.mean_rms_;
      return;
    }

    ulong total_count = count_ + stats.count_;
    double delta      = stats.mean_ - mean_;
    double delta2     = delta * delta;
    double delta3     = delta2 * delta;
    double delta4     = delta2 * delta2;

    double mean     = (count_ * mean_ + stats.count_ * stats.mean_) / total_count;
    double moment_2 = moment_2_ + stats.moment_2_ + delta2 * count_ * stats.count_ / total_count;
    double moment_3 =
        moment_3_ + stats.moment_3_ +
        delta3 * count_ * stats.count_ * (count_ - stats.count_) / (total_count * total_count) +
        3 * delta * (count_ * stats.moment_2_ - stats.count_ * moment_2_) / total_count;
    double moment_4 =
        moment_4_ + stats.moment_4_ +
        delta4 * count_ * stats.count_ *
            (count_ * count_ - count_ * stats.count_ + stats.count_ * stats.count_) /
            (total_count * total_count * total_count) +
        6 * delta2 * (count_ * count_ * stats.moment_2_ + stats.count_ * stats.count_ * moment_2_) /
            (total_count * total_count) +
        4 * delta * (count_ * stats.moment_3_ - stats.count_ * moment_3_) / total_count;

    //================
    // Update MinMax
    //================
    min_val_ = Math.Min(min_val_, stats.min_val_);
    if (stats.max_val_ > max_val_) {
      max_val_   = stats.max_val_;
      count_max_ = stats.count_max_;
    } else if (stats.max_val_ == max_val_) {
      count_max_ += stats.count_max_;
    }

    mean_geometric_ += stats.mean_geometric_;
    mean_harmonics_ += stats.mean_harmonics_;
    mean_rms_ = (count_ * mean_rms_ + stats.count_ * stats.mean_rms_) / total_count;

    count_ = total_count;
    count_0_ += stats.count_0_;
    mean_     = mean;
    moment_2_ = moment_2;
    moment_3_ = moment_3;
    moment_4_ = moment_4;
  }

  [Obsolete(
      "WARNING: The Vectorized input for RunningStatisticsAdvanced is not properly optimized yet, prefer scalar Pushing.")]
  public override void Push(Vector<double> values, Vector<ulong> counts,
      bool evaluate_minmax = true) {
    ulong b_cnt = MathExt.SumElements(counts);
    if (b_cnt == 0)
      return;

    count_0_ += Vector.Dot(counts, Vector.AsVectorUInt64(Vector.BitwiseAnd(Vector<long>.One,
                                       Vector.Equals(values, Vector<double>.Zero))));

    Vector<double> b_cnt_d = Vector.ConvertToDouble(counts);
    double b_sum           = Vector.Dot(values, b_cnt_d);
    double b_mean          = b_sum / b_cnt;
    Vector<double> b_dif   = values - new Vector<double>(b_mean);
    Vector<double> b_dif2  = b_dif * b_dif;
    double b_moment_2      = Vector.Dot(b_dif * b_dif, b_cnt_d);
    double b_moment_3      = Vector.Dot(b_dif2 * b_dif, b_cnt_d);
    double b_moment_4      = Vector.Dot(b_dif2 * b_dif2, b_cnt_d);

    double old_count = count_;
    count_ += b_cnt;
    double d1 = b_mean - mean_;
    double d2 = d1 * d1;
    double d3 = d2 * d1;
    double d4 = d2 * d2;
    double s  = d1 / count_ * b_cnt;
    double t1 = b_moment_2 + d1 * s * old_count;

    double moment_2 = moment_2_ + t1;
    double moment_3 = moment_3_ + b_moment_3 +
                      d3 * old_count * b_cnt * (old_count - b_cnt) / (count_ * count_) +
                      3 * d1 * (old_count * b_moment_2 - b_cnt * moment_2_) / count_;
    double moment_4 = moment_4_ + b_moment_4 +
                      d4 * old_count * b_cnt *
                          (old_count * old_count - old_count * b_cnt + b_cnt * b_cnt) /
                          (count_ * count_ * count_) +
                      6 * d2 * (old_count * old_count * b_moment_2 + b_cnt * b_cnt * moment_2_) /
                          (count_ * count_) +
                      4 * d1 * (old_count * b_moment_3 - b_cnt * moment_3_) / count_;

    mean_ += s;
    moment_2_ = moment_2;
    moment_3_ = moment_3;
    moment_4_ = moment_4;

    Vector<double> inv_val = Vector<double>.One / values;
    inv_val                = Vector.ConditionalSelect(Vector.Equals(values, Vector<double>.Zero),
                       Vector<double>.Zero, inv_val);
    mean_harmonics_ += Vector.Dot(b_cnt_d, inv_val);
#if NET9_0_OR_GREATER
    mean_geometric_ += Vector.Dot(b_cnt_d, Vector.Log(values));
#else
    Span<double> logtmp = stackalloc double[Vector<double>.Count];
    values.CopyTo(logtmp);
    for (int i = 0; i < Vector<double>.Count; ++i) {
      logtmp[i] = Math.Log(logtmp[i]);
    }
    mean_geometric_ += Vector.Dot(b_cnt_d, logtmp.ToVector());
#endif
    Vector<double> rms       = new(mean_rms_);
    Vector<double> new_count = Vector.ConvertToDouble(new Vector<ulong>(count_));
    mean_rms_ += Vector.Dot(values * values - rms, b_cnt_d / new_count);

    if (!evaluate_minmax)
      return;

    for (int i = 0; i < Vector<double>.Count; ++i) {
      ulong count = counts[i];
      if (count == 0)
        continue;

      double value = values[i];
      min_val_     = Math.Min(min_val_, value);
      EvaluateMax(value, count);
    }
  }

  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(List<>), typeof(CollectionsMarshal),
      nameof(CollectionsMarshal.AsSpan))]
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(ImmutableArray<>), typeof(ImmutableArray<>),
      "AsSpan()")]
  [Obsolete(
      "WARNING: The Vectorized input for RunningStatisticsAdvanced is not properly optimized yet, prefer scalar Pushing.")]
  public override void Push(ReadOnlySpan<double> values, ReadOnlySpan<ulong> freqs = default) {
    if (!freqs.IsEmpty && freqs.Length != values.Length)
      throw new ArgumentException(
          string.Format("[ERROR]: freqs is not empty, but values.Length: {0} != freqs.Length: {1}",
              values.Length, freqs.Length));

    int vlen = Vector<double>.Count;
    int alen = values.Length;
    int rem  = alen - vlen;

    Vector<double> min    = new(min_val_);
    Vector<double> max    = new(max_val_);
    Vector<ulong> max_acc = Vector<ulong>.Zero;

    int i = 0;
    for (; i <= rem; i += vlen) {
      Vector<double> value = values.Slice(i, vlen).ToVector();
      Vector<ulong> count  = freqs.IsEmpty ? Vector<ulong>.One : freqs.Slice(i, vlen).ToVector();
      Push(value, count, false);

      min     = Vector.Min(min, value);
      max_acc = Vector.ConditionalSelect(Vector.AsVectorUInt64(Vector.GreaterThan(value, max)),
          Vector<ulong>.Zero, max_acc);
      max     = Vector.Max(max, value);
      max_acc += count * Vector.BitwiseAnd(Vector<ulong>.One,  //
                             Vector.AsVectorUInt64(Vector.Equals(max, value)));
    }

    for (int j = 0; j < vlen; ++j) {
      min_val_ = Math.Min(min_val_, min[j]);
      EvaluateMax(max[j], max_acc[j]);
    }

    for (; i < alen; ++i) {
      if (freqs.IsEmpty)
        Push(values[i], 1);
      else
        Push(values[i], freqs[i]);
    }
  }
}
}
