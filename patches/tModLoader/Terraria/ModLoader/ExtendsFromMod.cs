using System;

namespace Terraria.ModLoader
{
	[AttributeUsage(AttributeTargets.Class, Inherited = true)]
	public sealed class ExtendsFromMod : Attribute
	{
		public readonly string[] Names;

		public ExtendsFromMod(params string[] names) {
			Names = names ?? throw new ArgumentNullException(nameof(names));
		}
	}
}
