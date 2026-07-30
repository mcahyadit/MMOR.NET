using ANcpLua.Roslyn.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MMOR.Roslyn {
public partial class OverloadGenerator {
  internal static void ParseParameters() {}

  internal static MethodModel ParseMethod(GeneratorAttributeSyntaxContext ctx,
      CancellationToken ct) {
    if (ctx.TargetNode is not MethodDeclarationSyntax method_decl)
      return default;
    //
    if (ctx.TargetSymbol is not IMethodSymbol method)
      return default;

    ImmutableArray<IParameterSymbol> parameters       = method.Parameters;
    SeparatedSyntaxList<ParameterSyntax> param_syntax = method_decl.ParameterList.Parameters;
    ParameterModel[] solved_params                    = new ParameterModel[parameters.Length];

    ImmutableArray<AttributeData> attributes = method.GetAttributes();
    List<string> resolved_attrs              = new();
    List<Typemap> typemaps                   = new();
    HashSet<string> from_set                 = new();
    for (int i = 0; i < attributes.Length; ++i) {
      INamedTypeSymbol? attr = attributes[i].AttributeClass;
      if (attr is null)
        continue;

      string attr_name = attr.ToDisplayString();
      if (attr_name != kMetadataName) {
        if (attr_name != kMethodImplDecl)
          resolved_attrs.Add($"[{attr_name}]");
        continue;
      }

      var attr_args = attributes[i].ConstructorArguments;
      if (attr_args.Length != 4)
        continue;

      if (attr_args.Any(x => x.Value is null))
        continue;

      string from_type = (attr_args[0].Value as INamedTypeSymbol)!.ToDisplayString();
      string to_type   = (attr_args[1].Value as INamedTypeSymbol)!.ToDisplayString();
      string class_lib = (attr_args[2].Value as INamedTypeSymbol)!.ToDisplayString();
      if (class_lib == to_type)
        class_lib = "";

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
          class_lib      = class_lib,
          marshal_method = "",
        });
      }
      typemaps.Add(new Typemap {
        from_type      = from_type,
        to_type        = to_type,
        class_lib      = class_lib,
        marshal_method = (attr_args[3].Value as string)!,
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
          if (x.StartsWith(typemap.from_type)) {
            param_typemaps.Add(typemap);
          }
        }
      }

      solved_params[i] = new ParameterModel {
        attributes = param.GetAttributes()
            .Select(x => x.AttributeClass!.ToDisplayString())
            .ToEquatableArray(),
        name          = param.Name,
        type          = param.Type.ToDisplayString(),
        ref_kind      = param.RefKind.RefKindToString(),
        default_value = param_syntax[i].Default?.Value.ToString(),

        typemaps = param_typemaps.ToEquatableArray(),
      };
    }

    return new MethodModel(method_decl, method) with {
      attributes  = resolved_attrs.ToEquatableArray(),
      marks_group = marks_group.ToEquatableArray(),
      marked      = marked,
      parameters  = solved_params.ToEquatableArray(),
      typemaps    = typemaps.ToEquatableArray(),
    };
  }
}
}
