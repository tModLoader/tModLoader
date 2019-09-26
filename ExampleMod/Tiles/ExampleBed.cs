using ExampleMod.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Tiles
{
	public class ExampleBed : ModTile
	{
		public override void SetDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileID.Sets.HasOutlines[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style4x2); //this style already takes care of direction for us
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
			TileObjectData.addTile(Type);
			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Example Bed");
			AddMapEntry(new Color(200, 200, 200), name);
			dustType = DustType<Sparkle>();
			disableSmartCursor = true;
			adjTiles = new int[] { TileID.Beds };
			bed = true;
		}

		public override bool HasSmartInteract() {
			return true;
		}

		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = 1;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			Item.NewItem(i * 16, j * 16, 64, 32, ItemType<Items.Placeable.ExampleBed>());
		}

		public override bool NewRightClick(int i, int j) {
			Player player = Main.LocalPlayer;
			Tile tile = Main.tile[i, j];
			int spawnX = i - tile.frameX / 18;
			int spawnY = j + 2;
			spawnX += tile.frameX >= 72 ? 5 : 2;
			if (tile.frameY % 38 != 0) {
				spawnY--;
			}
			player.FindSpawn();
			if (player.SpawnX == spawnX && player.SpawnY == spawnY) {
				player.RemoveSpawn();
				Main.NewText("Spawn point removed!", 255, 240, 20, false);
			}
			else if (Player.CheckSpawn(spawnX, spawnY)) {
				player.ChangeSpawn(spawnX, spawnY);
				Main.NewText("Spawn point set!", 255, 240, 20, false);
			}
			return true;
		}

		public override void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;
			player.noThrow = 2;
			player.showItemIcon = true;
			player.showItemIcon2 = ItemType<Items.Placeable.ExampleBed>();
		}
	}
}