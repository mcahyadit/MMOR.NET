using System;
using System.Collections.Generic;

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
    return CollectionUtils.IndexOf(AsSpan(), item);
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
