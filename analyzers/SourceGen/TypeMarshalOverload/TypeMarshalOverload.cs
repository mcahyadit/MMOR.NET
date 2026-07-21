using ANcpLua.Roslyn.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MMOR.Roslyn {

[Generator]
public class TypeMarshalOverloadGenerator : IIncrementalGenerator {
  public const string kMetadataName = "MMOR.Roslyn.TypeMarshalOverloadAttribute";
  public void Initialize(IncrementalGeneratorInitializationContext context) {
    context.RegisterPostInitializationOutput(i => {
      i.AddEmbeddedAttributeDefinition();
      i.AddSource($"{kMetadataName}.g.cs",
          @"
using System;

namespace MMOR.Roslyn {
[global::Microsoft.CodeAnalysis.EmbeddedAttribute]
[AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true)]
internal sealed class TypeMarshalOverloadAttribute : Attribute {
  public Type From { get; }
  public Type To { get; }
  public string MarshalFormat { get; }

  internal TypeMarshalOverloadAttribute(Type from, Type to, string marshal_format) {
    Type From            = from;
    Type To              = to;
    string MarshalFormat = marshal_format;
  }
}
}");
    });
    IncrementalValuesProvider<TypeMarshalOverloadModel> methods =
        context.SyntaxProvider
            .ForAttributeWithMetadataName(fullyQualifiedMetadataName: kMetadataName,
                predicate: HaveSpanAsParam, transform: ParseSpanMethod)
            .Where(x => x.valid = true);
    IncrementalValueProvider<ImmutableArray<TypeMarshalOverloadModel>> collected =
        methods.Collect();
    IncrementalValueProvider<ValueTuple<Compilation, ImmutableArray<TypeMarshalOverloadModel>>>
        combined = context.CompilationProvider.Combine(collected);
    context.RegisterSourceOutput(combined, GenerateSources);
  }

  internal static bool HaveSpanAsParam(SyntaxNode node, CancellationToken ct) {
    return node is MethodDeclarationSyntax;
  }

  public const string kMethodImplDecl = "System.Runtime.CompilerServices.MethodImplAttribute";
  internal static TypeMarshalOverloadModel ParseSpanMethod(GeneratorAttributeSyntaxContext ctx,
      CancellationToken ct) {
    if (ctx.TargetNode is not MethodDeclarationSyntax method_decl)
      return default;
    //
    if (ctx.TargetSymbol is not IMethodSymbol method)
      return default;

    ImmutableArray<IParameterSymbol> parameters       = method.Parameters;
    SeparatedSyntaxList<ParameterSyntax> param_syntax = method_decl.ParameterList.Parameters;
    TypeMarshalOverloadParameterModel[] solved_params =
        new TypeMarshalOverloadParameterModel[parameters.Length];

    ImmutableArray<AttributeData> attributes = method.GetAttributes();
    List<string> resolved_attrs              = new();
    List<Typemap> typemaps                   = new();
    HashSet<string> from_set                 = new();
    for (int i = 0; i < attributes.Length; ++i) {
      INamedTypeSymbol? attr = attributes[i].AttributeClass;
      if (attr is null)
        continue;

      string attr_name = attr.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
      if (attr_name != $"global::{kMetadataName}" && attr_name != kMetadataName) {
        if (attr_name != kMethodImplDecl && attr_name != $"global::{kMethodImplDecl}")
          resolved_attrs.Add($"[{attr_name}]");
        continue;
      }

      var attr_args = attributes[i].ConstructorArguments;
      if (attr_args.Length != 3)
        continue;

      if (attr_args.Any(x => x.Value is null))
        continue;

      string from_type = (attr_args[0].Value as INamedTypeSymbol)!.ToDisplayString();
      string to_type   = (attr_args[1].Value as INamedTypeSymbol)!.ToDisplayString();
      if (from_type.Contains("<>")) {
        from_type = from_type[..from_type.IndexOf('<')];
        to_type   = to_type[..to_type.IndexOf('<')];
      }

      if (!from_set.Contains(from_type)) {
        // Add default if not exists
        from_set.Add(from_type);
        typemaps.Add(new Typemap {
          from_type      = from_type,
          to_type        = from_type,
          marshal_method = "@",
        });
      }
      typemaps.Add(new Typemap {
        from_type      = from_type,
        to_type        = to_type,
        marshal_method = (attr_args[2].Value as string)!,
      });
    }

    string[] marks_group = new string[parameters.Length];
    ulong marked         = 0;

    for (int i = 0; i < parameters.Length; ++i) {
      IParameterSymbol param       = parameters[i];
      List<Typemap> param_typemaps = [];

      if (param.Type is INamedTypeSymbol named && param.RefKind is RefKind.None &&
          !param.HasExplicitDefaultValue) {
        INamedTypeSymbol def = named.OriginalDefinition;
        string x             = def.ToDisplayString();

        foreach (Typemap typemap in typemaps) {
          if (x.StartsWith(typemap.from_type) || x.StartsWith($"global::{typemap.from_type}")) {
            param_typemaps.Add(typemap);
          }
        }
      }

      solved_params[i] = new TypeMarshalOverloadParameterModel {
        attributes = param.GetAttributes()
            .Select(
                x => x.AttributeClass!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
            .ToEquatableArray(),
        name          = param.Name,
        type          = param.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
        ref_kind      = param.RefKind.RefKindToString(),
        default_value = param_syntax[i].Default?.Value.ToString(),

        typemaps = param_typemaps.ToEquatableArray(),
      };
    }

    return new TypeMarshalOverloadModel(method_decl, method) with {
      attributes  = resolved_attrs.ToEquatableArray(),
      marks_group = marks_group.ToEquatableArray(),
      marked      = marked,
      parameters  = solved_params.ToEquatableArray(),
      typemaps    = typemaps.ToEquatableArray(),
    };
  }

  internal static void GenerateSources(SourceProductionContext ctx,
      ValueTuple<Compilation, ImmutableArray<TypeMarshalOverloadModel>> vx) {
    StringBuilder b = new();
    b.AppendLine($"// cnt = {vx.Item2.Length}");

    foreach (TypeMarshalOverloadModel method in vx.Item2) {
      SourceText src = SourceText.From(GenerateOverloads(method), Encoding.UTF8);
      string name    = method.name;
      b.Append("// ").AppendLine($"{method.nspace}.{method.name}-{method.simp_hash}.g.cs");
      b.Append("// ").AppendLine(
          $"typemaps[{method.typemaps.Length}]: {string.Join(", ", method.typemaps)}");
      b.Append("// ").AppendLine($"attrs: {string.Join(", ", method.attributes)}");
      ctx.AddSource($"{method.nspace}.{method.name}-{method.simp_hash}.g.cs", src);
    }
    ctx.AddSource("SourceGen.Debug.g.cs", b.ToString());
  }

  const string kMethodImpl =
      "[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]";

  internal static string GenerateOverloads(TypeMarshalOverloadModel method) {
    StringBuilder sb = new();

    sb.Append("\nnamespace ").Append(method.nspace).Append(" {\n");
    foreach (string type in method.contype) {
      sb.Append(type).Append(" {\n");
    }
    // I wish we have defer

    // TODO: Generate Permutations
    ReadOnlySpan<int> permutations = CartesianCombination(method.parameters);
    int len                        = permutations.Length / method.parameters.Length;
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
      TypeMarshalOverloadModel method) {
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
      int idx = perm[i];
      if (i != 0)
        sb.Append(", ");

      TypeMarshalOverloadParameterModel param = method.parameters[i];
      sb.Append(param.ref_kind);
      foreach (string attribute in param.attributes) sb.Append(attribute).Append(" ");
      if (param.typemaps.Length > 0) {
        sb.Append(param.type.Replace(param.typemaps[idx].from_type, param.typemaps[idx].to_type));
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
      int idx = perm[i];
      if (i != 0)
        sb.Append(", ");
      TypeMarshalOverloadParameterModel param = method.parameters[i];
      sb.Append(param.ref_kind);
      foreach (string attribute in param.attributes) sb.Append(attribute).Append(" ");
      if (param.typemaps.Length > 0) {
        sb.Append(param.typemaps[idx].marshal_method.Replace("@", param.name));
      } else {
        sb.Append(param.name);
      }
    }
    sb.Append(");\n}\n");
  }

  internal static int[] CartesianCombination(
      ImmutableArray<TypeMarshalOverloadParameterModel> parameters) {
    int len           = parameters.Length;
    Span<int> radices = stackalloc int[len];
    int combins       = 1;
    for (int i = 0; i < len; ++i) {
      int radix  = Math.Max(parameters[i].typemaps.Length, 1);
      radices[i] = radix;
      combins *= radix;
    }

    int[] result = new int[len * combins];

    Span<int> counter = stackalloc int[len];
    for (int ci = 0; ci < combins; ++ci) {
      Span<int> vals = result.AsSpan().Slice(ci * len, len);
      for (int i = 0; i < len; ++i) {
        vals[i] = counter[i];
      }
      for (int i = 0; i < len; ++i) {
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
