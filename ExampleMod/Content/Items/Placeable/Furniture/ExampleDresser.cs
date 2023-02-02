using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable.Furniture
{
	public class ExampleDresser : ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.ExampleDresser>());

			Item.width = 26;
			Item.height = 22;
			Item.value = 500;
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.Dresser)
				.AddIngredient<ExampleBlock>(10)
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}