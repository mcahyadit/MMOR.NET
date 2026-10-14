using System;
using System.Buffers;
using System.Numerics;
using MMOR.NET.Bits;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  public static int ComputeMinRun(int length) {
    // Bit Manip for getting Power Of 2 until [32, 64)
    int log2    = BitOperations.Log2((uint)length);
    int idx     = Math.Max(0, log2 - 5);
    int min_run = (length >> idx) & 63;
    int add     = BitOps.Bzhi32((uint)length, idx) != 0 ? 1 : 0;
    return Math.Max(32, min_run + add);
  }

  public static int TailBinarySearch<T>(Span<T> slice, T value)
      where T : IComparable<T> {
    int hi = slice.Length;
    int lo = 0;
    while (lo < hi) {
      int mid = lo + ((hi - lo) >> 1);
      if (slice[mid].CompareTo(value) <= 0) {
        lo = mid + 1;
      } else {
        hi = mid;
      }
    }
    return lo;
  }

  public static int HeadBinarySearch<T>(Span<T> slice, T value)
      where T : IComparable<T> {
    int hi = slice.Length;
    int lo = 0;
    while (lo < hi) {
      int mid = lo + ((hi - lo) >> 1);
      if (slice[mid].CompareTo(value) < 0) {
        lo = mid + 1;
      } else {
        hi = mid;
      }
    }
    return lo;
  }

  public static void TimBinaryInsertion<T>(Span<T> slice)
      where T : IComparable<T> {
    int length = slice.Length;
    for (int c_idx = 1; c_idx < length; ++c_idx) {
      T value = slice[c_idx];
      if (value.CompareTo(slice[c_idx - 1]) >= 0)
        continue;

      int i_idx = TailBinarySearch(slice[..c_idx], value);
      slice[i_idx..c_idx].CopyTo(slice.Slice(i_idx + 1, c_idx - i_idx));
      slice[i_idx] = value;
    }
  }

  public static int CountRun<T>(Span<T> slice)
      where T : IComparable<T> {
    int length = slice.Length;
    if (length <= 1)
      return length;

    int run = 2;
    if (slice[1].CompareTo(slice[0]) >= 0) {
      while (run < length && slice[run].CompareTo(slice[run - 1]) >= 0) ++run;
    } else {
      while (run < length && slice[run].CompareTo(slice[run - 1]) < 0) ++run;
      slice[..run].Reverse();
    }
    return run;
  }

  public static int TimMerge<T>(Span<T> slice, int l_len, int min_gallop)
      where T : unmanaged, IComparable<T> {
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
          // Step-by-Step
          if (galloping) {
            ++min_gallop;
            galloping = false;
          }
          int curr_cmp = l_buff[l_idx].CompareTo(slice[r_idx]);
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
            while (r_idx + g_idx < length && slice[r_idx + g_idx].CompareTo(l_val) < 0) {
              g_idx = Math.Min(length - r_idx, g_idx == 0 ? 1 : g_idx << 1);
            }

            g_idx = HeadBinarySearch(slice.Slice(r_idx, g_idx), l_val);
            slice.Slice(r_idx, g_idx).CopyTo(slice[idx..]);
            r_idx += g_idx;
            gallop = g_idx;  // Switch to Left Gallop
          } else {
            // Gallop left side
            T r_val = slice[r_idx];
            while (l_idx + g_idx < l_len && l_buff[l_idx + g_idx].CompareTo(r_val) <= 0) {
              g_idx = Math.Min(l_len - l_idx, g_idx == 0 ? 1 : g_idx << 1);
            }

            g_idx = TailBinarySearch(l_buff.Slice(l_idx, g_idx), r_val);
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
        ArrayPool<T>.Shared.Return(l_mem);
    }
  }

  public static void TimSort<T>(this Span<T> slice)
      where T : unmanaged, IComparable<T> {
    int length = slice.Length;

    int min_run = ComputeMinRun(slice.Length);
    if (length <= min_run) {
      TimBinaryInsertion(slice);
      return;
    }

    int expect_runs = BitOperations.Log2((uint)length);
    expect_runs += 1 + (expect_runs >> 1);  // 1.5 * Log2(N)
    Span<int> run_start  = stackalloc int[expect_runs];
    Span<int> run_length = stackalloc int[expect_runs];
    int runs_count       = 0;
    int min_gallop       = 7;

    for (int i = 0; i < length;) {
      int stride = CountRun(slice[i..]);
      if (stride < min_run) {
        stride = Math.Min(min_run, length - i);
        TimBinaryInsertion(slice.Slice(i, stride));
      }
      run_start[runs_count]  = i;
      run_length[runs_count] = stride;
      ++runs_count;
      i += stride;

      while (runs_count > 1) {
        int len_1 = run_length[runs_count - 2];
        int len_2 = run_length[runs_count - 1];
        bool inv3 = runs_count > 2 && run_length[runs_count - 3] <= len_2 + len_1;
        bool inv4 = runs_count > 3 && run_length[runs_count - 4]  //
                                          <= len_1 + run_length[runs_count - 3];
        if (len_1 <= len_2 || inv3 || inv4) {
          int len_combined = len_1 + len_2;
          min_gallop       = TimMerge(slice.Slice(run_start[runs_count - 2], len_combined),  //
                    len_1, min_gallop);
          run_length[runs_count - 2] = len_combined;
          --runs_count;
        } else {
          break;
        }
      }
    }

    while (runs_count > 1) {
      int len_1  = run_length[runs_count - 2];
      int len_2  = run_length[runs_count - 1];
      int len_t  = len_1 + len_2;
      min_gallop = TimMerge(slice.Slice(run_start[runs_count - 2], len_t), len_1, min_gallop);
      run_length[runs_count - 2] = len_t;
      --runs_count;
    }
  }
}
}
