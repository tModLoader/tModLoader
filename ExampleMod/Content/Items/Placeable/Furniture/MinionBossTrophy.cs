using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable.Furniture
{
	public class MinionBossTrophy : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Minion Boss Trophy");
		}

		public override void SetDefaults() {
			Item.DefaultToPlacableTile(ModContent.TileType<Tiles.Furniture.MinionBossTrophy>()); //Vanilla has many useful methods like these, use them!
			Item.width = 32;
			Item.height = 32;
			Item.maxStack = 99;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.buyPrice(0, 1);
		}
	}
}
