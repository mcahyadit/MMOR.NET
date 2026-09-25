namespace MMOR.NET.Statistics {
public static partial class RunningStatisticsV2Extensions {
  public static void Clear(this RunningStatisticsV2 self) {
    self.Count = 0;
    self.Mean  = 0;

    self.moment_2nd_ = 0;
    self.moment_3rd_ = 0;
    self.moment_4th_ = 0;

    self.MinValue = double.MaxValue;
    self.MinCount = 0;
    self.MaxValue = double.MinValue;
    self.MaxCount = 0;
  }

  public static void Push(this RunningStatisticsV2 self, RunningStatisticsV2 other) {
    if (other.Count == 0) {
      return;
    } else if (self.Count == 0) {
      self.Count = other.Count;
      self.Mean  = other.Mean;

      self.moment_2nd_ = other.moment_2nd_;
      self.moment_3rd_ = other.moment_3rd_;
      self.moment_4th_ = other.moment_4th_;

      self.MinValue = other.MinValue;
      self.MinCount = other.MinCount;
      self.MaxValue = other.MaxValue;
      self.MaxCount = other.MaxCount;
      return;
    }

    ulong total_count = self.Count + other.Count;
    double delta      = other.Mean - self.Mean;
    double delta2     = delta * delta;
    double delta3     = delta2 * delta;
    double delta4     = delta2 * delta2;

    double count_cross   = self.CountF64 * other.Count;
    double self_count_2  = self.CountF64 * self.Count;
    double other_count_2 = other.CountF64 * other.Count;
    double total_count_2 = (double)total_count * total_count;
    double total_count_3 = total_count_2 * total_count;

    // Overflow safety over (self.Mean * self.Count + other.Mean * other.Count) / total_count)
    double moment_1st = self.Mean + delta * other.Count / total_count;
    double moment_2nd = self.moment_2nd_ + other.moment_2nd_  //
                        + delta2 * self.Count * other.Count / total_count;
    double moment_3rd = self.moment_3rd_ + other.moment_3rd_                               //
                        + delta3 *                                                         //
                              count_cross * (self.CountF64 - other.Count) / total_count_2  //
                        + 3 * delta *                                                      //
                              (self.Count * other.moment_2nd_ - other.Count * self.moment_2nd_) /
                              total_count;
    double moment_4th =
        self.moment_4th_ + other.moment_4th_                                     //
        + delta4 * count_cross * (self_count_2 - count_cross + other_count_2) /  //
              total_count_3                                                      //
        + 6 * delta2 *                                                           //
              (self_count_2 * other.moment_2nd_ + other_count_2 * self.moment_2nd_) /
              total_count_2                                                        //
        + 4 * delta *                                                              //
              (self.Count * other.moment_3rd_ - other.Count * self.moment_3rd_) /  //
              total_count;

    self.Count       = total_count;
    self.Mean        = moment_1st;
    self.moment_2nd_ = moment_2nd;
    self.moment_3rd_ = moment_3rd;
    self.moment_4th_ = moment_4th;

    if (other.MinValue < self.MinValue) {
      self.MinValue = other.MinValue;
      self.MinCount = other.MinCount;
    } else if (other.MinValue == self.MinValue) {
      self.MinCount += other.MinCount;
    }

    if (other.MaxValue > self.MaxValue) {
      self.MaxValue = other.MaxValue;
      self.MaxCount = other.MaxCount;
    } else if (other.MaxValue == self.MaxValue) {
      self.MaxCount += other.MaxCount;
    }
  }
}
}
