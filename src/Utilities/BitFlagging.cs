using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

#if UNITY_5_3_OR_NEWER || UNITY_2017_1_OR_NEWER
using Unity.Mathematics;
#else
using System.Numerics;
#endif

namespace MMOR.NET.Utilities {
  public static partial class Utilities {
    /**
     * <summary>
     * Determines whether a bitmask contains a specified flag, with special handling
     * for the <c>0</c> flag.
     * </summary>
     * <typeparam name="T">Enum type representing flags.</typeparam>
     * <param name="bitmask">The bitmask to test.</param>
     * <param name="flag">The flag to check for.</param>
     * <returns>
     * <c>true</c> if the <paramref name="bitmask"/> contains <paramref name="flag"/>.
     * When <paramref name="flag"/> is <c>0</c>, returns <c>true</c> only if
     * <paramref name="bitmask"/> is also <c>0</c>.
     * </returns>
     * */
    // Do not use Convert.ToInt() for the Enum checking,
    // .. the conversion demands a Garbage Collection and not ideal for Simulations
    public static bool ContainsFlag<T>(this T bitmask, T flag)
        where T : struct, Enum {
      return EqualityComparer<T>.Default.Equals(flag, (T)Enum.ToObject(typeof(T), 0))
                 ? EqualityComparer<T>.Default.Equals(bitmask, (T)Enum.ToObject(typeof(T), 0))
                 : bitmask.HasFlag(flag);
    }

    /// <inheritdoc cref="ContainsFlag{T}(T, T)"/>
    public static bool ContainsFlag(this int bitmask, int flag) {
      return flag == 0 ? bitmask == 0 : (bitmask & flag) == flag;
    }

    /**
     * <summary>
     * Computes the maximum possible bitmask value for an enum type by taking all defined flags
     * and producing a mask containing all bits up to the highest flag.
     * </summary>
     * <typeparam name="T">Enum type representing flags.</typeparam>
     * <returns>
     * The maximum combined-flag value representable by the enum, based on
     * all declared enum values.
     * </returns>
     * */
    public static int MaxFlag<T>()
        where T : struct, Enum => (Enum.GetValues(typeof(T)).Cast<int>().Max() << 1) - 1;

    /// <inheritdoc cref="MaxFlag{T}" />
    public static T MaxFlagCasted<T>()
        where T : struct, Enum => (T) Enum.ToObject(typeof(T), MaxFlag<T>());

    /**
     * <summary>
     * Retrieves a value mapped to a flag from a flag-based dictionary. If the exact flag
     * is not present, the method returns the value of the first key whose flag is contained
     * within the requested flag, falling back to the default key.
     * </summary>
     * <typeparam name="TFlag">An enum type representing flags.</typeparam>
     * <typeparam name="TElement">The value type mapped to flags.</typeparam>
     * <param name="dictionary">A dictionary mapping flags to values.</param>
     * <param name="flag">The flag whose associated value is desired.</param>
     * <returns>
     * The matched value, or the value mapped to the default key if no applicable flag is found.
     * </returns>
     * */
    public static TElement SafeGetFlag<TFlag, TElement>(
        this IReadOnlyDictionary<TFlag, TElement> dictionary, TFlag flag)
        where TFlag : struct, Enum {
      if (dictionary.TryGetValue(flag, out TElement value))
        return value;
      foreach (KeyValuePair<TFlag, TElement> kvp in dictionary)
        if (flag.ContainsFlag(kvp.Key))
          return kvp.Value;
      return dictionary[default];
    }

    /**
     * <summary>
     * Prepopulates a dictionary with values for every possible bitmask combination for
     * <typeparamref name="TFlag"/>. Missing entries are resolved either from matching
     * sub-flags or from the default value.
     * </summary>
     * <typeparam name="TFlag">An enum type representing flags.</typeparam>
     * <typeparam name="TElement">The value type mapped to flags.</typeparam>
     * <param name="dictionary">
     * The dictionary containing the explicitly mapped flag values.
     * </param>
     * <returns>
     * A new dictionary where all integer flag values up to <see cref="MaxFlag{TFlag}"/>
     * map to the most appropriate element.
     * </returns>
     * */
    public static IReadOnlyDictionary<TFlag, TElement> PrePopulateFlag<TFlag, TElement>(
        this Dictionary<TFlag, TElement> dictionary)
        where TFlag : struct, Enum {
      Type type    = typeof(TFlag);
      int max_flag = MaxFlag<TFlag>();
      var result   = new Dictionary<TFlag, TElement>();
      result.EnsureCapacity(max_flag);
      result.Add(default, dictionary[default]);

      for (var i = 1; i <= max_flag; ++i) {
        TFlag flag = (TFlag)Enum.ToObject(type, i);
        if (dictionary.TryGetValue(flag, out TElement match)) {
          result.Add(flag, match);
          continue;
        }
        bool found = false;
        foreach ((TFlag key, TElement value) in dictionary) {
          if (flag.ContainsFlag(key)) {
            result.Add(flag, value);
            found = true;
            break;
          }
        }
        if (!found) {
          result.Add(flag, dictionary[default]);
        }
      }
      return result;
    }

