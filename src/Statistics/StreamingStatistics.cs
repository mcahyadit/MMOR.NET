using System;
using System.Collections.Generic;
using System.Linq;

namespace MMOR.Utils.Statistics
{
    /// <summary>
    ///     <strong>Streaming Statistics</strong>
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     <br /> - Statistics for <see cref="Array" /> and like.
    ///     <br /> - Utilizes <see cref="IEnumerable{T}" /> for memory and for-looping efficiency.
    ///     <br /> - Most algorithm are based on
    ///     <see href="https://en.wikipedia.org/wiki/Algorithms_for_calculating_variance">Welford's Online Algorithm</see>,
    ///     which is more numerically stable.
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     <br /> - Based on
    ///     <seealso href="https://github.com/mathnet/mathnet-numerics/blob/master/src/Numerics/">Math.NET.Numerics</seealso>.
    /// </summary>
    public static class StreamingStatistics
    {
        //-+-+-+-+-+-+-+-+
        // Generics
        //-+-+-+-+-+-+-+-+
        /// <inheritdoc cref="Mean" />
        public static double Mean<T>(this IEnumerable<T> stream) where T : IConvertible
        {
            return Mean(stream.Select(x => x.ToDouble(null)));
        }

        /// <inheritdoc cref="Variance" />
        public static double Variance<T>(this IEnumerable<T> stream, bool sample = true) where T : IConvertible
        {
            return Variance(stream.Select(x => x.ToDouble(null)), sample);
        }

        /// <inheritdoc cref="StandardDeviation" />
        public static double StandardDeviation<T>(this IEnumerable<T> stream, bool sample = true) where T : IConvertible
        {
            return StandardDeviation(stream.Select(x => x.ToDouble(null)), sample);
        }

        /// <inheritdoc cref="StandardError" />
        public static double StandardError<T>(this IEnumerable<T> stream, bool sample = true) where T : IConvertible
        {
            return StandardError(stream.Select(x => x.ToDouble(null)), sample);
        }

        /// <inheritdoc cref="Covariance" />
        public static double Covariance<T>(IEnumerable<T> streamX, IEnumerable<T> streamY, bool sample = true)
            where T : IConvertible
        {
            return Covariance(streamX.Select(x => x.ToDouble(null)), streamY.Select(x => x.ToDouble(null)), sample);
        }

        /// <inheritdoc cref="GeometricMean" />
        public static double GeometricMean<T>(this IEnumerable<T> stream) where T : IConvertible
        {
            return GeometricMean(stream.Select(x => x.ToDouble(null)));
        }

        /// <inheritdoc cref="HarmonicMean" />
        public static double HarmonicMean<T>(this IEnumerable<T> stream) where T : IConvertible
        {
            return HarmonicMean(stream.Select(x => x.ToDouble(null)));
        }

        /// <inheritdoc cref="RootMeanSquare" />
        public static double RootMeanSquare<T>(this IEnumerable<T> stream) where T : IConvertible
        {
            return RootMeanSquare(stream.Select(x => x.ToDouble(null)));
        }

        /// <inheritdoc cref="GiniCoefficient" />
        public static double GiniCoefficient<T>(this IEnumerable<T> stream) where T : IConvertible
        {
            return GiniCoefficient(stream.Select(x => x.ToDouble(null)));
        }

        /// <inheritdoc cref="Entropy" />
        public static double Entropy<T>(this IEnumerable<T> stream) where T : IConvertible
        {
            return Entropy(stream.Select(x => x.ToDouble(null)));
        }

        //-+-+-+-+-+-+-+-+
        // Core
        //-+-+-+-+-+-+-+-+

        #region Core
        public static double Mean(this IEnumerable<double> stream)
        {
            double mean = 0;
            long count = 0;
            foreach (double value in stream)
                mean += (value - mean) / ++count;
            return mean;
        }

        public static (double variance, ulong count) VarBaseAlgo(this IEnumerable<double> stream, bool sample = true)
        {
            double variance = 0;
            double sum = 0;
            ulong count = 0;

            using (IEnumerator<double> iterator = stream.GetEnumerator())
            {
                if (iterator.MoveNext())
                {
                    count++;
                    sum = iterator.Current;
                }

                while (iterator.MoveNext())
                {
                    count++;
                    double xi = iterator.Current;
                    sum += xi;
                    double diff = count * xi - sum;
                    variance += diff * diff / (count * (count - 1.0));
                }
            }

            return (count > 1 ? variance / (count - (sample ? 1.0 : 0)) : double.NaN, count);
        }

        public static double Variance(this IEnumerable<double> stream, bool sample = true)
        {
            return VarBaseAlgo(stream, sample).variance;
        }

        public static double StandardDeviation(this IEnumerable<double> stream, bool sample = true)
        {
            return Math.Sqrt(Variance(stream, sample));
        }

