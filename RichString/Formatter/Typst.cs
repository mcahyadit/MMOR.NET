using System.Text;

namespace MMOR.NET.RichString {
  public static partial class RichStringFormatter {
    public static readonly RichStringTypstFormatter kTypst    = new();
    public static string ToTypst(this IRichString rich_str) => rich_str.Format(kTypst);
  }

  public readonly struct RichStringTypstFormatter : IRichStringFormatter {
    public StringBuilder Format(IRichString rich_str, StringBuilder? result) {
      result ??= new StringBuilder();

      switch (rich_str) {
        case RichStringBuilder master:
          FormatRichString(master, result);
          break;
        case RichStringColored colored:
          FormatColor(colored, result);
          break;
        case RichStringFontWeight weight:
          FormatWeight(weight, result);
          break;
        case RichStringBold bold:
          FormatBold(bold, result);
          break;
        case RichStringItalic italic:
          FormatItalic(italic, result);
          break;
        case RichStringUnderline underline:
          FormatUnderline(underline, result);
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

    private void FormatColor(RichStringColored rich_str, StringBuilder result) {
      ref RichStringColor col = ref rich_str.color;
      result.Append("#text(fill: rgb(");
      result.Append($"{col.R},{col.G},{col.B}");
      result.Append(")[");
      Format(rich_str.str, result);
      result.Append("]");
    }

    private void FormatWeight(RichStringFontWeight rich_str, StringBuilder result) {
      result.Append("#text(fill: weight:");
      result.Append(rich_str.font_weight);
      result.Append(")[");
      Format(rich_str.str, result);
      result.Append("]");
    }

    private void FormatBold(RichStringBold rich_string, StringBuilder result) {
      result.Append("*");
      Format(rich_string.str, result);
      result.Append("*");
    }

    private void FormatItalic(RichStringItalic rich_string, StringBuilder result) {
      result.Append("_");
      Format(rich_string.str, result);
      result.Append("_");
    }

    private void FormatUnderline(RichStringUnderline rich_string, StringBuilder result) {
      result.Append("#underline[");
      Format(rich_string.str, result);
      result.Append("]");
    }
  }
}
