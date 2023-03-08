using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable
{
	public class ExampleHerbSeeds : ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 25;
		}

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.ExampleHerb>());
			Item.width = 12;
			Item.height = 14;
			Item.value = 80;
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe(1)
				.AddIngredient(ModContent.ItemType<ExampleBlock>(), 1)
				.Register();
		}
	}
}
