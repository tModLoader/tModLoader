using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable.Furniture
{
	public class ExampleChest : ModItem
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.ExampleChest>());
			// Item.placeStyle = 1; // Use this to place the chest in its locked style
			Item.width = 26;
			Item.height = 22;
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

	public class ExampleChestKey : ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 3; // Biome keys usually take 1 item to research instead.
		}

		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GoldenKey);
		}
	}
}
