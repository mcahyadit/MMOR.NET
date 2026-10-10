using System;
using Xunit;

namespace MMOR.NET.Collections {
public partial class RentedListTest {
  public static readonly TheoryData<RandomizedMixParam> kRandomizedMixParams = [
    new RandomizedMixParam { identifier = "list-mix", seed = 2026, trials = 50, steps = 200 },
  ];

  public static readonly SampleEnum[] kEnumValues = { SampleEnum.A, SampleEnum.B, SampleEnum.C,
    SampleEnum.D, SampleEnum.B };

  public static readonly SampleStruct[] kStructValues = {
    new(1),
    new(2),
    new(3),
    new(2),
  };

  public static readonly double[] kDoubleValues = BuildDoubleValues();

  static double[] BuildDoubleValues() {
    double[] data = new double[100];
    for (int i = 0; i < data.Length; ++i) data[i] = i;
    data[42] = double.NaN;
    return data;
  }
}
}
