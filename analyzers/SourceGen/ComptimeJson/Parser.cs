using System.IO;
using ANcpLua.Roslyn.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MMOR.Roslyn {
public partial class BakeJsonGenerator {
  public const string kMetadataName      = "MMOR.Roslyn.BakedJsonAttribute";
  public const string kJsonPathAttrShort = "BakedJsonFileAttribute";

  public static BakedModel ParseBakedModel(GeneratorAttributeSyntaxContext ctx,
      CancellationToken ct) {
    if (ctx.TargetSymbol is not ITypeSymbol symbol)
      return default;

    ModelKind model_kind;
    string source_path;
    switch (ctx.TargetNode) {
      case InterfaceDeclarationSyntax syntax:
        model_kind  = ModelKind.Interface;
        source_path = syntax.SyntaxTree.FilePath;
        break;
      case ClassDeclarationSyntax syntax:
        model_kind  = ModelKind.Class;
        source_path = syntax.SyntaxTree.FilePath;
        break;
      case StructDeclarationSyntax syntax:
        model_kind  = ModelKind.Struct;
        source_path = syntax.SyntaxTree.FilePath;
        break;
      case RecordDeclarationSyntax syntax:
        model_kind  = ModelKind.Record;
        source_path = syntax.SyntaxTree.FilePath;
        break;
      default:
        return default;
    }

    // Get Properties
    ImmutableArray<IPropertySymbol> raw_props = [..symbol.GetMembers().OfType<IPropertySymbol>()];
    PropertyModel[] resolved_props            = new PropertyModel[raw_props.Length];
    for (int i = 0; i < raw_props.Length; ++i) {
      IPropertySymbol property = raw_props[i];

      resolved_props[i] = new PropertyModel {
        name = property.Name,
        type = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
        container_metadata = GetContainerMetadata(property.Type).ToEquatableArray(),
      };
    }

    // Json Paths
    string dirname                    = Path.GetDirectoryName(source_path);
    IEnumerable<AttributeData>? jsons = symbol.GetAttributesByShortName(kJsonPathAttrShort);
    List<string> json_paths           = new List<string>();
    List<string> json_names           = new List<string>();
    foreach (AttributeData json in jsons) {
      if (json.ConstructorArguments[0].Value is not string attr_path) {
        continue;
      }

      string path = Path.GetFullPath(Path.Combine(dirname, attr_path));
      json_paths.Add(path);

      if (json.ConstructorArguments.Length > 1 &&
          json.ConstructorArguments[1].Value is string comp_name &&
          !string.IsNullOrWhiteSpace(comp_name)) {
        json_names.Add(comp_name);
      } else {
        // No name provided, transform from path
        Span<char> raw_name = [..Path.GetFileName(path)];
        for (int i = 0; i < raw_name.Length; ++i) {
          char c = raw_name[i];
          if (!char.IsLetter(c) && !char.IsNumber(c) && c != '_') {
            raw_name[i] = '_';
          }
        }
        if (!char.IsLetter(raw_name[0]) && raw_name[0] != '_')
          json_names.Add("__" + raw_name.ToString());
        else
          json_names.Add(raw_name.ToString());
      }
    }

    return new BakedModel {
      valid      = true,
      kind       = model_kind,
      name       = symbol.Name,
      nspace     = symbol.ContainingNamespace.ToDisplayString(),
      properties = resolved_props.ToEquatableArray(),
      json_paths = json_paths.ToEquatableArray(),
      json_names = json_names.ToEquatableArray(),
    };
  }
}
}
