
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool VectorSupported<T>() {
#if !NETSTANDARD
    return Vector<T>.IsSupported;
#else
    return typeof(T) == typeof(byte) || typeof(T) == typeof(sbyte) || typeof(T) == typeof(short) ||
           typeof(T) == typeof(ushort) || typeof(T) == typeof(int) || typeof(T) == typeof(uint) ||
           typeof(T) == typeof(long) || typeof(T) == typeof(ulong) || typeof(T) == typeof(float) ||
           typeof(T) == typeof(double);
#endif
  }
}
}
