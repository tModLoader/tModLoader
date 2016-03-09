using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Items
{
	class ExampleGlobalRecipe : GlobalRecipe
	{
		int slimeKillIndex = Item.NPCtoBanner(NPCID.BlueSlime);

		public override bool RecipeAvailable(Recipe recipe)
		{
			if (recipe.createItem.createTile == TileID.Torches)
			{
				return NPC.killCount[slimeKillIndex] >= 10;
			}
			return true;
		}
	}
}
