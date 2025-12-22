using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MMOR.NET.Mathematics;
using MMOR.NET.Random;

namespace MMOR.NET.Utilities {
  //-+-+-+-+-+-+-+-+
  // Array Helpers
  //-+-+-+-+-+-+-+-+
  public static partial class Utilities {
    // ulong didn't have a built-in Sum LINQ, possibly since if you are summing such big number, it
    // bound to overflow
    public static ulong Sum(this IEnumerable<ulong> arr) => arr.IsNullOrEmpty()
                                                                ? 0
                                                                : arr.Aggregate(
                                                                      (sum, val) => sum + val);

    public static void Reverse<T>(IList<T> list, int begin, int end) {
      while (begin < end) {
        (list[begin], list[end]) = (list[end], list[begin]);
        begin++;
        end--;
      }
    }

    //-+-+-+-+-+-+-+-+
    // Null || Empty Check
    //-+-+-+-+-+-+-+-+
    public static bool IsNullOrEmpty<T>(
        [NotNullWhen(false)] this IEnumerable<T>? arr) => arr == null || arr.Count() <= 0;

    public static bool IsNullOrEmpty(
        [NotNullWhen(false)] this string str) => string.IsNullOrWhiteSpace(str);

    //-+-+-+-+-+-+-+-+
    // Creation
    //-+-+-+-+-+-+-+-+
    public static List<T> InitList<T>(int length, T value = default)
        where T : struct => Enumerable.Repeat(value, length).ToList();

    public static List<T> InitList<T>(int length, Func<T> constructor)
        where T : class => Enumerable.Range(0,
                  length).Select(
                  _ => constructor.Invoke()).ToList();

    public static T[] InitArr<T>(int length, T value = default)
        where T : struct => Enumerable.Repeat(value, length).ToArray();

    public static T[] InitArr<T>(int length, Func<T> constructor)
        where T : class => Enumerable.Range(0,
                  length).Select(
                  _ => constructor.Invoke()).ToArray();

    //-+-+-+-+-+-+-+-+
    // Fill & Clear
    //-+-+-+-+-+-+-+-+
    public static void Clear<T>(this T[] inArr) {
      Array.Clear(inArr, 0, inArr.Length);
    }

    public static void Fill<T>(this IList<T> list, T fillValue)
        where T : struct {
      int len = list.Count;
      for (var i = 0; i < len; i++) list[i] = fillValue;
    }

    public static void Fill<T>(this IList<T> list, Func<T> fillConstructor)
        where T : class {
      int len = list.Count;
      for (var i = 0; i < len; i++) list[i] = fillConstructor.Invoke();
    }

    public static void ConvertElements<T>(this IList<T> list, Func<IList<T>, int, T> function) {
      int len = list.Count;
      for (var i = 0; i < len; i++) list[i] = function(list, i);
    }

    public static void ConvertElements<TKey, TValue>(this IDictionary<TKey, TValue> dict,
        Func<IDictionary<TKey, TValue>, TKey, TValue> function) {
      foreach (TKey key in dict.Keys) dict[key] = function(dict, key);
    }

    /// <summary>
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     <br /> - Based of
    ///     <see
    ///     href="https://www.geeksforgeeks.org/shuffle-a-given-array-using-fisher-yates-shuffle-algorithm/">
    ///         Fisher-Yates
    ///         Shuffle Algorithm
    ///     </see>
    ///     .
    ///     <br /> - Takes optional <see cref="IRandom" /> for custom RNGs.
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// </summary>
    public static void ShuffleInplace<T>(this IList<T> list, IRandom? rng = null) {
      rng     ??= PCG.global;
      int len   = list.Count;

      for (int indexCurrent = len - 1; indexCurrent > 0; indexCurrent--) {
        int indexRand                         = rng.NextInt(0, indexCurrent + 1);
        (list[indexCurrent], list[indexRand]) = (list[indexRand], list[indexCurrent]);
      }
    }

    /// <summary>
    ///     <inheritdoc cref="ShuffleInplace" />
    ///     <br /> - This one returns a copy instead of shuffling the original.
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// </summary>
    public static IList<T> Shuffle<T>(this IReadOnlyList<T> list, IRandom? rng = null) {
      List<T> outList = list.ToList();
      outList.ShuffleInplace(rng);
      return outList;
    }

