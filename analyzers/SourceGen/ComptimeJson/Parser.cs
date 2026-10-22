using System.IO;
using ANcpLua.Roslyn.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MMOR.Roslyn {
public partial class BakeJsonGenerator {
  public const string kMetadataName          = "MMOR.Roslyn.BakedJsonAttribute";
  public const string kJsonPathAttrShort     = "BakedJsonFileAttribute";
  public const string kSerializableAttrShort = "BakedJsonGenerateSerializableAttribute";

  public static string? GetStringAtOrEmpty(AttributeData attr, int idx) {
    if (attr.ConstructorArguments.Length > idx &&
        attr.ConstructorArguments[idx].Value is string str && !string.IsNullOrWhiteSpace(str))
      return str;
    return null;
  }

  public static BakedModel ParseBakedModel(GeneratorAttributeSyntaxContext ctx,
      CancellationToken ct) {
    if (ctx.TargetSymbol is not ITypeSymbol symbol)
      return default;

    ModelKind model_kind;
    string source_path;
    bool seri        = false;
    string seri_name = "";
    switch (ctx.TargetNode) {
      case InterfaceDeclarationSyntax syntax:
        model_kind               = ModelKind.Interface;
        source_path              = syntax.SyntaxTree.FilePath;
        AttributeData? seri_attr = symbol.GetAttributeByShortName(kSerializableAttrShort);
        if (seri_attr is not null) {
          seri      = true;
          seri_name = GetStringAtOrEmpty(seri_attr, 0) ?? "";
        }
        if (string.IsNullOrWhiteSpace(seri_name)) {
          if (symbol.Name.Length > 1 && symbol.Name[0] == 'I' && char.IsUpper(symbol.Name[1])) {
            seri_name = symbol.Name[1..];
          } else {
            seri_name = symbol.Name + "Serializable";
          }
        }
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
    List<string> json_paths           = [];
    List<string> json_names           = [];
    foreach (AttributeData json in jsons) {
      if (json.ConstructorArguments[0].Value is not string attr_path) {
        continue;
      }

      string path = Path.GetFullPath(Path.Combine(dirname, attr_path));
      json_paths.Add(path);

      string? comp_name = GetStringAtOrEmpty(json, 1);
      if (comp_name is not null) {
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
      gen_seri   = seri,
      seri_name  = seri_name,
    };
  }
}
}
