using System;

namespace MMOR.NET.RichString
{
  public enum RichFontWeight : uint
  {
    Thin = 100,
    ExtraLight = 200,
    Light = 300,
    Regular = 400,
    Medium = 500,
    SemiBold = 600,
    Bold = 700,
    Heavy = 800,
    Black = 900,

    //-+-+-+-+-+-+-+-+
    // Common Alternate Names
    //-+-+-+-+-+-+-+-+
    UltraLight = 200,
    Normal = 400,
    Dark = 500,
    DemiBold = 600,
    ExtraBold = 800,
    UltraBold = 800,
  }

  public class RichStringFontWeight : IRecursiveRichString
  {
    public IRichString str { get; }
    public uint font_weight;

    public RichStringFontWeight(IRichString str, uint font_weight = 400)
    {
      this.str = str;
      this.font_weight = font_weight;
    }

    public RichStringFontWeight(
      IRichString str,
      RichFontWeight font_weight = RichFontWeight.Regular
    )
    {
      this.str = str;
      this.font_weight = (uint)font_weight;
    }

    public RichStringFontWeight(RichStringFontWeight copy)
    {
      this.str = copy.Clone();
      this.font_weight = copy.font_weight;
    }

    public IRichString Clone() => new RichStringFontWeight(this);

    public IRichString ReplaceString(IRichString new_str) =>
      new RichStringFontWeight(new_str, this.font_weight);
  }

  public static partial class RichStringUtils
  {
    public static RichStringFontWeight SetWeight(this string text, RichFontWeight font_weight) =>
      new((RichStringPlain)text, font_weight);

    public static RichStringFontWeight SetWeight<T>(this string text, T font_weight)
      where T : struct, IConvertible => new((RichStringPlain)text, font_weight.ToUInt32(null));

    public static RichStringFontWeight SetWeight(
      this IRichString text,
      RichFontWeight font_weight
    ) => new(text.Clone(), font_weight);

    public static RichStringFontWeight SetWeight<T>(this IRichString text, T font_weight)
      where T : struct, IConvertible => new(text.Clone(), font_weight.ToUInt32(null));
  }
}
