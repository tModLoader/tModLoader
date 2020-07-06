using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Tiles
{
	internal class ExampleAnimatedTileTile : ModTile
	{
		public override void SetDefaults() {
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2Top);
			TileObjectData.addTile(Type);
			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Red Firefly in a Bottle");
			AddMapEntry(new Color(238, 145, 105), name);

			//Can't use this since texture is vertical.
			//animationFrameHeight = 56;
		}

		// Our textures animation frames are arranged horizontally, which isn't typical, so here we specify animationFrameWidth which we use later in AnimateIndividualTile
		private readonly int animationFrameWidth = 18;

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = 0.93f;
			g = 0.11f;
			b = 0.12f;
		}

		public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
			// Flips the sprite if x coord is odd. Makes the tile more interesting.
			if (i % 2 == 1) {
				spriteEffects = SpriteEffects.FlipHorizontally;
			}
		}

		public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
			// Tweak the frame drawn by x position so tiles next to each other are off-sync and look much more interesting.
			int uniqueAnimationFrame = Main.tileFrame[Type] + i;
			if (i % 2 == 0) {
				uniqueAnimationFrame += 3;
			}
			if (i % 3 == 0) {
				uniqueAnimationFrame += 3;
			}
			if (i % 4 == 0) {
				uniqueAnimationFrame += 3;
			}
			uniqueAnimationFrame = uniqueAnimationFrame % 6;

			frameXOffset = uniqueAnimationFrame * animationFrameWidth;
		}

		// Below is an example completely manually drawing a tile. It shows some interesting concepts that may be useful for more advanced things.
		/*public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
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
		}*/

		public override void AnimateTile(ref int frame, ref int frameCounter) {
			/*
			// Spend 9 ticks on each of 6 frames, looping
			frameCounter++;
			if (frameCounter > 8)
			{
				frameCounter = 0;
				frame++;
				if (frame > 5)
				{
					frame = 0;
				}
			}
			// Or, more compactly:
			if (++frameCounter >= 9)
			{
				frameCounter = 0;
				frame = ++frame % 6;
			}
			*/
			// Above code works, but since we are just mimicking another tile, we can just use the same value.
			frame = Main.tileFrame[TileID.FireflyinaBottle];
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			Item.NewItem(i * 16, j * 16, 16, 32, ItemType<ExampleAnimatedTileItem>());
		}
	}

	internal class ExampleAnimatedTileItem : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Red Firefly in a Bottle");
		}

		public override void SetDefaults() {
			item.CloneDefaults(ItemID.FireflyinaBottle);
			item.createTile = TileType<ExampleAnimatedTileTile>();
		}
	}
}
