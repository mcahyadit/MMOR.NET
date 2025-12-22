using System.Collections.Generic;

namespace MMOR.NET.Utilities
{
  //-+-+-+-+-+-+-+-+
  // Frequency Map Helpers
  // .. treats dictionary as a map of <Value, number of times Value occurs>
  // .. more intensive functions are in StatisticsLibrary.cs
  //-+-+-+-+-+-+-+-+
  public static partial class Utilities
  {
    //-+-+-+-+-+-+-+-+
    // Add Frequency
    //-+-+-+-+-+-+-+-+
    #region Add Frequency
    /// <summary>
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     <br /> - Increment the <paramref name="frequencyMap" />[<paramref name="data" />].<i>Value</i> by
    ///     <paramref name="frequency" /> if exists.
    ///     <br /> - Otherwise, adds <see cref="KeyValuePair" />(<paramref name="data" />, <paramref name="frequency" />) to
    ///     <paramref name="frequencyMap" />.
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// </summary>
    public static void AddFrequency<T>(
      this IDictionary<T, uint> frequencyMap,
      in T data,
      uint frequency = 1
    )
      where T : notnull
    {
      if (frequencyMap.TryGetValue(data, out uint tmpInt))
        frequencyMap[data] = tmpInt + frequency;
      else
        frequencyMap.Add(data, frequency);
    }

    /// <summary>
    ///     <inheritdoc cref="AddFrequency{T}(System.Collections.Generic.IDictionary{T,uint},T,uint)" />
    /// </summary>
    public static void AddFrequency<T>(
      this IDictionary<T, ulong> frequencyMap,
      in T data,
      ulong frequency = 1
    )
      where T : notnull
    {
      if (frequencyMap.TryGetValue(data, out ulong tmpInt))
        frequencyMap[data] = tmpInt + frequency;
      else
        frequencyMap.Add(data, frequency);
    }

    /// <summary>
    ///     <inheritdoc cref="AddFrequency{T}(System.Collections.Generic.IDictionary{T,uint},T,uint)" />
    /// </summary>
    public static void AddFrequency<T>(this IDictionary<T, uint> frequencyMap, in (T, uint) data)
      where T : notnull
    {
      frequencyMap.AddFrequency(data.Item1, data.Item2);
    }

    /// <summary>
    ///     <inheritdoc cref="AddFrequency{T}(System.Collections.Generic.IDictionary{T,uint},T,uint)" />
    /// </summary>
    public static void AddFrequency<T>(this IDictionary<T, ulong> frequencyMap, in (T, ulong) data)
      where T : notnull
    {
      frequencyMap.AddFrequency(data.Item1, data.Item2);
    }

    /// <summary>
    ///     <inheritdoc cref="AddFrequency{T}(System.Collections.Generic.IDictionary{T,uint},T,uint)" />
    /// </summary>
    public static void AddFrequency<T>(
      this IDictionary<T, uint> frequencyMap,
      in KeyValuePair<T, uint> data
    )
      where T : notnull
    {
      frequencyMap.AddFrequency(data.Key, data.Value);
    }

    /// <summary>
    ///     <inheritdoc cref="AddFrequency{T}(System.Collections.Generic.IDictionary{T,uint},T,uint)" />
    /// </summary>
    public static void AddFrequency<T>(
      this IDictionary<T, ulong> frequencyMap,
      in KeyValuePair<T, ulong> data
    )
      where T : notnull
    {
      frequencyMap.AddFrequency(data.Key, data.Value);
    }

    //-+-+-+-+-+-+-+-+
    #endregion

    //-+-+-+-+-+-+-+-+
    // Combine Frequency
    //-+-+-+-+-+-+-+-+
    #region Combine Frequency
    /// <summary>
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     <br /> - Increment the <paramref name="a" />[<b>data</b>].<i>Value</i> by <paramref name="b" />[<b>data</b>].
    ///     <i>Value</i> if exists.
    ///     <br /> - Otherwise, adds <paramref name="b" />[<b>data</b>] to <paramref name="a" />.
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// </summary>
    public static void CombineFrequency<T>(
      this IDictionary<T, uint> a,
      IReadOnlyDictionary<T, uint> b
    )
      where T : notnull
    {
      foreach ((T value, uint freq) in b)
        a.AddFrequency(value, freq);
    }

    /// <summary>
    ///     <inheritdoc
    ///         cref="CombineFrequency{T}(System.Collections.Generic.IDictionary{T,uint},System.Collections.Generic.IReadOnlyDictionary{T,uint})" />
    /// </summary>
    public static void CombineFrequency<T>(
      this IDictionary<T, ulong> a,
      IReadOnlyDictionary<T, uint> b
    )
      where T : notnull
    {
      foreach ((T value, uint freq) in b)
        a.AddFrequency(value, freq);
    }

    /// <summary>
    ///     <inheritdoc
    ///         cref="CombineFrequency{T}(System.Collections.Generic.IDictionary{T,uint},System.Collections.Generic.IReadOnlyDictionary{T,uint})" />
    /// </summary>
    public static void CombineFrequency<T>(
      this IDictionary<T, ulong> a,
      IReadOnlyDictionary<T, ulong> b
    )
      where T : notnull
    {
      foreach ((T value, ulong freq) in b)
        a.AddFrequency(value, freq);
    }
    #endregion

    //-+-+-+-+-+-+-+-+
    // Conversion
    //-+-+-+-+-+-+-+-+
    #region Conversion
    /// <summary>
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     <br /> - Converts Linear <see cref="List{T}" />/<see cref="Array" /> to a Frequency Map.
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// </summary>
    public static Dictionary<T, uint> ToFrequencyMap<T>(this IEnumerable<T> list)
      where T : notnull
    {
      var result = new Dictionary<T, uint>();
      foreach (T item in list)
        result.AddFrequency(item);
      return result;
    }

    /// <summary>
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     <br /> - Converts Linear <see cref="List{T}" />/<see cref="Array" /> to a Sorted Frequency Map.
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// </summary>
    public static SortedDictionary<T, uint> ToSortedFrequencyMap<T>(this IEnumerable<T> list)
      where T : notnull
    {
      var result = new SortedDictionary<T, uint>();
      foreach (T item in list)
        result.AddFrequency(item);
      return result;
    }
    #endregion
  }
}
