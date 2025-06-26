using System;
using System.Numerics;

namespace MMOR.Utils.Mathematics
{
    public static class ColorUtils
    {
        public static Vector4 FromHexCode(in int hexCode)
        {
            var result = new Vector4();
            FromHexCode(hexCode, ref result);
            return result;
        }

        public static void FromHexCode(in int hexCode, ref Vector4 result)
        {
            result.X = ((hexCode >> 16) & 0xFF) / 255f;
            result.Y = ((hexCode >> 8) & 0xFF) / 255f;
            result.Z = ((hexCode >> 0) & 0xFF) / 255f;
            result.W = 1f;
        }

        public static Vector4 HSVtoRGB(in float Hue, in float Saturation, in float Value, in bool hdr = true)
        {
            Vector4 result = new(1f, 1f, 1f, 1f);
            HSVtoRGB(ref result, Hue, Saturation, Value, hdr);
            return result;
        }

        public static void HSVtoRGB(ref Vector4 result, in float H, in float S, in float V, in bool hdr = true)
        {
            if (S == 0f)
            {
                result.X = V;
                result.Y = V;
                result.Z = V;
            }
            else if (V == 0f)
            {
                result.X = 0f;
                result.Y = 0f;
                result.Z = 0f;
            }
            else
            {
                result.X = 0f;
                result.Y = 0f;
                result.Z = 0f;
                float num = H * 6f;
                var num2 = (int)MathF.Floor(num);
                float num3 = num - num2;
                float num4 = V * (1f - S);
                float num5 = V * (1f - S * num3);
                float num6 = V * (1f - S * (1f - num3));
                switch (num2)
                {
                    case 0:
                        result.X = V;
                        result.Y = num6;
                        result.Z = num4;
                        break;
                    case 1:
                        result.X = num5;
                        result.Y = V;
                        result.Z = num4;
                        break;
                    case 2:
                        result.X = num4;
                        result.Y = V;
                        result.Z = num6;
                        break;
                    case 3:
                        result.X = num4;
                        result.Y = num5;
                        result.Z = V;
                        break;
                    case 4:
                        result.X = num6;
                        result.Y = num4;
                        result.Z = V;
                        break;
                    case 5:
                        result.X = V;
                        result.Y = num4;
                        result.Z = num5;
                        break;
                    case 6:
                        result.X = V;
                        result.Y = num6;
                        result.Z = num4;
                        break;
                    case -1:
                        result.X = V;
                        result.Y = num4;
                        result.Z = num5;
                        break;
                }

                if (!hdr)
                {
                    result.X = Math.Clamp(result.X, 0f, 1f);
                    result.Y = Math.Clamp(result.Y, 0f, 1f);
                    result.Z = Math.Clamp(result.Z, 0f, 1f);
                }
            }
        }

        public static float Exposure(in float value, in float exposure) { return value * MathF.Pow(2, exposure); }
    }
}