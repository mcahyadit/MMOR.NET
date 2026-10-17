using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using MMOR.NET.Collections;
using MMOR.Roslyn;

namespace MMOR.NET.Statistics {
public static partial class TotalStatisticsExtension {
  // <!-- A  WARNING: Do no remove this comment pair
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(List<>), typeof(CollectionsMarshal),
      nameof(CollectionsMarshal.AsSpan))]
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(ImmutableArray<>), typeof(ImmutableArray<>),
      "AsSpan()")]
  public static TotalStatisticsV2 Calculate<TVal, TCnt>(ReadOnlySpan<TVal> values,
      ReadOnlySpan<TCnt> counts)
      where TVal : unmanaged, IConvertible
      where TCnt : unmanaged, IConvertible {
    int length = values.Length;
    if (!counts.IsEmpty && counts.Length != length)
      throw new ArgumentException(
          "[ERROR]: Length mismatch." +
          $"counts is passed as parameter, but counts.Length: {counts.Length} != values.Length: {length}");
    // A -->

    double[]? v_mem = length > kHeapThreshold ? ArrayPool<double>.Shared.Rent(length) : null;
    ulong[]? c_mem  = length > kHeapThreshold ? ArrayPool<ulong>.Shared.Rent(length) : null;
    try {
      Span<double> val_buffer = v_mem is null ? stackalloc double[length] : v_mem.AsSpan(0, length);
      Span<ulong> cnt_buffer  = c_mem is null ? stackalloc ulong[length] : c_mem.AsSpan(0, length);

      int minrun           = CollectionUtils.ComputeMinRun(length);
      int expect_runs      = length / minrun + 1;
      Span<int> run_start  = stackalloc int[expect_runs];
      Span<int> run_length = stackalloc int[expect_runs];
      int runs_count       = 0;

      run_start[0]       = 0;
      int cmp_dir        = 0;
      TimState run_state = 0;

      RunningStatistics stats = new();
      ulong count0            = 0;
      ulong count_neg         = 0;
      ulong count_pos         = 0;

      int skip_cnt  = 0;
      int valid_cnt = 0;
      // <!-- B  WARNING: Do no remove this comment pair
      for (int i = 0; i < length; ++i) {
        ulong count = counts.IsEmpty ? 1 : counts[i].ToUInt64(null);
        if (count == 0) {
          ++skip_cnt;
          continue;
        }

        double value = values[i].ToDouble(null);
        // B -->
        if (!double.IsFinite(value)) {
          throw new ArgumentException(
              "[ERROR]: TotalStatistics cannot process non-finite number\n" +
              $"Detected {value} at index: {i}");
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

        val_buffer[valid_cnt] = value;
        cnt_buffer[valid_cnt] = count;
        ++valid_cnt;

        if (valid_cnt == 1) {
          continue;
        }

        // Check if Still Ascending or Descending
        int cmp = value.CompareTo(val_buffer[valid_cnt - 2]);
        if (cmp_dir == 0) {
          // First Comparison
          cmp_dir = Math.Sign(cmp);
          if (cmp_dir < 0)
            run_state |= TimState.NeedsReverse;
        }

        int curr_run = ++run_length[runs_count];
        if (curr_run >= minrun) {
          run_state |= TimState.HitMinrun;
          // Mark Run ready to End
        }

        if (Math.Sign(cmp * cmp_dir) < 0) {
          if ((run_state & TimState.HitMinrun) != 0)
            run_state |= TimState.QueueMerge;
          else
            run_state |= TimState.NeedsSorting;
        }

        if ((run_state & TimState.QueueMerge) != 0) {
          int start            = run_start[runs_count];
          int r_len            = run_length[runs_count];
          Span<double> run_val = val_buffer.Slice(start, r_len);
          Span<ulong> run_cnt  = cnt_buffer.Slice(start, r_len);

          if ((run_state & TimState.NeedsReverse) != 0) {
            run_val.Reverse();
            run_cnt.Reverse();
          }
          if ((run_state & TimState.NeedsSorting) != 0) {
            CollectionUtils.BinaryInsertionSort(run_val, run_cnt);
          }

          ++runs_count;
          run_start[runs_count]  = valid_cnt - 1;
          run_length[runs_count] = 0;
          run_state              = 0;
          cmp_dir                = 0;
        }
      }

      // Empty Dataset
      if (stats.Count == 0)
        return TotalStatisticsV2.kDefault;

      val_buffer = val_buffer[..valid_cnt];
      cnt_buffer = cnt_buffer[..valid_cnt];
      CollectionUtils.TimSort(val_buffer, cnt_buffer);

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
