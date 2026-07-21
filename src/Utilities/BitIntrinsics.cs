using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
#if NET6_0_OR_GREATER
using System.Runtime.Intrinsics.X86;
#endif

namespace MMOR.NET.Bits {
public static partial class BitOps {
  /**
   * <summary>
   *  Calls <see cref="Bmi2.X64.ZeroHighBits"/> if supported. <br/>
   *  Fallbacks to software implementation otherwise. <br/>
   *  <br/>
   *  Returns <paramref name="value"/> with bits from indices higher
   *  than <paramref name="index"/> cleared out. <br/>
   *  <br/>
   *  For <see cref="uint"/>, use <see cref="Bzhi32"/>
   * </summary>
   */
  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong Bzhi64(ulong value, ulong index) {
#if NET6_0_OR_GREATER
    if (Bmi2.X64.IsSupported) {
      return Bmi2.X64.ZeroHighBits(value, index);
    }
#endif
    return Bzhi64Sw(value, index);
  }

  /**
   * <summary>
   *  Software equivalent of BZHI instruction for u64. <br/>
   *  <br/>
   *  Returns <paramref name="value"/> with bits from indices higher
   *  than <paramref name="index"/> cleared out. <br/>
   *  <br/>
   *  For <see cref="uint"/>, use <see cref="Bzhi32Sw"/>
   * </summary>
   */
  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong Bzhi64Sw(ulong value, ulong index) {
    ulong mask = index >= 64 ? ~0ul : (1ul << (int)index) - 1;
    return value & mask;
  }

  /**
   * <summary>
   *  Calls <see cref="Bmi2.ZeroHighBits"/> if supported. <br/>
   *  Fallbacks to software implementation otherwise. <br/>
   *  <br/>
   *  Returns <paramref name="value"/> with bits from indices higher
   *  than <paramref name="index"/> cleared out. <br/>
   *  <br/>
   *  For <see cref="ulong"/>, use <see cref="Bzhi64"/>
   * </summary>
   */
  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint Bzhi32(uint value, uint index) {
#if NET6_0_OR_GREATER
    if (Bmi2.IsSupported) {
      return Bmi2.ZeroHighBits(value, index);
    }
#endif
    return Bzhi32Sw(value, index);
  }

  /**
   * <summary>
   *  Software equivalent of BZHI instruction for u32. <br/>
   *  <br/>
   *  Returns <paramref name="value"/> with bits from indices higher
   *  than <paramref name="index"/> cleared out. <br/>
   *  <br/>
   *  For <see cref="ulong"/>, use <see cref="Bzhi64Sw"/>
   * </summary>
   */
  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint Bzhi32Sw(uint value, uint index) {
    uint mask = index >= 32 ? ~0u : (1u << (int)index) - 1;
    return value & mask;
  }

  /**
   * <summary>
   *  Calls <see cref="Bmi2.X64.ParallelBitExtract"/> if supported. <br/>
   *  Fallbacks to software implementation otherwise. <br/>
   *  <br/>
   *  Extracts each bit value in <paramref name="value"/> which bit position
   *  in <paramref name="mask"/> is active and packs them into lower bits. <br/>
   *  <br/>
   *  Visualization:
   *  <code>
   *   value  = [abcdefgh]
   *   mask   = [10101010]
   *   result = [____aceg]
   *  </code>
   * </summary>
   */
  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong Pext64(ulong value, ulong mask) {
#if NET6_0_OR_GREATER
    if (Bmi2.X64.IsSupported) {
      return Bmi2.X64.ParallelBitExtract(value, mask);
    }
#endif
    return Zp7.Pext64(value, mask);
  }

  /**
   * <summary>
   *  Calls <see cref="Bmi2.X64.ParallelBitDeposit"/> if supported. <br/>
   *  Fallbacks to software implementation otherwise. <br/>
   *  <br/>
   *  Distributes packed lower bits of <paramref name="value"/> to active
   *  bit positions in <paramref name="mask"/>. <br/>
   *  <br/>
   *  Visualization:
   *  <code>
   *   value  = [abcdefgh]
   *   mask   = [10101010]
   *   result = [e_f_g_h_]
   *  </code>
   * </summary>
   */
  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong Pdep64(ulong value, ulong mask) {
#if NET6_0_OR_GREATER
    if (Bmi2.X64.IsSupported) {
      return Bmi2.X64.ParallelBitDeposit(value, mask);
    }
#endif
    return Zp7.Pdep64(value, mask);
  }
}
}
