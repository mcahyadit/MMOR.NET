using System.Collections.Generic;
using System.Text;

namespace MMOR.NET.RichString {
  public static partial class RichStringFormatter {
    public static readonly RichStringTerminalFormatter kTerminal  = new();
    public static string ToTerminal(this IRichString rich_str)   => rich_str.Format(kTerminal);
  }

  public readonly struct RichStringTerminalFormatter : IRichStringFormatter {
    public StringBuilder Format(IRichString rich_str, StringBuilder? result) {
      Stack<string> escape_stack = new();
      escape_stack.Push(kAnsiEscapeReset);
      return Format(rich_str, result, escape_stack);
    }
    private const string kAnsiEscapeReset = "\u001b[0m";

    private StringBuilder Format(
        IRichString rich_str, StringBuilder? result, Stack<string> escape_stack) {
      result ??= new StringBuilder();

      switch (rich_str) {
        case RichStringBuilder master:
          FormatRichString(master, result, escape_stack);
          break;
        case RichStringColored colored:
          FormatColor(colored, result, escape_stack);
          break;
        case RichStringPlain plain:
          result.Append(plain.str);
          break;
        case IRecursiveRichString pass_through:
          Format(pass_through.str, result, escape_stack);
          break;
      }

      return result;
    }

    private void FormatRichString(
        RichStringBuilder rich_str, StringBuilder result, Stack<string> escape_stack) {
      foreach (IRichString rich_component in rich_str.Components)
        Format(rich_component, result, escape_stack);
    }

    private void FormatColor(
        RichStringColored rich_str, StringBuilder result, Stack<string> escape_stack) {
      ref RichStringColor col = ref rich_str.color;

      string color_ansi = $"\u001b[38;2;{col.R};{col.G};{col.B}m";
      result.Append(color_ansi);
      escape_stack.Push(color_ansi);
      Format(rich_str.str, result, escape_stack);
      escape_stack.Pop();
      result.Append(escape_stack.Peek());
    }
  }
}
