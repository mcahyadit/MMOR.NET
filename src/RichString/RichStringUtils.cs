using System;
using System.Collections.Generic;

namespace MMOR.NET.RichString {
  public static partial class RichStringUtils {
    public static readonly RichStringPlain kRichLineBreak = Environment.NewLine;
    public static readonly RichStringPlain kRichEmpty     = string.Empty;

    public static string Format(this IRichString rich_str,
        in IRichStringFormatter formatter) => formatter.Format(rich_str);

    public static RichStringBuilder PadSmart(this IRichString rich_str, int total_width,
        char padding_char = ' ') => total_width < 0 ? PadLeft(rich_str, -total_width, padding_char)
                                                    : PadRight(rich_str, total_width, padding_char);

    public static RichStringBuilder PadLeft(this IRichString rich_str, int total_width,
        char padding_char = ' ') {
      var result = new RichStringBuilder(rich_str);
      result.Prepend(
          (RichStringPlain) new string(padding_char, Math.Max(0, total_width - rich_str.Length)));
      return result;
    }

    public static RichStringBuilder PadRight(this IRichString rich_str, int total_width,
        char padding_char = ' ') {
      var result = new RichStringBuilder(rich_str);
      result.Append(
          (RichStringPlain) new string(padding_char, Math.Max(0, total_width - rich_str.Length)));
      return result;
    }

    public static RichStringBuilder PadCenter(this IRichString rich_str, int total_width,
        char padding_char = ' ') {
      var result = new RichStringBuilder(rich_str);
      int total  = Math.Abs(total_width) - rich_str.Length;
      int left   = total / 2;
      if (total_width < 0)
        total += 1;
      int right = total - left;
      result.Prepend((RichStringPlain) new string(padding_char, left));
      result.Append((RichStringPlain) new string(padding_char, right));
      return result;
    }

    public static IList<IRichString> SplitByNewLine(this IRichString input) {
      var result       = new List<IRichString>();
      var current_line = new RichStringBuilder();

      Process(input);
      EmitLine();  // flush the last line
      return result;

      void Process(IRichString rich) {
        switch (rich) {
          case RichStringPlain plain:
            string[] lines = plain.str.Split(Environment.NewLine);
            int len        = lines.Length;

            for (var i = 0; i < len; ++i) {
              if (i > 0)
                EmitLine();  // newline was found
              if (lines[i].Length > 0)
                current_line.Append((RichStringPlain)lines[i]);
            }
            break;
          case RichStringBuilder builder: {
            foreach (var component in builder.Components) Process(component);
            break;
          }

          case IRecursiveRichString rec: {
            // Split inner parts and wrap back in the same decorator
            foreach (IRichString? part in rec.str.SplitByNewLine()) {
              if (part.Length == 0) {
                EmitLine();
              } else {
                IRichString clone = rec.Clone();
                if (clone is IRecursiveRichString wrapped)
                  current_line.Append(wrapped.ReplaceString(part));
                else
                  current_line.Append(clone);
              }
            }
            break;
          }

          default:
            current_line.Append(rich);  // fallback
            break;
        }
      }

      void EmitLine() {
        result.Add(
            current_line.Components.Count == 0 ? kRichEmpty : new RichStringBuilder(current_line));
        current_line.Clear();
      }
    }

    public static RichStringPlain AsRichString(this string str) => str;

    /**
     * <summary>
     *  <br/> Appends, line-by-line, two or more <see cref="IRichString"/> next to each other,
     *    separated by <paramref name="separator"/>.
     *  <br/> Sees most use when using Rich Strings to print out Table like data.
     * </summary>
     * <param name="rich_strs">Sets of <see cref="IRichString"/> to be combined.</param>
     * <param name="separator">A string in between each RichString. Defaults to 3 spaces.</param>
     * <returns>
     *  Single RichString containing all contents of <paramref name="rich_strs"/> next to each other
     *  horizontally, separated by <paramref name="separator"/>.
     * </returns>
     * */
    public static IRichString AppendHorizontal(this IReadOnlyList<IRichString> rich_strs,
        IRichString? separator = null) {
      separator ??= new RichStringPlain("   ");
      int count   = rich_strs.Count;

      List<int> col_widths                        = new(count);
      List<IReadOnlyList<IRichString>> rich_lines = new(count);
      int max_lines                               = 0;
      foreach (IRichString rich_str in rich_strs) {
        IList<IRichString> rich_line = rich_str.SplitByNewLine();
        rich_lines.Add((IReadOnlyList<IRichString>)rich_line);

        max_lines = Math.Max(rich_line.Count, max_lines);
        col_widths.Add(rich_line.Max(x => x.Length));
      }

      RichStringBuilder str_builder = new();
      for (int row = 0; row < max_lines; ++row) {
        for (int col = 0; col < count; ++col) {
          if (row < rich_lines[col].Count)
            str_builder.Append(rich_lines[col][row].PadRight(col_widths[col]));
          else
            str_builder.Append(new string(' ', col_widths[col]));
          str_builder.Append(separator);
        }
        str_builder.AppendLine();
      }

      return str_builder;
    }
  }
}
