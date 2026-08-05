using System;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using MMOR.NET.Collections;
using MMOR.NET.Mathematics;

#if !NETSTANDARD
using System.Runtime.Intrinsics;
#endif

namespace MMOR.NET.Bits {
public static partial class BitOps {
  /// <summary> Vector of 64, where each lane is a mask of its own index. </summary>
  public static readonly Vector<ulong> kBitPosVec64 = ((Func<Vector<ulong>>)(() => {
    Span<ulong> buffer = stackalloc ulong[Vector<ulong>.Count];
    for (int i = 0; i < buffer.Length; ++i) {
      buffer[i] |= 1ul << i;
    }
    return buffer.ToVector();
  })).Invoke();

  /// <summary> Vector of 32, where each lane is a mask of its own index. </summary>
  public static readonly Vector<uint> kBitPosVec32 = ((Func<Vector<uint>>)(() => {
    Span<uint> buffer = stackalloc uint[Vector<uint>.Count];
    for (int i = 0; i < buffer.Length; ++i) {
      buffer[i] |= 1u << i;
    }
    return buffer.ToVector();
  })).Invoke();

  /**
   * <summary>
   *  Calls SSE or AVX's <c>PMOVMSKB</c> or AVX-512's <c>VPMOVQ2M</c> if supported. <br/>
   *  Fallbacks to software implementation otherwise. <br/>
   *  Reference <see href="https://www.felixcloutier.com/x86/vpmovb2m:vpmovw2m:vpmovd2m:vpmovq2m"/>
   *  <br/>
   *  Returns the most significant bit of each lane in <paramref name="vector"/>. <br/>
   *  For <see cref="uint"/>, use <see cref="MmMovemaskEpi32"/>
   * </summary>
   */
  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong MmMovemaskEpi64(Vector<ulong> vector) {
#if !NETSTANDARD
    if (Vector<ulong>.Count == Vector512<ulong>.Count) {
      Vector512<ulong> v512 = vector.AsVector512();
      return Vector512.ExtractMostSignificantBits(v512);
    } else if (Vector<ulong>.Count == Vector256<ulong>.Count) {
      Vector256<ulong> v256 = vector.AsVector256();
      return Vector256.ExtractMostSignificantBits(v256);
    } else if (Vector<ulong>.Count == Vector128<ulong>.Count) {
      Vector128<ulong> v128 = vector.AsVector128();
      return Vector128.ExtractMostSignificantBits(v128);
    }
#endif

    return MmMovemaskEpi64Sw(vector);
  }

  /**
   * <summary>
   *  Emulation of SSE or AVX's <c>PMOVMSKB</c> or AVX-512's <c>VPMOVQ2M</c>. <br/>
   *  Fallbacks to software implementation otherwise. <br/>
   *  Reference <see href="https://www.felixcloutier.com/x86/vpmovb2m:vpmovw2m:vpmovd2m:vpmovq2m"/>
   *  <br/>
   *  Returns the most significant bit of each lane in <paramref name="vector"/>. <br/>
   *  For <see cref="uint"/>, use <see cref="MmMovemaskEpi32Sw"/>
   * </summary>
   */
  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong MmMovemaskEpi64Sw(Vector<ulong> vector) {
    if (Vector.IsHardwareAccelerated) {
      Vector<long> signed    = Vector.AsVectorInt64(vector);
      Vector<long> mask      = Vector.LessThan(signed, Vector<long>.Zero);
      Vector<ulong> weighted = Vector.BitwiseAnd(Vector.AsVectorUInt64(mask), kBitPosVec64);
      return MathExt.SumElements(weighted);
    }

    ulong result = 0;
    for (int i = 0; i < Vector<ulong>.Count; ++i) {
      ulong msb = vector[i] >> 63;
      result |= msb << i;
    }
    return result;
  }

