using System;
using System.Buffers;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  public static int TimMerge<T, TComparer>(in Span<T> slice, in TComparer cmp, in int l_len,
      int min_gallop)
      where T : unmanaged
      where TComparer : IComparer<T> {
    int length = slice.Length;

    T[]? l_mem = l_len > kHeapThreshold ? ArrayPool<T>.Shared.Rent(l_len) : null;
    try {
      Span<T> l_buff = l_mem is null ? stackalloc T[l_len] : l_mem.AsSpan(0, l_len);
      slice[..l_len].CopyTo(l_buff);

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
          int curr_cmp = cmp.Compare(l_buff[l_idx], slice[r_idx]);
          if (curr_cmp <= 0) {
            slice[idx++] = l_buff[l_idx++];
            gallop       = gallop > 0 ? gallop + 1 : 1;
          } else {
            slice[idx++] = slice[r_idx++];
            gallop       = gallop < 0 ? gallop - 1 : -1;
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
            T l_val = l_buff[l_idx];

            g_idx = HeadGallopSearch(slice[r_idx..], l_val, cmp);
            slice.Slice(r_idx, g_idx).CopyTo(slice[idx..]);
            r_idx += g_idx;
            gallop = g_idx;  // Switch to Left Gallop
          } else {
            // Gallop left side
            T r_val = slice[r_idx];

            g_idx = TailGallopSearch(l_buff[l_idx..], r_val, cmp);
            l_buff.Slice(l_idx, g_idx).CopyTo(slice[idx..]);
            l_idx += g_idx;
            gallop = -g_idx;  // Switch to Right Gallop
          }
          idx += g_idx;
        }
      }
      for (; l_idx < l_len; ++l_idx, ++idx) {
        slice[idx] = l_buff[l_idx];
      }
      return min_gallop;
    } finally {
      if (l_mem is not null)
        ArrayPool<T>.Shared.Return(l_mem, true);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void TimCollapse<T, TComparer>(in Span<T> slice, in TComparer cmp, bool finish,
      ref int min_gallop, ref int count, in Span<int> run_start, in Span<int> run_length)
      where T : unmanaged
      where TComparer : IComparer<T> {
    while (count > 1) {
      int len_1 = run_length[count - 2];
      int len_2 = run_length[count - 1];
      bool inv3 = count > 2 && run_length[count - 3] <= len_2 + len_1;
      bool inv4 = count > 3 && run_length[count - 4]  //
                                   <= len_1 + run_length[count - 3];
      if (finish || len_1 <= len_2 || inv3 || inv4) {
        int len_combined      = len_1 + len_2;
        min_gallop            = TimMerge(slice.Slice(run_start[count - 2], len_combined),  //
                       cmp, len_1, min_gallop);
        run_length[count - 2] = len_combined;
        --count;
      } else {
        break;
      }
    }
  }

  /**
   * <summary>
   *  Stable Sort <paramref name="slice"/> in ascending order using timsort. <br/>
   *  <i>O(n)</i> best case. <i>O(n log n)</i> worst case. <br/>
   * </summary>
   */
  public static void TimSort<T, TComparer>(this Span<T> slice, in TComparer cmp)
      where T : unmanaged
      where TComparer : IComparer<T> {
    int length = slice.Length;

    int min_run = ComputeMinRun(slice.Length);
    if (length <= min_run) {
      BinaryInsertionSort(slice, cmp);
      return;
    }

    int expect_runs = BitOperations.Log2((uint)length);
    expect_runs += 1 + (expect_runs >> 1);  // 1.5 * Log2(N)
    Span<int> run_start  = stackalloc int[expect_runs];
    Span<int> run_length = stackalloc int[expect_runs];
    int runs_count       = 0;
    int min_gallop       = 7;

    for (int i = 0; i < length;) {
      int stride = CountRun<T, TComparer>(slice[i..], cmp);
      if (stride < 0) {
        stride = -stride;
        slice.Slice(i, stride).Reverse();
      }

      if (stride < min_run) {
        stride = Math.Min(min_run, length - i);
        BinaryInsertionSort(slice.Slice(i, stride), cmp);
      }
      run_start[runs_count]  = i;
      run_length[runs_count] = stride;
      ++runs_count;
      i += stride;

      TimCollapse(slice, cmp, false, ref min_gallop, ref runs_count, run_start, run_length);
    }

    TimCollapse(slice, cmp, true, ref min_gallop, ref runs_count, run_start, run_length);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void TimSort<T>(this Span<T> slice)
      where T : unmanaged, IComparable<T> => TimSort(slice, Comparer<T>.Default);
}
}
