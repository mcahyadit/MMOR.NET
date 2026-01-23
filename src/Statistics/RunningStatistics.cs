using System;
using System.Collections.Generic;
using System.Linq;

namespace MMOR.NET.Statistics {
  /** <summary>
   * <br/> Statistics accumulator with Online/Running algorithm,
   * <br/> i.e. capable of getting statistics without the use of Array-like structs.
   * <br/> ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   * <br/> Based on <see
   *        href="https://github.com/mathnet/mathnet-numerics/blob/master/src/Numerics/Statistics/RunningStatistics.cs">
   *        Math.NET.Numerics
   *      </see> (MIT License).
   * <br/> Expanded for compability with frequency mapping / histogram based data.
   * </summary>
   * */
  public class RunningStatistics {
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
    public double CountA       => count_ - count_0_;
    public double Minimum      => count_ > 0 ? min_val_ : double.NaN;
    public double Maximum      => count_ > 0 ? max_val_ : double.NaN;
    public double CountMaximum => count_max_;
    public double Sum          => mean_ * count_;
    public double Mean         => count_ > 0 ? mean_ : double.NaN;
    public double Variance     => count_ < 2 ? double.NaN : moment_2_ / (count_ - 1);
    public double StandardDeviation =>
        count_ < 2 ? double.NaN : Math.Sqrt(moment_2_ / (count_ - 1));
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
    protected ulong count_;
    protected ulong count_0_;
    protected double min_val_ = double.PositiveInfinity;
    protected double max_val_ = double.NegativeInfinity;
    protected ulong count_max_;
    protected double mean_;
    protected double moment_2_;

    /** <summary>
     * Resets the RunningStatistics to it's default state.
     * </summary>
     * */
    public virtual void Clear() {
      count_   = 0;
      count_0_ = 0;

      min_val_ = double.PositiveInfinity;
      max_val_ = double.NegativeInfinity;

      count_max_ = 0;
      mean_      = 0;
      moment_2_  = 0;
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
     *  Each will be considered indiviual entries. <br/>
     *  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━<br/>
     *  This parameter is often used to input massive amount of duplicate datas.
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

      //================
      // Update MinMax
      //================
      min_val_ = Math.Min(min_val_, value);
      if (value > max_val_) {
        max_val_   = value;
        count_max_ = count;
      } else if (value == max_val_) {
        count_max_ += count;
      }
    }

    /** <summary>
     * Adds in values from another RunningStatistics.
     * </summary>
     * <param name="stats">The RunningStatistics which data is to be added from.</param>
     * */
    public void Push(RunningStatistics stats) {
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
     * <param name="list">List of values.</param>
     * */
    public void Push(IEnumerable<(double, ulong)> map) {
      foreach ((double, ulong)kvp in map) Push(kvp.Item1, kvp.Item2);
    }

    /// <inheritdoc cref="Push(IEnumerable{ValueTuple{double, ulong}})"/>
    public void Push<T>(IEnumerable<KeyValuePair<T, ulong>> map)
        where T : IConvertible {
      Push(map.Select(x => (x.Key.ToDouble(null), x.Value)));
    }

    /// <inheritdoc cref="Push(IEnumerable{ValueTuple{double, uint}})"/>
    public void Push<T>(IEnumerable<KeyValuePair<T, uint>> map)
        where T : IConvertible {
      Push(map.Select(x => (x.Key.ToDouble(null), (ulong)x.Value)));
    }
  }
}