  /**
   * <summary>
   *  Calls SSE or AVX's <c>PMOVMSKB</c> or AVX-512's <c>VPMOVD2M</c> if supported. <br/>
   *  Fallbacks to software implementation otherwise. <br/>
   *  Reference <see href="https://www.felixcloutier.com/x86/vpmovb2m:vpmovw2m:vpmovd2m:vpmovq2m"/>
   *  <br/>
   *  Returns the most significant bit of each lane in <paramref name="vector"/>. <br/>
   *  For <see cref="ulong"/>, use <see cref="MmMovemaskEpi64"/>
   * </summary>
   */
  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong MmMovemaskEpi32(Vector<uint> vector) {
#if !NETSTANDARD
    if (Vector<uint>.Count == Vector512<uint>.Count) {
      Vector512<uint> v512 = vector.AsVector512();
      return Vector512.ExtractMostSignificantBits(v512);
    } else if (Vector<uint>.Count == Vector256<uint>.Count) {
      Vector256<uint> v256 = vector.AsVector256();
      return Vector256.ExtractMostSignificantBits(v256);
    } else if (Vector<uint>.Count == Vector128<uint>.Count) {
      Vector128<uint> v128 = vector.AsVector128();
      return Vector128.ExtractMostSignificantBits(v128);
    }
#endif

    return MmMovemaskEpi32Sw(vector);
  }

  /**
   * <summary>
   *  Emulation of SSE or AVX's <c>PMOVMSKB</c> or AVX-512's <c>VPMOVD2M</c>. <br/>
   *  Fallbacks to software implementation otherwise. <br/>
   *  Reference <see href="https://www.felixcloutier.com/x86/vpmovb2m:vpmovw2m:vpmovd2m:vpmovq2m"/>
   *  <br/>
   *  Returns the most significant bit of each lane in <paramref name="vector"/>. <br/>
   *  For <see cref="ulong"/>, use <see cref="MmMovemaskEpi64Sw"/>
   * </summary>
   */
  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong MmMovemaskEpi32Sw(Vector<uint> vector) {
    if (Vector.IsHardwareAccelerated) {
      Vector<int> signed    = Vector.AsVectorInt32(vector);
      Vector<int> mask      = Vector.LessThan(signed, Vector<int>.Zero);
      Vector<uint> weighted = Vector.BitwiseAnd(Vector.AsVectorUInt32(mask), kBitPosVec32);
      return MathExt.SumElements(weighted);
    }

    ulong result = 0;
    for (int i = 0; i < Vector<uint>.Count; ++i) {
      uint msb = vector[i] >> 31;
      result |= msb << i;
    }
    return result;
  }

  /**
   * <summary>
   *  Emulation of AVX-512's <c>VPMOVM2Q</c> since dotnet doesn't expose it. <br/>
   *  Reference <see href="https://www.felixcloutier.com/x86/vpmovm2b:vpmovm2w:vpmovm2d:vpmovm2q"/>
   *  <br/>
   *  Returns a vector of <see cref="ulong"/> where each active bit
   *  in <paramref name="mask"/> is set to all <c>1</c>. <br/>
   *  Only considers bits up to the lane-count. <br/>
   *  For <see cref="uint"/>, use <see cref="MmMovmEpi32Emu"/>
   * </summary>
   */
  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<ulong> MmMovmEpi64Emu(ulong mask) {
    Vector<ulong> vector = new(mask);
    vector               = Vector.BitwiseAnd(kBitPosVec64, vector);
    return Vector.GreaterThan(vector, Vector<ulong>.Zero);
  }

  /**
   * <summary>
   *  Emulation of AVX-512's <c>VPMOVM2D</c> since dotnet doesn't expose it. <br/>
   *  Reference <see href="https://www.felixcloutier.com/x86/vpmovm2b:vpmovm2w:vpmovm2d:vpmovm2q"/>
   *  <br/>
   *  Returns a vector of <see cref="uint"/> where each active bit
   *  in <paramref name="mask"/> is set to all <c>1</c>. <br/>
   *  Only considers bits up to the lane-count. <br/>
   *  For <see cref="uint"/>, use <see cref="MmMovmEpi64Emu"/>
   * </summary>
   */
  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<uint> MmMovmEpi32Emu(uint mask) {
    Vector<uint> vector = new(mask);
    vector              = Vector.BitwiseAnd(kBitPosVec32, vector);
    return Vector.GreaterThan(vector, Vector<uint>.Zero);
  }
}
}
