using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace ExampleMod.Content.Items
{
	public class ExampleSignItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<ExampleSign>(), 0);
			Item.width = 26;
			Item.height = 22;
			Item.value = 50;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddRecipeGroup(RecipeGroupID.Wood, 10)
				.AddIngredient(ItemID.DirtBlock, 1) 
				.Register();
		}
	}
}
