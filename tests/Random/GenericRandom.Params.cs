using System.Collections.Immutable;
using Xunit;

namespace MMOR.NET.Random {
public partial class GenericRandomTest {
  public static readonly TheoryData<GenericRandomU32Param> kParamsU32 = [
    new GenericRandomU32Param {
      // https://github.com/boostorg/random/blob/develop/test/test_mt19937.cpp
      identifier = "MT19937 Expected Sequence",
      ctor = ()         => new MT19937(5489u),
      expected_sequence  = ImmutableArray.Create([
        0xD091BB5Cu,
        0x22AE9EF6u,
        0xE7E1FAEEu,
        0xD5C31F79u,
       ]),
      // https://cppreference.net/cpp/numeric/random/mersenne_twister_engine/mersenne_twister_engine.html
      jump_value = new(10_000, 4123659995u),
    },
    new GenericRandomU32Param {
      // https://github.com/boostorg/random/blob/develop/test/test_mt11213b.cpp
      identifier = "MT11213b Expected Sequence",
      ctor = ()         => new MT11213b(5489u),
      expected_sequence  = ImmutableArray.Create([
        0xEF3F3F3Fu,
        0x70082175u,
        0xDAF6EAF5u,
        0x2A16A63Eu,
       ]),
      jump_value         = new(10_000, 3809585648u),
    },
    new GenericRandomU32Param {
      // https://pcg-random.org/using-pcg-c-basic.html
      identifier = "PCG Expected Sequence 1",
      ctor = ()         => new PCG(42u, 54u),
      expected_sequence  = ImmutableArray.Create([
        0xA15C02B7u,
        0x7B47F409u,
        0xBA1D3330u,
        0x83D2F293u,
        0xBFA4784Bu,
        0xCBED606Eu,
       ]),
    },
    new GenericRandomU32Param {
      // https://codeberg.org/ziglang/zig/src/commit/9da28756563d966484e79154c6ee56ca3065bfe6/lib/std/Random/Pcg.zig
      identifier = "PCG Expected Sequence 2",
      // NOTE: Zig flips the s0 and s1
      ctor = ()         => new PCG(0x84e9c579ef59bbf7, 0x9394bf54ce5d79de),
      expected_sequence  = ImmutableArray.Create([
        2881561918,
        3063928540u,
        1199791034u,
        2487695858u,
        1479648952u,
        3247963454u,
       ]),
    },
  ];

  public static readonly TheoryData<GenericRandomU64Param> kParamsU64 = [
    new GenericRandomU64Param {
      identifier = "MT19937u64 Expected Sequence",
      ctor = ()         => new MT19937u64(5489u),
      expected_sequence  = ImmutableArray.Create([
        // https://oeis.org/A221558
        14514284786278117030ul, 4620546740167642908ul, 13109570281517897720ul,
        17462938647148434322ul, 355488278567739596ul, 7469126240319926998ul, 4635995468481642529ul,
        418970542659199878ul, 9604170989252516556ul, 6358044926049913402ul, 5058016125798318033ul,
        10349215569089701407ul
      ]),
      // https://cppreference.net/cpp/numeric/random/mersenne_twister_engine/mersenne_twister_engine.html
      jump_value = new(10_000, 9981545732273789042ul),
    },
  ];
}
}
