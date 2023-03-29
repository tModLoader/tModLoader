using System;
using System.Collections.Generic;
using System.Reflection;

namespace Terraria.ModLoader;

/// <summary>
/// Allows for types to be registered with legacy/alias names for lookup via <see cref="ModContent.Find{T}(string)"/> and similar methods.
/// <br/>When manually loading content, use <see cref="ModTypeLookup{T}.RegisterLegacyNames(T, string[])"/> instead.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class LegacyNameAttribute : Attribute
{
	public readonly string[] Names;

	public LegacyNameAttribute(params string[] names)
	{
		Names = names ?? throw new ArgumentNullException(nameof(names));
	}

	public static IEnumerable<string> GetLegacyNamesOfType(Type type)
	{
		foreach (var attribute in type.GetCustomAttributes<LegacyNameAttribute>(false)) {
			foreach (string legacyName in attribute.Names) {
				yield return legacyName;
			}
		}
	}
}
