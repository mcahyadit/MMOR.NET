using System;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using MMOR.NET.Collections;
using MMOR.NET.Mathematics;
using MMOR.Roslyn;

#if !NETSTANDARD
using System.Runtime.Intrinsics;
#endif

namespace MMOR.NET.Bits {
public static partial class BitOps {
  /// <summary> Vector of u64, where each lane is a mask of its own index. </summary>
  public static readonly Vector<ulong> kBitPosEpiU64 = ((Func<Vector<ulong>>)(() => {
    Span<ulong> buffer = stackalloc ulong[Vector<ulong>.Count];
    for (int i = 0; i < buffer.Length; ++i) {
      buffer[i] |= 1ul << i;
    }
    return buffer.ToVector();
  })).Invoke();

  /// <summary> Vector of u32, where each lane is a mask of its own index. </summary>
  public static readonly Vector<uint> kBitPosEpiU32 = ((Func<Vector<uint>>)(() => {
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
   *  For <see cref="uint"/>, use <see cref="MmMovEpi32Mask"/>
   * </summary>
   */
  [Pure]
  [TypeMarshalOverload(typeof(Vector<ulong>), typeof(Vector<long>), typeof(Vector),
      nameof(Vector.AsVectorUInt64))]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong MmMovEpi64Mask(Vector<ulong> vector) {
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

    return MmMovEpi64MaskSw(vector);
  }

  /**
   * <summary>
   *  Emulation of SSE or AVX's <c>PMOVMSKB</c> or AVX-512's <c>VPMOVQ2M</c>. <br/>
   *  Fallbacks to software implementation otherwise. <br/>
   *  Reference <see href="https://www.felixcloutier.com/x86/vpmovb2m:vpmovw2m:vpmovd2m:vpmovq2m"/>
   *  <br/>
   *  Returns the most significant bit of each lane in <paramref name="vector"/>. <br/>
   *  For <see cref="uint"/>, use <see cref="MmMovEpi32MaskSw"/>
   * </summary>
   */
  [Pure]
  [TypeMarshalOverload(typeof(Vector<ulong>), typeof(Vector<long>), typeof(Vector),
      nameof(Vector.AsVectorUInt64))]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong MmMovEpi64MaskSw(Vector<ulong> vector) {
    if (Vector.IsHardwareAccelerated) {
      Vector<long> signed    = Vector.AsVectorInt64(vector);
      Vector<long> mask      = Vector.LessThan(signed, Vector<long>.Zero);
      Vector<ulong> weighted = Vector.BitwiseAnd(Vector.AsVectorUInt64(mask), kBitPosEpiU64);
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
   *  For <see cref="ulong"/>, use <see cref="MmMovEpi64Mask"/>
   * </summary>
   */
  [Pure]
  [TypeMarshalOverload(typeof(Vector<uint>), typeof(Vector<int>), typeof(Vector),
      nameof(Vector.AsVectorUInt32))]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong MmMovEpi32Mask(Vector<uint> vector) {
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

    return MmMovEpi32MaskSw(vector);
  }

  /**
   * <summary>
   *  Emulation of SSE or AVX's <c>PMOVMSKB</c> or AVX-512's <c>VPMOVD2M</c>. <br/>
   *  Fallbacks to software implementation otherwise. <br/>
   *  Reference <see href="https://www.felixcloutier.com/x86/vpmovb2m:vpmovw2m:vpmovd2m:vpmovq2m"/>
   *  <br/>
   *  Returns the most significant bit of each lane in <paramref name="vector"/>. <br/>
   *  For <see cref="ulong"/>, use <see cref="MmMovEpi64MaskSw"/>
   * </summary>
   */
  [Pure]
  [TypeMarshalOverload(typeof(Vector<uint>), typeof(Vector<int>), typeof(Vector),
      nameof(Vector.AsVectorUInt32))]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong MmMovEpi32MaskSw(Vector<uint> vector) {
    if (Vector.IsHardwareAccelerated) {
      Vector<int> signed    = Vector.AsVectorInt32(vector);
      Vector<int> mask      = Vector.LessThan(signed, Vector<int>.Zero);
      Vector<uint> weighted = Vector.BitwiseAnd(Vector.AsVectorUInt32(mask), kBitPosEpiU32);
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
   *  Calls SSE or AVX's <c>PMOVMSKB</c> or AVX-512's <c>VPMOVD2M</c> if supported. <br/>
   *  Fallbacks to software implementation otherwise. <br/>
   *  Reference <see href="https://www.felixcloutier.com/x86/vpmovb2m:vpmovw2m:vpmovd2m:vpmovq2m"/>
   *  <br/>
   *  Returns the most significant bit of each lane in <paramref name="vector"/>. <br/>
   * </summary>
   */
  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  [TypeMarshalOverload(typeof(Vector<ushort>), typeof(Vector<short>), typeof(Vector),
      nameof(Vector.AsVectorUInt16))]
  public static ulong MmMovEpi16Mask(Vector<ushort> vector) {
#if !NETSTANDARD
    if (Vector<ushort>.Count == Vector512<ushort>.Count) {
      Vector512<ushort> v512 = vector.AsVector512();
      return Vector512.ExtractMostSignificantBits(v512);
    } else if (Vector<ushort>.Count == Vector256<ushort>.Count) {
      Vector256<ushort> v256 = vector.AsVector256();
      return Vector256.ExtractMostSignificantBits(v256);
    } else if (Vector<ushort>.Count == Vector128<ushort>.Count) {
      Vector128<ushort> v128 = vector.AsVector128();
      return Vector128.ExtractMostSignificantBits(v128);
    }
#endif

    return MmMovEpi16MaskSw(vector);
  }

  /**
   * <summary>
   *  Emulation of SSE or AVX's <c>PMOVMSKB</c> or AVX-512's <c>VPMOVD2M</c>. <br/>
   *  Fallbacks to software implementation otherwise. <br/>
   *  Reference <see href="https://www.felixcloutier.com/x86/vpmovb2m:vpmovw2m:vpmovd2m:vpmovq2m"/>
   *  <br/>
   *  Returns the most significant bit of each lane in <paramref name="vector"/>. <br/>
   * </summary>
   */
  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  [TypeMarshalOverload(typeof(Vector<ushort>), typeof(Vector<short>), typeof(Vector),
      nameof(Vector.AsVectorUInt16))]
  public static ulong MmMovEpi16MaskSw(Vector<ushort> vector) {
    if (Vector.IsHardwareAccelerated) {
      Vector.Widen((Vector<short>)vector, out Vector<int> lower, out Vector<int> upper);
      ulong lower_mov = MmMovEpi32MaskSw(lower);
      ulong upper_mov = MmMovEpi32MaskSw(upper);
      return lower_mov | (upper_mov << Vector<uint>.Count);
    }

    ulong result = 0;
    for (int i = 0; i < Vector<ushort>.Count; ++i) {
      int msb = vector[i] >> 15;
      result |= (ushort)(msb << i);
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
   * </summary>
   */
  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  [TypeMarshalOverload(typeof(Vector<byte>), typeof(Vector<sbyte>), typeof(Vector),
      nameof(Vector.AsVectorByte))]
  public static ulong MmMovEpi8Mask(Vector<byte> vector) {
#if !NETSTANDARD
    if (Vector<byte>.Count == Vector512<byte>.Count) {
      Vector512<byte> v512 = vector.AsVector512();
      return Vector512.ExtractMostSignificantBits(v512);
    } else if (Vector<byte>.Count == Vector256<byte>.Count) {
      Vector256<byte> v256 = vector.AsVector256();
      return Vector256.ExtractMostSignificantBits(v256);
    } else if (Vector<byte>.Count == Vector128<byte>.Count) {
      Vector128<byte> v128 = vector.AsVector128();
      return Vector128.ExtractMostSignificantBits(v128);
    }
#endif

    return MmMovEpi8MaskSw(vector);
  }

  /**
   * <summary>
   *  Emulation of SSE or AVX's <c>PMOVMSKB</c> or AVX-512's <c>VPMOVD2M</c>. <br/>
   *  Fallbacks to software implementation otherwise. <br/>
   *  Reference <see href="https://www.felixcloutier.com/x86/vpmovb2m:vpmovw2m:vpmovd2m:vpmovq2m"/>
   *  <br/>
   *  Returns the most significant bit of each lane in <paramref name="vector"/>. <br/>
   * </summary>
   */
  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  [TypeMarshalOverload(typeof(Vector<byte>), typeof(Vector<sbyte>), typeof(Vector),
      nameof(Vector.AsVectorByte))]
  public static ulong MmMovEpi8MaskSw(Vector<byte> vector) {
    if (Vector.IsHardwareAccelerated) {
      Vector.Widen((Vector<sbyte>)vector, out Vector<short> lower, out Vector<short> upper);
      ulong lower_mov = MmMovEpi16MaskSw(lower);
      ulong upper_mov = MmMovEpi16MaskSw(upper);
      return lower_mov | (upper_mov << Vector<ushort>.Count);
    }

    ulong result = 0;
    for (int i = 0; i < Vector<byte>.Count; ++i) {
      int msb = vector[i] >> 7;
      result |= (byte)(msb << i);
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
   *  For <see cref="uint"/>, use <see cref="MmMovMaskEpi32Emu"/>
   * </summary>
   */
  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<ulong> MmMovMaskEpi64Emu(ulong mask) {
    Vector<ulong> vector = new(mask);
    vector               = Vector.BitwiseAnd(kBitPosEpiU64, vector);
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
   *  For <see cref="uint"/>, use <see cref="MmMovMaskEpi64Emu"/>
   * </summary>
   */
  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<uint> MmMovMaskEpi32Emu(ulong mask) {
    Vector<uint> vector = new((uint)mask);
    vector              = Vector.BitwiseAnd(kBitPosEpiU32, vector);
    return Vector.GreaterThan(vector, Vector<uint>.Zero);
  }

  /**
   * <summary>
   *  Emulation of AVX-512's <c>VPMOVM2W</c> since dotnet doesn't expose it. <br/>
   *  Reference <see href="https://www.felixcloutier.com/x86/vpmovm2b:vpmovm2w:vpmovm2d:vpmovm2q"/>
   *  <br/>
   *  Returns a vector of <see cref="uint"/> where each active bit
   *  in <paramref name="mask"/> is set to all <c>1</c>. <br/>
   *  Only considers bits up to the lane-count. <br/>
   * </summary>
   */
  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<ushort> MmMovMaskEpi16Emu(ulong mask) {
    Span<ushort> tmp = stackalloc ushort[Vector<byte>.Count >> 2];
    return Vector.Narrow(MmMovMaskEpi32Emu(mask), MmMovMaskEpi32Emu(mask >> Vector<uint>.Count));
  }

  /**
   * <summary>
   *  Emulation of AVX-512's <c>VPMOVM2W</c> since dotnet doesn't expose it. <br/>
   *  Reference <see href="https://www.felixcloutier.com/x86/vpmovm2b:vpmovm2w:vpmovm2d:vpmovm2q"/>
   *  <br/>
   *  Returns a vector of <see cref="uint"/> where each active bit
   *  in <paramref name="mask"/> is set to all <c>1</c>. <br/>
   *  Only considers bits up to the lane-count. <br/>
   * </summary>
   */
  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<byte> MmMovMaskEpi8Emu(ulong mask) {
    return Vector.Narrow(MmMovMaskEpi16Emu(mask), MmMovMaskEpi16Emu(mask >> Vector<ushort>.Count));
  }
}
}
