using System;

namespace Terraria.ModLoader;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ExtendsFromModAttribute : Attribute
{
	public readonly string[] Names;

	public ExtendsFromModAttribute(params string[] names)
	{
		Names = names ?? throw new ArgumentNullException(nameof(names));
	}
}
