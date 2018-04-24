using System;

namespace Terraria.ModLoader.Exceptions
{
	public class AddRecipesException : Exception
	{
		public override string HelpLink => "https://github.com/blushiemagic/tModLoader/wiki/Basic-Recipes";

		public readonly string modName;

		public AddRecipesException(Mod mod, string message, Exception inner)
			: base(message, inner)
		{
			this.modName = mod.Name;
		}
	}
}
