using System;
using System.Diagnostics.Contracts;

namespace MMOR.NET.Statistics {

public static partial class Vec2UInt64TableExtensions {
  [Pure]
  public static Vec2UInt64Table ToTransposed(this Vec2UInt64Table self) {
    Vec2UInt64Table result = new(self.Width, self.Height);

    for (int row = 0; row < self.Height; ++row) {
      for (int col = 0; col < self.Width; ++col) {
        int idx_self = row * self.Width + col;
        result.Register(col, row, self.value1_[idx_self], self.value2_[idx_self]);
      }
    }
    return result;
  }

  [Pure]
  public static Vec2UInt64Table ToSliced(this Vec2UInt64Table self, int row_start, int row_length,
      int col_start, int col_length) {
    if (row_start < 0 || col_start < 0)
      throw new ArgumentOutOfRangeException(                   //
          "[ERROR]: Trying to slice with negative value:\n" +  //
          $"  row_start: {row_start}\n" +                      //
          $"  col_start: {col_start}");
    if (row_length <= 0 || col_length <= 0)
      throw new ArgumentOutOfRangeException(                //
          "[ERROR]: Trying to slice with length <= 0:\n" +  //
          $"  row_length: {row_length}\n" +                 //
          $"  col_length: {col_length}");
    if (row_start > self.Height || (row_start + row_length) > self.Height ||
        col_start > self.Width || (col_start + col_length) > self.Width)
      throw new ArgumentOutOfRangeException(
          "[ERROR]: Trying to slice out of bounds:\n" +
          $"  self's Height: {self.Height} - row_start + length: {row_start} + {row_length}\n" +
          $"  self's Width: {self.Width} - col_start + length: {col_start} + {col_length}");

    Vec2UInt64Table result = new(row_length, col_length);

    for (int row = 0; row < row_length; ++row) {
      for (int col = 0; col < col_length; ++col) {
        int idx_self = (row + row_start) * self.Width + col + col_start;

        ulong value = self.value1_[idx_self];
        ulong freq  = self.value2_[idx_self];
        result.Register(row, col, value, freq);
      }
    }
    return result;
  }
}
}
