namespace MMOR.NET.Statistics {
public static partial class RunningStatisticsExtensions {
  public static void Push(this RunningStatistics self, double value, ulong count = 1) {
    if (count == 0)
      return;

    if (self.Count == 0) {
      self.Count    = count;
      self.moments_ = new(value);
      self.min_max_ = new(value, count);
      return;
    }

    var (moment_1st, moment_2nd, moment_3rd, moment_4th) = self.moments_;

    double old_count = self.Count;
    self.Count += count;
    double delta = value - moment_1st;
    double s     = delta / self.Count * count;
    double s2    = s * s / count;
    double t     = delta * s * old_count;

    moment_1st += s;
    moment_4th += t * s2 * (old_count * old_count - old_count * count + count * count) / count +
                  6 * s2 * count * moment_2nd - 4 * s * moment_3rd;
    moment_3rd += t * s * (old_count - count) / count - 3 * s * moment_2nd;
    moment_2nd += t;

    self.moments_ = new MomentsRecord {
      Raw1st     = moment_1st,
      Central2nd = moment_2nd,
      Central3rd = moment_3rd,
      Central4th = moment_4th,
    };

    PushMinMax(self, new(value, count));
  }

  public static void Clear(this RunningStatistics self) {
    self.Count = 0;
  }

  public static void Push(this RunningStatistics self, RunningStatistics other) {
    if (other.Count == 0) {
      return;
    } else if (self.Count == 0) {
      self.Count    = other.Count;
      self.moments_ = other.moments_;
      self.min_max_ = other.min_max_;
      return;
    }

    self.PushMoments(other.moments_, other.Count);
    self.PushMinMax(other.min_max_);
  }

  public static void PushMoments(this RunningStatistics self, in MomentsRecord m, in ulong count) {
    if (count == 0) {
      return;
    } else if (self.Count == 0) {
      self.Count    = count;
      self.moments_ = m;
      return;
    }

    var (moment_1st, moment_2nd, moment_3rd, moment_4th) = self.moments_;

    ulong total_count = self.Count + count;
    double delta      = m.Raw1st - moment_1st;
    double delta2     = delta * delta;
    double delta3     = delta2 * delta;
    double delta4     = delta2 * delta2;

    double count_cross   = self.CountF64 * count;
    double self_count_2  = self.CountF64 * self.Count;
    double other_count_2 = (double)count * count;
    double total_count_2 = (double)total_count * total_count;
    double total_count_3 = total_count_2 * total_count;

    // Overflow safety over (self.Mean * self.Count + other.Mean * count) / total_count)
    double m1 = moment_1st + delta * count / total_count;
    double m2 = moment_2nd + m.Central2nd  //
                + delta2 * self.Count * count / total_count;
    double m3 = moment_3rd + m.Central3rd                                         //
                + delta3 * count_cross * (self.CountF64 - count) / total_count_2  //
                + 3 * delta * (self.Count * m.Central2nd - count * moment_2nd) / total_count;
    double m4 = moment_4th + m.Central4th                                                         //
                + delta4 * count_cross * (self_count_2 - count_cross + other_count_2) /           //
                      total_count_3                                                               //
                + 6 * delta2 *                                                                    //
                      (self_count_2 * m.Central2nd + other_count_2 * moment_2nd) / total_count_2  //
                + 4 * delta *                                                                     //
                      (self.Count * m.Central3rd - count * moment_3rd) /                          //
                      total_count;

    self.Count    = total_count;
    self.moments_ = new MomentsRecord {
      Raw1st     = m1,
      Central2nd = m2,
      Central3rd = m3,
      Central4th = m4,
    };
  }

  public static void PushMinMax(this RunningStatistics self, in MinMaxRecord min_max) {
    if (min_max.MinCount == 0 || min_max.MaxCount == 0) {
      return;
    } else if (self.Count == 0) {
      self.min_max_ = min_max;
      return;
    }

    var (min_value, min_count, max_value, max_count) = self.min_max_;

    if (min_max.MinValue < min_value) {
      min_value = min_max.MinValue;
      min_count = min_max.MinCount;
    } else if (min_max.MinValue == min_value) {
      min_count += min_max.MinCount;
    }

    if (min_max.MaxValue > max_value) {
      max_value = min_max.MaxValue;
      max_count = min_max.MaxCount;
    } else if (min_max.MaxValue == max_value) {
      max_count += min_max.MaxCount;
    }

    self.min_max_ = new MinMaxRecord {
      MinValue = min_value,
      MinCount = min_count,
      MaxValue = max_value,
      MaxCount = max_count,
    };
  }
}
}
