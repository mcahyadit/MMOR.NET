using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Mathematics {
public static partial class MathExt {
  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T SumElements<T>(this in Vector<T> vector)
      where T : struct {
#if NETSTANDARD
    return Vector.Dot(vector, Vector<T>.One);
#else
    return Vector.Sum(vector);
#endif
  }
}
}
