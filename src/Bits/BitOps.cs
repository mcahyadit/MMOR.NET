
using System.Runtime.CompilerServices;

namespace MMOR.NET.Bits {
public static partial class BitOps {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TTo BitCast<TFrom, TTo>(in TFrom value)
      where TFrom : unmanaged
      where TTo : unmanaged {
#if NETSTANDARD
    TFrom defcp = value;
    return Unsafe.As<TFrom, TTo>(ref defcp);
#else
    return Unsafe.BitCast<TFrom, TTo>(value);
#endif
  }
}
}
