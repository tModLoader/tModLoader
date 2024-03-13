using System;

namespace Terraria.ModLoader.Annotations;

public sealed class AssociatedIdTypeAttribute(Type type) : Attribute
{
	public Type Type { get; } = type;
}
