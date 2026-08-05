using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using MMOR.NET.Collections;
using MMOR.NET.Mathematics;
using MMOR.Roslyn;

namespace MMOR.NET.Statistics {
/** <summary>
 * <br/> Statistics accumulator with Online/Running algorithm,
 * <br/> i.e. capable of getting statistics without the use of Array-like structs.
 * <br/> ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
 * <br/> Based on <see
 *        href="https://github.com/mathnet/mathnet-numerics/blob/master/src/Numerics/Statistics/RunningStatistics.cs">
 *        Math.NET.Numerics
 *      </see> (MIT License).
 * <br/> Expanded for compatibility with frequency mapping / histogram based data.
 * </summary>
 * */
public partial class RunningStatistics {
  //====================================
  // █ █▄░█ ▀█▀ █▀▀ █▀█ █▀▀ ▄▀█ █▀▀ █▀▀
  // █ █░▀█ ░█░ ██▄ █▀▄ █▀░ █▀█ █▄▄ ██▄
  //====================================
  public ulong Count_uint64  => count_;
  public ulong Count0_uint64 => count_0_;
  /// <summary>Count excluding 0.</summary>
  public ulong CountA_uint64 => count_ - count_0_;
  public double Count        => count_;
  public double Count0       => count_0_;
  /// <summary>Count excluding 0.</summary>
  public double CountA            => count_ - count_0_;
  public double Minimum           => count_ > 0 ? min_val_ : double.NaN;
  public double Maximum           => count_ > 0 ? max_val_ : double.NaN;
  public double CountMaximum      => count_max_;
  public double Sum               => mean_ * count_;
  public double Mean              => count_ > 0 ? mean_ : double.NaN;
  public double Variance          => count_ < 2 ? double.NaN : moment_2_ / (count_ - 1);
  public double StandardDeviation => count_ < 2 ? double.NaN : Math.Sqrt(moment_2_ / (count_ - 1));
  public double StandardError =>
      count_ < 2 ? double.NaN : Math.Sqrt(moment_2_ / (count_ - 1) / count_);

  /** <summary>
   * <br/> Ratio of <b>Standard Deviation</b> to <b>Mean</b>.
   * <br/> ━━━━━━━━━━━━━━━━━━━━━━━━━━
   * <br/> Alternate Names:
   * <br/> - Relative <b>Standard Deviation</b>
   * <br/> - Normalized Root-Mean-Squared Deviation
   * </summary>
   * */
  public double CoefficientOfVariation =>
      count_ < 1 ? double.NaN : Math.Sqrt(moment_2_ / (count_ - 1)) / mean_;
  /// <summary>Ratio of <b>Mean</b> to <b>Standard Deviation</b>.</summary>
  public double SignalToNoiseRatio =>
      count_ < 1 ? double.NaN : mean_ / Math.Sqrt(moment_2_ / (count_ - 1));

  //=================================
  // █ █▄░█ ▀█▀ █▀▀ █▀█ █▄░█ ▄▀█ █░░ 
  // █ █░▀█ ░█░ ██▄ █▀▄ █░▀█ █▀█ █▄▄ 
  //=================================
  protected ulong count_   = 0;
  protected ulong count_0_ = 0;

  protected double min_val_  = double.PositiveInfinity;
  protected double max_val_  = double.NegativeInfinity;
  protected ulong count_max_ = 0;

  protected double mean_     = 0;
  protected double moment_2_ = 0;

  /** <summary>
   * Resets the RunningStatistics to it's default state.
   * </summary>
   * */
  public virtual void Clear() {
    count_   = 0;
    count_0_ = 0;

    min_val_   = double.PositiveInfinity;
    max_val_   = double.NegativeInfinity;
    count_max_ = 0;

    mean_     = 0;
    moment_2_ = 0;
  }
  //=======================
  // █ █▄░█ █▀█ █░█ ▀█▀ █▀
  // █ █░▀█ █▀▀ █▄█ ░█░ ▄█
  //=======================
  /** <summary>
   * Adds value to the RunningStatistics.
   * </summary>
   * <param name="value">Value of the data.</param>
   * <param name="count">
   *  Number of times <paramref name="value"/> is being added. <br/>
   *  Each will be considered individual entries. <br/>
   *  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━<br/>
   *  This parameter is often used to input massive amount of duplicate data.
   * </param>
   * */
  public virtual void Push(double value, ulong count = 1) {
    if (count == 0)
      return;

    //=====================
    // Welford's Algorithm
    //=====================
    double old_count = count_;
    if (value == 0)
      count_0_ += count;
    count_ += count;
    double d = value - mean_;
    double s = d / count_ * count;
    double t = d * s * old_count;

    mean_ += s;
    moment_2_ += t;

    min_val_ = Math.Min(min_val_, value);
    EvaluateMax(value, count);
  }

