using ExampleMod.Content.Tiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items
{
	public class ExampleSignItem : ModItem
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<ExampleSign>(), 0);
			Item.width = 26;
			Item.height = 22;
			Item.value = 50;
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.Sign)
				.AddIngredient<ExampleItem>(10)
				.Register();
		}
	}
}
