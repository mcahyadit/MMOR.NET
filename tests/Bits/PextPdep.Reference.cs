using System.Numerics;

namespace MMOR.NET.Bits {
public partial class BitOpsTest {
  public static ulong Pext64Reference(ulong value, ulong mask) {
    ulong result = 0;
    int counter  = 0;
    while (mask != 0) {
      int idx = BitOperations.TrailingZeroCount(mask);
      mask &= mask - 1;

      ulong extract_val = (value >> idx) & 1;
      result |= extract_val << counter;
      ++counter;
    }
    return result;
  }

  public static ulong Pdep64Reference(ulong value, ulong mask) {
    ulong result = 0;
    int counter  = 0;
    while (mask != 0) {
      int idx = BitOperations.TrailingZeroCount(mask);
      mask &= mask - 1;

      ulong deposit_value = value & (1ul << counter);
      result |= deposit_value << (idx - counter);
      ++counter;
    }
    return result;
  }
}
}
