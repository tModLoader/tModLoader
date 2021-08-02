using ExampleMod.Content.Tiles;
using Terraria.ModLoader;
using Terraria.ID;

namespace ExampleMod.Content.Items.Placeable
{
	// See ExampleMod/Common/Systems/MusicLoadingSystem for an explanation on music.
	public class ExampleMusicBox : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Music Box (Marble Gallery)");
		}

		public override void SetDefaults() {
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTurn = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.autoReuse = true;
			Item.consumable = true;
			Item.createTile = ModContent.TileType<ExampleMusicBoxTile>();
			Item.width = 24;
			Item.height = 24;
			Item.rare = ItemRarityID.LightRed;
			Item.value = 100000;
			Item.accessory = true;
		}
	}
}
