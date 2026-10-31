#pragma warning disable CS8600
using System;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using MMOR.NET.Bits;

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

  public static readonly int kHashThreshold = 8 * Vector<byte>.Count / Unsafe.SizeOf<TKey>();
  public static readonly TKey kZero         = (TKey)Enum.ToObject(typeof(TKey), 0);

  public FrozenDictionary<TKey, TValue>? frozen_ { get; init; }
  public ImmutableArray<TValue> values_ { get; init; }
  public ImmutableArray<TKey> keys_ { get; init; }
  public TKey key_union_ { get; init; }

  // public readonly object hot_lock_ = new();
  // public TKey hot_key_;
  // public TValue hot_value_;

  public int Count                  => keys_.Length;
  public IEnumerable<TKey> Keys     => keys_;
  public IEnumerable<TValue> Values => frozen_ is not null ? frozen_.Values : values_!;

  public TValue this[TKey key] {
    get {
      // if (EqualityComparer<TKey>.Default.Equals(hot_key_, key))
      //   return hot_value_;

      key = EnumBitwise<TKey>.And(key, key_union_);
      if (frozen_ is not null) {
        if (frozen_.TryGetValue(key, out TValue result))
          return result;
        int idx = CollectionUtils.FirstSubmask(keys_, key);
        return frozen_[keys_[idx]];
      } else {
        return values_[CollectionUtils.IndexOfOrFirstSubmask(keys_, key)];
      }

      // bool locked = false;
      // try {
      //   Monitor.TryEnter(hot_lock_, ref locked);
      //   if (locked) {
      //     hot_key_   = key;
      //     hot_value_ = frozen_[keys_[idx]];
      //   }
      // } finally {
      //   if (locked)
      //     Monitor.Exit(hot_lock_);
      // }
      // return hot_value_;
    }
  }

  public bool ContainsKey(TKey key) => true;

  public bool TryGetValue(TKey key, out TValue value) {
    value = this[key];
    return true;
  }

  public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>> {
    readonly ReadOnlyMemory<TKey> keys_;
    readonly ReadOnlyMemory<TValue> values_;
    readonly Func<TKey, TValue>? hashtable_;
    int idx;

    public Enumerator(FrozenFlagDictionary<TKey, TValue> self) {
      idx        = -1;
      keys_      = self.keys_.AsMemory();
      values_    = self.values_.AsMemory();
      hashtable_ = self.frozen_ is not null? x => self.frozen_[x] : null;
    }

    public readonly KeyValuePair<TKey, TValue> Current {
      get {
        if (hashtable_ is null)
          return new KeyValuePair<TKey, TValue>(keys_.Span[idx], values_.Span[idx]);

        TKey key = keys_.Span[idx];
        return new KeyValuePair<TKey, TValue>(key, hashtable_(key));
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
            typeof(FrozenFlagDictionary), (T)Enum.ToObject(typeof(T), 0)));
    foreach (T key in keys) {
      sb.AppendFormat("{0}, ", key);
    }
    sb.Append(']');
    throw new ArgumentException(sb.ToString());
  }
  public static void ThrowZeroNotLast<T>()
      where T : unmanaged, Enum {
    throw new ArgumentException(
        string.Format("[ERROR]: {0} requires key of {1} needs to be the last key.",
            typeof(FrozenFlagDictionary), (T)Enum.ToObject(typeof(T), 0)));
  }

  public static FrozenFlagDictionary<TKey, TValue> Create<TKey, TValue>(
      IDictionary<TKey, TValue> source)
      where TKey : unmanaged, Enum {
    int length    = source.Count;
    bool use_hash = length >= FrozenFlagDictionary<TKey, TValue>.kHashThreshold;

    Span<TKey> keys = stackalloc TKey[length];
    int i           = 0;
    bool has_zero   = false;
    TKey zero       = (TKey)Enum.ToObject(typeof(TKey), 0);
    TKey key_union  = zero;
    foreach ((TKey key, TValue val) in source) {
      keys[i] = key;
      has_zero |= EqualityComparer<TKey>.Default.Equals(key, zero);
      key_union = EnumBitwise<TKey>.Or(key_union, key);
      ++i;
    }
    if (!has_zero) {
      ThrowNoZero<TKey>(keys);
    }
    if (!EqualityComparer<TKey>.Default.Equals(keys[^1], zero)) {
      ThrowZeroNotLast<TKey>();
    }
    return new FrozenFlagDictionary<TKey, TValue> {
      keys_      = ImmutableArray.Create(keys),
      frozen_    = use_hash ? FrozenDictionary.ToFrozenDictionary(source) : null,
      values_    = use_hash ? ImmutableArray<TValue>.Empty : source.Values.ToImmutableArray(),
      key_union_ = key_union,
      // hot_key_   = keys[^1],
      // hot_value_ = source[keys[^1]],
    };
  }

  public static FrozenFlagDictionary<TKey, TValue> Create<TKey, TValue>(
      IReadOnlyDictionary<TKey, TValue> source)
      where TKey : unmanaged, Enum {
    int length    = source.Count;
    bool use_hash = length >= FrozenFlagDictionary<TKey, TValue>.kHashThreshold;

    Span<TKey> keys = stackalloc TKey[length];
    int i           = 0;
    bool has_zero   = false;
    TKey zero       = (TKey)Enum.ToObject(typeof(TKey), 0);
    TKey key_union  = zero;
    foreach ((TKey key, _) in source) {
      keys[i] = key;
      has_zero |= EqualityComparer<TKey>.Default.Equals(key, zero);
      key_union = EnumBitwise<TKey>.Or(key_union, key);
      ++i;
    }
    if (!has_zero) {
      ThrowNoZero<TKey>(keys);
    }
    if (!EqualityComparer<TKey>.Default.Equals(keys[^1], zero)) {
      ThrowZeroNotLast<TKey>();
    }
    return new FrozenFlagDictionary<TKey, TValue> {
      keys_      = ImmutableArray.Create(keys),
      frozen_    = use_hash ? FrozenDictionary.ToFrozenDictionary(source) : null,
      values_    = use_hash ? ImmutableArray<TValue>.Empty : source.Values.ToImmutableArray(),
      key_union_ = key_union,
      // hot_key_   = keys[^1],
      // hot_value_ = source[keys[^1]],
    };
  }

  public static FrozenFlagDictionary<TKey, TValue> Create<TKey, TValue>(
      ReadOnlySpan<KeyValuePair<TKey, TValue>> source)
      where TKey : unmanaged, Enum {
    int length    = source.Length;
    bool use_hash = length >= FrozenFlagDictionary<TKey, TValue>.kHashThreshold;

    Span<TKey> keys                = stackalloc TKey[length];
    TValue[]? values               = use_hash ? null : new TValue[length];
    Dictionary<TKey, TValue>? dict = use_hash ? new(length) : null;
    bool has_zero                  = false;
    TKey zero                      = (TKey)Enum.ToObject(typeof(TKey), 0);
    TKey key_union                 = zero;
    for (int i = 0; i < length; ++i) {
      (TKey key, TValue val) = source[i];
      keys[i]                = key;
      has_zero |= EqualityComparer<TKey>.Default.Equals(key, zero);
      key_union = EnumBitwise<TKey>.Or(key_union, key);
      if (use_hash) {
        dict!.Add(key, val);
      } else {
        values![i] = val;
      }
    }
    if (!has_zero) {
      ThrowNoZero<TKey>(keys);
    }
    if (!EqualityComparer<TKey>.Default.Equals(keys[^1], zero)) {
      ThrowZeroNotLast<TKey>();
    }
    return new FrozenFlagDictionary<TKey, TValue> {
      keys_      = ImmutableArray.Create(keys),
      frozen_    = use_hash ? FrozenDictionary.ToFrozenDictionary(dict!) : null,
      values_    = use_hash ? ImmutableArray<TValue>.Empty : ImmutableArray.Create(values),
      key_union_ = key_union,
      // hot_key_   = keys[^1],
      // hot_value_ = source[keys[^1]],
    };
  }
}
}
