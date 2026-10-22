using System.Collections.Frozen;
using ANcpLua.Roslyn.Utilities;
using Microsoft.CodeAnalysis;

namespace MMOR.Roslyn {
public partial class BakeJsonGenerator {
  public static readonly FrozenSet<string> kListTypes = [
    "System.Collections.Generic.List",
    "System.Collections.Immutable.ImmutableArray",
  ];
  public static readonly FrozenSet<string> kDictTypes = [
    "System.Collections.Generic.Dictionary",
    "System.Collections.Immutable.ImmutableDictionary",
    "System.Collections.Frozen.FrozenDictionary",
  ];

  public const ulong kContDictType = 1ul;
  public const ulong kDictKeyEnum  = 1ul << 1;
  public const ulong kDictKeyStr   = 1ul << 2;
  public const ulong kDictKeyChar  = 1ul << 3;

  public static List<ulong> GetContainerMetadata(ITypeSymbol type) {
    List<ulong> result  = [];
    ITypeSymbol current = type;
    while (current is not null) {
      if (current is IArrayTypeSymbol array) {
        current = array.ElementType;
        result.Add(0ul);
        continue;
      }

      if (current is INamedTypeSymbol named && named.IsGenericType) {
        string dis_str = current.ToDisplayString();
        int sublen     = dis_str.IndexOf('<');

        if (kListTypes.Contains(dis_str[..sublen])) {
          current = current.GetAllTypeArguments()[0];
          result.Add(0ul);
          continue;
        }  // List Types

        if (kDictTypes.Contains(dis_str[..sublen])) {
          ulong cont_type = kContDictType;
          ITypeSymbol key = current.GetAllTypeArguments()[0];
          if (key.TypeKind == TypeKind.Enum) {
            cont_type |= kDictKeyEnum;
          } else {
            cont_type |= key.SpecialType switch {
              SpecialType.System_String => kDictKeyStr,
              SpecialType.System_Char   => kDictKeyChar,
              _                         => 0,
            };
          }

          current = current.GetAllTypeArguments()[1];
          result.Add(cont_type);
          continue;
        }  // Dictionary Types
      }  // Not Generic Container
      break;
    }
    return result;
  }
}
}
