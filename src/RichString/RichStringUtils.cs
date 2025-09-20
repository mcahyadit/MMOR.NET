using System;
using System.Collections.Generic;

namespace MMOR.NET.RichString
{
  public static partial class RichStringUtils
  {
    public static readonly RichStringPlain kRichLineBreak = Environment.NewLine;
    public static readonly RichStringPlain kRichEmpty = string.Empty;

    public static string Format(this IRichString rich_str, in IRichStringFormatter formatter) =>
      formatter.Format(rich_str);

    public static RichStringBuilder PadSmart(
      this IRichString rich_str,
      int total_width,
      char padding_char = ' '
    ) =>
      total_width < 0
        ? PadLeft(rich_str, -total_width, padding_char)
        : PadRight(rich_str, total_width, padding_char);

    public static RichStringBuilder PadLeft(
      this IRichString rich_str,
      int total_width,
      char padding_char = ' '
    )
    {
      var result = new RichStringBuilder(rich_str);
      result.Prepend(
        (RichStringPlain)new string(padding_char, Math.Max(0, total_width - rich_str.Length))
      );
      return result;
    }

    public static RichStringBuilder PadRight(
      this IRichString rich_str,
      int total_width,
      char padding_char = ' '
    )
    {
      var result = new RichStringBuilder(rich_str);
      result.Append(
        (RichStringPlain)new string(padding_char, Math.Max(0, total_width - rich_str.Length))
      );
      return result;
    }

    public static RichStringBuilder PadCenter(
      this IRichString rich_str,
      int total_width,
      char padding_char = ' '
    )
    {
      var result = new RichStringBuilder(rich_str);
      int total = Math.Abs(total_width) - rich_str.Length;
      int left = total / 2;
      if (total_width < 0)
        total += 1;
      int right = total - left;
      result.Prepend((RichStringPlain)new string(padding_char, left));
      result.Append((RichStringPlain)new string(padding_char, right));
      return result;
    }

    public static IList<IRichString> SplitByNewLine(this IRichString input)
    {
      var result = new List<IRichString>();
      var currentLine = new RichStringBuilder();

      Process(input);
      EmitLine(); // flush the last line
      return result;

      void Process(IRichString rich)
      {
        switch (rich)
        {
          case RichStringPlain plain:
            string[] lines = plain.str.Split(Environment.NewLine);
            int len = lines.Length;

            for (var i = 0; i < len; ++i)
            {
              if (i > 0)
                EmitLine(); // newline was found
              if (lines[i].Length > 0)
                currentLine.Append((RichStringPlain)lines[i]);
            }
            break;
          case RichStringBuilder builder:
          {
            foreach (var component in builder.Components)
              Process(component);
            break;
          }

          case IRecursiveRichString rec:
          {
            // Split inner parts and wrap back in the same decorator
            foreach (IRichString? part in rec.str.SplitByNewLine())
            {
              if (part.Length == 0)
              {
                EmitLine();
              }
              else
              {
                IRichString clone = rec.Clone();
                if (clone is IRecursiveRichString wrapped)
                  currentLine.Append(wrapped.ReplaceString(part));
                else
                  currentLine.Append(clone);
              }
            }
            break;
          }

          default:
            currentLine.Append(rich); // fallback
            break;
        }
      }

      void EmitLine()
      {
        result.Add(
          currentLine.Components.Count == 0 ? kRichEmpty : new RichStringBuilder(currentLine)
        );
        currentLine.Clear();
      }
    }

    public static RichStringPlain AsRichString(this string str) => str;
  }
}
