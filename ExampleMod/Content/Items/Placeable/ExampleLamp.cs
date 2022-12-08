using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable
{
	internal class ExampleLamp : ModItem
	{
		public override void SetStaticDefaults() {
			Item.SacrificeTotal = 1;
		}

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.ExampleLamp>());
			Item.width = 10;
			Item.height = 24;
			Item.value = 500;
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}
