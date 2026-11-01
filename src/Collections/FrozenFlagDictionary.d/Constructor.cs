using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using MMOR.NET.Bits;
using MMOR.NET.Collections.Hash;
using MMOR.NET.Mathematics;
using MMOR.NET.Utilities;

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
    int length = source.Length;
    // TODO: Hashed part still broken
    bool use_hash = false && UseHash<TKey>(length);

    TKey zero      = (TKey)Enum.ToObject(typeof(TKey), 0);
    bool has_zero  = false;
    TKey key_union = zero;
    TKey key_xor   = zero;

    TKey[] keys   = new TKey[length];
    TValue[] vals = new TValue[length];
    TKey[] hash   = new TKey[length - 1];

    for (int i = 0; i < length; ++i) {
      (TKey key, TValue val) = source[i];
      keys[i]                = key;
      vals[i]                = val;
      if (CollectionUtils.IndexOf(keys.AsSpan(0, i), key) != -1) {
        ThrowHelper.DuplicateKey(keys.AsSpan(0, i + 1), key);
      }

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

    ulong seed = 0;
    if (use_hash) {
      // Brute Force find seed
      for (int i = 0; i < length - 1; ++i) {
        TKey key = keys[i];
        ulong idx =
            MathExt.FastRange64(HashUtils.MurmurHash2A(key.ToUInt64(), seed), (ulong)length - 1);
        if (!EqualityComparer<TKey>.Default.Equals(hash[idx], zero)) {
          i = -1;
          ++seed;
          hash.Clear();
        }
        hash[idx] = key;
      }
      keys.AsSpan() [..(length - 1)].CopyTo(hash);
      CollectionUtils.PdqSort(hash.AsSpan(), vals.AsSpan(0, length - 1),
          new _Hasher1<TKey>(key => (int)MathExt.FastRange64(
                                 HashUtils.MurmurHash2A(key.ToUInt64(), seed), (ulong)length - 1)));
    }

    return new FrozenFlagDictionary<TKey, TValue> {
      keys_      = ImmutableArray.Create(keys),
      vals_      = ImmutableArray.Create(vals),
      hash_      = !use_hash ? ImmutableArray<TKey>.Empty : ImmutableArray.Create(hash),
      union_     = key_union,
      hash_seed  = seed,
      FreezeType = use_hash ? FreezeType.Hashed : FreezeType.Linear,
    };
  }

  public static FrozenFlagDictionary<TKey, TValue> Create<TKey, TValue>(
      Dictionary<TKey, TValue> source)
      where TKey : unmanaged, Enum {
    int length = source.Count;
    // TODO: Hashed part still broken
    bool use_hash = UseHash<TKey>(length);

    TKey zero      = (TKey)Enum.ToObject(typeof(TKey), 0);
    bool has_zero  = false;
    TKey key_union = zero;
    TKey key_xor   = zero;

    TKey[] keys   = new TKey[length];
    TValue[] vals = new TValue[length];
    TKey[] hash   = new TKey[length - 1];

    int i = 0;
    foreach ((TKey key, TValue val) in source) {
      keys[i] = key;
      vals[i] = val;
      if (CollectionUtils.IndexOf(keys.AsSpan(0, i), key) != -1) {
        ThrowHelper.DuplicateKey(keys.AsSpan(0, i + 1), key);
      }

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

    ulong seed = 0;
    if (use_hash) {
      // Brute Force find seed
      for (int k = 0; k < length - 1; ++k) {
        TKey key = keys[k];
        ulong idx =
            MathExt.FastRange64(HashUtils.MurmurHash2A(key.ToUInt64(), seed), (ulong)length - 1);
        if (!EqualityComparer<TKey>.Default.Equals(hash[idx], zero)) {
          k = -1;
          ++seed;
          hash.Clear();
        }
        hash[idx] = key;
      }
      keys.AsSpan() [..(length - 1)].CopyTo(hash);
      CollectionUtils.PdqSort(hash.AsSpan(), vals.AsSpan(0, length - 1),
          new _Hasher1<TKey>(key => (int)MathExt.FastRange64(
                                 HashUtils.MurmurHash2A(key.ToUInt64(), seed), (ulong)length - 1)));
    }

    return new FrozenFlagDictionary<TKey, TValue> {
      keys_      = ImmutableArray.Create(keys),
      vals_      = ImmutableArray.Create(vals),
      hash_      = !use_hash ? ImmutableArray<TKey>.Empty : ImmutableArray.Create(hash),
      union_     = key_union,
      hash_seed  = seed,
      FreezeType = use_hash ? FreezeType.Hashed : FreezeType.Linear,
    };
  }
  public readonly struct _Hasher1<T> : IComparer<T>
      where T : unmanaged, Enum {
    public readonly Func<T, int> hash_;
    public _Hasher1(Func<T, int> hash) {
      hash_ = hash;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int Compare(T x, T y) {
      return hash_(x).CompareTo(hash_(y));
    }
  }
}
}
