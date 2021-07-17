using ExampleMod.Content.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExampleMod.Content.Tiles.Furniture
{
	public class ExampleBed : ModTile
	{
		public override void SetDefaults() {
			// Properties
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileID.Sets.HasOutlines[Type] = true;
			TileID.Sets.CanBeSleptIn[Type] = true;
			TileID.Sets.IsValidSpawnPoint[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;

			DustType = ModContent.DustType<Sparkle>();
			AdjTiles = new int[] { TileID.Beds };

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style4x2); //this style already takes care of direction for us
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
			TileObjectData.addTile(Type);

			// Etc
			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Example Bed");
			AddMapEntry(new Color(200, 200, 200), name);
		}

		public override bool HasSmartInteract() => true;

		public override void NumDust(int i, int j, bool fail, ref int num) => num = 1;

		public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(i * 16, j * 16, 64, 32, ModContent.ItemType<Items.Placeable.Furniture.ExampleBed>());

		public override bool RightClick(int i, int j) {
			Player player = Main.LocalPlayer;

			Tile tile = Main.tile[i, j];
			int spawnX = (i - (tile.frameX / 18)) + (tile.frameX >= 72 ? 5 : 2);
			int spawnY = j + 2;
			if (tile.frameY % 38 != 0) {
				spawnY--;
			}

			if (!Player.IsHoveringOverABottomSideOfABed(i, j)) {
				if (player.IsWithinSnappngRangeToTile(i, j, 96)) {
					player.GamepadEnableGrappleCooldown();
					player.sleeping.StartSleeping(player, i, j);
				}
			}
			else {
				player.FindSpawn();
				if (player.SpawnX == spawnX && player.SpawnY == spawnY) {
					player.RemoveSpawn();
					Main.NewText(Language.GetTextValue("Game.SpawnPointRemoved"), byte.MaxValue, 240, 20);
				}
				else if (Player.CheckSpawn(spawnX, spawnY)) {
					player.ChangeSpawn(spawnX, spawnY);
					Main.NewText(Language.GetTextValue("Game.SpawnPointSet"), byte.MaxValue, 240, 20);
				}
			}

			return true;
		}

		public override void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;

			if (!Player.IsHoveringOverABottomSideOfABed(i, j)) {
				if (player.IsWithinSnappngRangeToTile(i, j, 96)) {
					player.noThrow = 2;
					player.cursorItemIconEnabled = true;
					player.cursorItemIconID = 5013;
				}
			}
			else {
				player.noThrow = 2;
				player.cursorItemIconEnabled = true;
				player.cursorItemIconID = ModContent.ItemType<Items.Placeable.Furniture.ExampleBed>();
			}
		}
	}
}