using System;

namespace Terraria.ModLoader;

/// <summary>
/// Excludes a class from JIT and also from autoloading. Annotate classes that inherit from classes from <see href="https://github.com/tModLoader/tModLoader/wiki/Expert-Cross-Mod-Content#weak-references-aka-weakreferences-expert">weakly referenced mods</see> with this attribute to prevent the game from attempting to autoload the class, which would cause load errors otherwise. See <see href="https://github.com/tModLoader/tModLoader/wiki/JIT-Exception#weak-references">this wiki page</see> for more information.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ExtendsFromModAttribute : Attribute
{
	public readonly string[] Names;

	public ExtendsFromModAttribute(params string[] names)
	{
		Names = names ?? throw new ArgumentNullException(nameof(names));
	}
}
