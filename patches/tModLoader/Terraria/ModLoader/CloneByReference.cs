using System;

namespace Terraria.ModLoader;

/// <summary>
/// Indicates that references to this object can be shared between clones, and does not need to be deep copied.
/// When applied to a class, applies to all fields/properties of that type.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
public class CloneByReference : Attribute
{
}
