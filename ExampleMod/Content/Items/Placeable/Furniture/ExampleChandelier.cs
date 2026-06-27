using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable.Furniture
{
	// One special thing about ExampleChandelier is it places a random style of the ExampleChandelier tile. This behavior is provided by the TileObjectData.RandomStyleRange of the tile, not the item.
	public class ExampleChandelier : ModItem
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.ExampleChandelier>());
			Item.value = Item.buyPrice(copper: 10);
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}