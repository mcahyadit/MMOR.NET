using System;
using MMOR.NET.Mathematics;
using Xunit;
using Xunit.Sdk;

namespace MMOR.NET {
using Random = System.Random;
public static class TestUtils {
  public static void AssertApproximately(double expected, double actual, int precision) {
    if (precision >= 0) {
      Assert.Equal(expected, actual, precision);
      return;
    }

    if (!MathExt.Approximately(expected, actual))
      throw EqualException.ForMismatchedValues(expected.ToString(), actual.ToString());
  }

  public static void AssertApproximately(double expected, double actual, double tolerance) {
    double scale = Math.Max(1.0, Math.Max(Math.Abs(expected), Math.Abs(actual)));
    Assert.Equal(expected, actual, tolerance * scale);
  }

  public static ulong NextU64(this System.Random rng) {
    return (ulong)rng.NextInt64(long.MinValue, long.MaxValue);
  }

  public static uint NextU32(this System.Random rng) {
    return (uint)rng.Next(int.MinValue, int.MaxValue);
  }
}
}
