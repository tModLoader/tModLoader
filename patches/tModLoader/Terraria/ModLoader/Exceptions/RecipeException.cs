using System;

namespace Terraria.ModLoader.Exceptions;

public class RecipeException : Exception
{
	public override string HelpLink => "https://github.com/tModLoader/tModLoader/wiki/Basic-Recipes";

	public RecipeException()
	{
	}

	public RecipeException(string message)
		: base(message)
	{
	}

	public RecipeException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
