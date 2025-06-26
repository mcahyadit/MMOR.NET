using System;
using System.Collections.Generic;
using System.Linq;
using MMOR.Utils.Mathematics;

namespace MMOR.Utils.Statistics
{
    //-+-+-+-+-+-+-+-+-+
    // SOON TO BE DEPRECATED
    // ..please use the classes inside Statistics folder instead
    //-+-+-+-+-+-+-+-+-+
    public static class QuantileFinder
    {
        /// <summary>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Returns the <paramref name="tau" />th Quantile from <paramref name="list" />.
        ///     <br /> - Function assumes <paramref name="list" /> is <strong>Pre-Sorted</strong>.
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        public static double QuantileSorted(this IReadOnlyList<double> list, double tau)
        {
            // Clamps tau
            if (tau > 1 || tau < 0)
                tau = tau.Repeat(1d);

            // NaN on Empty
            int count = list.Count;
            if (count == 0) return double.NaN;

            // First Last
            if (MathExt.Approximately(tau, 0) || count == 1)
                return list[0];
            if (MathExt.Approximately(tau, 1))
                return list[count - 1];

            // Quantile Definition
            double h = (count + 1 / 3d) * tau + 1 / 3d;
            var hf = (int)h;

            if (hf < 1)
                return list[0];
            if (hf >= count)
                return list[count - 1];
            double top = list[hf];
            double bot = list[hf - 1];
            return bot + (h - hf) * (top - bot);
        }

        /// <summary>
        ///     <inheritdoc cref="QuantileSorted(IReadOnlyList{double}, double)" />
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="tau"></param>
        /// <returns></returns>
        public static double QuantileSorted<T>(this IReadOnlyList<T> list, double tau) where T : IConvertible
        {
            return QuantileSorted(list.Select(x => x.ToDouble(null)).ToList(), tau);
        }

        public static double QuantileSorted(this SortedDictionary<double, uint> map, double tau)
        {
            // Clamps tau
            if (tau > 1 || tau < 0)
                tau = tau.Repeat(1d);

            // NaN on Empty
            ulong count = 0;
            foreach (KeyValuePair<double, uint> kvp in map)
                count += kvp.Value;
            if (count == 0) return double.NaN;

            // First Last
            if (MathExt.Approximately(tau, 0) || count == 1)
                return map.First().Key;
            if (MathExt.Approximately(tau, 1))
                return map.Last().Key;

            // Quantile Definition
            double h = (count + 1 / 3d) * tau + 1 / 3d;
            var hf = (ulong)h;
            if (hf < 1)
                return map.First().Key;
            if (hf >= count)
                return map.Last().Key;

            double? top = null;
            double? bot = null;
            count = 0;
            foreach (KeyValuePair<double, uint> kvp in map)
            {
                count += kvp.Value;
                if (count >= hf) top ??= kvp.Key;
                if (count >= hf - 1) bot ??= kvp.Key;
                if (top.HasValue && bot.HasValue) break;
            }

            if (top.HasValue && bot.HasValue)
                return bot.Value + (h - hf) * (top.Value - bot.Value);
            return double.NaN;
        }

        public static double QuantileSorted<T>(this SortedDictionary<T, uint> map, double tau) where T : IConvertible
        {
            // Clamps tau
            if (tau > 1 || tau < 0)
                tau = tau.Repeat(1d);

            // NaN on Empty
            ulong count = 0;
            foreach (KeyValuePair<T, uint> kvp in map)
                count += kvp.Value;
            if (count == 0) return double.NaN;

            // First Last
            if (MathExt.Approximately(tau, 0) || count == 1)
                return map.First().Key.ToDouble(null);
            if (MathExt.Approximately(tau, 1))
                return map.Last().Key.ToDouble(null);

            // Quantile Definition
            double h = (count + 1 / 3d) * tau + 1 / 3d;
            var hf = (ulong)h;
            if (hf < 1)
                return map.First().Key.ToDouble(null);
            if (hf >= count)
                return map.Last().Key.ToDouble(null);

            double? top = null;
            double? bot = null;
            count = 0;
            foreach (KeyValuePair<T, uint> kvp in map)
            {
                count += kvp.Value;
                if (count >= hf) top ??= kvp.Key.ToDouble(null);
                if (count >= hf - 1) bot ??= kvp.Key.ToDouble(null);
                if (top.HasValue && bot.HasValue) break;
            }

            if (top.HasValue && bot.HasValue)
                return bot.Value + (h - hf) * (top.Value - bot.Value);
            return double.NaN;
        }

        /// <summary>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Returns the <paramref name="tau" />th Quantile from <paramref name="list" />.
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        public static double Quantile(this IReadOnlyList<double> list, double tau)
        {
            return QuantileSorted(list.OrderBy(x => x).ToList(), tau);
        }

        /// <summary>
        ///     <inheritdoc cref="Quantile(IReadOnlyList{double}, double)" />
        /// </summary>
        public static double Quantile<T>(this IReadOnlyList<T> list, double tau) where T : IConvertible
        {
            return QuantileSorted(list.Select(x => x.ToDouble(null)).OrderBy(x => x).ToList(), tau);
        }

        public static double Quantile(this IDictionary<double, uint> map, double tau)
        {
            return QuantileSorted(new SortedDictionary<double, uint>(map), tau);
        }

        public static double Quantile<T>(this IDictionary<T, uint> map, double tau) where T : IConvertible
        {
            return QuantileSorted(map.ConvertToSortedDouble(), tau);
        }
    }
}