    //-+-+-+-+-+-+-+-+
    // Safe Getter
    //-+-+-+-+-+-+-+-+
    /// <summary>
    ///     <strong>CustomLibrary.SafeGet&lt;List>()</strong>
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     <br /> - Acts a normal <b><paramref name="list" />[<paramref name="index" />]</b> when
    ///     valid. <br /> - If <b>
    ///         <paramref name="index" />
    ///     </b>
    ///     is out of range:
    ///     <br /> - On
    ///     <b>
    ///         <paramref name="clamps" />
    ///     </b>
    ///     == <see langword="true" />, returns the index clamped into range.
    ///     <br /> - On
    ///     <b>
    ///         <paramref name="clamps" />
    ///     </b>
    ///     == <see langword="false" />, index will roll-over based on range.
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// </summary>
    public static T SafeGet<T>(this IEnumerable<T> list, int index, bool clamps = false) {
      switch (list) {
        case IReadOnlyList<T> roList:
          return clamps ? roList[Math.Clamp(index, 0, roList.Count - 1)]
                        : roList[index.Repeat(roList.Count)];
        case IList<T> mList:
          return clamps ? mList[Math.Clamp(index, 0, mList.Count - 1)]
                        : mList[index.Repeat(mList.Count)];
        default: {
          // fallback: enumerate (avoid if possible)
          T[] array = list.ToArray();
          return clamps ? array[Math.Clamp(index, 0, array.Length - 1)]
                        : array[index.Repeat(array.Length)];
        }
      }
    }

    /// <summary>
    ///     <strong>CustomLibrary.SafeGet&lt;Dictionary>()</strong>
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     <br /> - Acts a normal <b><paramref name="dictionary" />[<paramref name="key" />]</b>
    ///     when valid. <br /> - If pointed <b>
    ///         <paramref name="key" />
    ///     </b>
    ///     does not exists in
    ///     <b>
    ///         <paramref name="dictionary" />
    ///     </b>
    ///     , returns <b><paramref name="dictionary" />[<paramref name="default_key" />]</b>.
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// </summary>
    public static TValue SafeGet<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary,
        TKey key, TKey default_key = default!)
        where TKey : notnull {
      if (dictionary.TryGetValue(key, out TValue get)) {
        return get;
      }
      return dictionary[default_key];
    }

    //-+-+-+-+-+-+-+-+
    // Transformation
    //-+-+-+-+-+-+-+-+
    public static T[][] Transpose<T>(this IReadOnlyList<IReadOnlyList<T>> arr)
        where T : struct {
      int len  = arr.Count;
      var len2 = int.MinValue;
      for (var i = 0; i < len; i++) len2 = Math.Max(len2, arr[i].Count);

      var resArr = new T[len2][];

      for (var i = 0; i < len2; i++) resArr[i] = new T[len];

      for (var i = 0; i < len; i++) {
        int tmpInt = arr[i].Count;
        for (var j = 0; j < tmpInt; j++) resArr[j][i] = arr[i][j];
      }

      return resArr;
    }

    public static T[][] Transpose<T>(this IReadOnlyList<T> inArr)
        where T : struct {
      int len    = inArr.Count;
      var resArr = new T[len][];
      for (var i = 0; i < len; i++) resArr[i] = new[] { inArr[i] };

      return resArr;
    }

    public static Queue<T> toQueue<T>(this IEnumerable<T> list, Queue<T>? result = null) {
      result ??= new Queue<T>();
      foreach (T item in list) result.Enqueue(item);
      return result;
    }

    /// <summary>
    /// <br/> - Returns a List of integer of indices of <param name="boolList"/> that was <see
    /// langword="true"/>
    /// </summary>
    public static List<int> toIndicesList(this IReadOnlyList<bool> boolList,
        List<int>? outList = null) {
      outList ??= new List<int>();
      outList.Clear();

      int len = boolList.Count;
      for (var i = 0; i < len; i++)
        if (boolList[i])
          outList.Add(i);

      return outList;
    }

    public static string Join(this IEnumerable<string> stringList,
        string separator = "") => string.Join(separator, stringList);

    public static string Join(this IEnumerable<char> stringList,
        string separator = "") => string.Join(separator, stringList);

    public static IEnumerable<T> ListAll<T>()
        where T : Enum => Enum.GetValues(typeof(T)).Cast<T>();

//-+-+-+-+-+-+-+-+
// Upsert
//-+-+-+-+-+-+-+-+
#region Upsert
    public static void Upsert<T>(this IList<T> list, int index, T val) {
      int count = list.Count;
      while (count <= index) {
        count++;
        list.Add(default!);
      }

      list[index] = val;
    }

    public static void Upsert<T>(this IList<T> list, int index, Func<T, T> update) {
      int count = list.Count;
      while (count <= index) {
        count++;
        list.Add(default!);
      }

      list[index] = update(list[index]);
    }

    public static void Upsert<TKey, TValue>(this IDictionary<TKey, TValue> list, TKey index,
        TValue val)
        where TKey : struct {
      if (list.ContainsKey(index))
        list[index] = val;
      else
        list.Add(index, val);
    }

    public static void Upsert<TKey, TValue>(this IDictionary<TKey, TValue> list, TKey index,
        Func<TValue, TValue> update)
        where TKey : struct {
      if (list.TryGetValue(index, out TValue value))
        list[index] = update(value);
      else
        list.Add(index, update(default!));
    }
#endregion

    /// <summary>
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     <br /> - Compability support of <see cref="List{T}.AddRange(IEnumerable{T})" /> for <see
    ///     cref="IList{T}" />. <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// </summary>
    public static void AddRange<T>(this IList<T> list, IEnumerable<T> range) {
      if (list is List<T> listT)
        listT.AddRange(range);
      else
        foreach (T data in range) list.Add(data);
    }

