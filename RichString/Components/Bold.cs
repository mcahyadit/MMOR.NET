namespace MMOR.NET.RichString {
  public class RichStringBold : IRecursiveRichString {
    public IRichString str { get; }

    public RichStringBold(IRichString str) {
      this.str = str;
    }

    public RichStringBold(RichStringBold copy) {
      this.str = copy.Clone();
    }

    public IRichString Clone() => new RichStringBold(this);

    public IRichString ReplaceString(IRichString new_str) => new RichStringBold(new_str);
  }

  public static partial class RichStringUtils {
    public static RichStringBold Bold(this string text) => new((RichStringPlain)text);
  }
}
