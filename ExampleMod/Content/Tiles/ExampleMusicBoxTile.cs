using ExampleMod.Content.Items.Placeable;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExampleMod.Content.Tiles
{
	// See ExampleMod/Content/Items/Placeable/ExampleMusicBox.cs for more explanation on music.
	public class ExampleMusicBoxTile : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileObsidianKill[Type] = true;
			TileID.Sets.HasOutlines[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.Origin = new Point16(0, 1);
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.DrawYOffset = 2;
			TileObjectData.newTile.StyleLineSkip = 2;
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(191, 142, 111), Language.GetText("ItemName.MusicBox"));
		}

		public override void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;
			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
			player.cursorItemIconID = ModContent.ItemType<ExampleMusicBox>();
		}

		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) {
			return true;
		}

		public override void EmitParticles(int i, int j, Tile tileCache, short tileFrameX, short tileFrameY, Color tileLight, bool visible) {
			// This code spawns the music notes when the music box is open.
			Tile tile = Main.tile[i, j];

			if (!visible || tile.TileFrameX != 36 || tile.TileFrameY % 36 != 0 || (int)Main.timeForVisualEffects % 7 != 0 || !Main.rand.NextBool(3)) {
				return;
			}

			int MusicNote = Main.rand.Next(570, 573);
			Vector2 SpawnPosition = new Vector2(i * 16 + 8, j * 16 - 8);
			Vector2 NoteMovement = new Vector2(Main.WindForVisuals * 2f, -0.5f);
			NoteMovement.X *= Main.rand.NextFloat(0.5f, 1.5f);
			NoteMovement.Y *= Main.rand.NextFloat(0.5f, 1.5f);
			switch (MusicNote) {
				case 572:
					SpawnPosition.X -= 8f;
					break;
				case 571:
					SpawnPosition.X -= 4f;
					break;
			}

			Gore.NewGore(new EntitySource_TileUpdate(i, j), SpawnPosition, NoteMovement, MusicNote, 0.8f);
		}
	}
}
