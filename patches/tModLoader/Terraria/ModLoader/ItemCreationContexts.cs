using System.Collections.Generic;

namespace Terraria.ModLoader
{
	public abstract class ItemCreationContext
	{
	}

	public class RecipeCreationContext : ItemCreationContext
	{
		public Recipe recipe;

		/// <summary>
		/// Cloned list of Items consumed when crafting.
		/// </summary>
		public List<Item> ConsumedItems;
	}

	public class InitializationContext : ItemCreationContext
	{

	}
}