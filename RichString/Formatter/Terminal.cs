using System.Text;

namespace MMOR.NET.RichString
{
  public static partial class RichStringFormatter
  {
    public static readonly RichStringTerminalFormatter kTerminal = new();
  }

  public readonly struct RichStringTerminalFormatter : IRichStringFormatter
  {
    public StringBuilder Format(IRichString rich_str, StringBuilder? result)
    {
      result ??= new StringBuilder();

      switch (rich_str)
      {
        case RichStringBuilder master:
          FormatRichString(master, result);
          break;
        case RichStringColored colored:
          FormatColor(colored, result);
          break;
        case RichStringPlain plain:
          result.Append(plain.str);
          break;
        case IRecursiveRichString pass_through:
          Format(pass_through.str, result);
          break;
      }

      return result;
    }

    private void FormatRichString(RichStringBuilder rich_str, StringBuilder result)
    {
      foreach (IRichString rich_component in rich_str.Components)
        Format(rich_component, result);
    }

    private void FormatColor(RichStringColored rich_str, StringBuilder result)
    {
      ref RichStringColor col = ref rich_str.color;
      result.Append("\033[38;2;");
      result.Append($"{col.R};{col.G};{col.B}");
      result.Append("m");
      Format(rich_str.str, result);
      result.Append("\u001b[0m");
    }
  }
}
