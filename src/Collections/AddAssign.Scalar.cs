using System;
using System.Numerics;

namespace MMOR.NET.Collections {
public static partial class CollectionUtils {
  public static void AddAssign(this Span<ulong> self, ulong value) {
    int alen = self.Length;
    int vlen = Vector<ulong>.Count;
    int rem  = alen - vlen;

    Vector<ulong> value_v = new(value);

    int i = 0;
    for (; i <= rem; i += vlen) {
      Vector<ulong> self_v = self.Slice(i, vlen).ToVector();
      (self_v + value_v).CopyTo(self.Slice(i, vlen));
    }
    for (; i < alen; ++i) {
      self[i] += value;
    }
  }

  public static void AddAssign(this Span<int> self, int value) {
    int alen = self.Length;
    int vlen = Vector<int>.Count;
    int rem  = alen - vlen;

    Vector<int> value_v = new(value);

    int i = 0;
    for (; i <= rem; i += vlen) {
      Vector<int> self_v = self.Slice(i, vlen).ToVector();
      (self_v + value_v).CopyTo(self.Slice(i, vlen));
    }
    for (; i < alen; ++i) {
      self[i] += value;
    }
  }

  public static void AddAssign(this Span<double> self, double value) {
    int alen = self.Length;
    int vlen = Vector<double>.Count;
    int rem  = alen - vlen;

    Vector<double> value_v = new(value);

    int i = 0;
    for (; i <= rem; i += vlen) {
      Vector<double> self_v = self.Slice(i, vlen).ToVector();
      (self_v + value_v).CopyTo(self.Slice(i, vlen));
    }
    for (; i < alen; ++i) {
      self[i] += value;
    }
  }
}
}
