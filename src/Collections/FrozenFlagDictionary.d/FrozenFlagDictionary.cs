#pragma warning disable CS8600  // Converting null literal or possible null value to non-nullable
// type.
using System;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using MMOR.NET.Bits;
using MMOR.NET.Collections.Hash;
using MMOR.NET.Mathematics;

namespace MMOR.NET.Collections {
public enum FreezeType {
  /**
   * <summary>
   *  FrozenDictionary Implemented as Pair of Array, <br/>
   *  Lookup by SIMD-Accelerated Linear Search. <br/>
   *  Triggered when size is small enough that Hash is slower.
   * </summary>
   */
  Linear = 0,
  Hashed,
}

/**
 * <summary>
 *  Provides an immutable read-only dictionary based on the mechanics of
 *  <see cref="Utilities.Utilities.SafeGetFlag{TFlag, TElement}"/>. <br/>
 * </summary>
 */
public sealed class FrozenFlagDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    where TKey : unmanaged, Enum {
  public static readonly TKey kZero = (TKey)Enum.ToObject(typeof(TKey), 0);

  public ImmutableArray<TValue> vals_ { get; init; }
  public FrozenDictionary<TKey, TValue>? dict_ { get; init; }
  public ImmutableArray<TKey> keys_ { get; init; }
  public TKey union_ { get; init; }

  public FreezeType FreezeType { get; init; }

  public int Count                  => keys_.Length;
  public IEnumerable<TKey> Keys     => keys_;
  public IEnumerable<TValue> Values => vals_;

  public TValue this[TKey key] {
    get {
      // Keys outside of key_union_ doesn't need to be matched
      key = EnumOps.BitwiseAnd(key, union_);

      if (EqualityComparer<TKey>.Default.Equals(key, kZero)) return vals_[^1];

      if (FreezeType == FreezeType.Hashed) {
        if (dict_!.TryGetValue(key, out TValue val)) {
          return val;
        }
        return dict_[keys_[CollectionUtils.FirstSubmask(keys_, key)]];
      } else {
        return vals_[CollectionUtils.IndexOfOrFirstSubmask(keys_, key)];
      }
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
    readonly FrozenDictionary<TKey, TValue>? dict_;
    readonly FreezeType freeze_type_;
    int idx;

    public Enumerator(FrozenFlagDictionary<TKey, TValue> self) {
      idx          = -1;
      keys_        = self.keys_.AsMemory();
      values_      = self.vals_.AsMemory();
      dict_        = self.dict_;
      freeze_type_ = self.FreezeType;
    }

    public readonly KeyValuePair<TKey, TValue> Current {
      get {
        if (freeze_type_ == FreezeType.Linear)
          return new KeyValuePair<TKey, TValue>(keys_.Span[idx], values_.Span[idx]);
        TKey key = keys_.Span[idx];
        return new KeyValuePair<TKey, TValue>(key, dict_![key]);
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
          if (!first) sb.Append('|');
          sb.Append(flag);
          first = false;
        }
        if (first) sb.Append(kZero);
      }
      sb.AppendFormat("]: {0}\n", kvp.Value);
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
}
