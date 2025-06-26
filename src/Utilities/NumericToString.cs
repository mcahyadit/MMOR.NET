using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MMOR.Utils.Utilities
{
    //-+-+-+-+-+-+-+-+
    // Numeric to String
    //-+-+-+-+-+-+-+-+
    public static partial class Utilities
    {
        private static readonly NumberStyles numStyle = NumberStyles.Any;

        private static readonly Dictionary<char, decimal> largeNumbers = new()
        {
            { 'B', 10E9m },
            { 'M', 10E6m },
            { 'k', 10E3m }
        };

        internal static readonly char localeDecimalSeparator = 0.0.ToString("F1")[1];
        internal static readonly char localeThousandSeparator = 1000.0.ToString("N1")[1];

        internal static readonly char localeCurrencySymbol = 0.0.ToString("C0")[0];

        //private static readonly char decimalThousandSeparator = '\'';
        public static string SmartToString(this double value, int decimalPlaces = -1)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                return value.ToString(CultureInfo.InvariantCulture);

            double absValue = Math.Abs(value);

            // 1. Show full precision with thousands separator
            if (decimalPlaces < 0)
            {
                // Use "G17" to ensure full double precision
                var fullPrecision = value.ToString("G17", CultureInfo.InvariantCulture);
                if (double.TryParse(fullPrecision, out double parsed))
                    // Format with thousand separator
                    return parsed.ToString("#,0.#############################", CultureInfo.InvariantCulture);
                return fullPrecision;
            }

            // 2. Too small to be shown (rounded to 0)
            double precisionLimit = Math.Pow(10, -decimalPlaces);
            if (absValue > 0 && absValue < precisionLimit)
            {
                // Use scientific notation, upper-case E, with full precision
                //return value.ToString("E" + decimalPlaces, CultureInfo.InvariantCulture);
                var scientificNotation = value.ToString("E" + decimalPlaces, CultureInfo.InvariantCulture);

                // Post-process to remove leading zeros from the exponent
                // Example: 1.234567E-003 -> 1.234567E-3
                // Example: 1.234567E+003 -> 1.234567E+3
                // Example: 1.234567E+013 -> 1.234567E+13 (handles two-digit exponents gracefully)
                int eIndex = scientificNotation.IndexOf('E');
                if (eIndex != -1)
                {
                    string mantissa = scientificNotation.Substring(0, eIndex + 1); // Keep 'E'
                    string exponentPart = scientificNotation.Substring(eIndex + 1); // e.g., "-003" or "+013"

                    if (exponentPart.Length > 1 && exponentPart[0] == '-')
                    {
                        // Negative exponent: find first non-zero digit after '-'
                        var firstDigitIndex = 1; // Start checking after the '-'
                        while (firstDigitIndex < exponentPart.Length && exponentPart[firstDigitIndex] == '0')
                            firstDigitIndex++;
                        return mantissa + "-" + exponentPart.Substring(firstDigitIndex);
                    }

                    if (exponentPart.Length > 1 && exponentPart[0] == '+')
                    {
                        // Positive exponent: find first non-zero digit after '+'
                        var firstDigitIndex = 1; // Start checking after the '+'
                        while (firstDigitIndex < exponentPart.Length && exponentPart[firstDigitIndex] == '0')
                            firstDigitIndex++;
                        return mantissa + "+" + exponentPart.Substring(firstDigitIndex);
                    }
                    // If there's no sign or it's already a single digit exponent (e.g., E0, E1, E-1)
                    // or if it's already in the desired format (e.g., E12), return as is.
                }

                return scientificNotation; // Fallback if parsing fails or no 'E' found
            }

            // 3. Normal case: rounded with thousands separator
            string format = "#,0." + new string('0', decimalPlaces);
            return value.ToString(format, CultureInfo.InvariantCulture);
        }

        public static string toPercentage<T>(this T value, int decimalPlaces = 2) where T : IConvertible
        {
            double val = value.ToDouble(null) * 100;
            if (double.IsNaN(val) || double.IsInfinity(val))
                return $"{val}";
            return SmartToString(val, decimalPlaces) + "%";
        }

        public static string toCurrency<T>(this T value, uint decimalPlaces = 2) where T : IConvertible
        {
            var val = value.ToDecimal(null);
            bool isPositive = Math.Sign(val) >= 0;
            val = Math.Abs(val);
            var appender = new StringBuilder();

            foreach (KeyValuePair<char, decimal> kvp in largeNumbers)
                if (val >= 10m * kvp.Value)
                {
                    val /= kvp.Value;
                    appender.Append(kvp.Key);
                    break;
                }

            if (isPositive)
                return $"{val.ToString($"C{decimalPlaces}")}{appender}";
            return $"({val.ToString($"C{decimalPlaces}")}{appender})";
        }

        public static string toTime<T>(this T time) where T : struct, IComparable, IConvertible, IFormattable
        {
            var value = time.ToDouble(null);
            if (value > TimeSpan.MaxValue.TotalSeconds || value < TimeSpan.MinValue.TotalSeconds)
                return
                    $"TimeSpan Overflow protection, DEBUG - Recorded seconds: {value}, which is over {TimeSpan.MaxValue.TotalSeconds}";

            TimeSpan timeFormat = TimeSpan.FromSeconds(Convert.ToDouble(time));
            var strRes = timeFormat.ToString(@"hh\:mm\:ss\.fff");
            return strRes;
        }

        public static List<string> parseAnyRaw(this string strIn)
        {
            if (string.IsNullOrEmpty(strIn))
                return new List<string> { string.Empty };

            List<string> numericStrings = new();

            var numericString = string.Empty;
            var negativeCurrency = false;
            var lastWasNumber = false;
            for (var i = 0; i < strIn.Length; i++)
            {
                char charCurr = strIn[i];
                if (char.IsDigit(charCurr) || charCurr == '-' || charCurr == localeCurrencySymbol)
                {
                    numericString += charCurr;
                    lastWasNumber = true;
                    continue;
                }

                //-+-+-+-+-+-+-+-+
                // Check For Number Related Chars
                //-+-+-+-+-+-+-+-+
                if (lastWasNumber)
                {
                    if (charCurr == localeDecimalSeparator)
                    {
                        numericString += charCurr;
                        continue;
                    }

                    if (charCurr == localeThousandSeparator)
                    {
                        numericString += charCurr;
                        continue;
                    }

                    if (charCurr == '%')
                    {
                        numericString += charCurr;
                        lastWasNumber = false;
                        continue;
                    }

                    if (charCurr == 'E')
                    {
                        numericString += charCurr;
                        continue;
                    }

                    if (charCurr == '(')
                    {
                        negativeCurrency = true;
                        numericString += charCurr;
                        continue;
                    }

                    if (charCurr == ')' && negativeCurrency)
                    {
                        numericString += charCurr;
                        lastWasNumber = false;
                        continue;
                    }

                    if (largeNumbers.ContainsKey(charCurr))
                    {
                        numericString += charCurr;
                        lastWasNumber = false;
                        continue;
                    }
                }

                if (numericString.Count() > 0)
                {
                    if (negativeCurrency && numericString.Last() != ')')
                        numericString.Replace("(", "");

                    numericStrings.Add(numericString);
                    numericString = string.Empty;

                    negativeCurrency = false;
                    lastWasNumber = false;
                }
            }

            if (numericString.Count() > 0)
            {
                if (negativeCurrency && numericString.Last() != ')')
                    numericString.Replace("(", "");

                numericStrings.Add(numericString);
                numericString = string.Empty;

                negativeCurrency = false;
                lastWasNumber = false;
            }

            return numericStrings;
        }

        public static List<double> parseAny(this string strIn)
        {
            List<double> numericValues = new();

            foreach (string str in strIn.parseAnyRaw())
            {
                if (str == "-")
                    continue;
                if (string.IsNullOrEmpty(str))
                    numericValues.Add(0);
                else if (largeNumbers.ContainsKey(str.Last()))
                    numericValues.Add(double.Parse(str.Substring(0, str.Length - 1), numStyle) *
                                      (double)largeNumbers[str.Last()]);
                else
                    numericValues.Add(double.Parse(str, numStyle));
            }

            return numericValues;
        }

        public static string BoxPlotRepresentation(double Q1, double Q2, double Q3, Func<double, string> stringFormat,
            double min = double.MinValue, double max = double.MaxValue)
        {
            return
                $"{stringFormat(Math.Max(min, Q1 - 1.5 * (Q3 - Q1)))} <- [{stringFormat(Q1)} | {stringFormat(Q2)} | {stringFormat(Q3)}] -> {stringFormat(Math.Min(max, Q3 + 1.5 * (Q3 - Q1)))}";
        }

        public static string BoxPlotRepresentation(IReadOnlyList<double> Qs, Func<double, string> stringFormat,
            double min = double.MinValue, double max = double.MaxValue)
        {
            return BoxPlotRepresentation(Qs[0], Qs[1], Qs[2], stringFormat, min, max);
        }

        public static string BoxPlotRepresentation((double x, double y, double z) Qs, Func<double, string> stringFormat,
            double min = double.MinValue, double max = double.MaxValue)
        {
            return BoxPlotRepresentation(Qs.x, Qs.y, Qs.z, stringFormat, min, max);
        }
    }
}