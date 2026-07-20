using System.Security.Cryptography;
using ANcpLua.Roslyn.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MMOR.Roslyn {

internal readonly record struct Typemap {
  public readonly string from_type { get; init; }
  public readonly string to_type { get; init; }
  public readonly string marshal_method { get; init; }
}

internal readonly record struct TypeMarshalOverloadParameterModel {
  internal EquatableArray<string> attributes { get; init; }
  public string ref_kind { get; init; }
  public string type { get; init; }
  public string name { get; init; }
  public string? default_value { get; init; }

  public EquatableArray<Typemap> typemaps { get; init; }
}

internal record struct TypeMarshalOverloadModel {
  internal bool valid;
  internal EquatableArray<Typemap> typemaps;
  internal EquatableArray<string> marks_group;
  internal ulong marked;

  internal string asmname;
  internal string fullname;
  internal string? doc_id;
  internal string simp_hash;
  internal string nspace;
  internal EquatableArray<string> contype;

  internal EquatableArray<string> attributes;
  internal EquatableArray<string> modifiers;
  internal string return_type;
  internal string name;
  internal EquatableArray<ValueTuple<string, string>> type_args;
  internal EquatableArray<TypeMarshalOverloadParameterModel> parameters;

  public TypeMarshalOverloadModel(MethodDeclarationSyntax decl, IMethodSymbol method) {
    valid = true;

    asmname = method.ContainingAssembly.Name;

    fullname    = method.ContainingType.ToDisplayString() + '.' + method.Name;
    doc_id      = method.GetDocumentationCommentId();
    nspace      = method.ContainingNamespace.ToDisplayString();
    contype     = method.GetNestedTypeString().ToEquatableArray();
    return_type = method.ReturnType.ToDisplayString();
    name        = method.Name;
    type_args   = method.TypeParameters.Select(Roslynutils.TypeArgsToStringPair).ToEquatableArray();
    modifiers   = decl.Modifiers.Select(x => x.ToString()).ToEquatableArray();

    using MD5 md5 = MD5.Create();
    byte[] hash   = md5.ComputeHash(Encoding.UTF8.GetBytes(decl.ToFullString()));
    simp_hash     = Convert.ToBase64String(hash).Replace('/', '_').StripSuffix("==");
  }
}

}
