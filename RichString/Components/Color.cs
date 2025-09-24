using System.Numerics;

namespace MMOR.NET.RichString
{
  public struct RichStringColor
  {
    public byte R;
    public byte G;
    public byte B;
    public byte A;

    public RichStringColor(byte red, byte green, byte blue, byte alpha = 0xFF)
    {
      R = red;
      G = green;
      B = blue;
      A = alpha;
    }

    public RichStringColor(float red, float green, float blue, float alpha = 1.0f)
    {
      R = (byte)(red * 255);
      G = (byte)(green * 255);
      B = (byte)(blue * 255);
      A = (byte)(alpha * 255);
    }

    public RichStringColor(int hex)
    {
      if (hex > 0xFFFFFF)
      {
        G = (byte)((hex >> 24) & 0xFF);
        R = (byte)((hex >> 16) & 0xFF);
        B = (byte)((hex >> 08) & 0xFF);
        A = (byte)((hex >> 00) & 0xFF);
      }
      else
      {
        R = (byte)((hex >> 16) & 0xFF);
        G = (byte)((hex >> 08) & 0xFF);
        B = (byte)((hex >> 00) & 0xFF);
        A = 0xFF;
      }
    }

    public string GetHex()
    {
      return $"#{R:X2}{G:X2}{B:X2}" + (A != 255 ? $"{A:X2}" : string.Empty);
    }

    public static implicit operator RichStringColor(Vector4 vector) =>
      new(vector.X, vector.Y, vector.Z, vector.W);

    public static implicit operator RichStringColor(Vector3 vector) =>
      new(vector.X, vector.Y, vector.Z);
  }

  public class RichStringColored : IRecursiveRichString
  {
    public IRichString str { get; }
    public RichStringColor color;

    public RichStringColored(IRichString str, RichStringColor color)
    {
      this.str = str;
      this.color = color;
    }

    public RichStringColored(IRichString str, byte red, byte green, byte blue, byte alpha = 0xFF)
    {
      this.str = str;
      color = new RichStringColor(red, green, blue, alpha);
    }

    public RichStringColored(
      IRichString str,
      float red,
      float green,
      float blue,
      float alpha = 1.0f
    )
    {
      this.str = str;
      color = new RichStringColor(red, green, blue, alpha);
    }

    public RichStringColored(RichStringColored copy)
    {
      this.str = copy.Clone();
      this.color = copy.color;
    }

    public IRichString Clone() => new RichStringColored(str, color);

    public IRichString ReplaceString(IRichString new_str) =>
      new RichStringColored(new_str, this.color);
  }

  public static partial class RichStringUtils
  {
    public static RichStringColored SetColor(this string text, RichStringColor color) =>
      new((RichStringPlain)text, color);

    public static RichStringColored SetColor(this IRichString text, RichStringColor color) =>
      new(text.Clone(), color);
  }
}
