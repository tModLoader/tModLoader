using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Tiles
{
	// This class shows off many things common to Lamp tiles in Terraria. The process for creating this example is detailed in: https://github.com/tModLoader/tModLoader/wiki/Advanced-Vanilla-Code-Adaption#examplelamp-tile
	// If you can't figure out how to recreate a vanilla tile, see that guide for instructions on how to figure it out yourself.
	internal class ExampleLamp : ModTile
	{
		public override void SetDefaults() {
			// Main.tileFlame[Type] = true; This breaks it.
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileWaterDeath[Type] = true;
			Main.tileLavaDeath[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1xX);
			TileObjectData.newTile.WaterDeath = true;
			TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
			TileObjectData.newTile.LavaPlacement = LiquidPlacement.NotAllowed;
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(253, 221, 3), Language.GetText("MapObject.FloorLamp"));
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			Item.NewItem(i * 16, j * 16, 16, 48, ItemType<Items.Placeable.ExampleLamp>());
		}

		public override void HitWire(int i, int j) {
			Tile tile = Main.tile[i, j];
			int topY = j - tile.frameY / 18 % 3;
			short frameAdjustment = (short)(tile.frameX > 0 ? -18 : 18);
			Main.tile[i, topY].frameX += frameAdjustment;
			Main.tile[i, topY + 1].frameX += frameAdjustment;
			Main.tile[i, topY + 2].frameX += frameAdjustment;
			Wiring.SkipWire(i, topY);
			Wiring.SkipWire(i, topY + 1);
			Wiring.SkipWire(i, topY + 2);
			NetMessage.SendTileSquare(-1, i, topY + 1, 3, TileChangeType.None);
		}

		public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
			if (i % 2 == 1) {
				spriteEffects = SpriteEffects.FlipHorizontally;
			}
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			Tile tile = Main.tile[i, j];
			if (tile.frameX == 0) {
				// We can support different light colors for different styles here: switch (tile.frameY / 54)
				r = 1f;
				g = 0.75f;
				b = 1f;
			}
		}

		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex) {
			if (!Main.gamePaused && Main.instance.IsActive && (!Lighting.UpdateEveryFrame || Main.rand.NextBool(4))) {
				Tile tile = Main.tile[i, j];
				short frameX = tile.frameX;
				short frameY = tile.frameY;
				if (Main.rand.NextBool(40) && frameX == 0) {
					int style = frameY / 54;
					if (frameY / 18 % 3 == 0) {
						int dustChoice = -1;
						if (style == 0) {
							dustChoice = 21; // A purple dust.
						}
						// We can support different dust for different styles here
						if (dustChoice != -1) {
							int dust = Dust.NewDust(new Vector2(i * 16 + 4, j * 16 + 2), 4, 4, dustChoice, 0f, 0f, 100, default(Color), 1f);
							if (Main.rand.Next(3) != 0) {
								Main.dust[dust].noGravity = true;
							}
							Main.dust[dust].velocity *= 0.3f;
							Main.dust[dust].velocity.Y = Main.dust[dust].velocity.Y - 1.5f;
						}
					}
				}
			}
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			SpriteEffects effects = SpriteEffects.None;
			if (i % 2 == 1) {
				effects = SpriteEffects.FlipHorizontally;
			}

			Vector2 zero = new Vector2(Main.offScreenRange, Main.offScreenRange);
			if (Main.drawToScreen) {
				zero = Vector2.Zero;
			}

			Tile tile = Main.tile[i, j];
			int width = 16;
			int offsetY = 0;
			int height = 16;
			TileLoader.SetDrawPositions(i, j, ref width, ref offsetY, ref height);
			var flameTexture = mod.GetTexture("Tiles/ExampleLamp_Flame"); // We could also reuse Main.FlameTexture[] textures, but using our own texture is nice.

			ulong num190 = Main.TileFrameSeed ^ (ulong)((long)j << 32 | (long)(uint)i);
			// We can support different flames for different styles here: int style = Main.tile[j, i].frameY / 54;
			for (int c = 0; c < 7; c++) {
				float shakeX = Utils.RandomInt(ref num190, -10, 11) * 0.15f;
				float shakeY = Utils.RandomInt(ref num190, -10, 1) * 0.35f;
				Main.spriteBatch.Draw(flameTexture, new Vector2(i * 16 - (int)Main.screenPosition.X - (width - 16f) / 2f + shakeX, j * 16 - (int)Main.screenPosition.Y + offsetY + shakeY) + zero, new Rectangle(tile.frameX, tile.frameY, width, height), new Color(100, 100, 100, 0), 0f, default(Vector2), 1f, effects, 0f);
			}
		}
	}
}
