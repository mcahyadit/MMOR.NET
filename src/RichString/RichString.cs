using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using MMOR.NET.Utilities;

namespace MMOR.NET.RichString
{
  public interface IRichStringFormatter
  {
    public StringBuilder Format(IRichString rich_str, StringBuilder? result);

    public string Format(IRichString rich_str) => Format(rich_str, null).ToString();
  }

  public interface IRichString
  {
    //-+-+-+-+-+-+-+-+
    // Generic String API
    //-+-+-+-+-+-+-+-+
    int Length { get; }
    bool IsNullOrEmpty();
    bool IsNullOrWhiteSpace();
    IRichString Replace(in string old_value, in string new_value);
    IRichString Replace(char old_value, char new_value);
    IRichString Remove(int start_index, int length);
    IRichString Remove(int length) => Remove(0, length);

    // Special
    public static IRichString operator +(IRichString left, IRichString right) =>
      new RichStringBuilder(left, right);

    public IRichString Clone();
  }

  public interface IRecursiveRichString : IRichString
  {
    public IRichString str { get; }

    int IRichString.Length => str.Length;
    bool IRichString.IsNullOrEmpty() => str.IsNullOrEmpty();
    bool IRichString.IsNullOrWhiteSpace() => str.IsNullOrWhiteSpace();

    public IRichString ReplaceString(IRichString new_str);

    [Pure]
    IRichString IRichString.Replace(in string old_value, in string new_value) =>
      ReplaceString(str.Replace(old_value, new_value));

    [Pure]
    IRichString IRichString.Replace(char old_value, char new_value) =>
      ReplaceString(str.Replace(old_value, new_value));

    [Pure]
    IRichString IRichString.Remove(int start_index, int length) =>
      ReplaceString(str.Remove(start_index, length));
  }

  //-+-+-+-+-+-+-+-+
  // Recursive Class Lowest Level
  //-+-+-+-+-+-+-+-+
  public class RichStringPlain : IRichString
  {
    public readonly string str;

    public RichStringPlain(object obj) => str = obj.ToString();

    public static implicit operator RichStringPlain(string str) => new(str);

    public override string ToString() => str;

    public IRichString Clone() => new RichStringPlain(str);

    //-+-+-+-+-+-+-+-+
    // Generic String API
    //-+-+-+-+-+-+-+-+
    #region Generic String API
    public bool IsNullOrEmpty() => string.IsNullOrEmpty(str);

    public bool IsNullOrWhiteSpace() => string.IsNullOrWhiteSpace(str);

    public int Length => str.Length;

    public IRichString Replace(in string old_value, in string new_value)
    {
      return new RichStringPlain(str.Replace(old_value, new_value));
    }

    public IRichString Replace(char old_value, char new_value)
    {
      return new RichStringPlain(str.Replace(old_value, new_value));
    }

    public IRichString Remove(int start_value, int length)
    {
      return new RichStringPlain(str.Remove(start_value, length));
    }

    public IList<IRichString> Split(string separator)
    {
      return str.Split(separator).Select(x => (IRichString)x.AsRichString()).ToList();
    }
    #endregion
  }

  //-+-+-+-+-+-+-+-+

  //-+-+-+-+-+-+-+-+
  // Container for Appending
  //-+-+-+-+-+-+-+-+
  public class RichStringBuilder : IRichString
  {
    private readonly List<IRichString> components_;

    public IReadOnlyList<IRichString> Components => components_;

    public RichStringBuilder() => components_ = new List<IRichString>();

    public RichStringBuilder(params IRichString[] parts) => components_ = parts.ToList();

    public RichStringBuilder(params string[] parts) =>
      components_ = parts.Select(x => (IRichString)x.AsRichString()).ToList();

    public RichStringBuilder(RichStringBuilder copy) => components_ = copy.components_.ToList();

    public IRichString Clone() => new RichStringBuilder(this);

    public void Clear()
    {
      components_.Clear();
    }

    //-+-+-+-+-+-+-+-+
    // Generic String API
    //-+-+-+-+-+-+-+-+
    #region Generic String API
    public int Length => components_.Sum(x => x.Length);

    public bool IsNullOrEmpty() =>
      components_.IsNullOrEmpty() || components_.All(x => x.IsNullOrEmpty());

    public bool IsNullOrWhiteSpace() =>
      components_.IsNullOrEmpty() || components_.All(x => x.IsNullOrWhiteSpace());

    public IRichString Replace(in string old_value, in string new_value)
    {
      foreach (IRichString? component in components_)
      {
        component.Replace(old_value, new_value);
      }
      return this;
    }

    public IRichString Replace(char old_value, char new_value)
    {
      foreach (IRichString? component in components_)
      {
        component.Replace(old_value, new_value);
      }
      return this;
    }

    public override string ToString() => throw new InvalidCastException();

    public IRichString Remove(int start_index, int length)
    {
      if (this.IsNullOrEmpty() || start_index >= this.Length)
        return this;
      if (length <= 0 || start_index < 0)
        throw new ArgumentOutOfRangeException($"Remove({start_index}, {length})");

      int remaining_length = length;
      var p_char_dex = 0;
      int comp_count = components_.Count;
      for (var comp_dex = 0; comp_dex < comp_count && remaining_length > 0; )
      {
        IRichString comp = components_[comp_dex];
        int comp_len = comp.Length;

        // Skip until start_index
        if (start_index >= p_char_dex + comp_len)
        {
          p_char_dex += comp_len;
          ++comp_dex;
          continue;
        }

        int local_start_dex = start_index - p_char_dex;

        if (local_start_dex == 0 && remaining_length >= comp_len)
        {
          components_.RemoveAt(comp_dex);
          --comp_count;
        }
        else
        {
          comp.Remove(local_start_dex, remaining_length);
          ++comp_dex;
        }

        remaining_length -= comp_len;
        p_char_dex += comp_len;
      }
      return this;
    }

