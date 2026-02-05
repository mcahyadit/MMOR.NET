using System;

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
  public class RunningStatisticsAdvanced : RunningStatistics {
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
      count_ += count;
      double d  = value - mean_;
      double s  = d / count_ * count;
      double s2 = s * s / count;
      double t  = d * s * old_count;

      mean_ += s;
      moment_4_ +=
          t * s2 * (count_ * count_ - 3 * count_ + 3) + 6 * s2 * moment_2_ - 4 * s * moment_3_;
      moment_3_ += t * s * (count_ - 2) - 3 * s * moment_2_;
      moment_2_ += t;

      mean_harmonics_ += 1.0 / value * count;
      mean_geometric_ += Math.Log(value) * count;
      mean_rms_ += (value * value - mean_rms_) * count / count_;

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
          6 * delta2 *
              (count_ * count_ * stats.moment_2_ + stats.count_ * stats.count_ * moment_2_) /
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

      count_    = total_count;
      mean_     = mean;
      moment_2_ = moment_2;
      moment_3_ = moment_3;
      moment_4_ = moment_4;
    }
  }
}
