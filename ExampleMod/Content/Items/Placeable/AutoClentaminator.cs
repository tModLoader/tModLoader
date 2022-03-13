using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable
{
	public class AutoClentaminator : ModItem
	{
		public override void SetDefaults() {
			Item.width = 22;
			Item.height = 32;
			Item.maxStack = 99;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.consumable = true;
			Item.rare = ItemRarityID.Red;
			Item.value = Item.buyPrice(0, 20, 0, 0);
			Item.createTile = ModContent.TileType<Tiles.AutoClentaminator>();
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.Clentaminator)
				.AddIngredient(ItemID.Wire, 100)
				.AddIngredient(ItemID.Timer5Second)
				.AddTile(ItemID.MythrilAnvil)
				.Register();
		}
	}
}