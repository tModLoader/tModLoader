using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable
{
	public class ExampleGemsparkBlock : ModItem
	{
		public override void SetDefaults() {
			// Since the ExampleGemsparkBlock class is used for both the "on" and "off" variants of the tile, we can't use ModContent.TileType<>() to get the tile type. We need to use the internal name of the "on" tile directly instead.
			Item.DefaultToPlaceableTile(Mod.Find<ModTile>("ExampleGemsparkBlockOn").Type);
		}

		public override void AddRecipes() {
			CreateRecipe(20)
				.AddIngredient<ExampleGem>()
				.AddIngredient(ItemID.Glass, 20)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}
