namespace MMOR.NET.Statistics {

public readonly struct MomentsRecord {
  public readonly double Raw1st { get; init; }
  public readonly double Central2nd { get; init; }
  public readonly double Central3rd { get; init; }
  public readonly double Central4th { get; init; }

  public MomentsRecord(in double value) {
    Raw1st     = value;
    Central2nd = 0;
    Central3rd = 0;
    Central4th = 0;
  }

  public void Deconstruct(out double moment_1st, out double moment_2nd, out double moment_3rd,
      out double moment_4th) {
    moment_1st = Raw1st;
    moment_2nd = Central2nd;
    moment_3rd = Central3rd;
    moment_4th = Central4th;
  }
}

public readonly struct MinMaxRecord {
  public readonly double MinValue { get; init; }
  public readonly ulong MinCount { get; init; }
  public readonly double MaxValue { get; init; }
  public readonly ulong MaxCount { get; init; }

  public MinMaxRecord(in double value, in ulong count) {
    MinValue = value;
    MinCount = count;
    MaxValue = value;
    MaxCount = count;
  }

  public void Deconstruct(out double min_value, out ulong min_count, out double max_value,
      out ulong max_count) {
    min_value = MinValue;
    min_count = MinCount;
    max_value = MaxValue;
    max_count = MaxCount;
  }
}
}
