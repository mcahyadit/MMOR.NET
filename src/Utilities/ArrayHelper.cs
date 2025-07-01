using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using MMOR.Utils.Mathematics;
using MMOR.Utils.Random;

namespace MMOR.Utils.Utilities
{
    //-+-+-+-+-+-+-+-+
    // Array Helpers
    //-+-+-+-+-+-+-+-+
    public static partial class Utilities
    {
        private static readonly List<KeyValuePair<string, string>> collectionBrackets = new()
        {
            new KeyValuePair<string, string>("{", "}"),
            new KeyValuePair<string, string>("[", "]"),
            new KeyValuePair<string, string>("(", ")")
        };

        // ulong didn't have a built-in Sum LINQ, possibly since if you are summing such big number, it bound to overflow
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Sum(this IEnumerable<ulong> arr)
        {
            return arr.IsNullOrEmpty() ? 0 : arr.Aggregate((sum, val) => sum + val);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Reverse<T>(IList<T> list, int begin, int end)
        {
            while (begin < end)
            {
                (list[begin], list[end]) = (list[end], list[begin]);
                begin++;
                end--;
            }
        }

        //-+-+-+-+-+-+-+-+
        // Null || Empty Check
        //-+-+-+-+-+-+-+-+
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> arr) { return arr == null || arr.Count() <= 0; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty(string str) { return string.IsNullOrWhiteSpace(str); }

        //-+-+-+-+-+-+-+-+
        // Creation
        //-+-+-+-+-+-+-+-+
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<T> InitList<T>(int length, T value = default) where T : struct
        {
            return Enumerable.Repeat(value, length).ToList();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<T> InitList<T>(int length, Func<T> constructor) where T : class
        {
            return Enumerable.Range(0, length).Select(_ => constructor.Invoke()).ToList();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] InitArr<T>(int length, T value = default) where T : struct
        {
            return Enumerable.Repeat(value, length).ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] InitArr<T>(int length, Func<T> constructor) where T : class
        {
            return Enumerable.Range(0, length).Select(_ => constructor.Invoke()).ToArray();
        }

        //-+-+-+-+-+-+-+-+
        // Fill & Clear
        //-+-+-+-+-+-+-+-+
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear<T>(this T[] inArr) { Array.Clear(inArr, 0, inArr.Length); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Fill<T>(this IList<T> list, T fillValue) where T : struct
        {
            int len = list.Count;
            for (var i = 0; i < len; i++)
                list[i] = fillValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Fill<T>(this IList<T> list, Func<T> fillConstructor) where T : class
        {
            int len = list.Count;
            for (var i = 0; i < len; i++)
                list[i] = fillConstructor.Invoke();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConvertElements<T>(this IList<T> list, Func<IList<T>, int, T> function)
        {
            int len = list.Count;
            for (var i = 0; i < len; i++)
                list[i] = function(list, i);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConvertElements<TKey, TValue>(this IDictionary<TKey, TValue> dict,
            Func<IDictionary<TKey, TValue>, TKey, TValue> function)
        {
            foreach (TKey key in dict.Keys)
                dict[key] = function(dict, key);
        }

        /// <summary>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Based of
        ///     <see href="https://www.geeksforgeeks.org/shuffle-a-given-array-using-fisher-yates-shuffle-algorithm/">
        ///         Fisher-Yates
        ///         Shuffle Algorithm
        ///     </see>
        ///     .
        ///     <br /> - Takes optional <see cref="IRandom" /> for custom RNGs.
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ShuffleInplace<T>(this IList<T> list, IRandom rng = null)
        {
            rng ??= PCG.global;
            int len = list.Count;

            for (int indexCurrent = len - 1; indexCurrent > 0; indexCurrent--)
            {
                int indexRand = rng.NextInt(0, indexCurrent + 1);
                (list[indexCurrent], list[indexRand]) = (list[indexRand], list[indexCurrent]);
            }
        }

        /// <summary>
        ///     <inheritdoc cref="ShuffleInplace" />
        ///     <br /> - This one returns a copy instead of shuffling the original.
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IList<T> Shuffle<T>(this IReadOnlyList<T> list, IList<T> outList = null, IRandom rng = null)
        {
            outList = list.ToList();
            outList.ShuffleInplace(rng);
            return outList;
        }

        //-+-+-+-+-+-+-+-+
        // Safe Getter
        //-+-+-+-+-+-+-+-+
        /// <summary>
        ///     <strong>CustomLibrary.SafeGet&lt;List>()</strong>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Acts a normal <b><paramref name="list" />[<paramref name="index" />]</b> when valid.
        ///     <br /> - If
        ///     <b>
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T SafeGet<T>(this IEnumerable<T> list, int index, bool clamps = false)
        {
            switch (list)
            {
                case IReadOnlyList<T> roList:
                    return clamps
                        ? roList[Math.Clamp(index, 0, roList.Count - 1)]
                        : roList[index.Repeat(roList.Count)];
                case IList<T> mList:
                    return clamps
                        ? mList[Math.Clamp(index, 0, mList.Count - 1)]
                        : mList[index.Repeat(mList.Count)];
                default:
                {
                    // fallback: enumerate (avoid if possible)
                    T[] array = list.ToArray();
                    return clamps
                        ? array[Math.Clamp(index, 0, array.Length - 1)]
                        : array[index.Repeat(array.Length)];
                }
            }
        }

        /// <summary>
        ///     <strong>CustomLibrary.SafeGet&lt;Dictionary>()</strong>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Acts a normal <b><paramref name="dictionary" />[<paramref name="key" />]</b> when valid.
        ///     <br /> - If pointed
        ///     <b>
        ///         <paramref name="key" />
        ///     </b>
        ///     does not exists in
        ///     <b>
        ///         <paramref name="dictionary" />
        ///     </b>
        ///     , returns <b><paramref name="dictionary" />[<paramref name="defaultT1" />]</b>.
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TValue SafeGet<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key,
            TKey defaultT1 = default) where TKey : notnull
        {
            if (dictionary.ContainsKey(key))
                return dictionary[key];
            return dictionary[defaultT1];
        }

        //-+-+-+-+-+-+-+-+
        // Transformation
        //-+-+-+-+-+-+-+-+
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[][] Transpose<T>(this IReadOnlyList<IReadOnlyList<T>> arr) where T : struct
        {
            int len = arr.Count;
            var len2 = int.MinValue;
            for (var i = 0; i < len; i++)
                len2 = Math.Max(len2, arr[i].Count);

            var resArr = new T[len2][];

            for (var i = 0; i < len2; i++)
                resArr[i] = new T[len];

            for (var i = 0; i < len; i++)
            {
                int tmpInt = arr[i].Count;
                for (var j = 0; j < tmpInt; j++)
                    resArr[j][i] = arr[i][j];
            }

            return resArr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[][] Transpose<T>(this IReadOnlyList<T> inArr) where T : struct
        {
            int len = inArr.Count;
            var resArr = new T[len][];
            for (var i = 0; i < len; i++)
                resArr[i] = new[] { inArr[i] };

            return resArr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Queue<T> toQueue<T>(this IEnumerable<T> list, Queue<T> result = null)
        {
            result ??= new Queue<T>();
            foreach (T item in list)
                result.Enqueue(item);
            return result;
        }

        public static List<int> toIndicesList(this IReadOnlyList<bool> boolList, List<int> outList = null)
        {
            outList ??= new List<int>();
            outList.Clear();

            int len = boolList.Count;
            for (var i = 0; i < len; i++)
                if (boolList[i])
                    outList.Add(i);

            return outList;
        }

        public static string join(this IEnumerable<string> stringList, string separator = "")
        {
            return string.Join(separator, stringList);
        }

        public static string join(this IEnumerable<char> stringList, string separator = "")
        {
            return string.Join(separator, stringList);
        }

        public static string deepPrintArray<T>(this IEnumerable<T> array, int bracketId = 0)
        {
            StringBuilder strResult = new();
            strResult.Append(collectionBrackets.SafeGet(bracketId).Key);
            string bracketClose = collectionBrackets.SafeGet(bracketId).Value;
            foreach (T item in array)
            {
                strResult.Append(" ");
                if (!(item is string) && item is IEnumerable)
                {
                    string tmpStr = deepPrintArray(ConvertToEnum(item as IEnumerable), bracketId++);
                    strResult.Append(tmpStr);
                }
                else
                {
                    strResult.Append(item);
                }

                strResult.Append(',');
            }

            //strResult.Remove(strResult.Length - 1, 1);
            strResult.Append(bracketClose);
            strResult.Append('\n');

            return strResult.ToString();
        }

        private static IEnumerable<object> ConvertToEnum(IEnumerable iEnumerable)
        {
            foreach (object item in iEnumerable)
                yield return item;
        }

        public static IEnumerable<T> ListAll<T>() where T : Enum { return Enum.GetValues(typeof(T)).Cast<T>(); }

        public static void Upsert<T>(this IList<T> list, int index, T val)
        {
            int count = list.Count;
            while (count <= index)
            {
                count++;
                list.Add(default);
            }

            list[index] = val;
        }

        public static void Upsert<T>(this IList<T> list, int index, Func<T, T> update)
        {
            int count = list.Count;
            while (count <= index)
            {
                count++;
                list.Add(default);
            }

            list[index] = update(list[index]);
        }

        public static void Upsert<TKey, TValue>(this IDictionary<TKey, TValue> list, TKey index, TValue val)
            where TKey : struct
        {
            if (list.ContainsKey(index))
                list[index] = val;
            else
                list.Add(index, val);
        }

        public static void Upsert<TKey, TValue>(this IDictionary<TKey, TValue> list, TKey index,
            Func<TValue, TValue> update) where TKey : struct
        {
            if (list.TryGetValue(index, out TValue value))
                list[index] = update(value);
            else
                list.Add(index, update(default));
        }

        /// <summary>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Compability support of <see cref="List{T}.AddRange(IEnumerable{T})" /> for <see cref="IList{T}" />.
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        public static void AddRange<T>(this IList<T> list, IEnumerable<T> range)
        {
            if (list is List<T> listT)
                listT.AddRange(range);
            foreach (T data in range)
                list.Add(data);
        }

        //-+-+-+-+-+-+-+
        // Get Casted Value or Default
        //-+-+-+-+-+-+-+
        #region Get Casted Value or Default
        /// <summary>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Acts like a normal <see cref="Dictionary{TKey, TValue}.TryGetValue" />.
        ///     <br /> - Equipped with a cast, for better comtibility with <see cref="object" />.
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TValue GetCastedValueOrDefault<TKey, TValue>(this IDictionary<TKey, object> dictionary, TKey key,
            Func<object, TValue> castDefinition, TValue defaultValue = default)
        {
            return dictionary.TryGetValue(key, out object rawValue) ? castDefinition(rawValue) : defaultValue;
        }

        /// <summary>
        ///     <inheritdoc
        ///         cref="GetCastedValueOrDefault{TKey, TValue}(IDictionary{TKey, object}, TKey, Func{object, TValue}, TValue)" />
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetCastedValueOrDefault<TKey>(this IDictionary<TKey, object> dictionary, TKey key,
            string defaultValue = default)
        {
            return GetCastedValueOrDefault(dictionary, key, x => x as string, defaultValue);
        }

        /// <summary>
        ///     <inheritdoc
        ///         cref="GetCastedValueOrDefault{TKey, TValue}(IDictionary{TKey, object}, TKey, Func{object, TValue}, TValue)" />
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetCastedValueOrDefault<TKey>(this IDictionary<TKey, object> dictionary, TKey key,
            uint defaultValue = default)
        {
            return GetCastedValueOrDefault(dictionary, key, Convert.ToUInt32, defaultValue);
        }

        /// <summary>
        ///     <inheritdoc
        ///         cref="GetCastedValueOrDefault{TKey, TValue}(IDictionary{TKey, object}, TKey, Func{object, TValue}, TValue)" />
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetCastedValueOrDefault<TKey>(this IDictionary<TKey, object> dictionary, TKey key,
            int defaultValue = default)
        {
            return GetCastedValueOrDefault(dictionary, key, Convert.ToInt32, defaultValue);
        }

        /// <summary>
        ///     <inheritdoc
        ///         cref="GetCastedValueOrDefault{TKey, TValue}(IDictionary{TKey, object}, TKey, Func{object, TValue}, TValue)" />
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double GetCastedValueOrDefault<TKey>(this IDictionary<TKey, object> dictionary, TKey key,
            double defaultValue = default)
        {
            return GetCastedValueOrDefault(dictionary, key, Convert.ToDouble, defaultValue);
        }
        #endregion

        //-+-+-+-+-+-+-+-+
        // Deep Copy to Array
        //-+-+-+-+-+-+-+-+

        #region Deep Copy to Array
        // ..not using IList or IReadOnlyList, and multi function for each level of nest.
        // ..if not properly defined as List<List> or [][],
        //-+-+-+-+-+-+-+-+
        // ..compiler will treat the insides as Generic Type T, instead of the Interface type.
        // ..while algorithmically there is a work around, the return generic type T follows the original.
        // ..e.g. a List<List<List>> will turn as List<List>[] since T is recognized as List<List> instead.
        //-+-+-+-+-+-+-+-+

        public static T[][] deepToArray<T>(this IEnumerable<IEnumerable<T>> list) where T : struct
        {
            return list.Select(x => x.ToArray()).ToArray();
        }

        public static T[][][] deepToArray<T>(this IEnumerable<IEnumerable<IEnumerable<T>>> list) where T : struct
        {
            return list.Select(x => x.deepToArray()).ToArray();
        }

        //-+-+-+-+-+-+-+-+
        #endregion
    }
}