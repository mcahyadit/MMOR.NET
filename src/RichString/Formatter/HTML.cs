using System.Text;
using System.Web;

namespace MMOR.NET.RichString {
  public static partial class RichStringFormatter {
    public static readonly RichStringHtmlFormatter kHtml    = new();
    public static string ToHtml(this IRichString rich_str) => rich_str.Format(kHtml);
  }

  public readonly struct RichStringHtmlFormatter : IRichStringFormatter {
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
          result.Append(HttpUtility.HtmlEncode(plain.str));
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
      result.Append("<span style=\"color:");
      result.Append(rich_str.color.GetHex());
      result.Append("\">");
      Format(rich_str.str, result);
      result.Append("</span>");
    }

    private void FormatWeight(RichStringFontWeight rich_str, StringBuilder result) {
      result.Append("<span style=\"font-weight:");
      result.Append(rich_str.font_weight);
      result.Append("\">");
      Format(rich_str.str, result);
      result.Append("</span>");
    }

    private void FormatBold(RichStringBold rich_string, StringBuilder result) {
      result.Append("<strong>");
      Format(rich_string.str, result);
      result.Append("</strong>");
    }

    private void FormatItalic(RichStringItalic rich_string, StringBuilder result) {
      result.Append("<em>");
      Format(rich_string.str, result);
      result.Append("</em>");
    }

    private void FormatUnderline(RichStringUnderline rich_string, StringBuilder result) {
      result.Append("<u>");
      Format(rich_string.str, result);
      result.Append("</u>");
    }
  }
}
