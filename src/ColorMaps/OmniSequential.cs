using System;
using System.Numerics;
using MMOR.NET.Mathematics;

namespace MMOR.NET.ColorMaps
{
  // https://www.heavy.ai/blog/12-color-palettes-for-telling-better-stories-with-your-data
  public static partial class ColorMaps
  {
    public static readonly Vector4[] OmniBlueYellowMap =
    {
      ColorUtils.FromHexCode(0x115f9a),
      ColorUtils.FromHexCode(0x1984c5),
      ColorUtils.FromHexCode(0x22a7f0),
      ColorUtils.FromHexCode(0x48b5c4),
      ColorUtils.FromHexCode(0x76c68f),
      ColorUtils.FromHexCode(0xa6d75b),
      ColorUtils.FromHexCode(0xc9e52f),
      ColorUtils.FromHexCode(0xd0ee11),
      ColorUtils.FromHexCode(0xd0f400),
    };

    public static readonly Vector4[] OmniGrayRedMap =
    {
      ColorUtils.FromHexCode(0xd7e1ee),
      ColorUtils.FromHexCode(0xcbd6e4),
      ColorUtils.FromHexCode(0xbfcbdb),
      ColorUtils.FromHexCode(0xb3bfd1),
      ColorUtils.FromHexCode(0xa4a2a8),
      ColorUtils.FromHexCode(0xdf8879),
      ColorUtils.FromHexCode(0xc86558),
      ColorUtils.FromHexCode(0xb04238),
      ColorUtils.FromHexCode(0x991f17),
    };

    public static readonly Vector4[] OmniBlackPinkMap =
    {
      ColorUtils.FromHexCode(0x2e2b28),
      ColorUtils.FromHexCode(0x3b3734),
      ColorUtils.FromHexCode(0x474440),
      ColorUtils.FromHexCode(0x54504c),
      ColorUtils.FromHexCode(0x6b506b),
      ColorUtils.FromHexCode(0xab3da9),
      ColorUtils.FromHexCode(0xde25da),
      ColorUtils.FromHexCode(0xeb44e8),
      ColorUtils.FromHexCode(0xff80ff),
    };

    public static readonly Vector4[] OmniBluesMap =
    {
      ColorUtils.FromHexCode(0x0000b3),
      ColorUtils.FromHexCode(0x0010d9),
      ColorUtils.FromHexCode(0x0020ff),
      ColorUtils.FromHexCode(0x0040ff),
      ColorUtils.FromHexCode(0x0060ff),
      ColorUtils.FromHexCode(0x0080ff),
      ColorUtils.FromHexCode(0x009fff),
      ColorUtils.FromHexCode(0x00bfff),
      ColorUtils.FromHexCode(0x00ffff),
    };

    public static Vector4 OmniBlueYellow<T>(T value)
      where T : IConvertible =>
      MathExt.MultiLerp(Math.Clamp(value.ToSingle(null), 0f, 1f), OmniBlueYellowMap);

    public static Vector4 OmniGrayRed<T>(T value)
      where T : IConvertible =>
      MathExt.MultiLerp(Math.Clamp(value.ToSingle(null), 0f, 1f), OmniGrayRedMap);

    public static Vector4 OmniBlackPink<T>(T value)
      where T : IConvertible =>
      MathExt.MultiLerp(Math.Clamp(value.ToSingle(null), 0f, 1f), OmniBlackPinkMap);

    public static Vector4 OmniBlues<T>(T value)
      where T : IConvertible =>
      MathExt.MultiLerp(Math.Clamp(value.ToSingle(null), 0f, 1f), OmniBluesMap);
  }
}
