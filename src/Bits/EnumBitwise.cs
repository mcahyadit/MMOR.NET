using System;
using System.Linq.Expressions;

namespace MMOR.NET.Bits {
public static class EnumBitwise<T>
    where T : Enum {
  public static readonly Func<T, T, T> AndDelegate;
  public static readonly Func<T, T, T> OrDelegate;
  public static readonly Func<T, T> NotDelegate;

  static EnumBitwise() {
    ParameterExpression left  = Expression.Parameter(typeof(T), "left");
    ParameterExpression right = Expression.Parameter(typeof(T), "right");
    ParameterExpression value = Expression.Parameter(typeof(T), "value");

    AndDelegate =
        Expression.Lambda<Func<T, T, T>>(Expression.And(left, right), left, right).Compile();
    OrDelegate =
        Expression.Lambda<Func<T, T, T>>(Expression.Or(left, right), left, right).Compile();
    NotDelegate = Expression.Lambda<Func<T, T>>(Expression.Not(value), value).Compile();
  }

  public static T And(T left, T right) => AndDelegate(left, right);
  public static T Or(T left, T right)  => OrDelegate(left, right);
  public static T Not(T value)         => NotDelegate(value);
}
}
