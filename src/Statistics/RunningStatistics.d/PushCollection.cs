using System;
using System.Collections.Generic;

namespace MMOR.NET.Statistics {
public static partial class RunningStatisticsExtensions {
  public static void Push(this RunningStatistics self, IEnumerable<double> values) {
    foreach (double value in values) {
      Push(self, value);
    }
  }

  public static void Push<T>(this RunningStatistics self, IEnumerable<T> values)
      where T : struct, IConvertible {
    foreach (T value in values) {
      Push(self, value.ToDouble(null));
    }
  }

  public static void Push(this RunningStatistics self,
      IEnumerable<KeyValuePair<double, ulong>> values) {
    foreach ((double value, ulong count) in values) {
      Push(self, value, count);
    }
  }

  public static void Push<TValue, TCount>(this RunningStatistics self,
      IEnumerable<KeyValuePair<TValue, TCount>> values)
      where TValue : struct, IConvertible
      where TCount : struct, IConvertible {
    foreach ((TValue value, TCount count) in values) {
      Push(self, value.ToDouble(null), count.ToUInt64(null));
    }
  }

  public static void Push(this RunningStatistics self, IEnumerable<(double, ulong)> values) {
    foreach ((double value, ulong count) in values) {
      Push(self, value, count);
    }
  }

  public static void Push<TValue, TCount>(this RunningStatistics self,
      IEnumerable<(TValue, TCount)> values)
      where TValue : struct, IConvertible
      where TCount : struct, IConvertible {
    foreach ((TValue value, TCount count) in values) {
      Push(self, value.ToDouble(null), count.ToUInt64(null));
    }
  }
}
}
