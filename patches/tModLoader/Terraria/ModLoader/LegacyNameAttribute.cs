using System;
using System.Collections.Generic;
using System.Reflection;

namespace Terraria.ModLoader
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public sealed class LegacyNameAttribute : Attribute
	{
		public readonly string[] Names;

		public LegacyNameAttribute(params string[] names) {
			Names = names ?? throw new ArgumentNullException(nameof(names));
		}

		public static IEnumerable<string> GetLegacyNamesOfType(Type type) {
			foreach (var attribute in type.GetCustomAttributes<LegacyNameAttribute>(false)) {
				foreach (string legacyName in attribute.Names) {
					yield return legacyName;
				}
			}
		}
	}
}
