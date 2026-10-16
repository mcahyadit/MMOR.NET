using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using MMOR.Roslyn;

namespace MMOR.NET.Statistics {
public static partial class TotalStatisticsExtension {
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(List<>), typeof(CollectionsMarshal),
      nameof(CollectionsMarshal.AsSpan))]
  [TypeMarshalOverload(typeof(ReadOnlySpan<>), typeof(ImmutableArray<>), typeof(ImmutableArray<>),
      "AsSpan()")]
  public static TotalStatisticsV2 Calculate<T>(this in ReadOnlySpan<T> values)
      where T : unmanaged, IConvertible {
    return Calculate<T, ulong>(values, stackalloc ulong[0]);
  }

  public static TotalStatisticsV2 Calculate<TVal, TCnt>(
      this IReadOnlyDictionary<TVal, TCnt> dictionary)
      where TVal : unmanaged, IConvertible
      where TCnt : unmanaged, IConvertible {
    return CalculateCoreV2(dictionary.Keys.Select(x => x.ToDouble(null)),
        dictionary.Values.Select(x => x.ToUInt64(null)), dictionary.Count);
  }

  public static TotalStatisticsV2 Calculate<TVal, TCnt>(
      this IEnumerable<KeyValuePair<TVal, TCnt>> dictionary, int length)
      where TVal : unmanaged, IConvertible
      where TCnt : unmanaged, IConvertible {
    return CalculateCoreV2(dictionary.Select(x => x.Key.ToDouble(null)),
        dictionary.Select(x => x.Value.ToUInt64(null)), length);
  }
}
}
