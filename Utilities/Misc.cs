using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using MMOR.NET.Mathematics;

namespace MMOR.NET.Utilities {
  public static partial class Utilities {
    public static Vector4 ValueColor<T>(T value, double gamma = 1)
        where T : IConvertible {
      var val = value.ToSingle(null);
      if (val < 0)
        val = MathF.Sign(val) * MathF.Pow(MathF.Abs(val), 1 / (float)gamma);
      else if (val > 1)
        val = 1f + MathF.Pow(MathF.Abs(val - 1), (float)gamma);
      else
        val = MathF.Pow(val, (float)gamma);

      float H = 0;
      float S = 0;
      float V = 0;

      if (val < 0) {
        val = Math.Clamp(val, -1, 0);
        H   = MathF.Pow(val + 1, 1f / 2);
        S   = Math.Clamp(MathF.Pow(0.75f + val, 3), 0, 1) + 0.4f;
        V   = S;
      } else if (val > 1) {
        val = Math.Clamp(val, 1, 2);
        H   = MathF.Pow(val, 0.8f) * 0.92f;
        S   = 0.4f * MathF.Pow(2 - val, 10) + 0.25f - MathF.Sin(val / 15);
        V   = 1.0f;
      } else {
        H = MathF.Pow(MathF.Sin(val * MathF.PI / 2), 1.2f) * 0.92f;
        S = 0.4f + MathF.Pow((MathF.Cos(2 * MathF.PI * MathF.Pow(H, 1.7f)) + 1) / 3, 2.2f);
        V = 1f - 0.1f * MathF.Exp(-MathF.Pow((H - 0.154f) / 0.05f, 2)) -
            0.12f * MathF.Exp(-MathF.Pow((H - 0.317f) / 0.2f, 2)) -
            0.21f * MathF.Exp(-MathF.Pow((H - 0.48f) / 0.1f, 2));
      }

      Vector4 color = ColorUtils.HSVtoRGB(MathExt.Frac(H), S, V);
      return color;
    }

    public static T[][] As2D<T>(this IReadOnlyList<T> arr, IReadOnlyList<int> heights,
        T empty = default, bool transpose = true)
        where T : struct {
      var index = 0;

      int len       = heights.Count;
      int maxHeight = heights.Max();
      var result    = new T[len][];
      for (var col = 0; col < len; col++) {
        result[col] = new T[maxHeight];
        Array.Fill(result[col], empty);

        int reelHeight = heights[col];
        for (var row = 0; row < reelHeight; row++) result[col][row] = arr[index++];
      }

      return transpose ? result.Transpose() : result;
    }

    [Pure]
    public static string PadSmart(this string str, int total_width,
        char padding_char = ' ') => total_width < 0 ? str.PadLeft(-total_width, padding_char)
                                                    : str.PadRight(total_width, padding_char);

    [Pure]
    public static string PadCenter(this string str, int total_width, char padding_char = ' ') {
      int str_len = str.Length;
      if (total_width > 0) {
        return str.PadLeft((total_width + str_len) / 2, padding_char)
            .PadRight(total_width, padding_char);
      }
      total_width = -total_width;
      return str.PadRight((total_width + str_len) / 2, padding_char)
          .PadLeft(total_width, padding_char);
    }

    [Pure]
    public static string PadSmartToDecimal(
        this string str, int total_width, char padding_char = ' ') {
      int decimal_pos = str.IndexOf(localeDecimalSeparator);
      if (decimal_pos == -1)
        return str.PadSmart(total_width, padding_char);
      string left_side  = str[..decimal_pos];
      string right_side = str[decimal_pos..];
      return $"{left_side.PadSmart(total_width, padding_char)}{right_side}";
    }

    [Pure]
    public static string HtmlBody(this string str) => $"<body>{str}</body>";

    [Pure]
    public static string HtmlHead(this string str) => $"<head>{str}</head>";

    [Pure]
    public static string HtmlStyle(this string str) => $"<style>{str}</style>";

    [Pure]
    public static string HtmlParagraph(this string str) => $"<p>{str}</p>";
  }
}
