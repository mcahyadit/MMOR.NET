using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMOR.NET.Utilities {
  public static partial class Utilities {
    /// <summary>
    /// Determines whether a bitmask contains a specified flag, with special handling
    /// for the <c>0</c> flag.
    /// </summary>
    /// <typeparam name="T">Enum type representing flags.</typeparam>
    /// <param name="bitmask">The bitmask to test.</param>
    /// <param name="flag">The flag to check for.</param>
    /// <returns>
    /// <c>true</c> if the <paramref name="bitmask"/> contains <paramref name="flag"/>.
    /// When <paramref name="flag"/> is <c>0</c>, returns <c>true</c> only if
    /// <paramref name="bitmask"/> is also <c>0</c>.
    /// </returns>
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

    /// <summary>
    /// Computes the maximum possible bitmask value for an enum type by taking all defined flags
    /// and producing a mask containing all bits up to the highest flag.
    /// </summary>
    /// <typeparam name="T">Enum type representing flags.</typeparam>
    /// <returns>
    /// The maximum combined-flag value representable by the enum, based on
    /// all declared enum values.
    /// </returns>
    public static int MaxFlag<T>()
        where T : struct, Enum => (Enum.GetValues(typeof(T)).Cast<int>().Max() << 1) - 1;

    /// <inheritdoc cref="MaxFlag{T}" />
    public static T MaxFlagCasted<T>()
        where T : struct, Enum => (T) Enum.ToObject(typeof(T), MaxFlag<T>());

    /// <summary>
    /// Retrieves a value mapped to a flag from a flag-based dictionary. If the exact flag
    /// is not present, the method returns the value of the first key whose flag is contained
    /// within the requested flag, falling back to the default key.
    /// </summary>
    /// <typeparam name="TFlag">An enum type representing flags.</typeparam>
    /// <typeparam name="TElement">The value type mapped to flags.</typeparam>
    /// <param name="dictionary">A dictionary mapping flags to values.</param>
    /// <param name="flag">The flag whose associated value is desired.</param>
    /// <returns>
    /// The matched value, or the value mapped to the default key if no applicable flag is found.
    /// </returns>
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

    /// <summary>
    /// Prepopulates a dictionary with values for every possible bitmask combination for
    /// <typeparamref name="TFlag"/>. Missing entries are resolved either from matching
    /// sub-flags or from the default value.
    /// </summary>
    /// <typeparam name="TFlag">An enum type representing flags.</typeparam>
    /// <typeparam name="TElement">The value type mapped to flags.</typeparam>
    /// <param name="dictionary">
    /// The dictionary containing the explicitly mapped flag values.
    /// </param>
    /// <returns>
    /// A new dictionary where all integer flag values up to <see cref="MaxFlag{TFlag}"/>
    /// map to the most appropriate element.
    /// </returns>
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

    /// <summary>
    /// Produces a formatted string representing all flags present in the bitmask.
    /// </summary>
    /// <typeparam name="T">Enum type representing flags.</typeparam>
    /// <param name="bitmask">The bitmask to convert.</param>
    /// <param name="separator">
    /// A separator string inserted between flags. Defaults to <c>" | "</c>.
    /// </param>
    /// <returns>
    /// A string containing all contained flags, joined by <paramref name="separator"/>.
    /// If no flags are set, the default enum value is returned.
    /// </returns>
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

    /// <summary>
    /// Determines whether a bitmask contains at least one flag from a provided collection.
    /// </summary>
    /// <typeparam name="T">Enum type representing flags.</typeparam>
    /// <param name="list_flags">A collection of flags to test against.</param>
    /// <param name="bitmask">The bitmask to inspect.</param>
    /// <returns>
    /// <c>true</c> if any flag in <paramref name="list_flags"/> is contained within
    /// <paramref name="bitmask"/>; otherwise <c>false</c>.
    /// </returns>
    public static bool InList<T>(this IReadOnlyCollection<T> list_flags, T bitmask)
        where T : struct, Enum {
      return list_flags.Count(x => bitmask.ContainsFlag(x)) != 0;
    }

    /// <inheritdoc cref="InList{T}(IReadOnlyCollection{T}, T)" />
    public static bool InList<T>(this T bitmask, IReadOnlyCollection<T> list_flags)
        where T : struct, Enum => list_flags.InList(bitmask);
  }
}
