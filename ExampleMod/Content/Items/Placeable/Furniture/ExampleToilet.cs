using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable.Furniture
{
	public class ExampleToilet : ModItem
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.ExampleToilet>());
			Item.width = 16;
			Item.height = 24;
			Item.value = 150;
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.Toilet)
				.AddIngredient<ExampleItem>(10)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
