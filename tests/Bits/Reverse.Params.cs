namespace MMOR.NET.Bits {
public partial class BitReverseTest {
  private static readonly uint[] kSamples32 = [
    0u,
    1u,
    uint.MaxValue,
    0xAAAAAAAAu,
    0x55555555u,
    0x80000001u,
    0x0000FFFFu,
    0xDEADBEEFu,
  ];

  private static readonly ulong[] kSamples64 = [
    0ul,
    1ul,
    ulong.MaxValue,
    0xAAAAAAAAAAAAAAAAul,
    0x5555555555555555ul,
    0x8000000000000001ul,
    0x00000000FFFFFFFFul,
    0xDEADBEEFCAFEBABEul,
  ];
}
}
