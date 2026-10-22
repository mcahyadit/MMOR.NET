using System.IO;
using ANcpLua.Roslyn.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MMOR.Roslyn {
[Generator]
public partial class BakeJsonGenerator : IIncrementalGenerator {
  public void Initialize(IncrementalGeneratorInitializationContext context) {
    IncrementalValuesProvider<BakedModel> models =
        context.SyntaxProvider
            .ForAttributeWithMetadataName(fullyQualifiedMetadataName: kMetadataName,
                predicate: PredicateJsonModel, transform: ParseBakedModel)
            .Where(x => x.valid);

    IncrementalValuesProvider<JsonSourceModel> jsons =
        context.AdditionalTextsProvider.Select(static (text, ct) => new JsonSourceModel {
          path     = text.Path,
          contents = text.GetText(ct)?.ToString() ?? string.Empty,
        });

    IncrementalValueProvider<(ImmutableArray<BakedModel> model,
        ImmutableArray<JsonSourceModel> json)> collected =
        models.Collect().Combine(jsons.Collect());
    IncrementalValueProvider<(Compilation,
        (ImmutableArray<BakedModel>, ImmutableArray<JsonSourceModel>))> combined =
        context.CompilationProvider.Combine(collected);
    context.RegisterSourceOutput(combined, GenerateSources);
  }

  public static bool PredicateJsonModel(SyntaxNode node, CancellationToken ct) {
    return node is InterfaceDeclarationSyntax || node is ClassDeclarationSyntax ||
           node is StructDeclarationSyntax || node is RecordDeclarationSyntax;
  }

  internal static void GenerateSources(SourceProductionContext ctx,
      (Compilation,
          (ImmutableArray<BakedModel> models, ImmutableArray<JsonSourceModel> jsons))tuple) {
    StringBuilder debug = new();
    debug.AppendLine($"// cnt = {tuple.Item2.models.Length}");
    foreach (JsonSourceModel json in tuple.Item2.jsons) {
      debug.AppendLine($"// path = {json.path}");
    }

    foreach (BakedModel type_model in tuple.Item2.models) {
      debug.Append("// ").AppendLine($"{type_model.name}.g.cs");

      for (int i = 0; i < type_model.properties.Length; ++i) {
        PropertyModel prop = type_model.properties[i];
        if (prop.container_metadata.Length > 0) {
          debug.Append($"// type: {prop.type}\n");
          debug.Append($"// meta: {prop.container_metadata.Select(x => $"{x:X}").Join('-')}\n");
          debug.Append("\n");
        }
      }

      debug.Append("// Json Paths:\n");
      for (int i = 0; i < type_model.json_paths.Length; ++i) {
        string json_path = type_model.json_paths[i];
        string json_name = Path.GetFileNameWithoutExtension(json_path);
        debug.AppendLine($"// path = {json_path}");
        JsonSourceModel json_model = tuple.Item2.jsons.FirstOrDefaultFast(x => x.path == json_path);
        string content             = TranspileJson(type_model, i, json_model);
        ctx.AddSource($"{json_name}.g.cs", SourceText.From(content, Encoding.UTF8));
      }
    }
    ctx.AddSource("SourceGen.Debug.g.cs", debug.ToString());
  }
}
}
