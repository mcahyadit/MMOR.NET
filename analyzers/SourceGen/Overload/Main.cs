using ANcpLua.Roslyn.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MMOR.Roslyn {

[Generator]
public partial class OverloadGenerator : IIncrementalGenerator {
  public void Initialize(IncrementalGeneratorInitializationContext context) {
    IncrementalValuesProvider<MethodModel> methods =
        context.SyntaxProvider
            .ForAttributeWithMetadataName(fullyQualifiedMetadataName: kMetadataName,
                predicate: HaveSpanAsParam, transform: ParseMethod)
            .Where(x => x.valid = true);
    IncrementalValueProvider<ImmutableArray<MethodModel>> collected = methods.Collect();
    IncrementalValueProvider<ValueTuple<Compilation, ImmutableArray<MethodModel>>> combined =
        context.CompilationProvider.Combine(collected);
    context.RegisterSourceOutput(combined, GenerateSources);
  }

  internal static bool HaveSpanAsParam(SyntaxNode node, CancellationToken ct) {
    return node is MethodDeclarationSyntax;
  }

  internal static void GenerateSources(SourceProductionContext ctx,
      ValueTuple<Compilation, ImmutableArray<MethodModel>> vx) {
    StringBuilder b = new();
    b.AppendLine($"// cnt = {vx.Item2.Length}");

    foreach (MethodModel method in vx.Item2) {
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
}
}
