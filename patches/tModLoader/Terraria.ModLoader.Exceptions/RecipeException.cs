using System;

namespace Terraria.ModLoader.Exceptions
{
	public class RecipeException : Exception
	{
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
}
