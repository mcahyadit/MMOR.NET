using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MMOR.NET.Collections {
public partial class RentedList<T> : IReadOnlyList<T>
    where T : unmanaged {
  public static readonly ArrayPool<T> kArrayPool = ArrayPool<T>.Create();

  public Span<T> AsSpan() => data_.AsSpan(0, Count);

  public static implicit operator Span<T>(RentedList<T> self)         => self.AsSpan();
  public static implicit operator ReadOnlySpan<T>(RentedList<T> self) => self.AsSpan();

  public int GrowCapacity() {
    // NOTE: Apparently ArrayPool always rounds up to power of two either way.
    // Cannot apply memory saving techniques.
    if (Capacity < (2 << 20)) {
      return Capacity switch {
        0 => 16,
        _ => Capacity << 1,
      };
    } else {
      int new_capa = Capacity + 1;
      return (new_capa + (new_capa >> 3) + 6) & ~3;
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Add(T value) {
    if (Count == Capacity) {
      T[] old_data = data_;
      data_        = kArrayPool.Rent(GrowCapacity());

      Array.Copy(old_data, data_, Count);
      kArrayPool.Return(old_data);
    }
    data_[count_++] = value;
    ++version_;
  }

  public void AddRange(ReadOnlySpan<T> values) {
    int capa_req = Count + values.Length;
    if (capa_req > Capacity) {
      T[] old_data = data_;

      data_ = kArrayPool.Rent(capa_req);

      Array.Copy(old_data, data_, Count);
      kArrayPool.Return(old_data);
    }
    values.CopyTo(data_.AsSpan()[Count..]);
    Count += values.Length;
  }

  public void AddRange(ICollection<T> values) {
    int len      = values.Count;
    int capa_req = Count + len;
    if (capa_req > Capacity) {
      T[] old_data = data_;

      data_ = kArrayPool.Rent(capa_req);

      Array.Copy(old_data, data_, Count);
      kArrayPool.Return(old_data);
    }

    values.CopyTo(data_, Count);
    Count += len;
  }

  public void EnsureCapacity(int capacity) {
    if (Capacity >= capacity)
      return;

    if (capacity < Count)
      throw new ArgumentException(
          $"[ERROR]: Trying to EnsureCapacity: {capacity} less than current Count: {Count}.");

    T[] old_data = data_;

    data_ = kArrayPool.Rent(capacity);
    Array.Copy(old_data, data_, Count);
    kArrayPool.Return(old_data);
  }

  public void Clear() {
    AsSpan().Clear();
    Count = 0;
  }

  public void CopyTo(T[] array, int arrayIndex) {
    Array.Copy(data_, 0, array, arrayIndex, Count);
  }
}
}
