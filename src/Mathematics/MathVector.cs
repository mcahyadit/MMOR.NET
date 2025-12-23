using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Mathematics
{
  //-+-+-+-+-+-+-+-+
  // Math Extentions
  //-+-+-+-+-+-+-+-+
  public static partial class MathExt
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 Pow(in Vector4 x, float y) =>
      new(MathF.Pow(x.X, y), MathF.Pow(x.Y, y), MathF.Pow(x.Z, y), MathF.Pow(x.W, y));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 Pow(in Vector4 x, in Vector4 y) =>
      new(MathF.Pow(x.X, y.X), MathF.Pow(x.Y, y.Y), MathF.Pow(x.Z, y.Z), MathF.Pow(x.W, y.W));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 Log(in Vector4 x, float y) =>
      new(MathF.Log(x.X, y), MathF.Log(x.Y, y), MathF.Log(x.Z, y), MathF.Log(x.W, y));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 Log(in Vector4 x, in Vector4 y) =>
      new(MathF.Log(x.X, y.X), MathF.Log(x.Y, y.Y), MathF.Log(x.Z, y.Z), MathF.Log(x.W, y.W));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 Log2(in Vector4 x) =>
      new Vector4(MathF.Log(x.X), MathF.Log(x.Y), MathF.Log(x.Z), MathF.Log(x.W)) / log2f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 Log10(in Vector4 x) =>
      new(MathF.Log10(x.X), MathF.Log10(x.Y), MathF.Log10(x.Z), MathF.Log10(x.W));
  }
}
