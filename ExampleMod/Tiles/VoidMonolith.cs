using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExampleMod.Tiles
{
	public class VoidMonolith : ModTile
	{
		public override void SetDefaults() {
			Main.tileFrameImportant[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.Height = 3;
			TileObjectData.newTile.Origin = new Point16(1, 2);
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 18 };
			TileObjectData.addTile(Type);
			AddMapEntry(new Color(75, 139, 166));
			dustType = 1;
			animationFrameHeight = 56;
			disableSmartCursor = true;
			adjTiles = new int[] { TileID.LunarMonolith };
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			Item.NewItem(i * 16, j * 16, 32, 48, mod.ItemType("VoidMonolith"));
		}

		public override void NearbyEffects(int i, int j, bool closer) {
			if (Main.tile[i, j].frameY >= 56) {
				ExamplePlayer modPlayer = Main.LocalPlayer.GetModPlayer<ExamplePlayer>();
				modPlayer.voidMonolith = true;
			}
		}

		public override void AnimateTile(ref int frame, ref int frameCounter) {
			frame = Main.tileFrame[TileID.LunarMonolith];
			frameCounter = Main.tileFrameCounter[TileID.LunarMonolith];
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			Tile tile = Main.tile[i, j];
			Texture2D texture;
			if (Main.canDrawColorTile(i, j)) {
				texture = Main.tileAltTexture[Type, (int)tile.color()];
			}
			else {
				texture = Main.tileTexture[Type];
			}
			Vector2 zero = new Vector2(Main.offScreenRange, Main.offScreenRange);
			if (Main.drawToScreen) {
				zero = Vector2.Zero;
			}
			int height = tile.frameY % animationFrameHeight == 36 ? 18 : 16;
			int animate = 0;
			if (tile.frameY >= 56) {
				animate = Main.tileFrame[Type] * animationFrameHeight;
			}
			Main.spriteBatch.Draw(texture, new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero, new Rectangle(tile.frameX, tile.frameY + animate, 16, height), Lighting.GetColor(i, j), 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(mod.GetTexture("Tiles/VoidMonolith_Glow"), new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero, new Rectangle(tile.frameX, tile.frameY + animate, 16, height), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
			return false;
		}

		public override void RightClick(int i, int j) {
			Main.PlaySound(SoundID.Mech, i * 16, j * 16, 0);
			HitWire(i, j);
		}

		public override void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;
			player.noThrow = 2;
			player.showItemIcon = true;
			player.showItemIcon2 = mod.ItemType("VoidMonolith");
		}

		public override void HitWire(int i, int j) {
			int x = i - Main.tile[i, j].frameX / 18 % 2;
			int y = j - Main.tile[i, j].frameY / 18 % 3;
			for (int l = x; l < x + 2; l++) {
				for (int m = y; m < y + 3; m++) {
					if (Main.tile[l, m] == null) {
						Main.tile[l, m] = new Tile();
					}
					if (Main.tile[l, m].active() && Main.tile[l, m].type == Type) {
						if (Main.tile[l, m].frameY < 56) {
							Main.tile[l, m].frameY += 56;
						}
						else {
							Main.tile[l, m].frameY -= 56;
						}
					}
				}
			}
			if (Wiring.running) {
				Wiring.SkipWire(x, y);
				Wiring.SkipWire(x, y + 1);
				Wiring.SkipWire(x, y + 2);
				Wiring.SkipWire(x + 1, y);
				Wiring.SkipWire(x + 1, y + 1);
				Wiring.SkipWire(x + 1, y + 2);
			}
			NetMessage.SendTileSquare(-1, x, y + 1, 3);
		}
	}
}