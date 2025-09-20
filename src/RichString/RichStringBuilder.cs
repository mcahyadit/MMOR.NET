using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using MMOR.NET.Utilities;

namespace MMOR.NET.RichString
{
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
}
