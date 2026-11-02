using System.Text;
using Xunit;

namespace MMOR.NET.Collections.Hash {

public partial class XxHash3RandomTest {
  public record XxHash3Param {
    public string identifier { get; init; } = "Generic Test";
    public required string value { get; init; }
    public ulong seed { get; init; }
    public ulong expected { get; init; }
  }

  [Theory]
  [MemberData(nameof(kParams))]
  public void GenericTest(XxHash3Param p) {
    Assert.Equal(p.expected, HashUtils.XxHash3(Encoding.UTF8.GetBytes(p.value), p.seed));
  }
}
}
