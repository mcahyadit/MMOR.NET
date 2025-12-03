namespace MMOR.NET.RichString {
  public class RichStringItalic : IRecursiveRichString {
    public IRichString str { get; }

    public RichStringItalic(IRichString str) {
      this.str = str;
    }

    public RichStringItalic(RichStringItalic copy) {
      this.str = copy.Clone();
    }

    public IRichString Clone() => new RichStringItalic(this);

    public IRichString ReplaceString(IRichString new_str) => new RichStringItalic(new_str);
  }

  public static partial class RichStringUtils {
    public static RichStringItalic Italic(this string text) => new((RichStringPlain)text);
  }
}
