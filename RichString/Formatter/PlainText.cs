using System.Text;

namespace MMOR.NET.RichString {
  public static partial class RichStringFormatter {
    public static readonly RichStringPlainTextFormatter kPlainText  = new();
    public static string ToPlainText(this IRichString rich_str)    => rich_str.Format(kPlainText);
  }
  public readonly struct RichStringPlainTextFormatter : IRichStringFormatter {
    public StringBuilder Format(IRichString rich_str, StringBuilder? result) {
      result ??= new StringBuilder();

      switch (rich_str) {
        case RichStringBuilder master:
          FormatRichString(master, result);
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

    private void FormatRichString(RichStringBuilder rich_str, StringBuilder result) {
      foreach (IRichString rich_component in rich_str.Components) Format(rich_component, result);
    }
  }
}
