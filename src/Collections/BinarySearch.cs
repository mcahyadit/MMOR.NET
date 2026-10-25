using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MMOR.Roslyn;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  /**
   * <summary>
   *  Binary search on an ascending-sorted slice that returns the insertion point after
   *  the last occurrence of <paramref name="item"/> (upper bound), so duplicates stay stable.
   *  Returns <c>slice.Length</c> if no element exceeds <paramref name="item"/>.
   * </summary>
   */
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(List<>), typeof(CollectionsMarshal),
      nameof(CollectionsMarshal.AsSpan))]
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(ImmutableArray<>), typeof(ImmutableArray<>),
      "AsSpan()")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int TailBinarySearch<T, TComparer>(this in ReadOnlySpan<T> slice, in T item,
      in TComparer cmp)
      where TComparer : IComparer<T> {
    int hi = slice.Length;
    int lo = 0;
    while (lo < hi) {
      int mid = lo + ((hi - lo) >> 1);
      if (cmp.Compare(slice[mid], item) <= 0) {
        lo = mid + 1;
      } else {
        hi = mid;
      }
    }
    return lo;
  }

  /**
   * <inheritdoc cref="TailBinarySearch{T}(in ReadOnlySpan{T}, in T)"/>
   */
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(List<>), typeof(CollectionsMarshal),
      nameof(CollectionsMarshal.AsSpan))]
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(ImmutableArray<>), typeof(ImmutableArray<>),
      "AsSpan()")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int TailBinarySearch<T>(this in ReadOnlySpan<T> slice, in T item)
      where T : IComparable<T> => TailBinarySearch(slice, item, Comparer<T>.Default);

  /**
   * <summary>
   *  Binary search on an ascending-sorted slice that returns the insertion point at the
   *  first occurrence of <paramref name="item"/> (lower bound), so duplicates stay stable.
   *  Returns <c>0</c> if <paramref name="item"/> precedes every element.
   * </summary>
   */
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(List<>), typeof(CollectionsMarshal),
      nameof(CollectionsMarshal.AsSpan))]
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(ImmutableArray<>), typeof(ImmutableArray<>),
      "AsSpan()")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int HeadBinarySearch<T, TComparer>(this in ReadOnlySpan<T> slice, in T item,
      in TComparer cmp)
      where TComparer : IComparer<T> {
    int hi = slice.Length;
    int lo = 0;
    while (lo < hi) {
      int mid = lo + ((hi - lo) >> 1);
      if (cmp.Compare(slice[mid], item) < 0) {
        lo = mid + 1;
      } else {
        hi = mid;
      }
    }
    return lo;
  }

  /**
   * <inheritdoc cref="HeadBinarySearch{T}(in ReadOnlySpan{T}, in T)"/>
   */
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(List<>), typeof(CollectionsMarshal),
      nameof(CollectionsMarshal.AsSpan))]
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(ImmutableArray<>), typeof(ImmutableArray<>),
      "AsSpan()")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int HeadBinarySearch<T>(this in ReadOnlySpan<T> slice, in T item)
      where T : IComparable<T> => HeadBinarySearch(slice, item, Comparer<T>.Default);

  public static int HeadGallopSearch<T, TComparer>(this in ReadOnlySpan<T> slice, in T item,
      in TComparer cmp)
      where TComparer : IComparer<T> {
    int length = slice.Length;

    int g_idx = 1;
    while (g_idx < length && cmp.Compare(item, slice[g_idx]) > 0) {
      g_idx <<= 1;
    }

    int idx = HeadBinarySearch(slice[(g_idx >> 1)..Math.Min(g_idx, length)], item, cmp);
    return idx + (g_idx >> 1);
  }

  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(List<>), typeof(CollectionsMarshal),
      nameof(CollectionsMarshal.AsSpan))]
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(ImmutableArray<>), typeof(ImmutableArray<>),
      "AsSpan()")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int HeadGallopSearch<T>(this in ReadOnlySpan<T> slice, in T item)
      where T : IComparable<T> => HeadGallopSearch(slice, item, Comparer<T>.Default);

  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(List<>), typeof(CollectionsMarshal),
      nameof(CollectionsMarshal.AsSpan))]
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(ImmutableArray<>), typeof(ImmutableArray<>),
      "AsSpan()")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int TailGallopSearch<T, TComparer>(this in ReadOnlySpan<T> slice, in T item,
      in TComparer cmp)
      where TComparer : IComparer<T> {
    int length = slice.Length;

    int g_idx = 1;
    while (g_idx < length && cmp.Compare(item, slice[g_idx]) >= 0) {
      g_idx <<= 1;
    }

    int idx = TailBinarySearch(slice[(g_idx >> 1)..Math.Min(g_idx, length)], item, cmp);
    return idx + (g_idx >> 1);
  }

  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(List<>), typeof(CollectionsMarshal),
      nameof(CollectionsMarshal.AsSpan))]
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(ImmutableArray<>), typeof(ImmutableArray<>),
      "AsSpan()")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int TailGallopSearch<T>(this in ReadOnlySpan<T> slice, in T item)
      where T : IComparable<T> => TailGallopSearch(slice, item, Comparer<T>.Default);
}
}
