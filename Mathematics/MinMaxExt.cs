using System;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Mathematics
{
  public static partial class MathExt
  {
    //-+-+-+-+-+-+-+-+
    // Minimum
    //-+-+-+-+-+-+-+-+

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Min(int a, int b, params int[] more)
    {
      int result = Math.Min(a, b);
      foreach (int val in more)
        result = Math.Min(result, val);
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Min(long a, long b, params long[] more)
    {
      long result = Math.Min(a, b);
      foreach (long val in more)
        result = Math.Min(result, val);
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Min(float a, float b, params float[] more)
    {
      float result = Math.Min(a, b);
      foreach (float val in more)
        result = Math.Min(result, val);
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Min(double a, double b, params double[] more)
    {
      double result = Math.Min(a, b);
      foreach (double val in more)
        result = Math.Min(result, val);
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal Min(in decimal a, in decimal b, params decimal[] more)
    {
      decimal result = Math.Min(a, b);
      foreach (decimal val in more)
        result = Math.Min(result, val);
      return result;
    }

    //-+-+-+-+-+-+-+-+
    // Maximum
    //-+-+-+-+-+-+-+-+

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Max(int a, int b, params int[] more)
    {
      int result = Math.Max(a, b);
      foreach (int val in more)
        result = Math.Max(result, val);
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Max(long a, long b, params long[] more)
    {
      long result = Math.Max(a, b);
      foreach (long val in more)
        result = Math.Max(result, val);
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Max(float a, float b, params float[] more)
    {
      float result = Math.Max(a, b);
      foreach (float val in more)
        result = Math.Max(result, val);
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Max(double a, double b, params double[] more)
    {
      double result = Math.Max(a, b);
      foreach (double val in more)
        result = Math.Max(result, val);
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal Max(in decimal a, in decimal b, params decimal[] more)
    {
      decimal result = Math.Max(a, b);
      foreach (decimal val in more)
        result = Math.Max(result, val);
      return result;
    }
  }
}
