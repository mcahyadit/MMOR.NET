using System;
using MMOR.NET.Mathematics;
using Xunit;
using Xunit.Sdk;

namespace MMOR.NET {
public static class TestUtils {
  public static void AssertApproximately(double expected, double actual, int precision) {
    if (precision >= 0) {
      Assert.Equal(expected, actual, precision);
      return;
    }

    if (!MathExt.Approximately(expected, actual))
      throw EqualException.ForMismatchedValues(expected, actual);
  }

  public static void AssertApproximately(double expected, double actual, double tolerance) {
    double scale = Math.Max(1.0, Math.Max(Math.Abs(expected), Math.Abs(actual)));
    Assert.Equal(expected, actual, tolerance * scale);
  }
}
}
