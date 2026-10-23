using System.IO;
using System.Runtime.CompilerServices;
using ANcpLua.Roslyn.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MMOR.Roslyn {
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class BakedJsonAnalyzer : DiagnosticAnalyzer {
  public static readonly DiagnosticDescriptor BakedJsonSerializable = new(id: "MMOR001",
      title: $"[{BakeJsonGenerator.kSerializableAttrShort}] requires [{BakeJsonGenerator.kMetadataName}]",
      messageFormat: "Type '{0}' has [{1}] but is missing [{2}]. Add [{2}] to '{0}'.",
      category: "Usage", defaultSeverity: DiagnosticSeverity.Error, isEnabledByDefault: true);
  public static readonly DiagnosticDescriptor BakedJsonPath         = new(id: "MMOR002",
      title: $"[{BakeJsonGenerator.kJsonPathAttrShort}] requires [{BakeJsonGenerator.kMetadataName}]",
      messageFormat: "Type '{0}' has [{1}] but is missing [{2}]. Add [{2}] to '{0}'.",
      category: "Usage", defaultSeverity: DiagnosticSeverity.Error, isEnabledByDefault: true);
  public static readonly DiagnosticDescriptor BakedJsonExists       = new(id: "MMOR003",
      title: $"JSON file not found for [{BakeJsonGenerator.kJsonPathAttrShort}]",
      messageFormat: "JSON file '{0}' was not found. Ensure the path is relative to the source file, the file exists on disk, and it is included as an ItemGroup:AdditionalFiles item in the project file.",
      category: "Usage", defaultSeverity: DiagnosticSeverity.Error, isEnabledByDefault: true);
  public static readonly DiagnosticDescriptor BakedJsonDuplicate    = new(id: "MMOR004",
      title: $"Duplicate JSON path in [{BakeJsonGenerator.kJsonPathAttrShort}]",
      messageFormat: "JSON file '{0}' is referenced by multiple [{1}] attributes on the same type. Remove duplicate references.",
      category: "Usage", defaultSeverity: DiagnosticSeverity.Error, isEnabledByDefault: true);

  public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics {
    get;
  } =
      [
        BakedJsonSerializable,
        BakedJsonPath,
        BakedJsonExists,
        BakedJsonDuplicate,
      ];

  public override void Initialize(AnalysisContext context) {
    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.EnableConcurrentExecution();
    context.RegisterCompilationStartAction(start => {
      ImmutableHashSet<string> exists = [..start.Options.AdditionalFiles.Select(x => x.Path)];
      start.RegisterSymbolAction(AnalyzeSerializable, SymbolKind.NamedType);
      start.RegisterSymbolAction(c => AnalyzeJsonFile(c, exists), SymbolKind.NamedType);
    });
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool Predicate(in TypeKind type_kind) {
    return type_kind != TypeKind.Interface && type_kind != TypeKind.Class &&
           type_kind != TypeKind.Struct;
  }

  public static void AnalyzeJsonFile(SymbolAnalysisContext ctx, ImmutableHashSet<string> exists) {
    if (ctx.Symbol is not INamedTypeSymbol type || Predicate(type.TypeKind)) {
      return;
    }

    IEnumerable<AttributeData> jsons = type.GetAttributesByShortName(  //
        BakeJsonGenerator.kJsonPathAttrShort);
    if (!jsons.Any())
      return;

    string? curr_path     = null;
    HashSet<string> paths = [];
    foreach (AttributeData json in jsons) {
      // Should already be type checked
      if (json.ConstructorArguments[0].Value is not string attr_path)
        continue;

      SyntaxReference? syntax_ref = json.ApplicationSyntaxReference;
      SyntaxNode? node            = syntax_ref?.GetSyntax();

      Location loc   = node?.GetLocation() ?? type.Locations[0];
      curr_path    ??= Path.GetDirectoryName(node?.SyntaxTree.FilePath);
      if (!type.HasAttribute(BakeJsonGenerator.kMetadataName)) {
        ctx.ReportDiagnostic(Diagnostic.Create(BakedJsonPath, loc, type.Name,
            BakeJsonGenerator.kJsonPathAttrShort, BakeJsonGenerator.kMetadataNameShort));
      }

      string path = Path.GetFullPath(Path.Combine(curr_path, attr_path));
      if (!exists.Contains(path)) {
        ctx.ReportDiagnostic(Diagnostic.Create(BakedJsonExists, loc, path));
      }

      if (paths.Contains(path)) {
        ctx.ReportDiagnostic(
            Diagnostic.Create(BakedJsonDuplicate, loc, path, BakeJsonGenerator.kJsonPathAttrShort));
      }
      paths.Add(path);
    }
  }

  public static void AnalyzeSerializable(SymbolAnalysisContext ctx) {
    if (ctx.Symbol is not INamedTypeSymbol type || Predicate(type.TypeKind)) {
      return;
    }

    AttributeData? seri = type.GetAttributeByShortName(BakeJsonGenerator.kSerializableAttrShort);
    if (seri is null)
      return;

    SyntaxReference? syntax_ref = seri.ApplicationSyntaxReference;
    Location loc                = syntax_ref?.GetSyntax().GetLocation() ?? type.Locations[0];
    if (!type.HasAttribute(BakeJsonGenerator.kMetadataName)) {
      ctx.ReportDiagnostic(Diagnostic.Create(BakedJsonSerializable, loc, type.Name,
          BakeJsonGenerator.kSerializableAttrShort, BakeJsonGenerator.kMetadataNameShort));
      return;
    }
  }
}
}
