using System;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Statistics
{
  /// <summary>
  ///     <strong>Running Statistics Advanced</strong>
  ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
  ///     <br /> - Expanded version of <see cref="RunningStatistics" />.
  ///     <br /> - includes:
  ///     <list type="bullet">
  ///         <br /> <item><see cref="Skewness">Skewness</see>,</item>
  ///         <br /> <item><see cref="Kurtosis">Kurtosis</see>,</item>
  ///         <br /> <item><see cref="GeometricMean">Geometric Mean</see>,</item>
  ///         <br /> <item><see cref="HarmonicMean">Harmonic Mean</see>,</item>
  ///         <br />
  ///         <item>
  ///             <see cref="RootMeanSquare">Root Mean Square</see>
  ///         </item>
  ///     </list>
  ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
  /// </summary>
  public class RunningStatisticsAdvanced : RunningStatistics
  {
    /// <summary>
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     <br /> - Adds <paramref name="value" /> by <paramref name="count" /> times to the calculated statistics.
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// </summary>i
    /// <param name="value"></param>
    /// <param name="count"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Push(double value, uint count = 1)
    {
      double oldCount = _n;
      _n += count;
      double d = value - _m1;
      double s = d / _n * count;
      double s2 = s * s / count;
      double t = d * s * oldCount;

      _m1 += s;
      _m4 += t * s2 * (_n * _n - 3 * _n + 3) + 6 * s2 * _m2 - 4 * s * _m3;
      _m3 += t * s * (_n - 2) - 3 * s * _m2;
      _m2 += t;

      _h += 1.0 / value * count;
      _g += Math.Log(value) * count;
      _r += (value * value - _r) * count / _n;

      // Update Max Min
      _max = value > _max ? value : _max;
      _min = value < _min ? value : _min;
    }

    /// <summary>
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     <br /> - Combines with another <see cref="RunningStatisticsAdvanced" />.
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// </summary>
    /// <param name="stats"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(RunningStatisticsAdvanced stats)
    {
      long n = _n + stats._n;
      double d = stats._m1 - _m1;
      double d2 = d * d;
      double d3 = d2 * d;
      double d4 = d2 * d2;

      double m1 = (_n * _m1 + stats._n * stats._m1) / n;
      double m2 = _m2 + stats._m2 + d2 * _n * stats._n / n;
      double m3 =
        _m3
        + stats._m3
        + d3 * _n * stats._n * (_n - stats._n) / (n * n)
        + 3 * d * (_n * stats._m2 - stats._n * _m2) / n;
      double m4 =
        _m4
        + stats._m4
        + d4 * _n * stats._n * (_n * _n - _n * stats._n + stats._n * stats._n) / (n * n * n)
        + 6 * d2 * (_n * _n * stats._m2 + stats._n * stats._n * _m2) / (n * n)
        + 4 * d * (_n * stats._m3 - stats._n * _m3) / n;

      // Update Max Min
      _max = stats._max > _max ? stats._max : _max;
      _min = stats._min < _min ? stats._min : _min;

      _n = n;
      _m1 = m1;
      _m2 = m2;
      _m3 = m3;
      _m4 = m4;
    }

    //-+-+-+-+-+-+-+-+
    // Internal Calculation Containers
    //-+-+-+-+-+-+-+-+
    #region Internal Calucaltion Containers
    protected double _m3;
    protected double _m4;

    protected double _g;
    protected double _h;

    protected double _r;
    //-+-+-+-+-+-+-+-+
    #endregion
    //-+-+-+-+-+-+-+-+
    // Public Readables
    //-+-+-+-+-+-+-+-+
    #region Public Readables
    /// <inheritdoc cref="TotalStatistics.Skewness" />
    public double Skewness =>
      _n < 3
        ? double.NaN
        : _n * _m3 * Math.Sqrt(_m2 / (_n - 1)) / (_m2 * _m2 * (_n - 2)) * (_n - 1);

    /// <inheritdoc cref="TotalStatistics.Kurtosis" />
    public double Kurtosis =>
      _n < 4
        ? double.NaN
        : ((double)_n * _n - 1)
          / ((_n - 2) * (_n - 3))
          * (_n * _m4 / (_m2 * _m2) - 3 + 6.0 / (_n + 1));

    /// <inheritdoc cref="StreamingStatistics.GeometricMean" />
    public double GeometricMean => _n < 1 ? double.NaN : Math.Exp(_g / _n);

    /// <inheritdoc cref="StreamingStatistics.HarmonicMean" />
    public double HarmonicMean => _n < 1 ? double.NaN : _n / _h;

    /// <inheritdoc cref="StreamingStatistics.RootMeanSquare" />
    public double RootMeanSquare => _n < 1 ? double.NaN : Math.Sqrt(_r);

    //-+-+-+-+-+-+-+-+
    #endregion
  }
}
