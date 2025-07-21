using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MMOR.Utils.Mathematics;
using MMOR.Utils.Utilities;

namespace MMOR.Utils.Statistics
{
    /// <summary>
    ///     <strong>Total Statistics</strong>
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     <br /> - Single Time Calculation to generate all Descriptive Statistics.
    ///     <br /> - Supports Quantiles and other metrics unsupported by <see cref="RunningStatistics" />.
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// </summary>
    public class TotalStatistics
    {
        protected static HashSet<double> _t = new()
            { 0.0215, 0.0887, 0.10, 0.125, 0.25, 0.5, 0.75, 0.875, 0.90, 0.9113, 0.9785 };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected TotalStatistics(IEnumerable<KeyValuePair<double, ulong>> map)
        {
            FirstPassLoop(map);
            IOrderedEnumerable<KeyValuePair<double, ulong>> _oMap = map.OrderBy(x => x.Key);
            SecondPassLoop(_oMap);
            ThirdPassLoop(_oMap);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected TotalStatistics(IEnumerable<double> list)
        {
            IOrderedEnumerable<KeyValuePair<double, ulong>> _oMap = FirstPassLoopList(list).OrderBy(x => x.Key);
            SecondPassLoop(_oMap);
            ThirdPassLoop(_oMap);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TotalStatistics Calculate(IEnumerable<KeyValuePair<double, ulong>> map) => new(map);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TotalStatistics Calculate(IEnumerable<double> list) => new(list);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TotalStatistics Calculate<T>(IEnumerable<KeyValuePair<T, uint>> map) where T : IConvertible
        {
            return new TotalStatistics(map.Select(x => new KeyValuePair<double, ulong>(x.Key.ToDouble(null), x.Value)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TotalStatistics Calculate<T>(IEnumerable<KeyValuePair<T, ulong>> map) where T : IConvertible
        {
            return new TotalStatistics(map.Select(x => new KeyValuePair<double, ulong>(x.Key.ToDouble(null), x.Value)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TotalStatistics Calculate<T>(IEnumerable<T> list) where T : IConvertible
        {
            return new TotalStatistics(list.Select(x => x.ToDouble(null)));
        }

        //-+-+-+-+-+-+-+-+
        // Internal Calculation Containers
        //-+-+-+-+-+-+-+-+

        #region Internal Calucaltion Containers
        protected ulong _n;
        protected ulong _n0;
        protected double _nM;
        protected double _mode;
        protected double _min = double.PositiveInfinity;
        protected double _max = double.NegativeInfinity;

        protected double _m;
        protected double _v;

        protected double _nS;
        protected double _vS;

        protected double _ad;
        protected double _sk;
        protected double _kr;

        protected double _h;
        protected double _g;
        protected double _r;

        protected Dictionary<double, double> _q = new();
        protected double _iqr;
        protected double _fL;
        protected double _fU;
        protected ulong _nO;

        protected double _et;
        protected double _gi;
        protected double _hhi;
        protected double _ti;

        protected double _sdi;
        //-+-+-+-+-+-+-+-+
        #endregion

        //-+-+-+-+-+-+-+-+
        // Public Readables
        //-+-+-+-+-+-+-+-+

        #region Public Readables
        public double Count => _n;
        public ulong CountUL => _n;

        /// <summary>
        ///     Total Count of 0s in Data
        /// </summary>
        public double Count0 => _n0;

        public ulong Count0UL => _n0;

        /// <summary>
        ///     Total Count of non-0s in Data
        /// </summary>
        public double CountA => _n - _n0;

        public ulong CountAUL => _n - _n0;

        public double Mode => _n > 0 ? _mode : double.NaN;
        public double Minimum => _n > 0 ? _min : double.NaN;
        public double Maximum => _n > 0 ? _max : double.NaN;

        public double Sum => _n > 0 ? _m * _n : double.NaN;
        public double Mean => _n > 0 ? _m : double.NaN;
        public double Variance => _n < 2 ? double.NaN : _v / (_n - 1);
        public double StandardDeviation => _n < 2 ? double.NaN : Math.Sqrt(_v / (_n - 1));
        public double StandardError => _n < 2 ? double.NaN : Math.Sqrt(_v / (_n - 1) / _n);

        /// <summary>
        ///     <strong>Coefficient Of Variation</strong>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Ratio of <b>Standard Deviation</b> to <b>Mean</b>.
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Alternate Names:
        ///     <br /> - Relative <b>Standard Deviation</b>
        ///     <br /> - Normalized Room-Mean-Squared Deviation
        /// </summary>
        public double CoefficientOfVariation => _n < 1 ? double.NaN : Math.Sqrt(_v / (_n - 1)) / _m;

        /// <summary>
        ///     <strong>Signal To Noise Ratio</strong>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Ratio of <b>Mean</b> to <b>Standard Deviation</b>.
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        public double SignalToNoiseRatio => _n < 1 ? double.NaN : _m / Math.Sqrt(_v / (_n - 1));

        /// <summary>
        ///     <strong>Average Absolute Deviation</strong>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Similar to Variance, but takes absolute instead of squared of the deviations.
        ///     <code>
        ///     foreach(x in List&lt;double>)
        ///         sum += math.abs(x - mean);
        ///     return sum / count;
        /// </code>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Alternate Names:
        ///     <br /> - Mean Absolute Deviation
        /// </summary>
        public double AverageAbsoluteDeviation => _n < 1 ? double.NaN : _ad;

        public double SemiDeviation => _nS < 2 ? double.NaN : Math.Sqrt(_vS / (_nS - 1));

        /// <summary>
        ///     <strong>
        ///         <see href="https://www.scribbr.com/statistics/skewness/">Skewness</see>
        ///     </strong>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Measurement of asymetry of the distribution.
        ///     <br /> - 0 Skewness would mean Mean = Median.
        ///     <br /> - + Skewness would mean Mean > Median.
        ///     <br /> - - Skewness would mean Mean &lt; Median.
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        public double Skewness =>
            _n < 3 ? double.NaN : _n * _sk * Math.Sqrt(_v / (_n - 1)) / (_v * _v * (_n - 2)) * (_n - 1);

        /// <summary>
        ///     <strong>
        ///         <see href="https://www.scribbr.com/statistics/kurtosis/">Kurtosis</see>
        ///     </strong>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Measurement of <i>Tailedness</i> of the distribution.
        ///     <br /> - <i>Tailedness</i> refers to how much of the data are outliers.
        ///     <br /> - <i>Tailedness</i> are measured relative to Normal Distribution.
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        public double Kurtosis =>
            _n < 4
                ? double.NaN
                : ((double)_n * _n - 1) / ((_n - 2) * (_n - 3)) * (_n * _kr / (_v * _v) - 3 + 6.0 / (_n + 1));

        /// <inheritdoc cref="StreamingStatistics.GeometricMean" />
        public double GeometricMean => _n < 1 ? double.NaN : Math.Exp(_g / _n);

        /// <inheritdoc cref="StreamingStatistics.HarmonicMean" />
        public double HarmonicMean => _n < 1 ? double.NaN : _n / _h;

        /// <inheritdoc cref="StreamingStatistics.RootMeanSquare" />
        public double RootMeanSquare => _n < 1 ? double.NaN : Math.Sqrt(_r);

        public Dictionary<double, double> Quantiles => _q;
        public double Median => _q.GetValueOrDefault(.5, double.NaN);

        /// <summary>
        ///     <strong>Q1</strong>
        ///     <br /> -+-+-+-+-+-+-+-+-+
        ///     <br /> - First Quartile
        ///     <br /> - 25th Percentile
        ///     <br /> - 0.25 Quantile
        ///     <br /> -+-+-+-+-+-+-+-+-+
        /// </summary>
        public double Q1 => _q.GetValueOrDefault(.25, double.NaN);

        /// <summary>
        ///     <strong>Q3</strong>
        ///     <br /> -+-+-+-+-+-+-+-+-+
        ///     <br /> - Third Quartile
        ///     <br /> - 75th Percentile
        ///     <br /> - 0.75 Quantile
        ///     <br /> -+-+-+-+-+-+-+-+-+
        /// </summary>
        public double Q3 => _q.GetValueOrDefault(.75, double.NaN);

        public (double Q1, double Q2, double Q3) Qs => new(Q1, Median, Q3);

        /// <summary>
        ///     <strong>Lower Fence</strong>
        ///     <br /> -+-+-+-+-+-+-+-+-+
        ///     <br /> - Lower boundary for detecting outliers.
        ///     <br /> - <c>Q1 - 1.5 * IQR</c>
        ///     <br /> -+-+-+-+-+-+-+-+-+
        /// </summary>
        public double LowerFence => _fL;

        /// <summary>
        ///     <strong>Upper Fence</strong>
        ///     <br /> -+-+-+-+-+-+-+-+-+
        ///     <br /> - Upper boundary for detecting outliers.
        ///     <br /> - <c>Q3 + 1.5 * IQR</c>
        ///     <br /> -+-+-+-+-+-+-+-+-+
        /// </summary>
        public double UpperFence => _fU;

        /// <summary>
        ///     <strong>Inter Quartile Range</strong>
        ///     <br /> -+-+-+-+-+-+-+-+-+
        ///     <br /> - Difference between Q3 and Q1.
        ///     <br /> -+-+-+-+-+-+-+-+-+
        /// </summary>
        public double InterQuartileRange => _iqr;

        public double OutlierCount => _nO;

        /// <inheritdoc cref="StreamingStatistics.Entropy" />
        public double Entropy => -_et;

        /// <inheritdoc cref="StreamingStatistics.GiniCoefficient" />
        public double GiniCoefficient => 1.0 - _gi;

        public double HerfindahlHirschmanIndex => _hhi;
        public double TheilIndex => _n < 1 ? double.NaN : _ti / _n;
        public double SimpsonDiversityIndex => _n < 1 ? double.NaN : 1 - _sdi;
        #endregion

        //-+-+-+-+-+-+-+-+
        // Calculation
        //-+-+-+-+-+-+-+-+

        #region Calculation
        protected virtual void FirstPassLoop(IEnumerable<KeyValuePair<double, ulong>> map)
        {
            foreach ((double value, ulong freq) in map)
            {
                // Update Count
                ulong _nPrev = _n;
                if (value == 0)
                    _n0 += freq;
                _n += freq;

                // Run Mean and Variance
                double delta = value - _m;
                double m = delta / _n * freq;
                double m2 = m * m / freq;
                double v = delta * delta * _nPrev * freq / _n;
                _m += m;
                _kr += v * m2 * (_n * _n - 3 * _n + 3) + 6 * m2 * _v - 4 * m * _sk;
                _sk += v * m * (_n - 2) - 3 * m * _v;
                _v += delta * delta * _nPrev * freq / _n;

                _h += 1.0 / value * freq;
                _g += Math.Log(value) * freq;
                _r += (value * value - _r) * freq / _n;

                // Update Max Min
                if (freq > _nM)
                {
                    _nM = freq;
                    _mode = value;
                }

                _max = value > _max ? value : _max;
                _min = value < _min ? value : _min;
            }
        }

        protected virtual IEnumerable<KeyValuePair<double, ulong>> FirstPassLoopList(IEnumerable<double> list)
        {
            Dictionary<double, ulong> _map = new();
            foreach (double value in list)
            {
                // Update Count
                ulong _nPrev = _n;
                if (value == 0)
                    _n0++;
                _n++;

                // Run Mean and Variance
                double delta = value - _m;
                double m = delta / _n;
                double m2 = m * m;
                double v = delta * m * _nPrev;
                _m += m;
                _kr += v * m2 * (_n * _n - 3 * _n + 3) + 6 * m2 * _v - 4 * m * _sk;
                _sk += v * m * (_n - 2) - 3 * m * _v;
                _v += v;

                _h += 1.0 / value;
                _g += Math.Log(value);
                _r += (value * value - _r) / _n;

                // Update Max Min
                _map.AddFrequency(value);
                _max = value > _max ? value : _max;
                _min = value < _min ? value : _min;
            }

            _mode = _map.Select(kvp => kvp.Key).Max();
            return _map;
        }

        protected virtual void SecondPassLoop(IEnumerable<KeyValuePair<double, ulong>> map)
        {
            ulong reCount = 0;
            var cumProportionV = 0.0;
            double sum = _m * _n;

            double mS = 0;

            foreach ((double value, ulong freq) in map)
            {
                // Quantiles Calculator
                reCount += freq;
                foreach (double tau in _t)
                    if (!_q.ContainsKey(tau) && reCount > tau * _n)
                        _q.Add(tau, value);

                double proportionV = value / sum * freq;
                double proportionF = (double)freq / _n;
                cumProportionV += proportionV;
                _gi += 2.0 * proportionF * cumProportionV - proportionF * proportionV;
                _et += proportionF * MathExt.Log2(proportionF);
                _hhi += proportionV * proportionV / freq;
                double ratio = value / _m;
                _ti += freq * ratio * Math.Log(ratio);
                _sdi += proportionF * (freq - 1.0) / (_n - 1);

                _ad += Math.Abs(value - _m) * freq / _n;

                // Semi Deviation
                if (value < _m)
                {
                    _nS += freq;
                    double delta = value - mS;
                    mS += value * freq / _nS;
                    double delta2 = value - mS;
                    _vS += delta * delta2 * freq;
                }
            }

            if (_q.ContainsKey(.75) && _q.ContainsKey(.25))
            {
                _iqr = _q[.75] - _q[.25];
                _fL = Math.Max(_min, _q[.25] - 1.5 * _iqr);
                _fU = Math.Min(_max, _q[.75] + 1.5 * _iqr);
            }
        }

        protected virtual void ThirdPassLoop(IEnumerable<KeyValuePair<double, ulong>> map)
        {
            foreach ((double value, ulong freq) in map) _nO += value < _fL || value > _fU ? freq : 0;
        }

        //-+-+-+-+-+-+-+-+
        #endregion
    }
}