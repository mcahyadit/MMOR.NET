using System;
using System.Collections;
using System.Collections.Generic;

namespace MMOR.NET.Collections {
public partial class RentedList<T> {
  public struct Enumerator : IEnumerator<T> {
    readonly RentedList<T> list_;
    readonly int version_;
    int idx;
    public T Current { get; set; }

    public Enumerator(RentedList<T> self) {
      list_    = self;
      version_ = self.version_;
      idx      = 0;
      Current  = default;
    }

    readonly object IEnumerator.Current => Current;

    readonly public void Dispose() {}

    readonly public void ThrowIfModified() {
      if (version_ != list_.version_)
        throw new InvalidOperationException(
            "Collection was modified; enumeration operation may not execute.");
    }

    public bool MoveNext() {
      ThrowIfModified();

      if (idx >= list_.Count) {
        Current = default;
        return false;
      }

      Current = list_.data_[idx++];
      return true;
    }

    public void Reset() {
      ThrowIfModified();

      idx     = 0;
      Current = default;
    }
  }
  public Enumerator GetEnumerator() => new(this);

  IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
}