        public static double StandardError(this IEnumerable<double> stream, bool sample = true)
        {
            (double variance, ulong count) tmp = VarBaseAlgo(stream, sample);
            return tmp.count > 1 ? Math.Sqrt(tmp.variance / tmp.count) : double.NaN;
        }

        //-+-+-+-+-+-+-+-+
        #endregion

        //-+-+-+-+-+-+-+-+
        // Extra
        //-+-+-+-+-+-+-+-+

        #region Extra
        public static double Covariance(IEnumerable<double> streamX, IEnumerable<double> streamY, bool sample = true)
        {
            var count = 0;
            var mean1 = 0.0;
            var mean2 = 0.0;
            var comoment = 0.0;

            using (IEnumerator<double> s1 = streamX.GetEnumerator())
            using (IEnumerator<double> s2 = streamY.GetEnumerator())
            {
                while (s1.MoveNext())
                {
                    if (!s2.MoveNext()) throw new ArgumentException("All vectors must have the same dimensionality.");

                    double mean2Prev = mean2;
                    count++;
                    mean1 += (s1.Current - mean1) / count;
                    mean2 += (s2.Current - mean2) / count;
                    comoment += (s1.Current - mean1) * (s2.Current - mean2Prev);
                }

                if (s2.MoveNext()) throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            return count > 1 ? comoment / (count - (sample ? 1.0 : 0)) : double.NaN;
        }

        /// <summary>
        ///     <strong>Geometric Mean</strong>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - nth root of the product of all values.
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <code>
        ///     foreach(x in List&lt;double>)
        ///         product *= x;
        ///     return product ^ (1.0 / count);
        /// </code>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        public static double GeometricMean(this IEnumerable<double> stream)
        {
            double sum = 0;
            long count = 0;
            foreach (double value in stream)
            {
                sum += Math.Log(value);
                count++;
            }

            return Math.Exp(sum / count);
        }

        /// <summary>
        ///     <strong>Harmonic Mean</strong>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Reciprocal of the arithmetic mean of reciprocals.
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <code>
        ///     foreach(x in List&lt;double>)
        ///         sum += 1.0 / x;
        ///     return count / sum;
        /// </code>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        public static double HarmonicMean(this IEnumerable<double> stream)
        {
            double sum = 0;
            long count = 0;
            foreach (double value in stream)
            {
                sum += 1.0 / value;
                count++;
            }

            return count / sum;
        }

        /// <summary>
        ///     <strong>Root Mean Square</strong>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Square root of the mean of squared values.
        ///     <code>
        ///     foreach(x in List&lt;double>)
        ///         sum += x ^ 2;
        ///     return math.sqrt(sum / count);
        /// </code>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        public static double RootMeanSquare(this IEnumerable<double> stream)
        {
            double mean = 0;
            long count = 0;
            foreach (double value in stream)
                mean += (value * value - mean) / ++count;
            return Math.Sqrt(mean);
        }

        /// <summary>
        ///     <strong>Gini Coefficient / Gini Index</strong>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Metric of <i>in</i>equality, scales from 0.0 to 1.0.
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - e.g. in a datset of 20, with mean of 5.
        ///     <br /> - <strong>Gini Coefficient</strong> of 0.0 means all 20 data are valued at 5.
        ///     <br /> - <strong>Gini Coefficient</strong> of 1.0 means 19 of the data are valued at 0, while 1 data is valued at
        ///     100.
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        public static double GiniCoefficient(this IEnumerable<double> stream)
        {
            double total = 0;
            long count = 0;
            foreach (double value in stream)
            {
                total += value;
                count++;
            }

            var cumProportionV = 0.0;
            var giniSum = 0.0;
            var cumFrequency = 0.0;

            foreach (double value in stream)
            {
                double proportionV = value / total;
                double proportionF = ++cumFrequency / count;
                cumProportionV += proportionV;
                giniSum += proportionV * (2.0 * (count - cumFrequency + 0.5) / count);
            }

            return 1.0 - giniSum;
        }

        /// <summary>
        ///     <strong>
        ///         <seealso href="http://en.wikipedia.org/wiki/Shannon_entropy">Shannon's Entropy</seealso>
        ///     </strong>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Metric of uncertainty or randomness.
        ///     <br /> - The closer the <strong>Entropy</strong> to 0.0, the easier it is to predict the results.
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        public static double Entropy(this IEnumerable<double> stream)
        {
            var index = new Dictionary<double, double>();

            var totalCount = 0;
            foreach (double value in stream)
            {
                if (double.IsNaN(value)) return double.NaN;

                double currentValueCount;
                if (index.TryGetValue(value, out currentValueCount))
                    index[value] = ++currentValueCount;
                else
                    index.Add(value, 1);

                ++totalCount;
            }

            double entropy = 0;
            foreach (KeyValuePair<double, double> item in index)
            {
                double p = item.Value / totalCount;
                entropy += p * Math.Log(p, 2);
            }

            return -entropy;
        }

        //-+-+-+-+-+-+-+-+
        #endregion
    }
}