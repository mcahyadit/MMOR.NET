using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using MMOR.NET.Bits;

namespace MMOR.NET.Collections {
public static class FrozenFlagDictionary {
  public static class ThrowHelper {
    [DoesNotReturn]
    public static void NoZero<T>(ReadOnlySpan<T> keys)
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

    [DoesNotReturn]
    public static void ZeroNotLast<T>()
        where T : unmanaged, Enum {
      throw new ArgumentException(
          string.Format("[ERROR]: {0} requires key of {1} needs to be the last key.",
              typeof(FrozenFlagDictionary), (T)Enum.ToObject(typeof(T), 0)));
    }

    [DoesNotReturn]
    public static void DuplicateKey<T>(ReadOnlySpan<T> keys, T duplicate)
        where T : unmanaged, Enum {
      int first  = keys.IndexOf(duplicate);
      int second = first + 1 + keys[(first + 1)..].IndexOf(duplicate);
      throw new ArgumentException(string.Format(
          "[ERROR]: Detected duplicate key {0} at idx {1} and {2}.", duplicate, first, second));
    }
  }

  public static bool UseHash<T>(int length)
      where T : unmanaged {
    return Unsafe.SizeOf<T>() > 1 &&                              // No Hash for u8
           length * Unsafe.SizeOf<T>() > 8 * Vector<byte>.Count;  // Approximately 4 SIMD Iterations
  }

  public static FrozenFlagDictionary<TKey, TValue> Create<TKey, TValue>(
      ReadOnlySpan<KeyValuePair<TKey, TValue>> source)
      where TKey : unmanaged, Enum {
    int length    = source.Length;
    bool use_hash = UseHash<TKey>(length);

    TKey zero      = (TKey)Enum.ToObject(typeof(TKey), 0);
    bool has_zero  = false;
    TKey key_union = zero;
    TKey key_xor   = zero;

    TKey[] keys    = new TKey[length];
    TValue[]? vals = use_hash ? null : new TValue[length];
    // TODO: Custom Specialized Hashing
    Dictionary<TKey, TValue>? dict = use_hash ? new(length) : null;

    for (int i = 0; i < length; ++i) {
      (TKey key, TValue val) = source[i];
      keys[i]                = key;
      if (CollectionUtils.IndexOf(keys.AsSpan(0, i), key) != -1) {
        ThrowHelper.DuplicateKey(keys.AsSpan(0, i + 1), key);
      }
      if (use_hash) dict!.Add(key, val);
      else vals![i] = val;

      has_zero |= EqualityComparer<TKey>.Default.Equals(key, zero);
      key_union = EnumOps.BitwiseOr(key_union, key);
      if (i < length - 1) key_xor = i == 0 ? key : EnumOps.BitwiseXor(key_xor, key);
    }

    if (!has_zero) {
      ThrowHelper.NoZero<TKey>(keys);
    }
    if (!EqualityComparer<TKey>.Default.Equals(keys[^1], zero)) {
      ThrowHelper.ZeroNotLast<TKey>();
    }

    return new FrozenFlagDictionary<TKey, TValue> {
      keys_      = ImmutableArray.Create(keys),
      vals_      = use_hash ? ImmutableArray<TValue>.Empty : ImmutableArray.Create(vals),
      dict_      = use_hash ? FrozenDictionary.ToFrozenDictionary(dict!) : null,
      union_     = key_union,
      FreezeType = use_hash ? FreezeType.Hashed : FreezeType.Linear,
    };
  }

  public static FrozenFlagDictionary<TKey, TValue> Create<TKey, TValue>(
      Dictionary<TKey, TValue> source)
      where TKey : unmanaged, Enum {
    int length    = source.Count;
    bool use_hash = UseHash<TKey>(length);

    TKey zero      = (TKey)Enum.ToObject(typeof(TKey), 0);
    bool has_zero  = false;
    TKey key_union = zero;
    TKey key_xor   = zero;

    TKey[] keys    = new TKey[length];
    TValue[]? vals = use_hash ? null : new TValue[length];
    // TODO: Custom Specialized Hashing
    Dictionary<TKey, TValue>? dict = use_hash ? new(length) : null;

    int i = 0;
    foreach ((TKey key, TValue val) in source) {
      keys[i] = key;
      if (CollectionUtils.IndexOf(keys.AsSpan(0, i), key) != -1) {
        ThrowHelper.DuplicateKey(keys.AsSpan(0, i + 1), key);
      }
      if (use_hash) dict!.Add(key, val);
      else vals![i] = val;

      has_zero |= EqualityComparer<TKey>.Default.Equals(key, zero);
      key_union = EnumOps.BitwiseOr(key_union, key);
      if (i < length - 1) key_xor = i == 0 ? key : EnumOps.BitwiseXor(key_xor, key);
      ++i;
    }

    if (!has_zero) {
      ThrowHelper.NoZero<TKey>(keys);
    }
    if (!EqualityComparer<TKey>.Default.Equals(keys[^1], zero)) {
      ThrowHelper.ZeroNotLast<TKey>();
    }

    return new FrozenFlagDictionary<TKey, TValue> {
      keys_      = ImmutableArray.Create(keys),
      vals_      = use_hash ? ImmutableArray<TValue>.Empty : ImmutableArray.Create(vals),
      dict_      = use_hash ? FrozenDictionary.ToFrozenDictionary(dict!) : null,
      union_     = key_union,
      FreezeType = use_hash ? FreezeType.Hashed : FreezeType.Linear,
    };
  }
}
}
