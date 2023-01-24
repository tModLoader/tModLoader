using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable
{
	public class ExampleSlopedTile : ModItem
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.ExampleSlopeTile>());
			Item.width = 12;
			Item.height = 12;
		}

		public override void AddRecipes() {
			CreateRecipe(1)
				.AddIngredient(ModContent.ItemType<ExampleBlock>(), 1)
				.Register();
		}
	}
}
