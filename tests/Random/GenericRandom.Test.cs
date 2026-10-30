using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Xunit;

namespace MMOR.NET.Random {

public partial class GenericRandomTest {
  public record GenericRandomU32Param {
    public required string identifier { get; init; }
    public required Func<IRandom> ctor { get; init; }
    public required ImmutableArray<uint> expected_sequence { get; init; }
    public KeyValuePair<int, uint> jump_value { get; init; }
  }

  public record GenericRandomU64Param {
    public required string identifier { get; init; }
    public required Func<IRandom64> ctor { get; init; }
    public required ImmutableArray<ulong> expected_sequence { get; init; }
    public KeyValuePair<int, ulong> jump_value { get; init; }
  }

  [Theory]
  [MemberData(nameof(kParamsU32))]
  public void GenericTest(GenericRandomU32Param p) {
    IRandom rng = p.ctor.Invoke();
    foreach (uint expected in p.expected_sequence) {
      Assert.Equal(expected, rng.NextUInt());
    }
    if (p.jump_value.Key > 0) {
      rng.JumpTo((ulong)p.jump_value.Key - 1);
      Assert.Equal(p.jump_value.Value, rng.NextUInt());
    }
  }

  [Theory]
  [MemberData(nameof(kParamsU64))]
  public void GenericTest64(GenericRandomU64Param p) {
    IRandom64 rng = p.ctor.Invoke();
    foreach (ulong expected in p.expected_sequence) {
      Assert.Equal(expected, rng.NextUInt64());
    }
    if (p.jump_value.Key > 0) {
      rng.JumpTo((ulong)p.jump_value.Key - 1);
      Assert.Equal(p.jump_value.Value, rng.NextUInt64());
    }
  }
}
}
