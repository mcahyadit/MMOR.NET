using System.Text;
using System.Web;

namespace MMOR.NET.RichString {
  public static partial class RichStringFormatter {
    public static readonly RichStringGodotFormatter kGodot   = new();
    public static string ToGodot(this IRichString rich_str) => rich_str.Format(kGodot);
  }

  public readonly struct RichStringGodotFormatter : IRichStringFormatter {
    public StringBuilder Format(IRichString rich_str, StringBuilder? result) {
      result ??= new StringBuilder();

      switch (rich_str) {
        case RichStringBuilder master:
          FormatRichString(master, result);
          break;
        case RichStringColored colored:
          FormatColor(colored, result);
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
      result.Append("[color=");
      result.Append(rich_str.color.GetHex());
      result.Append("]");
      Format(rich_str.str, result);
      result.Append("[/color]");
    }

    private void FormatWeight(RichStringFontWeight rich_str, StringBuilder result) {
      bool fake_bold = rich_str.font_weight >= (uint)RichFontWeight.Bold;

      if (fake_bold)
        result.Append("[b]");
      Format(rich_str.str, result);
      if (fake_bold)
        result.Append("[/b]");
    }

    private void FormatBold(RichStringBold rich_string, StringBuilder result) {
      result.Append("[b]");
      Format(rich_string.str, result);
      result.Append("[/b]");
    }

    private void FormatItalic(RichStringItalic rich_string, StringBuilder result) {
      result.Append("[i]");
      Format(rich_string.str, result);
      result.Append("[/i]");
    }

    private void FormatUnderline(RichStringUnderline rich_string, StringBuilder result) {
      result.Append("[u]");
      Format(rich_string.str, result);
      result.Append("[/u]");
    }
  }
}
