using System;
using System.Collections.Generic;

namespace MMOR.NET.Statistics
{
  public static class IConvertibleHelper
  {
    public static void ConvertToSortedDouble<T>(
      IDictionary<T, uint> origin,
      IDictionary<double, uint> target
    )
      where T : IConvertible
    {
      target.Clear();
      foreach ((T value, uint freq) in origin)
        target.Add(value.ToDouble(null), freq);
    }

    public static SortedDictionary<double, uint> ConvertToSortedDouble<T>(
      this IDictionary<T, uint> origin
    )
      where T : IConvertible
    {
      var result = new SortedDictionary<double, uint>();
      ConvertToSortedDouble(origin, result);
      return result;
    }

    public static void ConvertToSortedDouble<T>(IList<T> origin, IList<double> target)
      where T : IConvertible
    {
      target.Clear();
      foreach (T value in origin)
        target.Add(value.ToDouble(null));
    }

    public static List<double> ConvertToSortedDouble<T>(this IList<T> origin)
      where T : IConvertible
    {
      var result = new List<double>();
      ConvertToSortedDouble(origin, result);
      result.Sort();
      return result;
    }
  }
}
