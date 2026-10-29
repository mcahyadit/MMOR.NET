using System;
using System.Buffers;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using MMOR.NET.Collections;

namespace MMOR.NET.RichStringV2 {
public sealed class RichStringBuilder : IRichString, IDisposable {
  public static readonly ArrayPool<char> kStringPool = ArrayPool<char>.Create();
  public static readonly ArrayPool<int> kInt32Pool   = ArrayPool<int>.Create();
  public static readonly ArrayPool<RichStringComponent> kCompPool =
      ArrayPool<RichStringComponent>.Create();

  public char[] string_mem_;
  public Span<char> string_buffer => string_mem_.AsSpan(0, Length);
  public int[] start_mem_;
  public Span<int> start_buffer => start_mem_.AsSpan(0, ComponentCount);
  public RichStringComponent[] comp_mem_;
  public Span<RichStringComponent> comp_buffer => comp_mem_.AsSpan(0, ComponentCount);

  public int StringCapacity    => string_mem_.Length;
  public int ComponentCapacity => Math.Min(comp_mem_.Length, start_mem_.Length);

  public int Length         = 0;
  public int ComponentCount = 0;

  public ReadOnlySpan<char> AsSpan() => string_buffer;

  public void Dispose() {
    if (string_mem_ is not null)
      kStringPool.Return(string_mem_);
    if (start_mem_ is not null)
      kInt32Pool.Return(start_mem_);
    if (comp_mem_ is not null)
      kCompPool.Return(comp_mem_);
  }

  public RichStringBuilder(int capacity) {
    string_mem_ = kStringPool.Rent(capacity);
    start_mem_  = kInt32Pool.Rent(4);
    comp_mem_   = kCompPool.Rent(4);
  }

  public RichStringBuilder(ReadOnlySpan<char> str) {
    Length      = str.Length;
    string_mem_ = kStringPool.Rent(Length);
    str.CopyTo(string_buffer);

    start_mem_ = kInt32Pool.Rent(4);
    comp_mem_  = kCompPool.Rent(4);
  }
}

public interface IRichString {
  public Span<char> string_buffer { get; }
  public Span<int> start_buffer { get; }
  public Span<RichStringComponent> comp_buffer { get; }
}

public enum RichStringType {
  Color,
  Style,
}

[StructLayout(LayoutKind.Sequential, Size = 16)]
public readonly struct RichStringComponent : IComparable<RichStringComponent> {
  public ulong data_ { get; init; }
  public int length_ { get; init; }
  public RichStringType type_ { get; init; }

  public int CompareTo(RichStringComponent other) {
    return length_.CompareTo(other.length_);
  }
}

public static class RichStringExtensions {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static RichStringBuilder AddComponent(this RichStringBuilder sb, int start,
      in RichStringComponent component) {
    int length     = 1;
    int min_length = sb.ComponentCount + length;

    int head  = CollectionUtils.HeadBinarySearch(sb.start_buffer, start);
    int tail  = CollectionUtils.TailGallopSearch(sb.start_buffer[head..], start);
    int index = CollectionUtils.TailBinarySearch(sb.comp_buffer.Slice(head, tail), component);
    if (min_length >= sb.ComponentCapacity) {
      int[] tmp_start                       = sb.start_mem_;
      RichStringComponent[] tmp_comp        = sb.comp_mem_;
      Span<int> start_buffer                = tmp_start.AsSpan(0, sb.ComponentCount);
      Span<RichStringComponent> comp_buffer = tmp_comp.AsSpan(0, sb.ComponentCount);

      sb.start_mem_ = RichStringBuilder.kInt32Pool.Rent(min_length);
      sb.comp_mem_  = RichStringBuilder.kCompPool.Rent(min_length);

      start_buffer[..index].CopyTo(sb.start_mem_);
      comp_buffer[..index].CopyTo(sb.comp_mem_);
      sb.start_mem_[index] = start;
      sb.comp_mem_[index]  = component;
      start_buffer[index..].CopyTo(sb.start_mem_.AsSpan(index + length));
      comp_buffer[index..].CopyTo(sb.comp_mem_.AsSpan(index + length));
      Array.Copy(tmp_start, sb.start_mem_, sb.ComponentCount);
      Array.Copy(tmp_comp, sb.comp_mem_, sb.ComponentCount);

      RichStringBuilder.kInt32Pool.Return(tmp_start);
      RichStringBuilder.kCompPool.Return(tmp_comp);
    } else {
      sb.start_buffer[index..].CopyTo(sb.start_mem_.AsSpan(index + length));
      sb.comp_buffer[index..].CopyTo(sb.comp_mem_.AsSpan(index + length));
      sb.start_mem_[index] = start;
      sb.comp_mem_[index]  = component;
    }
    sb.ComponentCount = min_length;
    return sb;
  }

  public static RichStringBuilder AddComponents(this RichStringBuilder sb,
      in ReadOnlySpan<int> starts, in ReadOnlySpan<RichStringComponent> comps) {
    int length = comps.Length;
    for (int i = 0; i < length; ++i) {
      sb.AddComponent(starts[i], comps[i]);
    }
    return sb;
  }

