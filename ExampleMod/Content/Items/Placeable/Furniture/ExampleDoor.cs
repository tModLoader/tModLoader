using Terraria;
using Terraria.ModLoader;
using ExampleMod.Content.Tiles.Furniture;

namespace ExampleMod.Content.Items.Placeable.Furniture
{
	public class ExampleDoor : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is a modded door.");

			Item.SacrificeTotal = 1;
		}

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<ExampleDoorClosed>());
			Item.width = 14;
			Item.height = 28;
			Item.value = 150;
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