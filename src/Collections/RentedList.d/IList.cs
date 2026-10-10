using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MMOR.NET.Bits;

namespace MMOR.NET.Collections {
public partial class RentedList<T> : IList<T> {
  public bool IsReadOnly => false;

  public void Insert(int index, T item) {
    if ((uint)index > Count)
      throw new ArgumentOutOfRangeException(nameof(index), index,
          "Index must be within the bounds of the List.");

    if (index == Count) {
      Add(item);
      return;
    }

    if (Count == Capacity) {
      T[] old_data = data_;
      data_        = kArrayPool.Rent(GrowCapacity());

      old_data.AsSpan(0, index).CopyTo(data_);
      old_data.AsSpan(index, Count - index).CopyTo(data_.AsSpan(index + 1));
      kArrayPool.Return(old_data);
    } else {
      data_.AsSpan(index, Count - index).CopyTo(data_.AsSpan(index + 1));
    }
    data_[index] = item;
    ++Count;
  }

  public void RemoveAt(int index) {
    if ((uint)index >= Count)
      throw new ArgumentOutOfRangeException(nameof(index), index,
          "Index was out of range. Must be non-negative and less than the size " +
              "of the collection.");
    data_.AsSpan(index + 1, Count - (index + 1)).CopyTo(data_.AsSpan(index));
    data_[--Count] = default;
  }

  public int IndexOf(T item) {
    int i = 0;

    Type type = typeof(T).IsEnum ? typeof(T).GetEnumUnderlyingType() : typeof(T);
    if (type.IsPrimitive) {
      if (type == typeof(double)) {
        double itemf64 = Unsafe.As<T, double>(ref item);
        // NOTE: Vector.EqualsAny cannot do NaN comparison
        if (double.IsNaN(itemf64))
          goto scalar_tail;

        int vlen = Vector<double>.Count;
        int rem  = Count - vlen;

        Vector<double> comp = new(itemf64);

        for (; i <= rem; i += vlen) {
          ReadOnlySpan<double> slice = MemoryMarshal.Cast<T, double>(data_.AsSpan(i, vlen));
          Vector<double> value_epi   = slice.ToVector();

          if (Vector.EqualsAny(value_epi, comp)) {
            Vector<long> cmp_eq = Vector.Equals(value_epi, comp);
            ulong movm          = BitOps.MmMovEpi64Mask(Vector.AsVectorUInt64(cmp_eq));
            return i + BitOperations.TrailingZeroCount(movm);
          }
        }
      } else if (type == typeof(float)) {
        float itemf32 = Unsafe.As<T, float>(ref item);
        if (float.IsNaN(itemf32))
          goto scalar_tail;

        int vlen = Vector<float>.Count;
        int rem  = Count - vlen;

        Vector<float> comp = new(itemf32);

        for (; i <= rem; i += vlen) {
          ReadOnlySpan<float> slice = MemoryMarshal.Cast<T, float>(data_.AsSpan(i, vlen));
          Vector<float> value_epi   = slice.ToVector();

          if (Vector.EqualsAny(value_epi, comp)) {
            Vector<int> cmp_eq = Vector.Equals(value_epi, comp);
            ulong movm         = BitOps.MmMovEpi32Mask(Vector.AsVectorUInt32(cmp_eq));
            return i + BitOperations.TrailingZeroCount(movm);
          }
        }
      } else if (type == typeof(long) || type == typeof(ulong)) {
        int vlen = Vector<ulong>.Count;
        int rem  = Count - vlen;

        Vector<ulong> comp = new(Unsafe.As<T, ulong>(ref item));

        for (; i <= rem; i += vlen) {
          ReadOnlySpan<ulong> slice = MemoryMarshal.Cast<T, ulong>(data_.AsSpan(i, vlen));
          Vector<ulong> value_epi   = slice.ToVector();

          if (Vector.EqualsAny(value_epi, comp)) {
            Vector<ulong> cmp_eq = Vector.Equals(value_epi, comp);
            ulong movm           = BitOps.MmMovEpi64Mask(cmp_eq);
            return i + BitOperations.TrailingZeroCount(movm);
          }
        }
      } else if (type == typeof(int) || type == typeof(uint)) {
        int vlen = Vector<uint>.Count;
        int rem  = Count - vlen;

        Vector<uint> comp = new(Unsafe.As<T, uint>(ref item));

        for (; i <= rem; i += vlen) {
          ReadOnlySpan<uint> slice = MemoryMarshal.Cast<T, uint>(data_.AsSpan(i, vlen));
          Vector<uint> value_epi   = slice.ToVector();

          if (Vector.EqualsAny(value_epi, comp)) {
            Vector<uint> cmp_eq = Vector.Equals(value_epi, comp);
            ulong movm          = BitOps.MmMovEpi32Mask(cmp_eq);
            return i + BitOperations.TrailingZeroCount(movm);
          }
        }
      } else if (type == typeof(char) || type == typeof(short) || type == typeof(ushort)) {
        int vlen = Vector<ushort>.Count;
        int rem  = Count - vlen;

        Vector<ushort> comp = new(Unsafe.As<T, ushort>(ref item));

        for (; i <= rem; i += vlen) {
          ReadOnlySpan<ushort> slice = MemoryMarshal.Cast<T, ushort>(data_.AsSpan(i, vlen));
          Vector<ushort> value_epi   = slice.ToVector();

          if (Vector.EqualsAny(value_epi, comp)) {
            Vector<ushort> cmp_eq = Vector.Equals(value_epi, comp);
            ulong movm            = BitOps.MmMovEpi16Mask(cmp_eq);
            return i + BitOperations.TrailingZeroCount(movm);
          }
        }
      } else if (type == typeof(bool) || type == typeof(byte) || type == typeof(sbyte)) {
        int vlen = Vector<byte>.Count;
        int rem  = Count - vlen;

        Vector<byte> comp = new(Unsafe.As<T, byte>(ref item));

        for (; i <= rem; i += vlen) {
          ReadOnlySpan<byte> slice = MemoryMarshal.Cast<T, byte>(data_.AsSpan(i, vlen));
          Vector<byte> value_epi   = slice.ToVector();

          if (Vector.EqualsAny(value_epi, comp)) {
            Vector<byte> cmp_eq = Vector.Equals(value_epi, comp);
            ulong movm          = BitOps.MmMovEpi8Mask(cmp_eq);
            return i + BitOperations.TrailingZeroCount(movm);
          }
        }
      }
    }

  scalar_tail:
    for (; i < Count; ++i) {
      T value = data_[i];
      if (EqualityComparer<T>.Default.Equals(value, item))
        return i;
    }

    return -1;
  }

  public bool Remove(T item) {
    int idx = IndexOf(item);
    if (idx == -1)
      return false;
    RemoveAt(idx);
    return true;
  }

  public bool Contains(T item) {
    return IndexOf(item) != -1;
  }
}
}
