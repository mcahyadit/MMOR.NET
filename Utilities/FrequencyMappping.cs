using System.Collections.Generic;

namespace MMOR.NET.Utilities {
  //================
  // Frequency Map Helpers
  // .. treats dictionary as a map of <Value, number of times Value occurs>
  // .. more intensive functions are in StatisticsLibrary.cs
  //================
  public static partial class Utilities {
    //================
    // Add Frequency
    //================
    public static void AddFrequency<T>(this IDictionary<T, uint> frequency_map, in T data,
        uint frequency = 1)
        where T : notnull {
      if (frequency_map.TryGetValue(data, out uint val))
        frequency_map[data] = val + frequency;
      else
        frequency_map.Add(data, frequency);
    }

    public static void AddFrequency<T>(this IDictionary<T, ulong> frequency_map, in T data,
        ulong frequency = 1)
        where T : notnull {
      if (frequency_map.TryGetValue(data, out ulong val))
        frequency_map[data] = val + frequency;
      else
        frequency_map.Add(data, frequency);
    }

    public static void AddFrequency<T>(this IDictionary<T, uint> frequency_map, in(T, uint)data)
        where T : notnull {
      frequency_map.AddFrequency(data.Item1, data.Item2);
    }

    public static void AddFrequency<T>(this IDictionary<T, ulong> frequency_map, in(T, ulong)data)
        where T : notnull {
      frequency_map.AddFrequency(data.Item1, data.Item2);
    }

    public static void AddFrequency<T>(this IDictionary<T, uint> frequency_map,
        in KeyValuePair<T, uint> data)
        where T : notnull {
      frequency_map.AddFrequency(data.Key, data.Value);
    }

    public static void AddFrequency<T>(this IDictionary<T, ulong> frequency_map,
        in KeyValuePair<T, ulong> data)
        where T : notnull {
      frequency_map.AddFrequency(data.Key, data.Value);
    }
    //================

    //================
    // Combine Frequency
    //================
    public static void CombineFrequency<T>(this IDictionary<T, uint> a,
        IReadOnlyDictionary<T, uint> b)
        where T : notnull {
      foreach ((T value, uint freq) in b) {
        a.AddFrequency(value, freq);
      }
    }

    public static void CombineFrequency<T>(this IDictionary<T, ulong> a,
        IReadOnlyDictionary<T, uint> b)
        where T : notnull {
      foreach ((T value, uint freq) in b) {
        a.AddFrequency(value, freq);
      }
    }

    public static void CombineFrequency<T>(this IDictionary<T, ulong> a,
        IReadOnlyDictionary<T, ulong> b)
        where T : notnull {
      foreach ((T value, ulong freq) in b) {
        a.AddFrequency(value, freq);
      }
    }

    //================
    // Conversion
    //================
    public static Dictionary<T, uint> ToFrequencyMap<T>(this IEnumerable<T> list)
        where T : notnull {
      var result = new Dictionary<T, uint>();
      foreach (T item in list) {
        result.AddFrequency(item);
      }
      return result;
    }

    public static SortedDictionary<T, uint> ToSortedFrequencyMap<T>(this IEnumerable<T> list)
        where T : notnull {
      var result = new SortedDictionary<T, uint>();
      foreach (T item in list) {
        result.AddFrequency(item);
      }
      return result;
    }
  }
}
