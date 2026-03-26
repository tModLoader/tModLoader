using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable
{
	public class ExampleCutTile : ModItem
	{
		public override void SetStaticDefaults() {
			// Prevent the tile from dropping this item
			ItemID.Sets.DisableAutomaticPlaceableDrop[Type] = true;
		}

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.ExampleCutTile>());
			Item.value = Item.buyPrice(silver: 10);
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.StoneBlock, 10)
				.AddIngredient(ItemID.Rope, 10)
				.AddTile(TileID.HeavyWorkBench)
				.Register();
		}
	}
}
