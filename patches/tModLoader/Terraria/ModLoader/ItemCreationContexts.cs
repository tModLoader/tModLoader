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
		/// An item stack that the created item will be combined with (via OnStack). For normal crafting, this is Main.mouseItem
		/// </summary>
		public Item DestinationStack;

		/// <summary>
		/// Cloned list of Items consumed when crafting.
		/// </summary>
		public List<Item> ConsumedItems;
	}

	public class InitializationContext : ItemCreationContext
	{

	}
}