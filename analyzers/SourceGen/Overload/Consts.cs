using System.Runtime.CompilerServices;

namespace MMOR.Roslyn {

public partial class OverloadGenerator {
  const string kMetadataName   = "MMOR.Roslyn.TypeMarshalOverloadAttribute";
  const string kMethodImplDecl = "System.Runtime.CompilerServices.MethodImplAttribute";
  static readonly string kMethodImpl =
      $"[{typeof(MethodImplAttribute).FullName}({typeof(MethodImplOptions).FullName}.{nameof(MethodImplOptions.AggressiveInlining)})]";
}
}
