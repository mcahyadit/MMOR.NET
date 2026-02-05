using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

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
}
