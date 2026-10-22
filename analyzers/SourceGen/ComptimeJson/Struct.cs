using ANcpLua.Roslyn.Utilities;

namespace MMOR.Roslyn {

public partial class BakeJsonGenerator {
  public enum ModelKind {
    Interface     = 0,
    Class         = 1 << 0,
    Struct        = 1 << 1,
    Record        = 1 << 2,
    ClassOrStruct = Class | Struct | Record,
  }

  public readonly record struct BakedModel {
    public readonly bool valid { get; init; } = false;

    public readonly ModelKind kind { get; init; }
    public readonly required string nspace { get; init; }
    public readonly required string name { get; init; }
    internal readonly EquatableArray<PropertyModel> properties { get; init; }
    internal readonly EquatableArray<string> json_paths { get; init; }
    internal readonly EquatableArray<string> json_names { get; init; }

    public BakedModel() {}
  }

  internal readonly record struct PropertyModel {
    public readonly string type { get; init; }
    public readonly string name { get; init; }
    public readonly EquatableArray<ulong> container_metadata { get; init; }
  }

  internal readonly record struct JsonSourceModel {
    public readonly string path { get; init; }
    public readonly string contents { get; init; }
  }
}
}
