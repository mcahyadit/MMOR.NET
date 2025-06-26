using System;
using System.Collections.Generic;
using System.Linq;

namespace MMOR.Utils.Statistics
{
    /// <summary>
    ///     <strong>Streaming Statistics</strong>
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     <br /> - <see cref="StreamingStatistics" /> adjusted for frequency maps.
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// </summary>
    public static class StreamingMapStatistics
    {
        //-+-+-+-+-+-+-+-+
        // Generics
        //-+-+-+-+-+-+-+-+
        /// <inheritdoc cref="Mean" />
        public static double Mean<T>(this IEnumerable<(T, uint)> map) where T : IConvertible
        {
            return Mean(map.Select(x => (x.Item1.ToDouble(null), x.Item2)));
        }

        /// <inheritdoc cref="Variance" />
        public static double Variance<T>(this IEnumerable<(T, uint)> map, bool sample = true) where T : IConvertible
        {
            return Variance(map.Select(x => (x.Item1.ToDouble(null), x.Item2)), sample);
        }

        /// <inheritdoc cref="StandardDeviation" />
        public static double StandardDeviation<T>(this IEnumerable<(T, uint)> map, bool sample = true)
            where T : IConvertible
        {
            return StandardDeviation(map.Select(x => (x.Item1.ToDouble(null), x.Item2)), sample);
        }

        /// <inheritdoc cref="StandardError" />
        public static double StandardError<T>(this IEnumerable<(T, uint)> map, bool sample = true)
            where T : IConvertible
        {
            return StandardError(map.Select(x => (x.Item1.ToDouble(null), x.Item2)), sample);
        }

        /// <inheritdoc cref="GeometricMean" />
        public static double GeometricMean<T>(this IEnumerable<(T, uint)> map) where T : IConvertible
        {
            return GeometricMean(map.Select(x => (x.Item1.ToDouble(null), x.Item2)));
        }

        /// <inheritdoc cref="HarmonicMean" />
        public static double HarmonicMean<T>(this IEnumerable<(T, uint)> map) where T : IConvertible
        {
            return HarmonicMean(map.Select(x => (x.Item1.ToDouble(null), x.Item2)));
        }

        /// <inheritdoc cref="RootMeanSquare" />
        public static double RootMeanSquare<T>(this IEnumerable<(T, uint)> map) where T : IConvertible
        {
            return RootMeanSquare(map.Select(x => (x.Item1.ToDouble(null), x.Item2)));
        }

        /// <inheritdoc cref="GiniCoefficient" />
        public static double GiniCoefficient<T>(this IEnumerable<(T, uint)> map) where T : IConvertible
        {
            return GiniCoefficient(map.Select(x => (x.Item1.ToDouble(null), x.Item2)));
        }

        /// <inheritdoc cref="Entropy" />
        public static double Entropy<T>(this IEnumerable<(T, uint)> map) where T : IConvertible
        {
            return Entropy(map.Select(x => (x.Item1.ToDouble(null), x.Item2)));
        }

        /// <inheritdoc cref="Mean" />
        public static double Mean<T>(this IEnumerable<KeyValuePair<T, uint>> map) where T : IConvertible
        {
            return Mean(map.Select(x => (x.Key.ToDouble(null), x.Value)));
        }

        /// <inheritdoc cref="Variance" />
        public static double Variance<T>(this IEnumerable<KeyValuePair<T, uint>> map, bool sample = true)
            where T : IConvertible
        {
            return Variance(map.Select(x => (x.Key.ToDouble(null), x.Value)), sample);
        }

        /// <inheritdoc cref="StandardDeviation" />
        public static double StandardDeviation<T>(this IEnumerable<KeyValuePair<T, uint>> map, bool sample = true)
            where T : IConvertible
        {
            return StandardDeviation(map.Select(x => (x.Key.ToDouble(null), x.Value)), sample);
        }

        /// <inheritdoc cref="StandardError" />
        public static double StandardError<T>(this IEnumerable<KeyValuePair<T, uint>> map, bool sample = true)
            where T : IConvertible
        {
            return StandardError(map.Select(x => (x.Key.ToDouble(null), x.Value)), sample);
        }

        /// <inheritdoc cref="GeometricMean" />
        public static double GeometricMean<T>(this IEnumerable<KeyValuePair<T, uint>> map) where T : IConvertible
        {
            return GeometricMean(map.Select(x => (x.Key.ToDouble(null), x.Value)));
        }

        /// <inheritdoc cref="HarmonicMean" />
        public static double HarmonicMean<T>(this IEnumerable<KeyValuePair<T, uint>> map) where T : IConvertible
        {
            return HarmonicMean(map.Select(x => (x.Key.ToDouble(null), x.Value)));
        }

        /// <inheritdoc cref="RootMeanSquare" />
        public static double RootMeanSquare<T>(this IEnumerable<KeyValuePair<T, uint>> map) where T : IConvertible
        {
            return RootMeanSquare(map.Select(x => (x.Key.ToDouble(null), x.Value)));
        }

