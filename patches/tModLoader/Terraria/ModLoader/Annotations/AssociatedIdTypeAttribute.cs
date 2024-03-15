using System;

namespace Terraria.ModLoader.Annotations;

public sealed class AssociatedIdTypeAttribute : Attribute
{
	public Type Type { get; }

	public AssociatedIdTypeAttribute(Type type)
	{
		Type = type;
	}
}
