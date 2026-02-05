namespace MMOR.NET.RichString {
  public class RichStringUnderline : IRecursiveRichString {
    public IRichString str { get; }

    public RichStringUnderline(IRichString str) {
      this.str = str;
    }

    public RichStringUnderline(RichStringUnderline copy) {
      this.str = copy.Clone();
    }

    public IRichString Clone() => new RichStringUnderline(this);

    public IRichString ReplaceString(IRichString new_str) => new RichStringUnderline(new_str);
  }

  public static partial class RichStringUtils {
    public static RichStringUnderline Underline(this string text) => new((RichStringPlain)text);
  }
}
