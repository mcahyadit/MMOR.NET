using System;
using MMOR.NET.Collections;

namespace MMOR.NET.Statistics {

public static partial class Vec2UInt64TableExtensions {
  public static void Register(this Vec2UInt64Table self, int row, int col, ulong value1,
      ulong value2) {
    if (row >= self.Height || col >= self.Width)
      throw new ArgumentOutOfRangeException(
          $"[ERROR]: Trying to access: row: {row}, col: {col}\n" +
          $"  which is out of bounds for table with Height: {self.Height}, Width: {self.Width}");

    int idx = row * self.Width + col;
    self.value1_[idx] += value1;
    self.value2_[idx] += value2;

    self.total_value1 += value1;
    self.total_value2 += value2;

    self.col_sum_value1_[col] += value1;
    self.col_sum_value2_[col] += value2;

    self.row_sum_value1_[row] += value1;
    self.row_sum_value2_[row] += value2;

    self.max_value1 = null;
    self.max_value2 = null;
    self.min_value1 = null;
    self.min_value2 = null;
  }

  public static void RegisterClamped(this Vec2UInt64Table self, int row, int col, ulong value1,
      ulong value2) {
    self.Register(Math.Clamp(row, 0, self.Height - 1), Math.Clamp(col, 0, self.Width - 1), value1,
        value2);
  }

  public static void Combine(this Vec2UInt64Table self, Vec2UInt64Table other) {
    if (self.Height != other.Height || self.Width != other.Width) {
      throw new ArgumentException(string.Format(
          "[ERROR]: Trying to combine two tables of different sizes:\nself: {0}x{1}\nother: {2}x{3}",
          self.Height, self.Width, other.Height, other.Width));
    }

    CollectionUtils.AddAssign(self.value1_, other.value1_);
    CollectionUtils.AddAssign(self.value2_, other.value2_);

    self.total_value1 += other.total_value1;
    self.total_value2 += other.total_value2;
    self.max_value1 = CollectionUtils.MaxElement(self.value1_);
    self.max_value2 = CollectionUtils.MaxElement(self.value2_);
    self.min_value1 = CollectionUtils.MinElementNonZero(self.value1_);
    self.min_value2 = CollectionUtils.MinElementNonZero(self.value2_);

    CollectionUtils.AddAssign(self.col_sum_value1_, other.col_sum_value1_);
    CollectionUtils.AddAssign(self.col_sum_value2_, other.col_sum_value2_);
    CollectionUtils.AddAssign(self.row_sum_value1_, other.row_sum_value1_);
    CollectionUtils.AddAssign(self.row_sum_value2_, other.row_sum_value2_);
  }

  public static void Clear(this Vec2UInt64Table self) {
    Array.Fill(self.value1_, 0ul);
    Array.Fill(self.value2_, 0ul);

    self.total_value1 = 0;
    self.total_value2 = 0;
    self.max_value1   = null;
    self.max_value2   = null;
    self.min_value1   = null;
    self.min_value2   = null;

    Array.Fill(self.col_sum_value1_, 0ul);
    Array.Fill(self.col_sum_value2_, 0ul);
    Array.Fill(self.row_sum_value1_, 0ul);
    Array.Fill(self.row_sum_value2_, 0ul);
  }
}
}
