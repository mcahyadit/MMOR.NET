namespace MMOR.NET.Statistics {
public class RunningCovariance {
  public double Count      => count_;
  public ulong CountU64    => count_;
  public double Covariance => count_ > 1? covariance_ / (count_ - 1) : double.NaN;

  protected ulong count_       = 0;
  protected double covariance_ = 0;
  protected double mean_a_     = 0;
  protected double mean_b_     = 0;

  public void Push(double a, double b) {
    ++count_;
    double delta_a = a - mean_a_;
    mean_a_ += delta_a / Count;
    mean_b_ += (b - mean_b_) / Count;

    covariance_ += delta_a * (b - mean_b_);
  }

  public void Clear() {
    count_      = 0;
    covariance_ = 0;
    mean_a_     = 0;
    mean_b_     = 0;
  }

  /** <summary>
   * Adds in values from another RunningCovariance.
   * </summary>
   * <param name="other">The RunningCovariance which data is to be added from.</param>
   * */
  public void Push(RunningCovariance other) {
    if (other.count_ == 0) {
      return;
    } else if (count_ == 0) {
      count_      = other.count_;
      covariance_ = other.covariance_;
      mean_a_     = other.mean_a_;
      mean_b_     = other.mean_b_;
      return;
    }

    ulong total_count = count_ + other.count_;
    double delta_a    = mean_a_ - other.mean_a_;
    double delta_b    = mean_b_ - other.mean_b_;

    covariance_ += other.covariance_ + delta_a * delta_b * count_ * other.count_ / total_count;
    mean_a_ = (count_ * mean_a_ + other.count_ * other.mean_a_) / total_count;
    mean_b_ = (count_ * mean_b_ + other.count_ * other.mean_b_) / total_count;
    count_  = total_count;
  }
}
}
