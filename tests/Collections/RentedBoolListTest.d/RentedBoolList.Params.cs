using Xunit;

namespace MMOR.NET.Collections {
public partial class RentedBoolListTest {
  public static readonly TheoryData<RandomizedMixParam> kRandomizedMixParams = [
    new RandomizedMixParam { identifier = "bool-mix", seed = 99, trials = 30, steps = 200 },
  ];

  public static readonly TheoryData<int> kIndexOfLengths =
      [0, 1, 5, 63, 64, 65, 100, 127, 128, 129, 130, 192, 200, 256];

  public static readonly TheoryData<int> kLastPartialWordLengths = [1, 63, 64, 65, 129, 130, 200];

  public static readonly TheoryData<int, bool> kInsertParams = BuildInsertParams();

  public static readonly TheoryData<int> kRemoveAtIndexes = [0, 1, 63, 64, 65, 129];

  static TheoryData<int, bool> BuildInsertParams() {
    TheoryData<int, bool> data = [];
    data.Add(0, true);
    data.Add(1, false);
    data.Add(63, true);
    data.Add(64, false);
    data.Add(65, true);
    data.Add(129, false);
    data.Add(130, true);
    return data;
  }
}
}
