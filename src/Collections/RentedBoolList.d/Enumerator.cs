using System;
using System.Collections;
using System.Collections.Generic;

namespace MMOR.NET.Collections {
public partial class RentedBoolList {
  public struct Enumerator : IEnumerator<bool> {
    public bool Current { get; set; }
    readonly RentedBoolList vector_;
    readonly int version_;
    int idx;

    public Enumerator(RentedBoolList self) {
      vector_  = self;
      version_ = self.version_;
      idx      = 0;
      Current  = default;
    }

    readonly object IEnumerator.Current => Current;

    readonly public void Dispose() {}

    readonly public void ThrowIfModified() {
      if (version_ != vector_.version_)
        throw new InvalidOperationException(
            "Collection was modified; enumeration operation may not execute.");
    }

    public bool MoveNext() {
      ThrowIfModified();

      if (idx >= vector_.Count) {
        Current = default;
        return false;
      }

      Current = vector_[idx++];
      return true;
    }

    public void Reset() {
      ThrowIfModified();

      idx     = 0;
      Current = default;
    }
  }

  public Enumerator GetEnumerator() => new(this);

  IEnumerator<bool> IEnumerable<bool>.GetEnumerator() => GetEnumerator();

  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
}
