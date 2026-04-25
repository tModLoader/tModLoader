using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable
{
	public class ExampleCampfire : ModItem
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.ExampleCampfire>(), 0);
		}

		public override void AddRecipes() {
			CreateRecipe()
#if COMPILE_ERROR_TODOS
				.AddRecipeGroup(RecipeGroupID.Wood, 10)
#endif
				.AddIngredient<ExampleTorch>(5)
				.Register();
		}
	}
}
