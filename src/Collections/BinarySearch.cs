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
  public static int TailBinarySearch<T>(this in ReadOnlySpan<T> slice, in T item)
      where T : IComparable<T> {
    int hi = slice.Length;
    int lo = 0;
    while (lo < hi) {
      int mid = lo + ((hi - lo) >> 1);
      if (slice[mid].CompareTo(item) <= 0) {
        lo = mid + 1;
      } else {
        hi = mid;
      }
    }
    return lo;
  }

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
  public static int HeadBinarySearch<T>(this in ReadOnlySpan<T> slice, in T item)
      where T : IComparable<T> {
    int hi = slice.Length;
    int lo = 0;
    while (lo < hi) {
      int mid = lo + ((hi - lo) >> 1);
      if (slice[mid].CompareTo(item) < 0) {
        lo = mid + 1;
      } else {
        hi = mid;
      }
    }
    return lo;
  }
}
}
