using System;
using System.Buffers;
using System.Collections.Generic;

namespace MMOR.NET.Collections {
/**
 * <summary>
 *  Dynamic-Sized Array backed with <see cref="ArrayPool{T}"/> for less GC-Pressure.
 * </summary>
 */
public partial class RentedList<T> : IDisposable {
  /**
   * <summary> Counter to guard foreach loop on edits. </summary>
   * <remarks> WARNING: Internal value, edit at your own risk. </remarks>
   */
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

  public T this[int index] {
    get => AsSpan()[index];
    set {
      AsSpan()[index] = value;
      ++version_;
    }
  }

  public T this[Index index] {
    get => AsSpan()[index];
    set {
      AsSpan()[index] = value;
      ++version_;
    }
  }

  /**
   * <remarks> WARNING: Internal value, edit at your own risk. </remarks>
   */
  public int Capacity => data_.Length;
  public T[] data_     = null!;

  public RentedList(int capacity = 4) {
    data_ = kArrayPool.Rent(capacity);
  }

  public RentedList(T initial_value, int count) {
    data_ = kArrayPool.Rent(count);
    Count = count;
    Array.Fill(data_, initial_value);
  }

  public RentedList(ReadOnlySpan<T> span) {
    Count = span.Length;
    data_ = kArrayPool.Rent(Count);
    span.CopyTo(data_);
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
}
}
