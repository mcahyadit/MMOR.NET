using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using MMOR.Utils.Mathematics;

namespace MMOR.Utils.Utilities
{
    /// <summary>
    ///     <strong>CustomLibrary</strong>
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     <br /> - Function Libraries for multiple utilities.
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// </summary>
    public static partial class Utilities
    {
        /// <summary>
        ///     <strong>CustomLibrary.ValueColor()</strong>
        ///     <br />
        ///     -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Returns a <see cref="Color" /> to represent <paramref name="value" />.
        ///     <br /> - Returned <see cref="Color" /> roughly goes from red -> orange -> yellow -> green -> blue -> navy -> purple
        ///     -> pink, from <paramref name="value" /> <i>0.0</i> to <i>1.0</i>.
        ///     <br /> - Returned <see cref="Color" /> goes closer to white the further <paramref name="value" /> goes beyond
        ///     <i>1.0</i>.
        ///     <br /> - Returned <see cref="Color" /> goes closer to black the further <paramref name="value" /> goes below
        ///     <i>0.0</i>.
        ///     <br />
        ///     -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - <paramref name="gamma" /> controls the curve of the function.
        ///     <br /> - Higher values will widen the range around red -> yellow,
        ///     <br /> - Lower values will widen the range around blue -> pink,
        ///     <br />
        ///     -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 ValueColor<T>(T value, double gamma = 1) where T : IConvertible
        {
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

            if (val < 0)
            {
                val = Math.Clamp(val, -1, 0);
                H = MathF.Pow(val + 1, 1f / 2);
                S = Math.Clamp(MathF.Pow(0.75f + val, 3), 0, 1) + 0.4f;
                V = S;
            }
            else if (val > 1)
            {
                val = Math.Clamp(val, 1, 2);
                H = MathF.Pow(val, 0.8f) * 0.92f;
                S = 0.4f * MathF.Pow(2 - val, 10) + 0.25f - MathF.Sin(val / 15);
                V = 1.0f;
            }
            else
            {
                H = MathF.Pow(MathF.Sin(val * MathF.PI / 2), 1.2f) * 0.92f;
                S = 0.4f + MathF.Pow((MathF.Cos(2 * MathF.PI * MathF.Pow(H, 1.7f)) + 1) / 3, 2.2f);
                V = 1f -
                    0.1f * MathF.Exp(-MathF.Pow((H - 0.154f) / 0.05f, 2)) -
                    0.12f * MathF.Exp(-MathF.Pow((H - 0.317f) / 0.2f, 2)) -
                    0.21f * MathF.Exp(-MathF.Pow((H - 0.48f) / 0.1f, 2));
            }

            Vector4 color = ColorUtils.HSVtoRGB(MathExt.Frac(H), S, V);
            return color;
        }

        /// <summary>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Prints out a 1D Array <paramref name="arr" /> as 2D Array with <paramref name="heights" />.
        ///     <br /> - Filling empty cells with <paramref name="empty" />.
        ///     <br /> - Only for Debugging.
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        public static T[][] As2D<T>(this IReadOnlyList<T> arr, IReadOnlyList<int> heights, T empty = default,
            bool transpose = true) where T : struct
        {
            var index = 0;

            int len = heights.Count;
            int maxHeight = heights.Max();
            var result = new T[len][];
            for (var col = 0; col < len; col++)
            {
                result[col] = new T[maxHeight];
                Array.Fill(result[col], empty);

                int reelHeight = heights[col];
                for (var row = 0; row < reelHeight; row++)
                    result[col][row] = arr[index++];
            }

            return transpose ? result.Transpose() : result;
        }
    }
}