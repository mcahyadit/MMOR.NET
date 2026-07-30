#pragma warning disable CS9113  // Parameter is unread.
using System;

namespace MMOR.Roslyn {
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class TypeMarshalOverloadAttribute(Type from, Type to, Type class_lib,
    string marshal_function)
    : Attribute;

}
