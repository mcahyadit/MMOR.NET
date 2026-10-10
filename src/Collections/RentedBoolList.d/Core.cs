using System;
using System.Buffers;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MMOR.NET.Bits;

namespace MMOR.NET.Collections {
public partial class RentedBoolList : IReadOnlyList<bool>, IList<bool>, IDisposable {
  public static readonly ArrayPool<byte> kArrayPool = ArrayPool<byte>.Create();

  const int kWordBytes = sizeof(ulong);

  /// <inheritdoc cref="RentedList{T}.version_"/>
  public int version_ = 0;

  int count_ = 0;
  /**
   * <inheritdoc cref="ICollection{T}.Count"/>
   * <remarks> WARNING: Internal value, edit at your own risk. </remarks>
   */
  public int Count {
    get => count_;
    set {
      count_ = value;
      ++version_;
    }
  }

  public int Capacity => data_.Length << 3;
  public byte[] data_;

  /**
   * <summary> Reinterpret the raw byte storage as 64-bit words for bit-twiddling. </summary>
   */
  Span<ulong> QWords => MemoryMarshal.Cast<byte, ulong>(data_);

  public bool this[int index] {
    get {
      if ((uint)index >= Count) {
        throw new ArgumentOutOfRangeException(nameof(index), index,
            $"[ERROR]: trying to access index: {index} in array of {Count}");
      }
      return ((data_[index >> 3] >> (index & 7)) & 1) == 1;
    }
    set {
      if ((uint)index >= Count) {
        throw new ArgumentOutOfRangeException(nameof(index), index,
            $"[ERROR]: trying to access index: {index} in array of {Count}");
      }
      ulong idx_mask = 1ul << (index & 63);
      ref ulong word = ref QWords[index >> 6];
      word           = (word & ~idx_mask) | ((0ul - Unsafe.As<bool, byte>(ref value)) & idx_mask);
      ++version_;
    }
  }

  public Span<ulong> AsSpan() => QWords.Slice(0, (Count + 63) >> 6);

  /**
   * <summary> Convert a bit count into a byte count, rounded up to whole words. </summary>
   * <remarks>
   *   Always rents at least one <c>ulong</c> worth of bytes so that
   *   <see cref="MemoryMarshal.Cast{TFrom, TTo}(Span{TFrom})"/> always yields a usable word span.
   * </remarks>
   */
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  static int RentBytes(int bits) {
    int bytes = Math.Max((bits + 7) >> 3, kWordBytes);
    return (bytes + kWordBytes - 1) & ~(kWordBytes - 1);
  }

  public RentedBoolList(int capacity = 64) {
    data_ = kArrayPool.Rent(RentBytes(capacity));
  }

  public void Dispose() {
    if (data_ == null) {
      return;
    }

    kArrayPool.Return(data_);
    data_ = null!;
    ++version_;
    GC.SuppressFinalize(this);
  }

  public void Add(bool value) {
    if (Count == Capacity) {
      byte[] old_data = data_;
      data_           = kArrayPool.Rent(Math.Max(old_data.Length << 1, kWordBytes));

      Array.Copy(old_data, data_, old_data.Length);
      kArrayPool.Return(old_data);
    }
    int idx        = count_++;
    ulong idx_mask = 1ul << (idx & 63);
    ref ulong word = ref QWords[idx >> 6];
    word           = (word & ~idx_mask) | ((0ul - Unsafe.As<bool, byte>(ref value)) & idx_mask);
    ++version_;
  }

  public void EnsureCapacity(int capacity) {
    if (Capacity >= capacity)
      return;

    if (capacity < Count)
      throw new ArgumentException($"[ERROR]: capacity: {capacity}, is less than Count: {Count}.");

    byte[] old_data = data_;

    data_ = kArrayPool.Rent(RentBytes(capacity));
    Array.Copy(old_data, data_, old_data.Length);
    kArrayPool.Return(old_data);
  }

  public void Clear() {
    data_.AsSpan().Clear();
    Count = 0;
  }