        /// <inheritdoc cref="GiniCoefficient" />
        public static double GiniCoefficient<T>(this IEnumerable<KeyValuePair<T, uint>> map) where T : IConvertible
        {
            return GiniCoefficient(map.Select(x => (x.Key.ToDouble(null), x.Value)));
        }

        /// <inheritdoc cref="Entropy" />
        public static double Entropy<T>(this IEnumerable<KeyValuePair<T, uint>> map) where T : IConvertible
        {
            return Entropy(map.Select(x => (x.Key.ToDouble(null), x.Value)));
        }

        //-+-+-+-+-+-+-+-+
        // Core
        //-+-+-+-+-+-+-+-+

        #region Core
        /// <inheritdoc cref="StreamingStatistics.Mean" />
        public static double Mean(this IEnumerable<(double, uint)> map)
        {
            double mean = 0;
            ulong count = 0;
            foreach ((double value, uint freq) in map)
            {
                count += freq;
                mean += (value - mean) * freq / count;
            }

            return mean;
        }

        private static (double variance, ulong count) VarBaseAlgo(this IEnumerable<(double, uint)> map,
            bool sample = true)
        {
            double variance = 0;
            double mean = 0;
            ulong count = 0;

            using (IEnumerator<(double, uint)> iterator = map.GetEnumerator())
            {
                if (iterator.MoveNext())
                {
                    (double, uint) firstIter = iterator.Current;
                    count += firstIter.Item2;
                    mean = firstIter.Item1 * firstIter.Item2;
                }

                while (iterator.MoveNext())
                {
                    (double xi, uint freq) = iterator.Current;
                    count += freq;
                    double delta = xi - mean;
                    mean += xi * freq / count;
                    double delta2 = xi - mean;
                    variance += delta * delta2 * freq;
                }
            }

            return (count > 1 ? variance / (count - (sample ? 1.0 : 0)) : double.NaN, count);
        }

        /// <inheritdoc cref="StreamingStatistics.Variance" />
        public static double Variance(this IEnumerable<(double, uint)> map, bool sample = true)
        {
            return VarBaseAlgo(map, sample).variance;
        }

        /// <inheritdoc cref="StreamingStatistics.StandardDeviation" />
        public static double StandardDeviation(this IEnumerable<(double, uint)> map, bool sample = true)
        {
            return Math.Sqrt(Variance(map, sample));
        }

        /// <inheritdoc cref="StreamingStatistics.StandardError" />
        public static double StandardError(this IEnumerable<(double, uint)> map, bool sample = true)
        {
            (double variance, ulong count) tmp = VarBaseAlgo(map, sample);
            return tmp.count > 1 ? Math.Sqrt(tmp.variance / tmp.count) : double.NaN;
        }

        //-+-+-+-+-+-+-+-+
        #endregion

        //-+-+-+-+-+-+-+-+
        // Extra
        //-+-+-+-+-+-+-+-+

        #region Extra
        /// <inheritdoc cref="StreamingStatistics.GeometricMean" />
        public static double GeometricMean(this IEnumerable<(double, uint)> map)
        {
            double sum = 0;
            ulong count = 0;
            foreach ((double value, uint freq) in map)
            {
                sum += Math.Log(value) * freq;
                count += freq;
            }

            return Math.Exp(sum / count);
        }

        /// <inheritdoc cref="StreamingStatistics.HarmonicMean" />
        public static double HarmonicMean(this IEnumerable<(double, uint)> map)
        {
            double sum = 0;
            ulong count = 0;
            foreach ((double value, uint freq) in map)
            {
                sum += 1.0 / value * freq;
                count += freq;
            }

            return count / sum;
        }

        /// <inheritdoc cref="StreamingStatistics.RootMeanSquare" />
        public static double RootMeanSquare(this IEnumerable<(double, uint)> map)
        {
            double mean = 0;
            ulong count = 0;
            foreach ((double value, uint freq) in map)
            {
                count += freq;
                mean += (value * value - mean) * freq / count;
            }

            return Math.Sqrt(mean);
        }

        /// <inheritdoc cref="StreamingStatistics.GiniCoefficient" />
        public static double GiniCoefficient(this IEnumerable<(double, uint)> map)
        {
            double total = 0;
            double count = 0;
            foreach ((double value, uint freq) in map)
            {
                total += value * freq;
                count += freq;
            }

            var cumProportionV = 0.0;
            var giniSum = 0.0;
            foreach ((double value, uint freq) in map)
            {
                double proportionV = value * freq / total;
                double proportionF = freq / count;
                cumProportionV += proportionV;
                giniSum += 2.0 * proportionF * cumProportionV - proportionF * proportionV;
            }

            return 1.0 - giniSum;
        }

        /// <inheritdoc cref="StreamingStatistics.Entropy" />
        public static double Entropy(this IEnumerable<(double, uint)> map)
        {
            double count = 0;
            foreach ((double _, uint freq) in map)
                count += freq;

            double entropy = 0;
            foreach ((double _, uint freq) in map)
            {
                double p = freq / count;
                entropy += p * Math.Log(p, 2);
            }

            return -entropy;
        }

        //-+-+-+-+-+-+-+-+
        #endregion
    }
}