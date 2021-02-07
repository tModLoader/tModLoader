using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable
{
	public class ItemCollector : ModItem
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
			Item.value = Item.buyPrice(0, 20);
			Item.createTile = ModContent.TileType<Tiles.ItemCollector>();
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.Chest)
				.AddIngredient(ItemID.VoidVault)
				.AddTile(ItemID.MythrilAnvil)
				.Register();
		}
	}
}