    public static void Copy<T>(IReadOnlyList<T> source, IList<T> target)
        where T : struct {
      int len = source.Count;
      if (target is T[] targetArr && source is T[] sourceArr)
        Array.Copy(sourceArr, targetArr, len);
      else
        for (var i = 0; i < len; i++) target[i] = source[i];
    }

//-+-+-+-+-+-+-+
// Get Casted Value or Default
//-+-+-+-+-+-+-+
#region Get Casted Value or Default
    /// <summary>
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     <br /> - Acts like a normal <see cref="Dictionary{TKey, TValue}.TryGetValue" />.
    ///     <br /> - Equipped with a cast, for better compatibility with <see cref="object" />.
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// </summary>
    public static TValue GetCastedValueOrDefault<TKey, TValue>(
        this IDictionary<TKey, object> dictionary, TKey key, Func<object, TValue> castDefinition,
        TValue defaultValue = default!) => dictionary.TryGetValue(key, out object rawValue)
                                               ? castDefinition(rawValue)
                                               : defaultValue;

    /// <summary>
    ///     <inheritdoc
    ///         cref="GetCastedValueOrDefault{TKey, TValue}(IDictionary{TKey, object}, TKey,
    ///         Func{object, TValue}, TValue)" />
    /// </summary>
    public static string GetCastedValueOrDefault<TKey>(this IDictionary<TKey, object> dictionary,
        TKey key, string defaultValue = "") => GetCastedValueOrDefault(dictionary, key,
        x => x as string ?? defaultValue, defaultValue);

    /// <summary>
    ///     <inheritdoc
    ///         cref="GetCastedValueOrDefault{TKey, TValue}(IDictionary{TKey, object}, TKey,
    ///         Func{object, TValue}, TValue)" />
    /// </summary>
    public static uint GetCastedValueOrDefault<TKey>(this IDictionary<TKey, object> dictionary,
        TKey key, uint defaultValue = 0) => GetCastedValueOrDefault(dictionary, key,
        Convert.ToUInt32, defaultValue);

    /// <summary>
    ///     <inheritdoc
    ///         cref="GetCastedValueOrDefault{TKey, TValue}(IDictionary{TKey, object}, TKey,
    ///         Func{object, TValue}, TValue)" />
    /// </summary>
    public static int GetCastedValueOrDefault<TKey>(this IDictionary<TKey, object> dictionary,
        TKey key, int defaultValue = 0) => GetCastedValueOrDefault(dictionary, key, Convert.ToInt32,
        defaultValue);

    /// <summary>
    ///     <inheritdoc
    ///         cref="GetCastedValueOrDefault{TKey, TValue}(IDictionary{TKey, object}, TKey,
    ///         Func{object, TValue}, TValue)" />
    /// </summary>
    public static double GetCastedValueOrDefault<TKey>(this IDictionary<TKey, object> dictionary,
        TKey key, double defaultValue = 0) => GetCastedValueOrDefault(dictionary, key,
        Convert.ToDouble, defaultValue);
#endregion

//-+-+-+-+-+-+-+-+
// Deep Copy to Array
//-+-+-+-+-+-+-+-+
#region Deep Copy to Array
    // ..not using IList or IReadOnlyList, and multi function for each level of nest.
    // ..if not properly defined as List<List> or [][],
    //-+-+-+-+-+-+-+-+
    // ..compiler will treat the insides as Generic Type T, instead of the Interface type.
    // ..while algorithmically there is a work around, the return generic type T follows the
    // original.
    // ..e.g. a List<List<List>> will turn as List<List>[] since T is recognized as List<List>
    // instead.
    //-+-+-+-+-+-+-+-+

    public static T[][] DeepToArray<T>(this IEnumerable<IEnumerable<T>> list)
        where T : struct => list.Select(x => x.ToArray()).ToArray();

    public static T[][][] DeepToArray<T>(this IEnumerable<IEnumerable<IEnumerable<T>>> list)
        where T : struct => list.Select(x => x.DeepToArray()).ToArray();

    public static List<List<T>> DeepToList<T>(this IEnumerable<IEnumerable<T>> list)
        where T : struct => list.Select(x => x.ToList()).ToList();

    public static List<List<List<T>>> DeepToList<T>(
        this IEnumerable<IEnumerable<IEnumerable<T>>> list)
        where T : struct => list.Select(x => x.DeepToList()).ToList();

    public static ImmutableArray<ImmutableArray<T>> DeepToImmutableArray<T>(
        this IEnumerable<IEnumerable<T>> list)
        where T : struct => list.Select(x => x.ToImmutableArray()).ToImmutableArray();

    public static ImmutableArray<ImmutableArray<ImmutableArray<T>>> DeepToImmutableArray<T>(
        this IEnumerable<IEnumerable<IEnumerable<T>>> list)
        where T : struct => list.Select(x => x.DeepToImmutableArray()).ToImmutableArray();

//-+-+-+-+-+-+-+-+
#endregion
  }
}
