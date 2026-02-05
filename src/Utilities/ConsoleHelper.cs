using System;

namespace MMOR.NET.Utilities {
  public static partial class Utilities {
    public delegate bool TryParseDelegate<T>(string s, out T result);

    /// <summary>
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
    ///     <br /> - <see cref="Console.ReadLine" /> with Utilities.
    ///     <br /> - Ignores any input that:
    ///     <br /> - 1. Cannot be parsed by <paramref name="parseFunc" />.
    ///     <br /> - 2. Does not pass <paramref name="predicate" /> check.
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="parseFunc"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static T ConsoleSafeRead<T>(TryParseDelegate<T> parseFunc,
        Func<T, bool>? predicate = null) {
      predicate ??=
          _ => true;
      while (true)
        if (parseFunc(Console.ReadLine(), out T result) && predicate(result))
          return result;
    }

    /// <summary>
    ///     <inheritdoc cref="ConsoleSafeRead{T}(TryParseDelegate{T}, Func{T, bool})" />
    /// </summary>
    public static void ConsoleSafeRead<T>(out T result, TryParseDelegate<T> parseFunc,
        Func<T, bool>? predicate = null) {
      result = ConsoleSafeRead(parseFunc, predicate);
    }

    /// <summary>
    ///     <inheritdoc cref="ConsoleSafeRead{T}(TryParseDelegate{T}, Func{T, bool})" />
    ///     <br /> - Cleaned up for <see langword="enum" /> and <see cref="FlagsAttribute" />.
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
    /// </summary>
    public static T ConsoleSafeRead<T>(out T result, Func<T, bool>? predicate = null)
        where T : struct, Enum {
      predicate ??=
          _   => true;
      int max  = MaxFlag<T>();
      while (true) {
        ConsoleSafeRead(out int tmpInt, int.TryParse, x => x >= 0 && x <= max);
        result = (T)Enum.ToObject(typeof(T), tmpInt);
        if (predicate(result))
          return result;
      }
    }
  }

  /// <summary>
  ///     <br /> - Robust <see cref="Console.WriteLine()" /> for printing in same area.
  /// </summary>
  public class ConsoleStationaryPrint {
    private int posEnd;
    private int? posSta;

    public void Print(in string text) {
      if (text.IsNullOrEmpty())
        return;

      posSta ??= Console.CursorTop;

      Console.SetCursorPosition(0, posSta.Value);
      string[] lines = text.Split(Environment.NewLine);
      var newLines   = 0;
      foreach (string line in lines) {
        int len         = line.Length;
        int bufferWidth = Console.BufferWidth;
        var cursor      = 0;

        // Handle wrapped texts
        while (cursor <= len) {
          int subLen = Math.Min(bufferWidth, len - cursor);
          Console.WriteLine(line.Substring(cursor, subLen).PadRight(bufferWidth));
          cursor += bufferWidth;
          newLines++;
        }
      }

      // If Previous was larger,
      // ..fill with empty lines
      for (int i = newLines; i <= posEnd; i++)
        Console.WriteLine(new string(' ', Console.BufferWidth));
      posEnd = newLines;
      Console.SetCursorPosition(0, posSta.Value + posEnd);
    }

    public void Clear() {
      posSta = null;
      posEnd = 0;
    }
  }
}
