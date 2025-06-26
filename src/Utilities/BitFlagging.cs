using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace MMOR.Utils.Utilities
{
    //-+-+-+-+-+-+-+-+
    // BitMask Functions
    // ..quick read for those unfamiliar with the concept:
    // https://abdulapopoola.com/2016/05/30/understanding-bit-masks/
    //-+-+-+-+-+-+-+-+
    public static partial class Utilities
    {
        private static int _threadId = -1;

        /// <summary>
        ///     <strong>CustomLibrary.ContainsFlag()</strong>
        ///     <br />
        ///     -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Practically the default C# <see cref="Enum.HasFlag(Enum)" />.
        ///     <br /> - Added a different check on <b>flag</b> == 0, where it will check whether the <paramref name="bitmask" />
        ///     is empty.
        ///     <br />
        ///     -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Do not use Convert.ToInt() for the Enum checking, the conversion demands a Garbage Collection and not ideal for Simulations
        public static bool ContainsFlag<T>(this T bitmask, T flag) where T : struct, Enum
        {
            return EqualityComparer<T>.Default.Equals(flag, default)
                ? EqualityComparer<T>.Default.Equals(bitmask, default)
                : bitmask.HasFlag(flag);
        }

        /// <summary>
        ///     <inheritdoc cref="ContainsFlag" />
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsFlag(this int bitmask, int flag)
        {
            return flag == 0 ? bitmask == 0 : (bitmask & flag) == flag;
        }

        /// <summary>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Returns the highest possible value of flag combination of type <typeparamref name="T" />.
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MaxFlag<T>() where T : struct, Enum => (Enum.GetValues(typeof(T)).Cast<int>().Max() << 1) - 1;

        /// <summary>
        ///     <inheritdoc cref="MaxFlag{T}" />
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T MaxFlagCasted<T>() where T : struct, Enum { return (T)Enum.ToObject(typeof(T), MaxFlag<T>()); }

        /// <summary>
        ///     <br />
        ///     -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Fills in empty Keys in <paramref name="dictionary" />, based on the logic of
        ///     <see cref="SafeGetFlag{T}(IDictionary{int, T}, in int)" />.
        ///     <br />
        ///     -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        /// <returns></returns>
        public static IDictionary<TFlag, TElement> PrePopulateFlag<TFlag, TElement>(
            this IDictionary<TFlag, TElement> dictionary) where TFlag : struct, Enum
        {
            Type type = typeof(TFlag);
            int combinations = MaxFlag<TFlag>() + 1;
            for (var i = 0; i < combinations; i++)
                _ = dictionary.SafeGetFlagCachedThreadSafe((TFlag)Enum.ToObject(type, i));
            return dictionary;
        }

        /// <summary>
        ///     <strong>CustomLibrary.SafeGetFlag()</strong>
        ///     <br />
        ///     -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Gets the element of a <b>Dictionary</b> that uses <i>bitmask</i> as <i>Key</i>s at
        ///     <paramref name="flag" />.
        ///     <br /> - If <paramref name="flag" /> does not exist as a <i>Key</i>, it will run <paramref name="flag" />.
        ///     <see cref="ContainsFlag" /> for each <i>Key</i>s in the <paramref name="dictionary" /> and returns the first match.
        ///     <br /> - If there is still none, returns the element at
        ///     <b><see langword="default" />(<typeparamref name="TFlag" />)</b> instead.
        ///     <br />
        ///     -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TElement SafeGetFlag<TFlag, TElement>(this IReadOnlyDictionary<TFlag, TElement> dictionary,
            TFlag flag) where TFlag : struct, Enum
        {
            if (dictionary.TryGetValue(flag, out TElement value))
                return value;
            foreach (KeyValuePair<TFlag, TElement> kvp in dictionary)
                if (flag.ContainsFlag(kvp.Key))
                    return kvp.Value;
            return dictionary[default];
        }

        /// <summary>
        ///     <inheritdoc cref="SafeGetFlag{TFlag, TElement}" />
        ///     <br /> - This version provides Auto-Population. The getter logic remains the same.
        ///     <br />
        ///     -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TElement SafeGetFlagCached<TFlag, TElement>(this IDictionary<TFlag, TElement> dictionary,
            TFlag flag) where TFlag : struct, Enum
        {
            if (dictionary.TryGetValue(flag, out TElement value))
                return value;
            foreach (KeyValuePair<TFlag, TElement> kvp in dictionary)
                if (flag.ContainsFlag(kvp.Key))
                {
                    value = kvp.Value;
                    dictionary.Add(flag, value);
                    return value;
                }

            value = dictionary[default];
            dictionary.Add(flag, value);
            return value;
        }

        /// <summary>
        ///     <inheritdoc cref="SafeGetFlag{TFlag, TElement}" />
        ///     <br /> - This version provides Auto-Population and Thread-Safety. The getter logic remains the same.
        ///     <br />
        ///     -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TElement SafeGetFlagCachedThreadSafe<TFlag, TElement>(
            this IDictionary<TFlag, TElement> dictionary, TFlag flag) where TFlag : struct, Enum
        {
            // Exists, simply return value
            if (dictionary.TryGetValue(flag, out TElement value))
                return value;

            // Else,
            // ..find match, while caching the result

            //-+-+-+-+-+-+-+-+
            // By Default, ThreadId will be null
            // ..we first set it to whichever Thread is currently accessing this
            // ..this will lock ThreadId to the first-come, while others will
            // ..access this like the old version
            int currThread = Environment.CurrentManagedThreadId;
            bool firstThread = Interlocked.CompareExchange(ref _threadId, currThread, -1) == -1;

            TFlag match;
            foreach (KeyValuePair<TFlag, TElement> kvp in dictionary)
            {
                match = kvp.Key;
                if (flag.ContainsFlag(match))
                {
                    value = dictionary[match];

                    //-+-+-+-+-+-+-+-+
                    // If the current accessor is the first-come Thread,
                    // ..Populate the Dictionary
                    // ..Otherwise, simply return value
                    if (firstThread)
                    {
                        lock (dictionary)
                        {
                            dictionary.Add(flag, value);
                        }

                        // Reset the ThreadId ahead for next Accessor
                        _threadId = -1;
                    }

                    return value;
                }
            }

            value = dictionary[default];

            //-+-+-+-+-+-+-+-+
            // If the current accessor is the first-come Thread,
            // ..Populate the Dictionary
            // ..Otherwise, simply return value
            if (firstThread)
            {
                lock (dictionary)
                {
                    dictionary.Add(flag, value);
                }

                // Reset the ThreadId ahead for next Accessor
                _threadId = -1;
            }

            return value;
        }

        /// <summary>
        ///     <strong>CustomLibrary.ToStringAll()</strong>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Prints all the flags contained within the <paramref name="bitmask" />, separated by
        ///     <paramref name="separator" />.
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToStringAll<T>(this T bitmask, in string separator = " | ") where T : struct, Enum
        {
            var notFirst = false;
            StringBuilder strResult = new();
            // Add flags as string
            foreach (T flag in Enum.GetValues(typeof(T)))
                if (bitmask.ContainsFlag(flag))
                {
                    if (notFirst)
                    {
                        notFirst = true;
                        strResult.Append(separator);
                    }

                    strResult.Append(flag.ToString());
                }

            // If no flags were added, attach the default flag
            if (strResult.Length <= 0)
                return default(T).ToString();
            // Remove last separator
            return strResult.ToString();
        }

        /// <summary>
        ///     <strong>CustomLibrary.InList()</strong>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Checks if <paramref name="bitmask" /> contains a flag in a <paramref name="listFlags" />.
        ///     <br /> - Used to check for any invalid <paramref name="bitmask" /> combination.
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - While initial thought might lead into using <see cref="HashSet{T}.Contains(T)" />.
        ///     <br /> - It can only check for exact match, which does not work well with bitmasking.
        ///     <br /> - e.g. an bitmask of A | B | C, will get passed by with invalid flag A | B.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InList<T>(this IReadOnlyCollection<T> listFlags, T bitmask) where T : struct, Enum
        {
            return listFlags.Count(x => bitmask.ContainsFlag(x)) != 0;
        }

        /// <inheritdoc cref="InList" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InList<T>(this T bitmask, IReadOnlyCollection<T> listFlags) where T : struct, Enum
        {
            return listFlags.InList(bitmask);
        }

        /// <summary>
        ///     <strong>CustomLibrary.FlagToIndex()</strong>
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     <br /> - Returns the bit position of <paramref name="flag" />.
        ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FlagToIndex<T>(this T flag) where T : struct, Enum
        {
            var i = 0;
            var f = Convert.ToInt32(flag);
            while (f != 0)
            {
                f >>= 1;
                i++;
            }

            return i;
        }
    }
}