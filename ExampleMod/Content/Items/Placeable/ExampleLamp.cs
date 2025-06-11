using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable
{
	internal class ExampleLamp : ModItem
	{
		// This example uses LocalizedText.Empty to prevent any translation key from being generated. This can be used for items that definitely won't have a tooltip, keeping the localization file cleaner.
		public override LocalizedText Tooltip => LocalizedText.Empty;

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
