using System.Text;
using Xunit;

namespace MMOR.NET.Collections.Hash {
public partial class XxHash3RandomTest {
  public static string Repeat1To0(int repeat) {
    StringBuilder sb = new();
    for (int i = 0; i < repeat; ++i) {
      sb.Append("1234567890");
    }
    return sb.ToString();
  }

  public static readonly TheoryData<XxHash3Param> kParams = [
    new XxHash3Param {
      value    = "",
      seed     = 0,
      expected = 0x2D06800538D394C2,
    },
    new XxHash3Param {
      value    = "a",
      seed     = 0,
      expected = 0xE6C632B61E964E1F,
    },
    new XxHash3Param {
      value    = "abc",
      seed     = 0,
      expected = 0x78AF5F94892F3950,
    },
    new XxHash3Param {
      identifier = "Length 7",
      value      = "message",
      seed       = 0,
      expected   = 0x0B1CA9B8977554FA,
    },
    new XxHash3Param {
      value    = "message digest",
      seed     = 0,
      expected = 0x160D8E9329BE94F9,
    },
    new XxHash3Param {
      value    = "abcdefghijklmnopqrstuvwxyz",
      seed     = 0,
      expected = 0x810F9CA067FBB90C,
    },
    new XxHash3Param {
      value    = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789",
      seed     = 0,
      expected = 0x643542BB51639CB2,
    },
    new XxHash3Param {
      value    = Repeat1To0(8),
      seed     = 0,
      expected = 0x7F58AA2520C681F9,
    },
    new XxHash3Param {
      identifier = "Hash240 - UnSeeded",
      value      = Repeat1To0(22)[..^ 2],
      seed       = 0,
      expected   = 0xB66EA795B5EDC38C,
    },
    // new XxHash3Param {
    //   value      = Repeat1To0(32),
    //   seed       = 0,
    //   expected   = 0x8845E0B1B57330DE,
    // },

    new XxHash3Param {
      value    = "",
      seed     = 1,
      expected = 0x4DC5B0CC826F6703,
    },
    new XxHash3Param {
      value    = "a",
      seed     = 1,
      expected = 0xD2F6D0996F37A720,
    },
    new XxHash3Param {
      value    = "abc",
      seed     = 1,
      expected = 0x6B4467B443C76228,
    },
    new XxHash3Param {
      identifier = "Length 7 - Seeded",
      value      = "message",
      seed       = 1,
      expected   = 0x73FB1CF20D561766,
    },
    new XxHash3Param {
      value    = "message digest",
      seed     = 1,
      expected = 0xFE71A82A70381174,
    },
    new XxHash3Param {
      value    = "abcdefghijklmnopqrstuvwxyz",
      seed     = 1,
      expected = 0x902A2C2D016A37BA,
    },
    new XxHash3Param {
      value    = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789",
      seed     = 1,
      expected = 0xBF552E540C5C6882,
    },
    new XxHash3Param {
      value    = Repeat1To0(8),
      seed     = 1,
      expected = 0xF2CA33235A6B865B,
    },
    new XxHash3Param {
      identifier = "Hash240 - Seeded",
      value      = Repeat1To0(22)[..^ 2],
      seed       = 1,
      expected   = 0x6EF5CF958BA52C4,
    },
    // new XxHash3Param {
    //   value      = Repeat1To0(32),
    //   seed       = 1,
    //   expected   = 0xFBC5F9C53D21CB2F,
    // },
  ];
}
}