  /** <summary>
   * Adds in values from another RunningStatistics.
   * </summary>
   * <param name="stats">The RunningStatistics which data is to be added from.</param>
   * */
  public void Push(RunningStatistics stats) {
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
      return;
    }

    ulong total_count = count_ + stats.count_;
    double delta      = stats.mean_ - mean_;
    double delta_sq   = delta * delta;

    double mean    = (count_ * mean_ + stats.count_ * stats.mean_) / total_count;
    double mean_sq = moment_2_ + stats.moment_2_ + delta_sq * count_ * stats.count_ / total_count;

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

    count_ = total_count;
    count_0_ += stats.count_0_;
    mean_     = mean;
    moment_2_ = mean_sq;
  }

  public void EvaluateMax(double value, ulong count) {
    if (value > max_val_) {
      max_val_   = value;
      count_max_ = count;
    } else if (value == max_val_) {
      count_max_ += count;
    }
  }

  /**
   * <inheritdoc cref="Push(double, ulong)"/>
   * <param name="evaluate_minmax">
   *  Can set be set to <c>false</c>, <br/>
   *  <br/>
   *  Since min max evaluation requires vector reduction,
   *  using this in a loop will remove the advantage of
   *  vectorized operation in the first place.
   * </param>
   */
  public virtual void Push(Vector<double> values, Vector<ulong> counts,
      bool evaluate_minmax = true) {
    ulong b_cnt = MathExt.SumElements(counts);
    if (b_cnt == 0)
      return;

    count_0_ += Vector.Dot(counts, Vector.AsVectorUInt64(Vector.BitwiseAnd(Vector<long>.One,
                                       Vector.Equals(values, Vector<double>.Zero))));

    Vector<double> counts_d = Vector.ConvertToDouble(counts);
    double b_sum            = Vector.Dot(values, counts_d);
    double b_mean           = b_sum / b_cnt;
    Vector<double> b_dif    = values - new Vector<double>(b_mean);
    double b_moment_2       = Vector.Dot(b_dif * b_dif, counts_d);

    double old_count = count_;
    count_ += b_cnt;
    double d = b_mean - mean_;
    double s = d / count_ * b_cnt;
    double t = b_moment_2 + d * s * old_count;

    mean_ += s;
    moment_2_ += t;

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

  /**
   * <summary>
   *  Adds in multiple value-frequency pairs to the RunningStatistics.
   * </summary>
   */
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(List<>), typeof(CollectionsMarshal),
      nameof(CollectionsMarshal.AsSpan))]
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(ImmutableArray<>), typeof(ImmutableArray<>),
      "AsSpan()")]
  public virtual void Push(ReadOnlySpan<double> values, ReadOnlySpan<ulong> freqs = default) {
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

  /** <summary>
   * Adds in values from a set of values to the RunningStatistics.
   * </summary>
   * <param name="list">List of values.</param>
   * */
  public void Push(IEnumerable<double> list) {
    foreach (double value in list) Push(value);
  }

  /// <inheritdoc cref="Push(IEnumerable{double})"/>
  public void Push<T>(IEnumerable<T> list)
      where T : IConvertible {
    Push(list.Select(x => x.ToDouble(null)));
  }

  /** <summary>
   * Adds in values from a value-frequency pairs to the RunningStatistics.
   * </summary>
   * <param name="map">List of values.</param>
   * */
  public void Push(IEnumerable<(double, ulong)> map) {
    foreach ((double, ulong)kvp in map) Push(kvp.Item1, kvp.Item2);
  }

  /// <inheritdoc cref="Push(IEnumerable{ValueTuple{double, ulong}})"/>
  public void Push<T>(IEnumerable<KeyValuePair<T, ulong>> map)
      where T : IConvertible {
    Push(map.Select(x => (x.Key.ToDouble(null), x.Value)));
  }

  /// <inheritdoc cref="Push(IEnumerable{ValueTuple{double, ulong}})"/>
  public void Push<T>(IEnumerable<KeyValuePair<T, uint>> map)
      where T : IConvertible {
    Push(map.Select(x => (x.Key.ToDouble(null), (ulong)x.Value)));
  }
}
}
