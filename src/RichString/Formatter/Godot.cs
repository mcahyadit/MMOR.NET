using System.Text;
using System.Web;

namespace MMOR.NET.RichString {
public static partial class RichStringFormatter {
  public static readonly RichStringGodotFormatter kGodot = new();
}

public readonly struct RichStringGodotFormatter : IRichStringFormatter {
  public StringBuilder Format(IRichString rich_str, StringBuilder? result) {
    result ??= new StringBuilder();

    switch(rich_str) {
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
    foreach(IRichString rich_component in rich_str.Components)
      Format(rich_component, result);
  }

  private void FormatColor(RichStringColored rich_str, StringBuilder result) {
    result.Append("[color=");
    result.Append(rich_str.color.GetHex());
    result.Append("]");
    Format(rich_str.str, result);
    result.Append("[/color]");
  }
}
}