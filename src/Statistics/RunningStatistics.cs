using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MMOR.Utils.Statistics
{
    /// <summary>
    ///     <strong>Running Statistics</strong>
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     <br /> - Data Struct to caluclate statistics.
    ///     <br /> - Operates on Online/Running algortihms,
    ///     <br /> - i.e. can update the results for each new element without keeping track of the array.
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     <br /> - Based on
    ///     <seealso href="https://github.com/mathnet/mathnet-numerics/blob/master/src/Numerics/">Math.NET.Numerics</seealso>.
    ///     <br /> - Expanded for compability with our frequency mapping based data.
    /// </summary>
    public class RunningStatistics
    {
        public virtual void Clear()
        {
            _n = 0;
            _n0 = 0;

            _min = double.PositiveInfinity;
            _max = double.NegativeInfinity;

            _nM = 0;
            _m1 = 0;
            _m2 = 0;
        }

        /// <summary>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Adds <paramref name="value" /> by <paramref name="count" /> times to the calculated statistics.
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        /// <param name="value"></param>
        /// <param name="count"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Push(double value, uint count = 1)
        {
            double oldCount = _n;
            if (value == 0) _n0 += count;
            _n += count;
            double d = value - _m1;
            double s = d / _n * count;
            double t = d * s * oldCount;

            _m1 += s;
            _m2 += t;

            // Update Max Min
            if (value > _max)
            {
                _max = value;
                _nM = 0;
            }

            if (value == _max) _nM++;
            _min = value < _min ? value : _min;
        }

        /// <summary>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Adds all values in <paramref name="list" /> to the calculated statistics.
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        /// <param name="list"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(IEnumerable<double> list)
        {
            foreach (double value in list)
                Push(value);
        }

        /// <summary>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Adds all values in <paramref name="list" /> to the calculated statistics.
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push<T>(IEnumerable<T> list) where T : IConvertible { Push(list.Select(x => x.ToDouble(null))); }

        /// <summary>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Adds all values in value-frequency pairs of <paramref name="map" /> to the calculated statistics.
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        /// <param name="map"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(IEnumerable<(double, uint)> map)
        {
            foreach ((double, uint) kvp in map)
                Push(kvp.Item1, kvp.Item2);
        }

        /// <summary>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Adds all values in value-frequency pairs of <paramref name="map" /> to the calculated statistics.
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="map"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push<T>(IEnumerable<KeyValuePair<T, uint>> map) where T : IConvertible
        {
            Push(map.Select(x => (x.Key.ToDouble(null), x.Value)));
        }

        /// <summary>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Combines with another <see cref="RunningStatistics" />.
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        /// <param name="stats"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(RunningStatistics stats)
        {
            long n = _n + stats._n;
            double d = stats._m1 - _m1;
            double d2 = d * d;

            double m1 = (_n * _m1 + stats._n * stats._m1) / n;
            double m2 = _m2 + stats._m2 + d2 * _n * stats._n / n;

            // Update Max Min
            if (stats._max > _max)
            {
                _max = stats._max;
                _nM = stats._nM;
            }
            else if (stats._max == _max) { _nM += stats._nM; }

            _min = stats._min < _min ? stats._min : _min;

            _n = n;
            _n0 += stats._n0;
            _m1 = m1;
            _m2 = m2;
        }

        //-+-+-+-+-+-+-+-+
        // Internal Calculation Containers
        //-+-+-+-+-+-+-+-+
        #region Internal Calucaltion Containers
        protected long _n;
        protected long _n0;
        protected double _min = double.PositiveInfinity;
        protected double _max = double.NegativeInfinity;
        protected long _nM;
        protected double _m1;

        protected double _m2;
        //-+-+-+-+-+-+-+-+
        #endregion
        //-+-+-+-+-+-+-+-+
        // Public Readables
        //-+-+-+-+-+-+-+-+
        #region Public Readables
        public double Count => _n;
        public double Count0 => _n0;
        public double CountA => _n - _n0;
        public double Minimum => _n > 0 ? _min : double.NaN;
        public double Maximum => _n > 0 ? _max : double.NaN;
        public double CountMaximum => _nM;
        public double Sum => _m1 * _n;
        public double Mean => _n > 0 ? _m1 : double.NaN;
        public double Variance => _n < 2 ? double.NaN : _m2 / (_n - 1);
        public double StandardDeviation => _n < 2 ? double.NaN : Math.Sqrt(_m2 / (_n - 1));
        public double StandardError => _n < 2 ? double.NaN : Math.Sqrt(_m2 / (_n - 1) / _n);

        /// <summary>
        ///     <strong>Coefficient Of Variation</strong>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Ratio of <b>Standard Deviation</b> to <b>Mean</b>.
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Alternate Names:
        ///     <br /> - Relative <b>Standard Deviation</b>
        ///     <br /> - Normalized Room-Mean-Squared Deviation
        /// </summary>
        public double CoefficientOfVariation => _n < 1 ? double.NaN : Math.Sqrt(_m2 / (_n - 1)) / _m1;

        /// <summary>
        ///     <strong>Signal To Noise Ratio</strong>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Ratio of <b>Mean</b> to <b>Standard Deviation</b>.
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        public double SignalToNoiseRatio => _n < 1 ? double.NaN : _m1 / Math.Sqrt(_m2 / (_n - 1));

        //-+-+-+-+-+-+-+-+
        #endregion
    }
}