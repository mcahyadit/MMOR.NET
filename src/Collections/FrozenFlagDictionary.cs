#pragma warning disable CS8600
using System;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace MMOR.NET.Collections {
/**
 * <summary>
 *  Provides an immutable read-only dictionary based on the mechanics of
 *  <see cref="Utilities.Utilities.SafeGetFlag{TFlag, TElement}"/>. <br/>
 *  Backed with <see cref="FrozenDictionary"/> and SIMD lookup for submasks.
 * </summary>
 */
public class FrozenFlagDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    where TKey : unmanaged, Enum {
#pragma warning disable CS8618
  public FrozenFlagDictionary() {}
#pragma warning restore CS8618

  public static readonly TKey kZero = (TKey)Enum.ToObject(typeof(TKey), 0);
  public FrozenDictionary<TKey, TValue> frozen_ { get; init; }
  public ImmutableArray<TKey> keys_ { get; init; }
  public TKey hot_key_;
  public TValue hot_value_;

  public int Count                  => keys_.Length;
  public IEnumerable<TKey> Keys     => keys_;
  public IEnumerable<TValue> Values => frozen_.Values;

  public TValue this[TKey key] {
    get {
      if (EqualityComparer<TKey>.Default.Equals(hot_key_, key))
        return hot_value_;

      if (frozen_.TryGetValue(key, out TValue result))
        return result;

      int idx = CollectionUtils.FirstSubmask(keys_, key);
      if (idx == -1) {
        hot_key_   = kZero;
        hot_value_ = frozen_[kZero];
        return hot_value_;
      }
      hot_key_   = keys_[idx];
      hot_value_ = frozen_[hot_key_];
      return hot_value_;
    }
  }

  /// <remarks> Perform stricter check than <see cref="TryGetValue"/> </remarks>
  public bool ContainsKey(TKey key) => frozen_.ContainsKey(key);

  public bool TryGetValue(TKey key, out TValue value) {
    value = this[key];
    return true;
  }

  public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>> {
    readonly ReadOnlyMemory<TKey> keys_;
    readonly Func<TKey, TValue> values_;
    int idx;

    public Enumerator(FrozenFlagDictionary<TKey, TValue> self) {
      idx     = -1;
      keys_   = self.keys_.AsMemory();
      values_ = x => self.frozen_[x];
    }

    public readonly KeyValuePair<TKey, TValue> Current {
      get {
        TKey key = keys_.Span[idx];
        return new KeyValuePair<TKey, TValue>(key, values_(key));
      }
    }
    readonly object IEnumerator.Current => Current;
    public readonly void Dispose() {}
    public bool MoveNext() => ++idx < keys_.Length;
    public void Reset() => idx = -1;
  }

  public override string ToString() {
    StringBuilder sb = new(string.Format("{0}[{1}]: {{\n",  //
        base.ToString(), Count));
    foreach (var kvp in this) {
      bool first = true;
      sb.Append("  [");
      foreach (TKey flag in Enum.GetValues(typeof(TKey))) {
        if (kvp.Key.HasFlag(flag)) {
          if (!first)
            sb.Append('|');
          sb.Append(flag);
          first = false;
        }
        if (first)
          sb.Append(kZero);
        sb.AppendFormat("]: {0}\n", kvp.Value);
      }
    }
    sb.Append('}');
    return sb.ToString();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Enumerator GetEnumerator() => new Enumerator(this);
  IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() =>
      GetEnumerator();
  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public static class FrozenFlagDictionary {
  [DoesNotReturn]
  public static void ThrowNoZero<T>(ReadOnlySpan<T> keys)
      where T : unmanaged, Enum {
    StringBuilder sb = new StringBuilder(
        string.Format("[ERROR]: {0} requires key of {1} to exist.\nKeys registered in source: [ ",
            typeof(FrozenDictionary), (T)Enum.ToObject(typeof(T), 0)));
    foreach (T key in keys) {
      sb.AppendFormat("{0}, ", key);
    }
    sb.Append(']');
    throw new ArgumentException(sb.ToString());
  }

  public static FrozenFlagDictionary<TKey, TValue> Create<TKey, TValue>(
      IDictionary<TKey, TValue> source)
      where TKey : unmanaged, Enum {
    int length      = source.Count;
    Span<TKey> keys = stackalloc TKey[length];
    int i           = 0;
    bool has_zero   = false;
    TKey zero       = (TKey)Enum.ToObject(typeof(TKey), 0);
    foreach ((TKey key, _) in source) {
      keys[i] = key;
      has_zero |= EqualityComparer<TKey>.Default.Equals(key, zero);
      ++i;
    }
    if (!has_zero) {
      ThrowNoZero<TKey>(keys);
    }
    return new FrozenFlagDictionary<TKey, TValue> {
      keys_      = ImmutableArray.Create(keys),
      frozen_    = FrozenDictionary.ToFrozenDictionary(source),
      hot_key_   = keys[^1],
      hot_value_ = source[keys[^1]],
    };
  }

  public static FrozenFlagDictionary<TKey, TValue> Create<TKey, TValue>(
      IReadOnlyDictionary<TKey, TValue> source)
      where TKey : unmanaged, Enum {
    int length      = source.Count;
    Span<TKey> keys = stackalloc TKey[length];
    int i           = 0;
    bool has_zero   = false;
    TKey zero       = (TKey)Enum.ToObject(typeof(TKey), 0);
    foreach ((TKey key, _) in source) {
      keys[i] = key;
      has_zero |= EqualityComparer<TKey>.Default.Equals(key, zero);
      ++i;
    }
    if (!has_zero) {
      ThrowNoZero<TKey>(keys);
    }
    return new FrozenFlagDictionary<TKey, TValue> {
      keys_      = ImmutableArray.Create(keys),
      frozen_    = FrozenDictionary.ToFrozenDictionary(source),
      hot_key_   = keys[^1],
      hot_value_ = source[keys[^1]],
    };
  }

  public static FrozenFlagDictionary<TKey, TValue> Create<TKey, TValue>(
      ReadOnlySpan<KeyValuePair<TKey, TValue>> source)
      where TKey : unmanaged, Enum {
    int length                    = source.Length;
    Span<TKey> keys               = stackalloc TKey[length];
    Dictionary<TKey, TValue> dict = new(length);
    bool has_zero                 = false;
    TKey zero                     = (TKey)Enum.ToObject(typeof(TKey), 0);
    for (int i = 0; i < length; ++i) {
      (TKey key, TValue val) = source[i];
      keys[i]                = key;
      has_zero |= EqualityComparer<TKey>.Default.Equals(key, zero);
      dict.Add(key, val);
    }
    if (!has_zero) {
      ThrowNoZero<TKey>(keys);
    }
    return new FrozenFlagDictionary<TKey, TValue> {
      keys_      = ImmutableArray.Create(keys),
      frozen_    = FrozenDictionary.ToFrozenDictionary(dict),
      hot_key_   = keys[^1],
      hot_value_ = source[^1].Value,
    };
  }
}
}