    /**
     * <summary>
     * Produces a formatted string representing all flags present in the bitmask.
     * </summary>
     * <typeparam name="T">Enum type representing flags.</typeparam>
     * <param name="bitmask">The bitmask to convert.</param>
     * <param name="separator">
     * A separator string inserted between flags. Defaults to <c>" | "</c>.
     * </param>
     * <returns>
     * A string containing all contained flags, joined by <paramref name="separator"/>.
     * If no flags are set, the default enum value is returned.
     * </returns>
     * */
    public static string ToStringAll<T>(this T bitmask, in string separator = " | ")
        where T : struct, Enum {
      var not_first            = false;
      StringBuilder str_result = new();
      // Add flags as string
      foreach (T flag in Enum.GetValues(typeof(T)))
        if (bitmask.ContainsFlag(flag)) {
          if (not_first) {
            not_first = true;
            str_result.Append(separator);
          }
          str_result.Append(flag.ToString());
        }
      // If no flags were added, attach the default flag
      if (str_result.Length <= 0)
        return default(T).ToString();
      // Remove last separator
      return str_result.ToString();
    }

    /**
     * <summary>
     * Determines whether a bitmask contains at least one flag from a provided collection.
     * </summary>
     * <typeparam name="T">Enum type representing flags.</typeparam>
     * <param name="list_flags">A collection of flags to test against.</param>
     * <param name="bitmask">The bitmask to inspect.</param>
     * <returns>
     * <c>true</c> if any flag in <paramref name="list_flags"/> is contained within
     * <paramref name="bitmask"/>; otherwise <c>false</c>.
     * </returns>
     * */
    public static bool InList<T>(this IReadOnlyCollection<T> list_flags, T bitmask)
        where T : struct, Enum {
      return list_flags.Count(x => bitmask.ContainsFlag(x)) != 0;
    }

    /// <inheritdoc cref="InList{T}(IReadOnlyCollection{T}, T)" />
    public static bool InList<T>(this T bitmask, IReadOnlyCollection<T> list_flags)
        where T : struct, Enum => list_flags.InList(bitmask);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int PopCount(ulong x) {
#if UNITY_5_3_OR_NEWER || UNITY_2017_1_OR_NEWER
      return math.countbits(x);
#else
      return BitOperations.PopCount(x);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CountRZero(ulong x) {
#if UNITY_5_3_OR_NEWER || UNITY_2017_1_OR_NEWER
      return math.tzcnt(x);
#else
      return BitOperations.TrailingZeroCount(x);
#endif
    }

    /**
     * <summary>
     * Converts a list of indices into a bitmask representation.
     * </summary>
     * <exception cref="ArgumentOutOfRangeException">
     * Thrown when one of the indices inside the list is either greater than or equal to 64 or is a
     * negative value.
     * </exception>
     * */
    public static ulong IndicesToMask(this IReadOnlyList<int> indices) {
      ulong result = 0;
      foreach (int index in indices) {
        if (index < 0 || index >= 64) {
          throw new ArgumentOutOfRangeException(nameof(indices), index,
              $"List contains index: {index} that cannot be stored in a UInt64.");
        }
        result |= 1ul << index;
      }
      return result;
    }

    /**
     * <summary>
     * Converts a list of booleans into its bitmask representation.
     * </summary>
     * <exception cref="ArgumentOutOfRangeException">
     * Thrown when the size of <paramref name="bools"/> is greater than 64;
     * </exception>
     * */
    public static ulong ToBitmask(this IReadOnlyList<bool> bools) {
      int len = bools.Count;
      if (len > 64) {
        throw new ArgumentOutOfRangeException(nameof(bools),
            $"Tried to convert a List of boolean that is too big (Count: {len}) for UInt64.");
      }
      ulong result = 0;
      for (int i = 0; i < len; ++i) {
        if (bools[i])
          result |= 1ul << i;
      }
      return result;
    }

    /**
     * <summary>
     * Converts the bitmask to a list. Listing position of each active bits.
     * </summary>
     * */
    public static void MaskToIndices(this ulong bitmask, IList<int> indices) {
      indices.Clear();
      if (indices is List<int> list) {
        int count = PopCount(bitmask);
        if (list.Capacity < count)
          list.Capacity = count;
      }
      while (bitmask != 0) {
        indices.Add(CountRZero(bitmask));
        bitmask &= (bitmask - 1);
      }
    }

    /// <inheritdoc cref="MaskToIndices(ulong, IList{int})"/>
    public static List<int> MaskToIndices(this ulong bitmask) {
      List<int> result = new(PopCount(bitmask));
      bitmask.MaskToIndices(result);
      return result;
    }

    /**
     * <summary>
     * Converts the bitmask to a List of booleans.
     * </summary>
     * <param name="size">
     *  <br/> Target Size for the List, in case where all 64 bits are not needed.
     *  <br/> Defaults to 64.
     * </param>
     * */
    public static void ToBoolList(this ulong bitmask, IList<bool> bools, int size = 64) {
      bools.Clear();
      if (bools is List<bool> list) {
        if (list.Capacity < size)
          list.Capacity = size;
      }
      for (int i = 0; i < size; ++i) {
        bools.Add((bitmask & (1ul << i)) != 0);
      }
    }

    /// <inheritdoc cref="ToBoolList(ulong, IList{bool})"/>
    public static List<bool> ToBoolList(this ulong bitmask, int size = 64) {
      List<bool> result = new(size);
      bitmask.ToBoolList(result, size);
      return result;
    }
  }
}