  public static RichStringBuilder Append(this RichStringBuilder sb, in ReadOnlySpan<char> str) {
    if (sb is null)
      throw new NullReferenceException();

    int length     = str.Length;
    int min_length = sb.Length + length;
    if (min_length >= sb.StringCapacity) {
      char[] tmp     = sb.string_mem_;
      sb.string_mem_ = RichStringBuilder.kStringPool.Rent(min_length);
      Array.Copy(tmp, sb.string_mem_, sb.Length);
      RichStringBuilder.kStringPool.Return(tmp);
    }
    str.CopyTo(sb.string_mem_.AsSpan(length));
    sb.Length = min_length;
    return sb;
  }

  public static RichStringBuilder Insert(this RichStringBuilder sb, int index,
      in ReadOnlySpan<char> str) {
    int length     = str.Length;
    int min_length = sb.Length + length;
    if (min_length >= sb.StringCapacity) {
      char[] tmp_mem        = sb.string_mem_;
      Span<char> tmp_buffer = tmp_mem.AsSpan(0, sb.Length);
      sb.string_mem_        = RichStringBuilder.kStringPool.Rent(min_length);
      tmp_buffer[..index].CopyTo(sb.string_mem_);
      str.CopyTo(sb.string_mem_.AsSpan(index));
      tmp_buffer[index..].CopyTo(sb.string_mem_.AsSpan(index + length));
      RichStringBuilder.kStringPool.Return(tmp_mem);
    } else {
      sb.string_buffer[index..].CopyTo(sb.string_mem_.AsSpan(index + length));
      str.CopyTo(sb.string_mem_.AsSpan(index));
    }
    sb.Length = min_length;
    return sb;
  }

  public static RichStringBuilder Append<T1, T2>(this T1 first, T2 second)
      where T1 : IRichString
      where T2 : IRichString {
    if (first is not RichStringBuilder sb) {
      sb = new(first.string_buffer);
      sb.AddComponents(first.start_buffer, first.comp_buffer);
    }
    sb.Append(second.string_buffer);
    sb.AddComponents(second.start_buffer, second.comp_buffer);
    return sb;
  }

  public static RichStringBuilder Replace(this RichStringBuilder sb,
      in ReadOnlySpan<char> old_value, in ReadOnlySpan<char> new_value) {
    int old_length = old_value.Length;
    int new_length = new_value.Length;
    int len_diff   = new_length - old_length;

    int last_component              = 0;
    Span<char> slice                = sb.string_buffer;
    Span<int> starts                = sb.start_buffer;
    Span<RichStringComponent> comps = sb.comp_buffer;
    int match                       = slice.IndexOf(old_value);
    while (match > 0 && last_component > 0) {
      last_component = CollectionUtils.HeadGallopSearch(starts, match) + 1;
      while (last_component > 0 &&  //
             starts[--last_component] + comps[last_component].length_ >= match) {
      }
      last_component = Math.Max(0, last_component);
      starts         = starts[last_component..];
      comps          = comps[last_component..];
      for (int i = 0; i < starts.Length && starts[i] < old_length; ++i) {
        RichStringComponent comp = comps[i];
        comps[i]                 = new RichStringComponent {
          data_   = comp.data_,
          length_ = comp.length_ + len_diff,
          type_   = comp.type_,
        };
      }
      CollectionUtils.AddAssign(starts, len_diff);

      int tmp_length = match + old_length;
      char[] tmp_mem = RichStringBuilder.kStringPool.Rent(slice[tmp_length..].Length);
      try {
        slice[tmp_length..].CopyTo(tmp_mem);
        sb.Length = match;
        sb.Append(new_value).Append(tmp_mem.AsSpan(0, slice[tmp_length..].Length));

        slice = slice[tmp_length..];
        match = slice.IndexOf(old_value);
      } finally {
        RichStringBuilder.kStringPool.Return(tmp_mem);
      }
    }
    return sb;
  }

  public static string Format(this RichStringBuilder rich) {
    int str_len  = rich.Length;
    int comp_cnt = rich.ComponentCount;

    StringBuilder sb        = new(rich.Length);
    Stack<string> close_tag = new(BitOperations.Log2((uint)comp_cnt));
    Stack<int> close_idx    = new(BitOperations.Log2((uint)comp_cnt));
    int str_cursor          = 0;
    for (int i = 0; i < comp_cnt; ++i) {
      int start = rich.start_buffer[i];

      if (close_idx.Count > 0) {
        while (close_idx.Peek() >= start) {
          int idx = close_idx.Pop();
          sb.Append(rich.string_buffer[str_cursor..idx]);
          sb.Append(close_tag.Pop());
          str_cursor = idx;
        }
      }

      if (start > str_cursor) {
        sb.Append(rich.string_buffer[str_cursor..start]);
        str_cursor = start;
      }

      RichStringComponent comp = rich.comp_buffer[i];
      switch (comp.type_) {
        case RichStringType.Color:
          sb.Append("<span style=\"color:");
          sb.Append(string.Format("{0:X8}", comp.data_));
          sb.Append("\">");
          break;
      }
      // TODO:
      // - check rich.comp_mem_[i].type_;
      sb.Append("TODO: Tag");

      close_idx.Push(start + comp.length_);
      close_tag.Push("TODO: Tag");
    }
    return sb.ToString();
  }
}

}
