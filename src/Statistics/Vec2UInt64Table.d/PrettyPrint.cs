using System;
using System.Collections.Generic;
using MMOR.NET.RichString;

namespace MMOR.NET.Statistics {

public static class Vec2UInt64PrettyPrint {
  public static IRichString PrettyPrint(this Vec2UInt64Table self, Vec2UInt64TablePrintOpts opts) {
    IRichString[] r_value1      = new IRichString[self.Height * self.Width];
    IRichString[] r_value2      = new IRichString[self.Height * self.Width];
    IRichString[] r_col_headers = new IRichString[self.Width];
    IRichString[] r_row_headers = new IRichString[self.Height];
    IRichString[] r_cs_value1   = new IRichString[self.Width];
    IRichString[] r_cs_value2   = new IRichString[self.Width];
    IRichString[] r_rs_value1   = new IRichString[self.Height];
    IRichString[] r_rs_value2   = new IRichString[self.Height];
    int longest_value1          = 0;
    int longest_value2          = 0;
    int longest_row_header      = 0;
    int longest_rs_value1       = 0;
    int longest_col_header      = 0;

    //============================
    // █▀█ █▀▀ █▀ █▀█ █░░ █░█ █▀▀
    // █▀▄ ██▄ ▄█ █▄█ █▄▄ ▀▄▀ ██▄
    //============================
    int row_prints = 0;
    int col_prints = 0;
    // Column Header
    for (int col = 0; col < self.Width; ++col) {
      if (!opts.col_predicate(self, col)) {
        continue;
      }
      ++col_prints;
      IRichString col_header_str = opts.col_header(col);
      r_col_headers[col]         = col_header_str;
      longest_col_header         = Math.Max(longest_col_header, col_header_str.Length);
    }

    for (int row = 0; row < self.Height; ++row) {
      if (!opts.row_predicate(self, row)) {
        continue;
      }
      ++row_prints;

      IRichString row_header_str = opts.row_header(row);
      r_row_headers[row]         = row_header_str;
      longest_row_header         = Math.Max(longest_row_header, row_header_str.Length);

      for (int col = 0; col < self.Width; ++col) {
        if (!opts.col_predicate(self, col)) {
          continue;
        }

        int idx = row * self.Width + col;

        ulong value1   = self.value1_[idx];
        ulong value2   = self.value2_[idx];
        IRichString v1 = opts.value1_print(self, row, col, value1);
        IRichString v2 = opts.value2_print(self, row, col, value2);

        r_value1[idx] = v1;
        r_value2[idx] = v2;

        longest_value1 = Math.Max(longest_value1, v1.Length);
        longest_value2 = Math.Max(longest_value2, v2.Length);
      }

      if (self.Height > 1) {
        IRichString v1 = opts.row_sum_value1_print(self, row, self.row_sum_value1_[row]);
        IRichString v2 = opts.row_sum_value2_print(self, row, self.row_sum_value2_[row]);

        r_rs_value1[row] = v1;
        r_rs_value2[row] = v2;

        longest_rs_value1 = Math.Max(longest_rs_value1, v1.Length);
      }
    }

    // Column Sum
    if (self.Width > 1) {
      for (int col = 0; col < self.Width; ++col) {
        if (!opts.col_predicate(self, col)) {
          continue;
        }
        IRichString col_value1_str =
            opts.col_sum_value1_print(self, col, self.col_sum_value1_[col]);
        IRichString col_value2_str =
            opts.col_sum_value2_print(self, col, self.col_sum_value2_[col]);
        longest_value1 = Math.Max(longest_value1, col_value1_str.Length);
        longest_value2 = Math.Max(longest_value2, col_value2_str.Length);

        r_cs_value1[col] = col_value1_str;
        r_cs_value2[col] = col_value2_str;
      }
    }

    int half_head  = (longest_col_header - opts.values_separator.Length) / 2;
    longest_value1 = Math.Max(longest_value1, half_head);
    longest_value2 = Math.Max(longest_value2, half_head);

    if (longest_value1 == 0 && longest_value2 == 0)
      return RichStringUtils.kRichEmpty;

    longest_value1 += 1;
    longest_value2 += 1;

    //===========================
    // █▀▀ █▀█ █▀█ █▀▄▀█ ▄▀█ ▀█▀
    // █▀░ █▄█ █▀▄ █░▀░█ █▀█ ░█░
    //===========================
    RichStringBuilder sb = new();
    int length_sum       = longest_value1 + opts.values_separator.Length + longest_value2;
    sb.Append(new string(' ', longest_row_header))
        .Append(opts.column_header_separator ?? opts.column_separator);
    for (int col = 0; col < self.Width; ++col) {
      if (!opts.col_predicate(self, col)) {
        continue;
      }
      sb.Append(r_col_headers[col].PadCenter(length_sum)).Append(opts.column_separator);
    }
    sb.AppendLine();

    if (opts.table_delimiter is char delim) {
      sb.Append(new string(' ', longest_row_header))
          .Append(opts.column_header_separator ?? opts.column_separator);
      for (int col = 0; col < self.Width; ++col) {
        if (!opts.col_predicate(self, col)) {
          continue;
        }
        sb.Append(new string(delim, length_sum)).Append(opts.column_separator);
      }
      sb.AppendLine();
    }

    for (int row = 0; row < self.Height; ++row) {
      if (!opts.row_predicate(self, row)) {
        continue;
      }

      sb.Append(r_row_headers[row].PadLeft(longest_row_header))
          .Append(opts.column_header_separator ?? opts.column_separator);

      // value1s
      for (int col = 0; col < self.Width; ++col) {
        if (!opts.col_predicate(self, col)) {
          continue;
        }
        int idx = row * self.Width + col;
        if (r_value1[idx].Length == 0 && r_value2[idx].Length == 0)
          sb.Append(new string(opts.content_guide, length_sum)).Append(opts.column_separator);
        else
          sb.Append(r_value1[idx].PadLeft(longest_value1, opts.content_guide))
              .Append(opts.values_separator)
              .Append(r_value2[idx].PadRight(longest_value2, opts.content_guide))
              .Append(opts.column_separator);
      }

      if (row_prints > 1) {
        sb.Append(r_rs_value1[row].PadLeft(longest_rs_value1, opts.sum_guide))
            .Append(opts.values_separator)
            .Append(r_rs_value2[row]);
      }
      sb.AppendLine();
    }
    if (col_prints > 1) {
      sb.Append(new string(' ', longest_row_header))
          .Append(opts.column_header_separator ?? opts.column_separator);
      for (int col = 0; col < self.Width; ++col) {
        if (!opts.col_predicate(self, col)) {
          continue;
        }
        sb.Append(r_cs_value1[col].PadLeft(longest_value1, opts.sum_guide))
            .Append(opts.values_separator)
            .Append(r_cs_value2[col].PadRight(longest_value2, opts.sum_guide))
            .Append(opts.column_separator);
      }
    }

    if (opts.row_colors != null && opts.row_colors.Length > 0) {
      int color_len            = opts.row_colors.Length;
      IList<IRichString> lines = sb.SplitByNewLine();
      sb.Clear();

      for (int i = 0; i < lines.Count; ++i) {
        sb.AppendLine(lines[i].SetColor(opts.row_colors[i % color_len]));
      }
    }

    return sb;
  }
}
}
