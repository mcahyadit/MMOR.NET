using System;

namespace MMOR.NET.Bits {
public partial class BitReverseTest {
  public static ulong Reverse64Reference(ulong value, int start = 0, int length = 64) {
    // BitMask -> Bool[]
    Span<bool> boolarr = stackalloc bool[64];
    for (int i = 0; i < boolarr.Length; ++i) {
      boolarr[i] = ((value >> i) & 1) == 1;
    }

    // Reverse
    boolarr.Slice(start, length).Reverse();

    // Bool[] -> BitMask
    ulong result = 0;
    for (int i = 0; i < boolarr.Length; ++i) {
      result |= boolarr[i] ? 1ul << i : 0;
    }
    return result;
  }

  public static uint Reverse32Reference(uint value, int start = 0, int length = 32) {
    Span<bool> boolarr = stackalloc bool[32];
    for (int i = 0; i < boolarr.Length; ++i) {
      boolarr[i] = ((value >> i) & 1) == 1;
    }
    boolarr.Slice(start, length).Reverse();
    uint result = 0;
    for (int i = 0; i < boolarr.Length; ++i) {
      result |= boolarr[i] ? 1u << i : 0;
    }
    return result;
  }

  public static byte Reverse8Reference(byte value, int start = 0, int length = 8) {
    Span<bool> boolarr = stackalloc bool[8];
    for (int i = 0; i < boolarr.Length; ++i) {
      boolarr[i] = ((value >> i) & 1) == 1;
    }
    boolarr.Slice(start, length).Reverse();
    byte result = 0;
    for (int i = 0; i < boolarr.Length; ++i) {
      result |= (byte)(boolarr[i] ? 1u << i : 0);
    }
    return result;
  }
}
}
