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
		/// The original item before being stacked with the created item.  Only useful for crafting a stackable item with the same item in the mouseItem.
		/// </summary>
		public Item original;

		/// <summary>
		/// Cloned list of Items consumed when crafting.
		/// </summary>
		public List<Item> ConsumedItems;
	}

	public class InitializationContext : ItemCreationContext
	{

	}
}