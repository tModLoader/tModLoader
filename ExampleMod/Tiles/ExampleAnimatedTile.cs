using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExampleMod.Tiles
{
	class ExampleAnimatedTileTile : ModTile
	{
		public override void SetDefaults()
		{
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2Top);
			TileObjectData.addTile(Type);
			AddMapEntry(new Color(238, 145, 105), "Red Firefly in a Bottle");

			//Can't use this since texture is virtical.
			//animationFrameHeight = 56;
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = 0.93f;
			g = 0.11f;
			b = 0.12f;
		}

		int animationFrameWidth = 18;

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			// Flips the sprite
			SpriteEffects effects = SpriteEffects.None;
			if (i % 2 == 1)
			{
				effects = SpriteEffects.FlipHorizontally;
			}

			// Tweak the frame drawn by x position so tiles next to each other are off-sync and look much more interesting.
			int k = Main.tileFrame[Type] + i % 6;
			if (i % 2 == 0)
			{
				k += 3;
			}
			if (i % 3 == 0)
			{
				k += 3;
			}
			if (i % 4 == 0)
			{
				k += 3;
			}
			k = k % 6;

			Tile tile = Main.tile[i, j];
			Texture2D texture;
			if (Main.canDrawColorTile(i, j))
			{
				texture = Main.tileAltTexture[Type, (int)tile.color()];
			}
			else
			{
				texture = Main.tileTexture[Type];
			}
			Vector2 zero = new Vector2(Main.offScreenRange, Main.offScreenRange);
			if (Main.drawToScreen)
			{
				zero = Vector2.Zero;
			}
			int animate = k * animationFrameWidth;

			Main.spriteBatch.Draw(
				texture,
				new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero,
				new Rectangle(tile.frameX + animate, tile.frameY, 16, 16),
				Lighting.GetColor(i, j), 0f, default(Vector2), 1f, effects, 0f);

			return false; // return false to stop vanilla draw.
		}

		public override void AnimateTile(ref int frame, ref int frameCounter)
		{
			/*frameCounter++;
			if (frameCounter > 8)
			{
				frameCounter = 0;
				frame++;
				if (frame > 5)
				{
					frame = 0;
				}
			}*/
			// Above code works, but since we are just mimicing another tile, we can just use the same value.
			frame = Main.tileFrame[TileID.FireflyinaBottle];
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Item.NewItem(i * 16, j * 16, 16, 32, mod.ItemType("ExampleAnimatedTileItem"));
		}
	}

	class ExampleAnimatedTileItem : ModItem
	{
		public override void SetDefaults()
		{
			item.CloneDefaults(ItemID.FireflyinaBottle);
			item.name = "Red Firefly in a Bottle";
			item.createTile = mod.TileType("ExampleAnimatedTileTile");
		}
	}
}
