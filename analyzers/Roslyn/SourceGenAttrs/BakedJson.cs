#pragma warning disable CS9113  // Parameter is unread.

using System;

namespace MMOR.Roslyn {
/**
 * <summary>
 *  Enables <see cref="BakedJsonFileAttribute"/> and
 *  <see cref="BakedJsonSerializableAttribute"/>.
 * </summary>
 */
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct,
    AllowMultiple = false)]
public sealed class BakedJsonAttribute() : Attribute;

/**
 * <summary>
 *  Generates a Serializable class of the interface to accept json.
 * </summary>
 */
[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
public sealed class BakedJsonSerializableAttribute(string name = "") : Attribute;

/**
 * <summary>
 *  Bakes the values of the json in <paramref name="path"/> into hardcoded C#.
 * </summary>
 */
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct,
    AllowMultiple = true)]
public sealed class BakedJsonFileAttribute(string path, string name = "") : Attribute;
}
