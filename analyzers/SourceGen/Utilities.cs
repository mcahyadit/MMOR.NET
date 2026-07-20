using ANcpLua.Roslyn.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MMOR.Roslyn {

public record struct MethodParameterModel {
  internal EquatableArray<string> attributes;
  public string ref_kind;
  public string type;
  public string name;
  public string? default_value;
}

public static class Roslynutils {
  public static readonly SymbolEqualityComparer kCompare = SymbolEqualityComparer.Default;

  public static string RefKindToString(this RefKind rk) {
    return rk switch {
      RefKind.In  => "in ",
      RefKind.Out => "out ",
      RefKind.Ref => "ref ",
      _           => "",
    };
  }

  public static ImmutableArray<string> GetNestedTypeString(this IMethodSymbol symb) {
    Stack<string> type_rec    = new();
    INamedTypeSymbol symb_rec = symb.ContainingType;
    StringBuilder sb          = new();
    while (symb_rec != null) {
      sb.Clear();

      string acc = symb_rec.DeclaredAccessibility switch {
        Accessibility.Internal             => "internal ",
        Accessibility.Public               => "public ",
        Accessibility.Protected            => "protected ",
        Accessibility.ProtectedAndInternal => "private protected ",
        Accessibility.Private              => "private ",
        Accessibility.ProtectedOrInternal  => "protected internal ",
        _                                  => "",
      };
      sb.Append(acc);

      if (symb_rec.IsStatic)
        sb.Append("static ");
      if (symb_rec.IsAbstract)
        sb.Append("abstract ");
      if (symb_rec.IsSealed)
        sb.Append("sealed ");
      if (symb_rec.IsReadOnly)
        sb.Append("readonly ");

      sb.Append("partial ");
      if (symb_rec.IsRecord) {
        sb.Append("record ");
      }
      string typekind = symb_rec.TypeKind switch {
        TypeKind.Class     => "class ",
        TypeKind.Struct    => "struct ",
        TypeKind.Interface => "interface ",
        _                  => "",
      };
      sb.Append(typekind);
      sb.Append(symb_rec.Name);
      type_rec.Push(sb.ToString());
      symb_rec = symb_rec.ContainingType;
    }

    string[] result = new string[type_rec.Count];
    for (int i = 0; i < result.Length; ++i) {
      result[i] = type_rec.Pop();
    }
    return [..result];
  }

  public static string AttrToString(AttributeData attr) {
    StringBuilder sb = new();

    sb.Append('[');
    sb.Append(attr.AttributeClass.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));

    int len = attr.ConstructorArguments.Length;
    if (len > 0) {
      sb.Append('(');
      for (int i = 0; i < len; ++i) {
        if (i != 0)
          sb.Append(", ");
        sb.Append(attr.ConstructorArguments[i].ToCSharpString());
      }
      sb.Append(')');
    }
    sb.Append(']');
    return sb.ToString();
  }

  public static bool HasAnyConstraint(this ITypeParameterSymbol symb) {
    return symb.HasReferenceTypeConstraint | symb.HasValueTypeConstraint |
           symb.HasUnmanagedTypeConstraint | symb.HasNotNullConstraint |
           symb.HasConstructorConstraint | symb.AllowsRefLikeType | symb.ConstraintTypes.Length > 0;
  }

  public static ValueTuple<string, string> TypeArgsToStringPair(ITypeParameterSymbol symb) {
    if (!symb.HasAnyConstraint())
      return (symb.ToDisplayString(), string.Empty);

    string t                 = symb.ToDisplayString();
    List<string> constraints = new(3 + symb.ConstraintTypes.Length);
    if (symb.HasReferenceTypeConstraint)
      constraints.Add("class");
    else if (symb.HasUnmanagedTypeConstraint)
      constraints.Add("unmanaged");
    else if (symb.HasValueTypeConstraint)
      constraints.Add("struct");
    else if (symb.HasNotNullConstraint)
      constraints.Add("notnull");

    foreach (ITypeSymbol typed in symb.ConstraintTypes) {
      constraints.Add(typed.ToDisplayString());
    }

    // Needs Last
    if (symb.HasConstructorConstraint)
      constraints.Add("new()");
    if (symb.AllowsRefLikeType)
      constraints.Add("allows ref struct");

    // Follows ...

    return (t, $" where {t} : {string.Join(", ", constraints)} ");
  }
}
}
