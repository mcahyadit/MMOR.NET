using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MMOR.Utils.Utilities
{
    //-+-+-+-+-+-+-+-+
    // Frequency Map Helpers
    // .. treats dictionary as a map of <Value, number of times Value occurs>
    // .. more intensive functions are in StatisticsLibrary.cs
    //-+-+-+-+-+-+-+-+
    public static partial class Utilities
    {
        /// <summary>
        ///     <strong>CustomLibrary.AddFrequency()</strong>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Increment the <paramref name="frequencyMap" />[<paramref name="data" />].<i>Value</i> by
        ///     <paramref name="frequency" /> if exists.
        ///     <br /> - Otherwise, adds <see cref="KeyValuePair" />(<paramref name="data" />, <paramref name="frequency" />) to
        ///     <paramref name="frequencyMap" />.
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddFrequency<T>(this IDictionary<T, uint> frequencyMap, T data, uint frequency = 1)
            where T : notnull
        {
            if (frequencyMap.TryGetValue(data, out uint tmpInt))
                frequencyMap[data] = tmpInt + frequency;
            else
                frequencyMap.Add(data, frequency);

            ///-+-+-+-+-+-+-+-+
            /// Code above, runs slighly faster than:
            ///<![CDATA[
            ///if (frequencyMap.ContainsKey(data))
            ///  frequencyMap[data] += frequency;
            ///else
            ///  frequencyMap.Add(data, frequency);
            ///]]>
            /// Reason being <![CDATA[data +=]]> access the dictionary twice, 
            /// ..once to get the original value, another for assigning the memory location.
            /// In comparison, <![CDATA[TryGetValue]]>, combines <![CDATA[ContainsKey]]>
            /// ..with the getter, so the memory access goes down from 3 to 2.
            ///-+-+-+-+-+-+-+-+
        }

        /// <summary>
        ///     <inheritdoc cref="AddFrequency{T}(System.Collections.Generic.IDictionary{T,uint},T,uint)" />
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddFrequency<T>(this IDictionary<T, uint> frequencyMap, in (T, uint) data)
            where T : notnull
        {
            frequencyMap.AddFrequency(data.Item1, data.Item2);
        }

        /// <summary>
        ///     <inheritdoc cref="AddFrequency{T}(System.Collections.Generic.IDictionary{T,uint},T,uint)" />
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddFrequency<T>(this IDictionary<T, uint> frequencyMap, in KeyValuePair<T, uint> data)
            where T : notnull
        {
            frequencyMap.AddFrequency(data.Key, data.Value);
        }

        /// <summary>
        ///     <strong>CustomLibrary.CombineFrequency()</strong>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Increment the <paramref name="a" />[<b>data</b>].<i>Value</i> by <paramref name="b" />[<b>data</b>].
        ///     <i>Value</i> if exists.
        ///     <br /> - Otherwise, adds <paramref name="b" />[<b>data</b>] to <paramref name="a" />.
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CombineFrequency<T>(this IDictionary<T, uint> a, IDictionary<T, uint> b) where T : notnull
        {
            // For some reason, using IReadOnlyDictionary b causes this function to not be able to keep up in multi-thread speed
            // ..most likely due to IReadOnlyDictionary being read as IEnumerable makes it more volatile
            // ..for now, b is kept as Dictionary instead
            var copy = new Dictionary<T, uint>(b);
            foreach (KeyValuePair<T, uint> kvp in copy)
                //foreach (var kvp in b)
                a.AddFrequency(kvp.Key, kvp.Value);
        }

        /// <summary>
        ///     <strong>CustomLibrary.ToFrequencyMap()</strong>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Converts Linear <see cref="List{T}" />/<see cref="Array" /> to a Frequency Map.
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        public static Dictionary<T, uint> ToFrequencyMap<T>(this IEnumerable<T> list) where T : notnull
        {
            var result = new Dictionary<T, uint>();
            foreach (T item in list)
                result.AddFrequency(item);
            return result;
        }
    }
}