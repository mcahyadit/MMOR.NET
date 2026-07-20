using System.Security.Cryptography;
using ANcpLua.Roslyn.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MMOR.Roslyn {

internal record struct SpanOverloadModel {
  internal bool valid;
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
  // internal EquatableArray<string> type_args;
  internal EquatableArray<ValueTuple<string, string>> type_args;
  internal EquatableArray<MethodParameterModel> parameters;

  public SpanOverloadModel(MethodDeclarationSyntax decl, IMethodSymbol method) {
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

    attributes = method.GetAttributes()
                     .Where(x => x.AttributeClass!.ToDisplayString() !=
                                 "System.Runtime.CompilerServices.MethodImplAttribute")
                     .Select(Roslynutils.AttrToString)
                     .ToEquatableArray();

    using MD5 md5 = MD5.Create();
    byte[] hash   = md5.ComputeHash(Encoding.UTF8.GetBytes(decl.ToFullString()));
    simp_hash     = Convert.ToBase64String(hash).Replace('/', '_').StripSuffix("==");
  }
}

[Generator]
public class SpanOverloadGenerator : IIncrementalGenerator {
  public void Initialize(IncrementalGeneratorInitializationContext context) {
    IncrementalValuesProvider<SpanOverloadModel> methods =
        context.SyntaxProvider
            .CreateSyntaxProvider(predicate: HaveSpanAsParam, transform: ParseSpanMethod)
            .Where(x => x.valid);
    IncrementalValueProvider<ImmutableArray<SpanOverloadModel>> collected = methods.Collect();
    IncrementalValueProvider<ValueTuple<Compilation, ImmutableArray<SpanOverloadModel>>> combined =
        context.CompilationProvider.Combine(collected);
    context.RegisterSourceOutput(combined, GenerateSources);
  }

  internal static bool HaveSpanAsParam(SyntaxNode node, CancellationToken ct) {
    if (node is not MethodDeclarationSyntax method_decl) {
      return false;
    }

    if (!method_decl.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword))) {
      return false;
    }

    SeparatedSyntaxList<ParameterSyntax> parameters = method_decl.ParameterList.Parameters;
    foreach (ParameterSyntax param in parameters) {
      if (ct.IsCancellationRequested)
        break;

      if (param.Type is not GenericNameSyntax type)
        continue;

      string type_name = type.Identifier.ValueText;
      if (type_name.Contains("Span"))
        return true;
    }

    return false;
  }

  internal static SpanOverloadModel ParseSpanMethod(GeneratorSyntaxContext ctx,
      CancellationToken ct) {
    if (ctx.Node is not MethodDeclarationSyntax method_decl)
      return default;
    //
    if (ctx.SemanticModel.GetDeclaredSymbol(method_decl, ct) is not IMethodSymbol method)
      return default;

    ImmutableArray<IParameterSymbol> parameters       = method.Parameters;
    SeparatedSyntaxList<ParameterSyntax> param_syntax = method_decl.ParameterList.Parameters;
    MethodParameterModel[] solved                     = new MethodParameterModel[parameters.Length];

    string[] marks_group = new string[parameters.Length];
    ulong marked         = 0;

    for (int i = 0; i < parameters.Length; ++i) {
      IParameterSymbol param = parameters[i];
      solved[i]              = new MethodParameterModel {
        attributes =
            param_syntax[i].AttributeLists.Select(x => x.ToFullString()).ToEquatableArray(),
        name          = param.Name,
        type          = param.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
        ref_kind      = param.RefKind.RefKindToString(),
        default_value = param_syntax[i].Default?.Value.ToString(),
      };

      if (param.Type is not INamedTypeSymbol named)
        continue;

      if (param.RefKind is not RefKind.None)
        continue;

      INamedTypeSymbol def = named.OriginalDefinition;
      string x             = def.ToDisplayString();

      int count = 0;
      foreach (string key in kOverloadMap.Keys) {
        if (x.StartsWith(key)) {
          marked |= 1ul << i;
          marks_group[i] = key;
          break;
        }
      }
    }

    return new SpanOverloadModel(method_decl, method) with {
      marks_group = marks_group.ToEquatableArray(),
      marked      = marked,
      parameters  = solved.ToEquatableArray(),
    };
  }

  internal static void GenerateSources(SourceProductionContext ctx,
      ValueTuple<Compilation, ImmutableArray<SpanOverloadModel>> vx) {
    StringBuilder b = new();
    b.AppendLine($"// cnt = {vx.Item2.Length}");

    foreach (SpanOverloadModel method in vx.Item2) {
      SourceText src = SourceText.From(GenerateOverloads(method), Encoding.UTF8);
      string name    = method.name;
      b.Append("// ").AppendLine(method.simp_hash);
      b.Append("// ").AppendLine($"{method.nspace}.{method.name}-{method.simp_hash}.g.cs");
      ctx.AddSource($"{method.nspace}.{method.name}-{method.simp_hash}.g.cs", src);
    }
    ctx.AddSource("SourceGen.Debug.g.cs", b.ToString());
  }

  const string kMethodImpl =
      "[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]";

  static readonly ImmutableDictionary<string,
      ImmutableArray<ValueTuple<string, string>>> kOverloadMap =
      new Dictionary<string, ImmutableArray<ValueTuple<string, string>>>() {
        ["System.Span"] =
            [
              ("System.Span", "@"),
              ("System.Collections.Generic.List",
                  "System.Runtime.InteropServices.CollectionsMarshal.AsSpan(@)"),
            ],
        ["System.ReadOnlySpan"] =
            [
              ("System.ReadOnlySpan", "@"),
              ("System.Collections.Generic.List",
                  "@ is not null ? System.Runtime.InteropServices.CollectionsMarshal.AsSpan(@) : default"),
              ("System.Collections.Immutable.ImmutableArray", "@.AsSpan()"),
            ],
      }
          .ToImmutableDictionary();

  internal static string GenerateOverloads(SpanOverloadModel method) {
    StringBuilder sb = new();

    sb.Append("\nnamespace ").Append(method.nspace).Append(" {\n");
    foreach (string type in method.contype) {
      sb.Append(type).Append(" {\n");
    }
    // I wish we have defer

    // TODO: Generate Permutations
    ReadOnlySpan<int> permutations =
        CartesianCombination(method.marks_group, kOverloadMap, method.parameters.Length);
    int len = permutations.Length / method.parameters.Length;
    for (int i = 1; i < len; ++i) {
      // Skip first
      WriteOverload(sb, permutations.Slice(i * method.parameters.Length, method.parameters.Length),
          method);
    }

    foreach (string type in method.contype) {
      sb.Append("}  // ").Append(type).Append('\n');
    }
    sb.Append("}  // ").Append(method.nspace).Append('\n');

    return sb.ToString();
  }

  /// <inheritdoc cref="WriteOverload"/>
  /// <remarks></remarks>
  internal static void WriteOverload(StringBuilder sb, ReadOnlySpan<int> perm,
      SpanOverloadModel method) {
    if (false) {
      // NOTE: no satisfactory results for now
      sb.Append($"/// <inheritdoc file=\"{method.asmname}.xml\" path=\"doc/members/member[@name='")
          .Append(method.doc_id)
          .Append("']/*\"/>\n");
      sb.Append("/// <remarks> <b>NOTE:</b> Auto-generated Overload </remarks>\n");
    }
    foreach (string attribute in method.attributes) sb.AppendLine(attribute);
    sb.Append(kMethodImpl).Append('\n');

    foreach (string attribute in method.modifiers) sb.Append(attribute).Append(" ");
    sb.Append(method.return_type).Append(" ");
    sb.Append(method.name);
    if (method.type_args.Length > 0) {
      sb.Append('<');
      for (int i = 0; i < method.type_args.Length; ++i) {
        if (i != 0)
          sb.Append(", ");
        sb.Append(method.type_args[i].Item1);
      }
      sb.Append('>');
    }
    sb.Append("(");

    int len = method.parameters.Length;
    for (int i = 0; i < len; ++i) {
      int idx    = perm[i];
      string gid = (((method.marked >> i) & 1) == 1) ? method.marks_group[i] : "_";
      if (i != 0)
        sb.Append(", ");

      MethodParameterModel param = method.parameters[i];
      sb.Append(param.ref_kind);
      foreach (string attribute in param.attributes) sb.Append(attribute).Append(" ");
      if (gid != "_") {
        sb.Append(param.type.Replace(gid, kOverloadMap[gid][idx].Item1));
      } else {
        sb.Append(param.type);
      }
      sb.Append(' ').Append(param.name);

      if (param.default_value is not null)
        sb.Append(" = ").Append(param.default_value);
    }
    sb.Append(")");
    if (method.type_args.Length > 0) {
      for (int i = 0; i < method.type_args.Length; ++i) {
        sb.Append(method.type_args[i].Item2);
      }
    }
    sb.Append("{\n");

    // Void cannot compile with return
    if (method.return_type != "void")
      sb.Append("return ");

    sb.Append(method.name).Append("(");
    for (int i = 0; i < len; ++i) {
      int idx    = perm[i];
      string gid = (((method.marked >> i) & 1) == 1) ? method.marks_group[i] : "_";
      if (i != 0)
        sb.Append(", ");
      MethodParameterModel param = method.parameters[i];
      sb.Append(param.ref_kind);
      foreach (string attribute in param.attributes) sb.Append(attribute).Append(" ");
      if (gid != "_") {
        sb.Append(kOverloadMap[gid][idx].Item2.Replace("@", param.name));
      } else {
        sb.Append(param.name);
      }
    }
    sb.Append(");\n}\n");
  }

  internal static int[] CartesianCombination(EquatableArray<string> group_id,
      ImmutableDictionary<string, ImmutableArray<ValueTuple<string, string>>> group_val_cnt,
      int target_cnt) {
    Span<int> radices = stackalloc int[target_cnt];
    int combins       = 1;
    for (int i = 0; i < target_cnt; ++i) {
      string gid = group_id[i];
      int radix  = string.IsNullOrWhiteSpace(gid) ? 1 : group_val_cnt[gid].Length;
      radices[i] = radix;
      combins *= radix;
    }

    int[] result = new int[target_cnt * combins];

    Span<int> counter = stackalloc int[target_cnt];
    for (int ci = 0; ci < combins; ++ci) {
      Span<int> vals = result.AsSpan().Slice(ci * target_cnt, target_cnt);
      for (int i = 0; i < target_cnt; ++i) {
        vals[i] = counter[i];
      }
      for (int i = 0; i < target_cnt; ++i) {
        if (++counter[i] < radices[i]) {
          break;
        }
        counter[i] = 0;
      }
    }
    return result;
  }
}
}