  public int IndexOf(bool item) {
    Span<ulong> words = QWords;
    int alen          = AsSpan().Length;
    if (alen == 0)
      return -1;

    int brem = Count & 63;
    int vlen = Vector<ulong>.Count;
    int vend = alen - (alen % vlen);
    if (brem != 0 && vend == alen)
      vend -= vlen;

    for (int i = 0; i < vend; i += vlen) {
      Vector<ulong> word_epi = words.Slice(i, vlen).ToVector();
      if (!item)
        word_epi = Vector.OnesComplement(word_epi);
      Vector<ulong> cmp_eq = Vector.Equals(word_epi, Vector<ulong>.Zero);
      ulong movq2m         = BitOps.Bzhi64(~BitOps.MmMovEpi64Mask(cmp_eq), vlen);

      if (movq2m != 0) {
        int vidx = BitOperations.TrailingZeroCount(movq2m);
        int idx  = i + vidx;
        return (idx << 6) + BitOperations.TrailingZeroCount(word_epi[vidx]);
      }
    }

    for (int i = vend; i < alen; ++i) {
      ulong mask = item ? words[i] : ~words[i];
      if (mask != 0) {
        if (brem != 0 && i == alen - 1)
          mask = BitOps.Bzhi64(mask, Count & 63);
        return mask == 0 ? -1 : (i << 6) + BitOperations.TrailingZeroCount(mask);
      }
    }

    return -1;
  }

  public bool Contains(bool item) {
    return IndexOf(item) != -1;
  }

  public void Insert(int index, bool item) {
    if ((uint)index > Count)
      throw new ArgumentOutOfRangeException(nameof(index), index,
          "Index must be within the bounds of the List.");

    if (index == Count) {
      Add(item);
      return;
    }

    int widx = index >> 6;
    int bidx = index & 63;

    ulong value = item ? 1ul << bidx : 0;

    if (Count == Capacity) {
      byte[] old_data = data_;
      data_           = kArrayPool.Rent(Math.Max(old_data.Length << 1, kWordBytes));

      Array.Copy(old_data, data_, old_data.Length);
      kArrayPool.Return(old_data);
    }
    ++Count;

    // Insert
    Span<ulong> words = QWords;
    ulong m_word      = words[widx];
    ulong lower       = BitOps.Bzhi64(m_word, bidx);
    ulong upper       = m_word & ~lower;
    ulong curr_carry  = m_word >> 63;
    words[widx]       = lower | value | (upper << 1);

    // Shift Rest
    int wlen = AsSpan().Length;
    for (int i = widx + 1; i < wlen; ++i) {
      ulong word       = words[i];
      ulong next_carry = word >> 63;
      words[i]         = curr_carry | (word << 1);
      curr_carry       = next_carry;
    }
  }

  public void RemoveAt(int index) {
    if ((uint)index >= Count)
      throw new ArgumentOutOfRangeException(nameof(index), index,
          "Index was out of range. Must be non-negative and less than the size " +
              "of the collection.");

    int widx = index >> 6;
    int bidx = index & 63;

    Span<ulong> words = QWords;
    ulong curr_carry  = 0;
    for (int i = AsSpan().Length - 1; i > widx; --i) {
      ulong word       = words[i];
      ulong next_carry = word << 63;
      words[i]         = (word >> 1) | curr_carry;
      curr_carry       = next_carry;
    }

    ulong mask   = 1ul << bidx;
    ulong m_word = words[widx];
    ulong lower  = BitOps.Bzhi64(m_word, bidx);
    ulong upper  = m_word & ~(lower | mask);
    words[widx]  = lower | (upper >> 1) | curr_carry;
    --Count;
  }

  public void CopyTo(bool[] array, int arrayIndex) {
    Span<bool> span = array.AsSpan(arrayIndex);
    int alen        = span.Length;
    if (alen < Count)
      throw new ArgumentOutOfRangeException(
          $"[ERROR]: copy area Length: {alen} is less then source Length: {Count}");

    Span<ulong> words = QWords;
    int wlen          = AsSpan().Length;
    for (int widx = 0; widx < wlen; ++widx) {
      int bidx = widx << 6;
      int len  = Math.Min(alen - bidx, Count - bidx);
      BitOps.WriteToBoolArray(words[widx], span.Slice(bidx, len));
    }
  }

  public bool Remove(bool item) {
    int idx = IndexOf(item);
    if (idx == -1)
      return false;
    RemoveAt(idx);
    return true;
  }

  public bool IsReadOnly => false;
}
}
