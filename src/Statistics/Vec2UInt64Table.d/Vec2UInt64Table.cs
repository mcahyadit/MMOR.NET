using System;

namespace MMOR.NET.Statistics {

/**
 * <summary>
 *  SoA of two UInt64, with Height-Width slicing for Table-like access. <br/>
 *  Not thread-safe.
 * </summary>
 */
public class Vec2UInt64Table {
  public ulong[] value1_;
  public ulong[] value2_;

  public readonly int Height;
  public readonly int Width;

  public ulong total_value1 = 0;
  public ulong total_value2 = 0;

  public ulong? max_value1 = null;
  public ulong? max_value2 = null;
  public ulong? min_value1 = null;
  public ulong? min_value2 = null;

  public ulong[] row_sum_value1_;
  public ulong[] row_sum_value2_;

  public ulong[] col_sum_value1_;
  public ulong[] col_sum_value2_;

  public Vec2UInt64Table(int height, int width) {
    if (height <= 0 || width <= 0) {
      throw new ArgumentException($"[ERROR]: invalid height: {height}, width: {width}");
    }
    Height   = height;
    Width    = width;
    int size = height * width;
    value1_  = new ulong[size];
    value2_  = new ulong[size];

    col_sum_value1_ = new ulong[width];
    col_sum_value2_ = new ulong[width];

    row_sum_value1_ = new ulong[height];
    row_sum_value2_ = new ulong[height];
  }

  public (ulong value1, ulong value2) this[int row, int col] {
    get {
      int idx = row * Width + col;
      return (value1_[idx], value2_[idx]);
    }
  }
}
}