    #endregion
    //-+-+-+-+-+-+-+-+
    // Operators
    //-+-+-+-+-+-+-+-+
    #region Operators
    public static RichStringBuilder operator +(RichStringBuilder left, RichStringBuilder right)
    {
      var copy = new RichStringBuilder(left);
      copy.components_.AddRange(right.components_);
      return copy;
    }

    public static RichStringBuilder operator +(RichStringBuilder left, IRichString right)
    {
      var copy = new RichStringBuilder(left);
      copy.components_.Add(right);
      return copy;
    }

    public static RichStringBuilder operator +(IRichString left, RichStringBuilder right)
    {
      var copy = new RichStringBuilder(right);
      copy.components_.Insert(0, left);
      return copy;
    }

    public static RichStringBuilder operator +(string left, RichStringBuilder right)
    {
      var copy = new RichStringBuilder(right);
      copy.components_.Insert(0, (RichStringPlain)left);
      return copy;
    }

    public static RichStringBuilder operator +(RichStringBuilder left, string right)
    {
      var copy = new RichStringBuilder(left);
      copy.components_.Add((RichStringPlain)right);
      return copy;
    }

    public RichStringBuilder AppendLine()
    {
      components_.Add(RichStringUtils.kRichLineBreak);
      return this;
    }

    public RichStringBuilder Append(IRichString component)
    {
      components_.Add(component);
      return this;
    }

    public RichStringBuilder AppendLine(IRichString component)
    {
      components_.Add(component);
      components_.Add(RichStringUtils.kRichLineBreak);
      return this;
    }

    public RichStringBuilder Prepend(IRichString component)
    {
      components_.Insert(0, component);
      return this;
    }

    public RichStringBuilder Append(in object component)
    {
      components_.Add((RichStringPlain)component.ToString());
      return this;
    }

    public RichStringBuilder Prepend(in object component)
    {
      components_.Insert(0, (RichStringPlain)component.ToString());
      return this;
    }

    public RichStringBuilder AppendLine(in object component)
    {
      components_.Add((RichStringPlain)component.ToString());
      components_.Add(RichStringUtils.kRichLineBreak);
      return this;
    }

    //-+-+-+-+-+-+-+-+
    #endregion
  }

  public static partial class RichStringUtils
  {
    public static readonly RichStringPlain kRichLineBreak = Environment.NewLine;
    public static readonly RichStringPlain kRichEmpty = string.Empty;

    public static string Format(this IRichString rich_str, in IRichStringFormatter formatter) =>
      formatter.Format(rich_str);

    public static RichStringBuilder PadSmart(
      this IRichString rich_str,
      int total_width,
      char padding_char = ' '
    ) =>
      total_width < 0
        ? PadLeft(rich_str, -total_width, padding_char)
        : PadRight(rich_str, total_width, padding_char);

    public static RichStringBuilder PadLeft(
      this IRichString rich_str,
      int total_width,
      char padding_char = ' '
    )
    {
      var result = new RichStringBuilder(rich_str);
      result.Prepend(
        (RichStringPlain)new string(padding_char, Math.Max(0, total_width - rich_str.Length))
      );
      return result;
    }

    public static RichStringBuilder PadRight(
      this IRichString rich_str,
      int total_width,
      char padding_char = ' '
    )
    {
      var result = new RichStringBuilder(rich_str);
      result.Append(
        (RichStringPlain)new string(padding_char, Math.Max(0, total_width - rich_str.Length))
      );
      return result;
    }

    public static RichStringBuilder PadCenter(
      this IRichString rich_str,
      int total_width,
      char padding_char = ' '
    )
    {
      var result = new RichStringBuilder(rich_str);
      int total = Math.Abs(total_width) - rich_str.Length;
      int left = total / 2;
      if (total_width < 0)
        total += 1;
      int right = total - left;
      result.Prepend((RichStringPlain)new string(padding_char, left));
      result.Append((RichStringPlain)new string(padding_char, right));
      return result;
    }

    public static IList<IRichString> SplitByNewLine(this IRichString input)
    {
      var result = new List<IRichString>();
      var currentLine = new RichStringBuilder();

      Process(input);
      EmitLine(); // flush the last line
      return result;

      void Process(IRichString rich)
      {
        switch (rich)
        {
          case RichStringPlain plain:
            string[] lines = plain.str.Split(Environment.NewLine);
            int len = lines.Length;

            for (var i = 0; i < len; ++i)
            {
              if (i > 0)
                EmitLine(); // newline was found
              if (lines[i].Length > 0)
                currentLine.Append((RichStringPlain)lines[i]);
            }
            break;
          case RichStringBuilder builder:
          {
            foreach (var component in builder.Components)
              Process(component);
            break;
          }

          case IRecursiveRichString rec:
          {
            // Split inner parts and wrap back in the same decorator
            foreach (IRichString? part in rec.str.SplitByNewLine())
            {
              if (part.Length == 0)
              {
                EmitLine();
              }
              else
              {
                IRichString clone = rec.Clone();
                if (clone is IRecursiveRichString wrapped)
                  currentLine.Append(wrapped.ReplaceString(part));
                else
                  currentLine.Append(clone);
              }
            }
            break;
          }

          default:
            currentLine.Append(rich); // fallback
            break;
        }
      }

      void EmitLine()
      {
        result.Add(
          currentLine.Components.Count == 0 ? kRichEmpty : new RichStringBuilder(currentLine)
        );
        currentLine.Clear();
      }
    }

    public static RichStringPlain AsRichString(this string str) => str;
  }
}
