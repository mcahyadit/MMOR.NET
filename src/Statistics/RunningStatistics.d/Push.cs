namespace MMOR.NET.Statistics {
public static partial class RunningStatisticsV2Extensions {
  public static void Push(this RunningStatisticsV2 self, double value, ulong count = 1) {
    if (count == 0)
      return;

    double old_count = self.Count;
    self.Count += count;
    double delta = value - self.moment_1st_;
    double s     = delta / self.Count * count;
    double s2    = s * s / count;
    double t     = delta * s * old_count;

    double moment_1st = self.moment_1st_ + s;
    self.moment_1st_  = moment_1st;
    self.moment_4th_ +=
        t * s2 * (old_count * old_count - old_count * count + count * count) / count +
        6 * s2 * count * self.moment_2nd_ - 4 * s * self.moment_3rd_;
    self.moment_3rd_ += t * s * (old_count - count) / count - 3 * s * self.moment_2nd_;
    self.moment_2nd_ += t;

    if (value < self.min_value_) {
      self.min_value_ = value;
      self.min_count_ = count;
    } else if (value == self.min_value_) {
      self.min_count_ += count;
    }

    if (value > self.max_value_) {
      self.max_value_ = value;
      self.max_count_ = count;
    } else if (value == self.max_value_) {
      self.max_count_ += count;
    }
  }
}
}
