using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace MMOR.Utils.Utilities
{
    public enum vFontWeight
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

        UltraLight = 200,
        Normal = 400,
        Dark = 500,
        DemiBold = 600,
        ExtraBold = 800,
        UltraBold = 800
    }

    //-+-+-+-+-+-+-+-+
    // TMPro Tags
    // https://docs.unity3d.com/Packages/com.unity.textmeshpro@4.0/manual/RichText.html
    // ..to help with TextMeshPro
    //-+-+-+-+-+-+-+-+
    public static partial class Utilities
    {
        //-+-+-+-+-+-+-+-+
        // Rich Text Tags
        //-+-+-+-+-+-+-+-+
        private static readonly HashSet<string> RichTags = new()
        {
            "align",
            "allcaps",
            "alpha",
            "b",
            "color",
            "cspace",
            "font",
            "font-weight",
            "gradient",
            "i",
            "indent",
            "line-height",
            "line-indent",
            "link",
            "lowercase",
            "margin",
            "mark",
            "mspace",
            "nobr",
            "noparse",
            "page",
            "pos",
            "rotate",
            "s",
            "size",
            "smallcaps",
            "space",
            "sprite",
            "style",
            "sub",
            "sup",
            "u",
            "uppercase",
            "voffset",
            "width"
        };

        private static readonly Regex RichRegex = new($"<\\/?({string.Join("|", RichTags.Select(Regex.Escape))}).*?>");

        //-+-+-+-+-+-+-+-+
        public static string SetWeight<T>(this string inStr, T weight) where T : struct, Enum =>
            $"<font-weight={Convert.ToInt32(weight)}>{inStr}</font-weight>";

        public static string SetWeight(this string inStr, vFontWeight weight) =>
            $"<font-weight={(int)weight}>{inStr}</font-weight>";

        public static string SetWeight(this string inStr, int weight) => $"<font-weight={weight}>{inStr}</font-weight>";

        public static string SetSize(this string inStr, float size) => $"<size={size}>{inStr}</size>";

        public static string SetRelativeSize(this string inStr, float size) => $"<size={size * 100}%>{inStr}</size>";

        public static string StrikeThrough(this string inStr) => $"<s>{inStr}</s>";

        public static string Italic(this string inStr) => $"<i>{inStr}</i>";

        public static string ColorIt(this string inStr, Vector4 color) =>
            string.Format("<color=#{0:X2}{1:X2}{2:X2}{3:X2}>{4}</color>", (byte)(color.X * 255f),
                (byte)(color.Y * 255f), (byte)(color.Z * 255f), (byte)(color.W * 255f), inStr);

        /// <summary>
        ///     <strong>CustomLibrary.SplitRichTags()</strong>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Takes <see langword="string" /> <paramref name="input" /> and splits it into a <see cref="List{T}" /> of
        ///     pairs of <see langword="string" /> and <see langword="bool" />.
        ///     <br /> - The <see langword="string" /> part will represent the string as is.
        ///     <br /> - The <see langword="bool" /> part will indicate if that section is a Rich Text Tag.
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        public static List<(string text, bool isTag)> SplitRichTagsPair(string input)
        {
            var listResult = new List<(string str, bool isTag)>();
            var lastIndex = 0;

            // Find all tags using regex
            foreach (Match match in RichRegex.Matches(input))
            {
                // Add text before the tag
                if (match.Index > lastIndex)
                    listResult.Add((input.Substring(lastIndex, match.Index - lastIndex), false));

                // Add the tag itself
                listResult.Add((match.Value, true));
                lastIndex = match.Index + match.Length;
            }

            // Add the remaining plain text after the last tag
            if (lastIndex < input.Length)
                listResult.Add((input.Substring(lastIndex), false));

            return listResult;
        }

        public static string StripAllTags(this string input)
        {
            List<(string text, bool isTag)> map = SplitRichTagsPair(input);
            return map.Where(x => !x.isTag).Select(x => x.text).join();
        }

        //-+-+-+-+-+-+-+-+
        // Padding
        //-+-+-+-+-+-+-+-+
        /// <summary>
        ///     <strong>CustomLibrary.RichLength()</strong>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Gets the length of the <paramref name="text" />, disregarding the <i>Rich Text Tags</i>.
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        public static int RichLength(this string text)
        {
            return SplitRichTagsPair(text).Where(x => !x.isTag).Select(x => x.text).join().Length;
        }

        /// <summary>
        ///     <strong>CustomLibrary.RichPad()</strong>
        ///     <br />
        ///     -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Performs a
        ///     <i>
        ///         <see cref="string.PadLeft(int)" />
        ///     </i>
        ///     or
        ///     <i>
        ///         <see cref="string.PadRight(int)" />
        ///     </i>
        ///     while ignoring the length taken by <i>RichText Tags</i>.
        ///     <br />
        ///     -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - On
        ///     <b>
        ///         <paramref name="totalWidth" />
        ///     </b>
        ///     &lt; 0, performs a
        ///     <i>
        ///         <see cref="string.PadLeft(int)" />
        ///     </i>
        ///     .
        ///     <br /> - On
        ///     <b>
        ///         <paramref name="totalWidth" />
        ///     </b>
        ///     > 0, performs a
        ///     <i>
        ///         <see cref="string.PadRight(int)" />
        ///     </i>
        ///     .
        ///     <br /> - If you kept getting the
        ///     <b>
        ///         <paramref name="totalWidth" />
        ///     </b>
        ///     sign wrong, try imagining it as the <i>x-axis</i> in 2D Cartesian Coordinates.
        ///     <br />
        ///     -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - On
        ///     <b>
        ///         <paramref name="padInside" />
        ///     </b>
        ///     == <see langword="true" />, the padding will be done inside the <i>RichText Tags</i>, this will apply the effects
        ///     of the <i>RichText Tags</i> to the
        ///     <b>
        ///         <paramref name="paddingChar" />
        ///     </b>
        ///     .
        ///     <br /> - On
        ///     <b>
        ///         <paramref name="padInside" />
        ///     </b>
        ///     == <see langword="false" />, the padding will be done outside of the <i>RichText Tags</i>.
        ///     <br />
        ///     -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        public static string RichPad(this string inStr, int totalWidth = 0, char paddingChar = ' ',
            bool padInside = false)
        {
            List<(string text, bool isTag)> listSubStr = SplitRichTagsPair(inStr);

            bool padLeft = totalWidth < 0;
            totalWidth = Math.Abs(totalWidth);
            StringBuilder strResult = new();
            var plainLen = 0;
            foreach ((string text, bool isTag) subStr in listSubStr)
                if (subStr.isTag) { strResult.Append(subStr.text); }
                else
                {
                    plainLen += subStr.text.Length;
                    if (padInside)
                        strResult.Append(padLeft
                            ? subStr.text.PadLeft(totalWidth, paddingChar)
                            : subStr.text.PadRight(totalWidth, paddingChar));
                    else
                        strResult.Append(subStr.text);
                }

            var result = "";
            if (!padInside)
            {
                int totalLen = inStr.Length;
                result = padLeft
                    ? strResult.ToString().PadLeft(totalWidth + (totalLen - plainLen), paddingChar)
                    : strResult.ToString().PadRight(totalWidth + (totalLen - plainLen), paddingChar);
            }

            return result;
        }

        /// <summary>
        ///     <inheritdoc cref="RichPad" />
        ///     <br /> - Padding will only be applied up to <b>just before</b> the decimal separator.
        ///     <br />
        ///     -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        public static string RichPadDecimal(this string inStr, int totalWidth = 0, char paddingChar = ' ',
            bool padInside = false)
        {
            string[] strParts = inStr.Split(localeDecimalSeparator);
            if (strParts.Length > 1)
            {
                strParts[0] = RichPad(strParts[0], totalWidth, paddingChar, padInside) + localeDecimalSeparator;
                StringBuilder strResult = new();
                foreach (string subStr in strParts) strResult.Append(subStr);
                return strResult.ToString();
            }

            return RichPad(inStr, totalWidth, paddingChar, padInside);
        }

        /// <summary>
        ///     <strong>CustomLibrary.RichPadCenter()</strong>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Pads the string such that it is as centered as possible.
        ///     <br /> - Follows the same system as <see cref="RichPad" />
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        public static string RichPadCenter(this string inStr, int totalWidth, char paddingChar = ' ',
            bool padInside = false) =>
            inStr.RichPad((Math.Abs(totalWidth) + inStr.Length) / 2, paddingChar, padInside)
                .RichPad(-Math.Abs(totalWidth), paddingChar, padInside);
    }
}