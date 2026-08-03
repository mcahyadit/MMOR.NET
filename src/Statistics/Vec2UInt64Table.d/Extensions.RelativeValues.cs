using System;
using MMOR.NET.Collections;
using MMOR.NET.Mathematics;

namespace MMOR.NET.Statistics {

public static partial class Vec2UInt64TableExtensions {
  public static (float value1, float value2)
      GetRelativeValue(this Vec2UInt64Table self, int row, int col) {
    if ((uint)row >= self.Height || (uint)col >= self.Width)
      throw new ArgumentOutOfRangeException(
          $"[ERROR]: Trying to access: row: {row}, col: {col}\n" +
          $"  which is out of bounds for table with Height: {self.Height}, Width: {self.Width}");
    int idx = row * self.Width + col;

    self.max_value1 ??= CollectionUtils.MaxElement(self.value1_);
    self.max_value2 ??= CollectionUtils.MaxElement(self.value2_);

    self.min_value1 ??= CollectionUtils.MinElementNonZero(self.value1_);
    self.min_value2 ??= CollectionUtils.MinElementNonZero(self.value2_);

    float value1 = MathExt.Remap(self.value1_[idx], self.min_value1.Value, self.max_value1.Value);
    float value2 = MathExt.Remap(self.value2_[idx], self.min_value2.Value, self.max_value2.Value);
    return (value1, value2);
  }

  public static (float value1, float value2)
      GetRelativeReciprocalValue(this Vec2UInt64Table self, int row, int col) {
    if ((uint)row >= self.Height || (uint)col >= self.Width)
      throw new ArgumentOutOfRangeException(
          $"[ERROR]: Trying to access: row: {row}, col: {col}\n" +
          $"  which is out of bounds for table with Height: {self.Height}, Width: {self.Width}");
    int idx = row * self.Width + col;

    self.max_value1 ??= CollectionUtils.MaxElement(self.value1_);
    self.max_value2 ??= CollectionUtils.MaxElement(self.value2_);

    self.min_value1 ??= CollectionUtils.MinElementNonZero(self.value1_);
    self.min_value2 ??= CollectionUtils.MinElementNonZero(self.value2_);

    float value1 = self.value1_[idx] == 0
                       ? 0
                       : MathExt.Remap(1f / self.value1_[idx], 1f / self.max_value1.Value,
                             1f / self.min_value1.Value);
    float value2 = self.value2_[idx] == 0
                       ? 0
                       : MathExt.Remap(1f / self.value2_[idx], 1f / self.max_value2.Value,
                             1f / self.min_value2.Value);
    return (value1, value2);
  }

  public static (float value1, float value2) GetRelativeLogarithmicValue(this Vec2UInt64Table self,
      int row, int col, float log_base = 10) {
    if ((uint)row >= self.Height || (uint)col >= self.Width)
      throw new ArgumentOutOfRangeException(
          $"[ERROR]: Trying to access: row: {row}, col: {col}\n" +
          $"  which is out of bounds for table with Height: {self.Height}, Width: {self.Width}");
    int idx = row * self.Width + col;

    self.max_value1 ??= CollectionUtils.MaxElement(self.value1_);
    self.max_value2 ??= CollectionUtils.MaxElement(self.value2_);

    self.min_value1 ??= CollectionUtils.MinElementNonZero(self.value1_);
    self.min_value2 ??= CollectionUtils.MinElementNonZero(self.value2_);

    float value1 = MathExt.Remap(MathF.Log(self.value1_[idx] + 1, log_base),
        MathF.Log(self.min_value1.Value + 1, log_base),
        MathF.Log(self.max_value1.Value + 1, log_base));
    float value2 = MathExt.Remap(MathF.Log(self.value2_[idx] + 1, log_base),
        MathF.Log(self.min_value2.Value + 1, log_base),
        MathF.Log(self.max_value2.Value + 1, log_base));
    return (value1, value2);
  }
}
}
