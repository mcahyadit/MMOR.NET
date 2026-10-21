using System.Numerics;

namespace MMOR.NET.Bits {
public partial class BitOpsTest {
  public static ulong Bzhi64Reference(ulong value, int index) {
    index &= 0xFF;
    for (int i = index; i < 64; ++i) {
      value &= ~(1ul << i);
    }
    return value;
  }

  public static uint Bzhi32Reference(uint value, int index) {
    index &= 0xFF;
    for (int i = index; i < 32; ++i) {
      value &= ~(1u << i);
    }
    return value;
  }
}
}
