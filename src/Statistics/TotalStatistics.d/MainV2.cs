using System;
using System.Buffers;
using MMOR.NET.Collections;

namespace MMOR.NET.Statistics {
public static partial class TotalStatisticsExtension {
  public const int kHeapThreshold = 128;

  public static TotalStatisticsV2 CalculateCoreV2<T>(this ReadOnlySpan<T> values)
      where T : unmanaged, IConvertible {
    return CalculateCoreV2<T, ulong>(values, stackalloc ulong[0]);
  }

  public static TotalStatisticsV2 CalculateCoreV2<TVal, TCnt>(this ReadOnlySpan<TVal> values,
      ReadOnlySpan<TCnt> counts)
      where TVal : unmanaged, IConvertible
      where TCnt : unmanaged, IConvertible {
    int length = values.Length;

    double[]? v_mem = length > kHeapThreshold ? ArrayPool<double>.Shared.Rent(length) : null;
    ulong[]? c_mem  = length > kHeapThreshold ? ArrayPool<ulong>.Shared.Rent(length) : null;
    try {
      Span<double> val_buffer = v_mem is null ? stackalloc double[length] : v_mem.AsSpan(0, length);
      Span<ulong> cnt_buffer  = c_mem is null ? stackalloc ulong[length] : c_mem.AsSpan(0, length);

      int sort_heur = 1;
      int last_cmp  = 0;

      RunningStatistics stats = new();
      ulong count0            = 0;
      ulong count_neg         = 0;
      ulong count_pos         = 0;

      int skip = 0;
      for (int i = 0; i < length; ++i) {
        ulong count = counts.IsEmpty ? 1 : counts[i].ToUInt64(null);
        if (count == 0) {
          ++skip;
          continue;
        }

        double value = values[i].ToDouble(null);
        if (!double.IsFinite(value)) {
          throw new ArgumentException(
              $"[ERROR]: TotalStatistics cannot process non-finite number\nDetected {value} on element at: {i}");
        }

        stats.Push(value, count);

        switch (value) {
          case 0:
            count0 += count;
            break;
          case < 0:
            count_neg += count;
            break;
          default:
            count_pos += count;
            break;
        }

        val_buffer[i] = value;
        cnt_buffer[i] = count;

        if (i > 0) {
          int curr_cmp = Math.Sign(val_buffer[i].CompareTo(val_buffer[i - 1]));
          if (last_cmp == curr_cmp || curr_cmp == 0) {
            last_cmp = curr_cmp;
            curr_cmp *= Math.Sign(sort_heur);
          } else {
            last_cmp = curr_cmp;
          }
          sort_heur += curr_cmp;
        }
      }

      // Empty Dataset
      if (stats.Count == 0)
        return TotalStatisticsV2.kDefault;

      val_buffer = val_buffer[..(length - skip)];
      cnt_buffer = cnt_buffer[..(length - skip)];
      if (CollectionUtils.CountRun<double>(val_buffer) < val_buffer.Length / 2) {
        CollectionUtils.PdqSort(val_buffer, cnt_buffer);
      } else {
        CollectionUtils.TimSort(val_buffer, cnt_buffer);
      }

      return SecondPassUnsafe(val_buffer, cnt_buffer, stats, count_pos, count_neg, count0);
    } finally {
      if (v_mem is not null) {
        ArrayPool<double>.Shared.Return(v_mem, true);
        ArrayPool<ulong>.Shared.Return(c_mem!, true);
      }
    }
  }
}
}
