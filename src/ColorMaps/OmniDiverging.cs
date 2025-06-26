using System;
using System.Numerics;
using MMOR.Utils.Mathematics;

namespace MMOR.Utils.ColorMaps
{
    // https://www.heavy.ai/blog/12-color-palettes-for-telling-better-stories-with-your-data
    public static partial class ColorMaps
    {
        public static readonly Vector4[] OmniBlueRedMap =
        {
            ColorUtils.FromHexCode(0x1984c5),
            ColorUtils.FromHexCode(0x22a7f0),
            ColorUtils.FromHexCode(0x63bff0),
            ColorUtils.FromHexCode(0xa7d5ed),
            ColorUtils.FromHexCode(0xe2e2e2),
            ColorUtils.FromHexCode(0xe1a692),
            ColorUtils.FromHexCode(0xde6e56),
            ColorUtils.FromHexCode(0xe14b31),
            ColorUtils.FromHexCode(0xc23728)
        };

        public static readonly Vector4[] OmniOrangePurpleMap =
        {
            ColorUtils.FromHexCode(0xffb400),
            ColorUtils.FromHexCode(0xd2980d),
            ColorUtils.FromHexCode(0xa57c1b),
            ColorUtils.FromHexCode(0x786028),
            ColorUtils.FromHexCode(0x363445),
            ColorUtils.FromHexCode(0x48446e),
            ColorUtils.FromHexCode(0x5e569b),
            ColorUtils.FromHexCode(0x776bcd),
            ColorUtils.FromHexCode(0x9080ff)
        };

        public static readonly Vector4[] OmniPinkFoamMap =
        {
            ColorUtils.FromHexCode(0x54bebe),
            ColorUtils.FromHexCode(0x76c8c8),
            ColorUtils.FromHexCode(0x98d1d1),
            ColorUtils.FromHexCode(0xbadbdb),
            ColorUtils.FromHexCode(0xdedad2),
            ColorUtils.FromHexCode(0xe4bcad),
            ColorUtils.FromHexCode(0xdf979e),
            ColorUtils.FromHexCode(0xd7658b),
            ColorUtils.FromHexCode(0xc80064)
        };

        public static readonly Vector4[] OmniSalmonAquaMap =
        {
            ColorUtils.FromHexCode(0xe27c7c),
            ColorUtils.FromHexCode(0xa86464),
            ColorUtils.FromHexCode(0x6d4b4b),
            ColorUtils.FromHexCode(0x503f3f),
            ColorUtils.FromHexCode(0x333333),
            ColorUtils.FromHexCode(0x3c4e4b),
            ColorUtils.FromHexCode(0x466964),
            ColorUtils.FromHexCode(0x599e94),
            ColorUtils.FromHexCode(0x6cd4c5)
        };

        public static Vector4 OmniBlueRed<T>(T value) where T : IConvertible
        {
            return MathExt.MultiLerp(Math.Clamp(value.ToSingle(null), 0f, 1f), OmniBlueRedMap);
        }

        public static Vector4 OmniOrangePurple<T>(T value) where T : IConvertible
        {
            return MathExt.MultiLerp(Math.Clamp(value.ToSingle(null), 0f, 1f), OmniOrangePurpleMap);
        }

        public static Vector4 OmniPinkFoam<T>(T value) where T : IConvertible
        {
            return MathExt.MultiLerp(Math.Clamp(value.ToSingle(null), 0f, 1f), OmniPinkFoamMap);
        }

        public static Vector4 OmniSalmonAqua<T>(T value) where T : IConvertible
        {
            return MathExt.MultiLerp(Math.Clamp(value.ToSingle(null), 0f, 1f), OmniSalmonAquaMap);
        }
    }
}