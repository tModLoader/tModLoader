using System;

namespace Terraria.ModLoader.Exceptions
{
	internal class AddRecipesException : LoadingException
	{
		public override string HelpLink => "https://github.com/blushiemagic/tModLoader/wiki/Basic-Recipes";
		
		public AddRecipesException(Mod mod, string message, Exception inner) : base(mod, message, inner)
		{ }
	}
}
