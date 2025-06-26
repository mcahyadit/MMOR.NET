using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using MMOR.Utils.Utilities;

namespace MMOR.Utils.Mathematics
{
    //-+-+-+-+-+-+-+-+
    // Math Extentions
    //-+-+-+-+-+-+-+-+
    public static partial class MathExt
    {
        //-+-+-+-+-+-+-+-+
        // Approximately
        // ..For safer comparator between floating values
        //-+-+-+-+-+-+-+-+
        private const float absToleranceF = 8E-9f;
        private const double absToleranceD = 8E-17;

        //-+-+-+-+-+-+-+-+
        // Log2
        // ..Systems.Math.Log2 is not available to .NET Standard 2.1
        // ..this leverages mathematical definition of Logarithm with a predefined const
        // ..to get the result of Log2, by calling LogE
        //-+-+-+-+-+-+-+-+
        private static readonly float log2f = MathF.Log(2f);
        private static readonly double log2d = Math.Log(2d);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GCD(int a, int b) { return b == 0 ? a : GCD(b, a % b); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GCD(IEnumerable<int> list) { return list.Aggregate(GCD); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Approximately(float a, float b, float tolerance) { return Math.Abs(a - b) < tolerance; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Approximately(float a, float b)
        {
            return Math.Abs(a - b) < Math.Max(1E-06f * Math.Max(Math.Abs(a), Math.Abs(b)), absToleranceF);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Approximately(in Vector4 a, in Vector4 b)
        {
            return Approximately(a.X, b.X) &&
                   Approximately(a.Y, b.Y) &&
                   Approximately(a.Z, b.Z) &&
                   Approximately(a.W, b.W);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Approximately(double a, double b, double tolerance) { return Math.Abs(a - b) < tolerance; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Approximately(double a, double b)
        {
            return Math.Abs(a - b) < Math.Max(1E-15 * Math.Max(Math.Abs(a), Math.Abs(b)), absToleranceD);
        }

#if !NETCOREAPP3_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Log2(float value) { return MathF.Log(value) / log2f; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Log2(double value) { return Math.Log(value) / log2d; }
#endif

        //-+-+-+-+-+-+-+-+
        // Repeat
        // ..Wraps around value within length (exclusive)
        // e.g.
        // (-1, 5) => 4
        // (0, 5) => 4
        // (12, 5) => 2
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Repeat(this int value, int length)
        {
            return Math.Clamp(value - value / length * length, 0, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Repeat(this float value, float length)
        {
            return Math.Clamp(value - MathF.Floor(value / length) * length, 0, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Repeat(this double value, double length)
        {
            return Math.Clamp(value - Math.Floor(value / length) * length, 0, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Repeat(this decimal value, in decimal length)
        {
            return Math.Clamp(value - Math.Floor(value / length) * length, 0, length);
        }

        //-+-+-+-+-+-+-+-+
        // Remap
        // ..Mathematically known as Linear Interpolation
        // ..But due to Lerp often having its own definition in Computer Science
        // ..renamed to Remap
        // Mathmatically defined as:
        // ..y = yMin + (x - xMin) * (yMax - yMin) / (xMax - xMin)
        //-+-+-+-+-+-+-+-+
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Remap(this float value, float staMin, float staMax, float endMin = 0, float endMax = 1)
        {
            return (value - staMin) / (staMax - staMin) * (endMax - endMin) + endMin;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Remap(this double value, double staMin, double staMax, double endMin = 0,
            double endMax = 1)
        {
            return (value - staMin) / (staMax - staMin) * (endMax - endMin) + endMin;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Remap(this decimal value, in decimal staMin, in decimal staMax, in decimal endMin = 0,
            in decimal endMax = 1)
        {
            return (value - staMin) / (staMax - staMin) * (endMax - endMin) + endMin;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Remap(this Vector4 value, in Vector4 staMin, in Vector4 staMax)
        {
            return (value - staMin) / (staMax - staMin);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Remap(this Vector4 value, in Vector4 staMin, in Vector4 staMax, in Vector4 endMin,
            in Vector4 endMax)
        {
            return Remap(value, staMin, staMax) * (endMax - endMin) + endMin;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Remap(this Vector4 value, float staMin, float staMax, float endMin = 0, float endMax = 1)
        {
            float staRange = staMax - staMin;
            float endRange = endMax - endMin;
            float staRangeInv = 1f / staRange;

            return new Vector4(
                (value.X - staMin) * staRangeInv * endRange + endMin,
                (value.Y - staMin) * staRangeInv * endRange + endMin,
                (value.Z - staMin) * staRangeInv * endRange + endMin,
                (value.W - staMin) * staRangeInv * endRange + endMin
            );
        }
        //-+-+-+-+-+-+-+-+

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Frac(float x) { return x - MathF.Floor(x); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Frac(double x)
        {
            return x - Math.Floor(x);
            // Can't belive there's no native frac for double
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Frac(in decimal x) { return x - Math.Floor(x); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Frac(Vector4 x)
        {
            return new Vector4(
                x.X - MathF.Floor(x.X),
                x.Y - MathF.Floor(x.Y),
                x.Z - MathF.Floor(x.Z),
                x.W - MathF.Floor(x.W)
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Lerp(float a, float b, float alpha) { return a + (b - a) * alpha; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Lerp(double a, double b, double alpha) { return a + (b - a) * alpha; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Lerp(in decimal a, in decimal b, in decimal alpha) { return a + (b - a) * alpha; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Lerp(in Vector4 a, in Vector4 b, in Vector4 alpha)
        {
            return new Vector4(
                a.X + (b.X - a.X) * alpha.X,
                a.Y + (b.Y - a.Y) * alpha.Y,
                a.Z + (b.Z - a.Z) * alpha.Z,
                a.W + (b.W - a.W) * alpha.W
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Lerp(in Vector4 a, in Vector4 b, float alpha) { return Vector4.Lerp(a, b, alpha); }

        //-+-+-+-+-+-+-+-+
        // MultiLerp
        //-+-+-+-+-+-+-+-+
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MultipointLerp(float alpha, params float[] lerpMap)
        {
            int len = lerpMap.Length - 1;

            float alphaScale = alpha * len;
            var mapIndexOI = (int)MathF.Ceiling(alphaScale);
            if (Approximately(mapIndexOI, alpha * len))
                return lerpMap[mapIndexOI];

            return Lerp(lerpMap.SafeGet(mapIndexOI - 1), lerpMap.SafeGet(mapIndexOI), Frac(alphaScale));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double MultipointLerp(double alpha, params double[] lerpMap)
        {
            int len = lerpMap.Length - 1;

            double alphaScale = alpha * len;
            var mapIndexOI = (int)Math.Ceiling(alphaScale);
            if (Approximately(mapIndexOI, alpha * len))
                return lerpMap[mapIndexOI];

            return Lerp(lerpMap.SafeGet(mapIndexOI - 1), lerpMap.SafeGet(mapIndexOI), Frac(alphaScale));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 MultiLerp(float alpha, params Vector4[] lerpMap)
        {
            int len = lerpMap.Length - 1;

            float alphaScale = alpha * len;
            var mapIndexOI = (int)Math.Ceiling(alphaScale);
            if (Approximately(mapIndexOI, alpha * len))
                return lerpMap[mapIndexOI];

            return Lerp(lerpMap.SafeGet(mapIndexOI - 1), lerpMap.SafeGet(mapIndexOI), Frac(alphaScale));
        }
    }
}