using System;
using System.Numerics;
using MMOR.NET.RichString;

namespace MMOR.NET.Statistics {
using MMOR.NET.ColorMaps;

public class Vec2UInt64TablePrintOpts {
  /**
   * <summary>
   *  Determines if a row is printed or not. <br/>
   *  Takes in: <br/>
   *  - <see cref="Vec2UInt64Table"/> <c>s</c>: reference to the table <br/>
   *  - <see cref="int"/> <c>r</c>: current row <br/>
   * </summary>
   */
  public Func<Vec2UInt64Table, int, bool> row_predicate = (_, _) => true;

  /**
   * <summary>
   *  Determines if a column is printed or not. <br/>
   *  Takes in: <br/>
   *  - <see cref="Vec2UInt64Table"/> <c>s</c>: reference to the table <br/>
   *  - <see cref="int"/> <c>c</c>: current col <br/>
   * </summary>
   */
  public Func<Vec2UInt64Table, int, bool> col_predicate = (_, _) => true;

  /**
   * <summary>
   *  Dictates how <see cref="Vec2UInt64Table.value1_"/> should be printed in table. <br/>
   *  Takes in: <br/>
   *  - <see cref="Vec2UInt64Table"/> <c>s</c>: reference to the table <br/>
   *  - <see cref="int"/> <c>r</c>: current row <br/>
   *  - <see cref="int"/> <c>c</c>: current col <br/>
   *  - <see cref="ulong"/> <c>v</c>: current value <br/>
   * </summary>
   */
  public Func<Vec2UInt64Table, int, int, ulong, IRichString> value1_print = (s, r, c, v) =>
      v == 0 ? RichStringUtils.kRichEmpty
             : v.ToString().SetColor(ColorMaps.OmniBlueYellow(s.GetRelativeValue(r, c).value1));

  /**
   * <summary>
   *  Dictates how <see cref="Vec2UInt64Table.value2_"/> should be printed in table. <br/>
   *  Takes in: <br/>
   *  - <see cref="Vec2UInt64Table"/> <c>s</c>: reference to the table <br/>
   *  - <see cref="int"/> <c>r</c>: current row <br/>
   *  - <see cref="int"/> <c>c</c>: current col <br/>
   *  - <see cref="ulong"/> <c>v</c>: current value <br/>
   * </summary>
   */
  public Func<Vec2UInt64Table, int, int, ulong, IRichString> value2_print = (s, r, c, v) =>
      v == 0 ? RichStringUtils.kRichEmpty
             : v.ToString().SetColor(ColorMaps.OmniGrayRed(s.GetRelativeValue(r, c).value2));

  /**
   * <summary>
   *  Dictates how <see cref="Vec2UInt64Table.row_sum_value1_"/> should be printed. <br/>
   *  Takes in: <br/>
   *  - <see cref="Vec2UInt64Table"/> <c>s</c>: reference to the table <br/>
   *  - <see cref="int"/> <c>c</c>: current col <br/>
   *  - <see cref="ulong"/> <c>v</c>: current value <br/>
   * </summary>
   */
  public Func<Vec2UInt64Table, int, ulong, IRichString> row_sum_value1_print = (_, _, v) =>
      v.ToString().AsRichString();

  /**
   * <summary>
   *  Dictates how <see cref="Vec2UInt64Table.row_sum_value2_"/> should be printed. <br/>
   *  Takes in: <br/>
   *  - <see cref="Vec2UInt64Table"/> <c>s</c>: reference to the table <br/>
   *  - <see cref="int"/> <c>c</c>: current col <br/>
   *  - <see cref="ulong"/> <c>v</c>: current value <br/>
   * </summary>
   */
  public Func<Vec2UInt64Table, int, ulong, IRichString> row_sum_value2_print = (_, _, v) =>
      v.ToString().AsRichString();

  /**
   * <summary>
   *  Dictates how <see cref="Vec2UInt64Table.col_sum_value1_"/> should be printed. <br/>
   *  Takes in: <br/>
   *  - <see cref="Vec2UInt64Table"/> <c>s</c>: reference to the table <br/>
   *  - <see cref="int"/> <c>r</c>: current row <br/>
   *  - <see cref="ulong"/> <c>v</c>: current value <br/>
   * </summary>
   */
  public Func<Vec2UInt64Table, int, ulong, IRichString> col_sum_value1_print = (_, _, v) =>
      v.ToString().AsRichString();

  /**
   * <summary>
   *  Dictates how <see cref="Vec2UInt64Table.col_sum_value2_"/> should be printed. <br/>
   *  Takes in: <br/>
   *  - <see cref="Vec2UInt64Table"/> <c>s</c>: reference to the table <br/>
   *  - <see cref="int"/> <c>r</c>: current row <br/>
   *  - <see cref="ulong"/> <c>v</c>: current value <br/>
   * </summary>
   */
  public Func<Vec2UInt64Table, int, ulong, IRichString> col_sum_value2_print = (_, _, v) =>
      v.ToString().AsRichString();

  public Func<int, IRichString> col_header = (col) => $"#{col + 1}".AsRichString();
  public Func<int, IRichString> row_header = (row) => $"#{row + 1} ".AsRichString();

  public string column_separator         = "|";
  public string? column_header_separator = "||";
  public string values_separator         = " ";
  public char content_guide              = '-';
  public char sum_guide                  = '=';
  public char? table_delimiter           = '=';

  public Vector4[]? row_colors = null;
}

}
