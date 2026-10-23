using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  /// <inheritdoc cref="TimMerge{T}(in Span{T}, in int, int)"/>
  public static int TimMerge<TKey, TValue>(in Span<TKey> keys, in Span<TValue> values, in int l_len,
      int min_gallop)
      where TKey : unmanaged, IComparable<TKey>
      where TValue : unmanaged {
    int length = keys.Length;

    TKey[]? l_mem   = l_len > kHeapThreshold ? ArrayPool<TKey>.Shared.Rent(l_len) : null;
    TValue[]? v_mem = l_len > kHeapThreshold ? ArrayPool<TValue>.Shared.Rent(l_len) : null;
    try {
      Span<TKey> l_buff   = l_mem is null ? stackalloc TKey[l_len] : l_mem.AsSpan(0, l_len);
      Span<TValue> v_buff = v_mem is null ? stackalloc TValue[l_len] : v_mem.AsSpan(0, l_len);
      keys[..l_len].CopyTo(l_buff);
      values[..l_len].CopyTo(v_buff);

      int idx   = 0;
      int l_idx = 0;
      int r_idx = l_len;

      int gallop     = 0;
      bool galloping = false;

      while (l_idx < l_len && r_idx < length) {
        int threshold = galloping ? 7 : min_gallop;
        if (Math.Abs(gallop) < threshold) {
          // Step-by-Step Comparison
          if (galloping) {
            ++min_gallop;
            galloping = false;
          }
          int curr_cmp = l_buff[l_idx].CompareTo(keys[r_idx]);
          if (curr_cmp <= 0) {
            keys[idx]   = l_buff[l_idx];
            values[idx] = v_buff[l_idx];
            ++idx;
            ++l_idx;
            gallop = gallop > 0 ? gallop + 1 : 1;
          } else {
            keys[idx]   = keys[r_idx];
            values[idx] = values[r_idx];
            ++idx;
            ++r_idx;
            gallop = gallop < 0 ? gallop - 1 : -1;
          }
        } else {
          // Galloping
          if (!galloping) {
            ++min_gallop;
            galloping = true;
          }
          if (min_gallop > 1)
            --min_gallop;

          int g_idx = 0;
          if (gallop < 0) {
            // Gallop right side
            TKey l_val = l_buff[l_idx];

            g_idx = HeadGallopSearch(keys[r_idx..], l_val);
            keys.Slice(r_idx, g_idx).CopyTo(keys[idx..]);
            values.Slice(r_idx, g_idx).CopyTo(values[idx..]);
            r_idx += g_idx;
            gallop = g_idx;  // Switch to Left Gallop
          } else {
            // Gallop left side
            TKey r_val = keys[r_idx];

            g_idx = TailGallopSearch(l_buff[l_idx..], r_val);
            l_buff.Slice(l_idx, g_idx).CopyTo(keys[idx..]);
            v_buff.Slice(l_idx, g_idx).CopyTo(values[idx..]);
            l_idx += g_idx;
            gallop = -g_idx;  // Switch to Right Gallop
          }
          idx += g_idx;
        }
      }
      for (; l_idx < l_len; ++l_idx, ++idx) {
        keys[idx]   = l_buff[l_idx];
        values[idx] = v_buff[l_idx];
      }
      return min_gallop;
    } finally {
      if (l_mem is not null) {
        ArrayPool<TKey>.Shared.Return(l_mem, true);
        ArrayPool<TValue>.Shared.Return(v_mem!, true);
      }
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void TimCollapse<TKey, TValue>(in Span<TKey> keys, in Span<TValue> values,
      bool finish, ref int min_gallop, ref int count, in Span<int> run_start,
      in Span<int> run_length)
      where TKey : unmanaged, IComparable<TKey>
      where TValue : unmanaged {
    while (count > 1) {
      int len_1 = run_length[count - 2];
      int len_2 = run_length[count - 1];
      bool inv3 = count > 2 && run_length[count - 3] <= len_2 + len_1;
      bool inv4 = count > 3 && run_length[count - 4]  //
                                   <= len_1 + run_length[count - 3];
      if (finish || len_1 <= len_2 || inv3 || inv4) {
        int len_combined      = len_1 + len_2;
        min_gallop            = TimMerge(keys.Slice(run_start[count - 2], len_combined),  //
                       values.Slice(run_start[count - 2], len_combined), len_1, min_gallop);
        run_length[count - 2] = len_combined;
        --count;
      } else {
        break;
      }
    }
  }

  /// <inheritdoc cref="TimSort{T}(Span{T})"/>
  public static void TimSort<TKey, TValue>(Span<TKey> keys, Span<TValue> values)
      where TKey : unmanaged, IComparable<TKey>
      where TValue : unmanaged {
    int length = keys.Length;

    int min_run = ComputeMinRun(length);
    if (length <= min_run) {
      BinaryInsertionSort(keys, values);
      return;
    }

    int expect_runs = BitOperations.Log2((uint)length);
    expect_runs += 1 + (expect_runs >> 1);  // 1.5 * Log2(N)
    Span<int> run_start  = stackalloc int[expect_runs];
    Span<int> run_length = stackalloc int[expect_runs];
    int runs_count       = 0;
    int min_gallop       = 7;

    for (int i = 0; i < length;) {
      int stride = CountRun<TKey>(keys[i..]);
      if (stride < 0) {
        stride = -stride;
        keys.Slice(i, stride).Reverse();
        values.Slice(i, stride).Reverse();
      }

      if (stride < min_run) {
        stride = Math.Min(min_run, length - i);
        BinaryInsertionSort(keys.Slice(i, stride), values.Slice(i, stride));
      }
      run_start[runs_count]  = i;
      run_length[runs_count] = stride;
      ++runs_count;
      i += stride;

      TimCollapse(keys, values, false, ref min_gallop, ref runs_count, run_start, run_length);
    }

    TimCollapse(keys, values, true, ref min_gallop, ref runs_count, run_start, run_length);
  }
}
}
