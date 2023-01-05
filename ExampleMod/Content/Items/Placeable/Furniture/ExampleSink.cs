using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable.Furniture
{
	public class ExampleSink : ModItem
	{
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Example Sink");
			/* Tooltip.SetDefault("Counts as a water, lava, and honey source\n"
							 + "'A shame there's no example liquids (yet)'"); */

			Item.ResearchUnlockCount = 1;
		}

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.ExampleSink>());
			Item.width = 24;
			Item.height = 30;
			Item.value = 3000;
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}
