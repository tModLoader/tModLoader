using ExampleMod.Content.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExampleMod.Content.Tiles
{
	internal class ExampleAnimatedTile : ModTile
	{
		private Asset<Texture2D> flameTexture;
		public override void Load() {
			flameTexture = ModContent.Request<Texture2D>(Texture + "_Flame");
		}

		// If you want to know more about tiles, please follow this link
		// https://github.com/tModLoader/tModLoader/wiki/Basic-Tile
		public override void SetStaticDefaults() {
			// If a tile is a light source
			Main.tileLighted[Type] = true;
			// This changes a Framed tile to a FrameImportant tile
			// For modders, just remember to set this to true when you make a tile that uses a TileObjectData
			// Or basically all tiles that aren't like dirt, ores, or other basic building tiles
			Main.tileFrameImportant[Type] = true;
			// Set to True if you'd like your tile to die if hit by lava
			Main.tileLavaDeath[Type] = true;
			// This allows the tile to sway in the wind and from player interaction when used together with the code in PreDraw below.
			TileID.Sets.MultiTileSway[Type] = true;
			// Use this to utilize an existing template
			// The names of styles are self explanatory usually (you can see all existing templates at the link mentioned earlier)
			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2Top);
			// This last call adds a new tile
			// Before that, you can make some changes to newTile like height, origin and etc.
			TileObjectData.addTile(Type);

			// AddMapEntry is for setting the color and optional text associated with the Tile when viewed on the map
			LocalizedText name = CreateMapEntryName();
			AddMapEntry(new Color(238, 145, 105), name);

			// Can't use this since texture is vertical
			// AnimationFrameHeight = 56;
		}

		// Our textures animation frames are arranged horizontally, which isn't typical, so here we specify animationFrameWidth which we use later in AnimateIndividualTile
		private readonly int animationFrameWidth = 18;

		// This method allows you to determine how much light this block emits
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = 0.93f;
			g = 0.11f;
			b = 0.12f;
		}

		// This method allows you to determine whether or not the tile will draw itself flipped in the world
		public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
			// Flips the sprite if x coord is odd. Makes the tile more interesting
			if (i % 2 == 1)
				spriteEffects = SpriteEffects.FlipHorizontally;
		}

		public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
			// Tweak the frame drawn by x position so tiles next to each other are off-sync and look much more interesting
			int uniqueAnimationFrame = Main.tileFrame[Type] + i;
			if (i % 2 == 0)
				uniqueAnimationFrame += 3;
			if (i % 3 == 0)
				uniqueAnimationFrame += 3;
			if (i % 4 == 0)
				uniqueAnimationFrame += 3;
			uniqueAnimationFrame %= 6;

			// frameYOffset = modTile.AnimationFrameHeight * Main.tileFrame[type] will already be set before this hook is called
			// But we have a horizontal animated texture, so we use frameXOffset instead of frameYOffset
			frameXOffset = uniqueAnimationFrame * animationFrameWidth;
		}

		// This method allows you to change the sound a tile makes when hit
		public override bool KillSound(int i, int j, bool fail) {
			// Play the glass shattering sound instead of the normal digging sound if the tile is destroyed on this hit
			if (!fail) {
				SoundEngine.PlaySound(SoundID.Shatter, new Vector2(i, j).ToWorldCoordinates());
				return false;
			}
			return base.KillSound(i, j, fail);
		}

		// TODO: It's better to have an actual class for this example, instead of comments

		// Below is an example completely manually drawing a tile. It shows some interesting concepts that may be useful for more advanced things
		/*public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			// Instead of SetSpriteEffects
			// Flips the sprite if x coord is odd. Makes the tile more interesting
			SpriteEffects effects = SpriteEffects.None;
			if (i % 2 == 1)
				effects = SpriteEffects.FlipHorizontally;

			// Instead of AnimateIndividualTile
			// Tweak the frame drawn by x position so tiles next to each other are off-sync and look much more interesting
			int uniqueAnimationFrame = Main.tileFrame[Type] + i % 6;
			if (i % 2 == 0)
				uniqueAnimationFrame += 3;
			if (i % 3 == 0)
				uniqueAnimationFrame += 3;
			if (i % 4 == 0)
				uniqueAnimationFrame += 3;
			uniqueAnimationFrame %= 6;

			int frameXOffset = uniqueAnimationFrame * animationFrameWidth;


			Tile tile = Main.tile[i, j];
			Texture2D texture = TextureAssets.Tile[Type].Value;

			// If you are using ModTile.SpecialDraw or PostDraw or PreDraw, use this snippet and add zero to all calls to spriteBatch.Draw
			// The reason for this is to accommodate the shift in drawing coordinates that occurs when using the different Lighting mode
			// While at 100% world zoom, press Shift+F9 to change lighting modes quickly to verify your code works for all lighting modes
			Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);

			Main.spriteBatch.Draw(
				texture,
				new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero,
				new Rectangle(tile.frameX + frameXOffset, tile.frameY, 16, 16),
				Lighting.GetColor(i, j), 0f, default, 1f, effects, 0f);

			return false; // return false to stop vanilla draw
		}*/

		public override void AnimateTile(ref int frame, ref int frameCounter) {
			/*
			// Spend 9 ticks on each of 6 frames, looping
			frameCounter++;
			if (frameCounter >= 9) {
				frameCounter = 0;
				if (++frame >= 6) {
					frame = 0;
				}
			}

			// Or, more compactly:
			if (++frameCounter >= 9) {
				frameCounter = 0;
				frame = ++frame % 6;
			}*/

			// Above code works, but since we are just mimicking another tile, we can just use the same value
			frame = Main.tileFrame[TileID.FireflyinaBottle];
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			Tile tile = Main.tile[i, j];

			if (TileObjectData.IsTopLeft(tile)) {
				// Makes this tile sway in the wind and with player interaction when used with TileID.Sets.MultiTileSway
				Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.MultiTileVine);
			}

			// We must return false here to prevent the normal tile drawing code from drawing the default static tile. Without this a duplicate tile will be drawn.
			return false;
		}

		public override void AdjustMultiTileVineParameters(int i, int j, ref float? overrideWindCycle, ref float windPushPowerX, ref float windPushPowerY, ref bool dontRotateTopTiles, ref float totalWindMultiplier, ref Texture2D glowTexture, ref Color glowColor) {
			overrideWindCycle = 1f;
			windPushPowerY = 0f;
		}

		public override void GetTileFlameData(int i, int j, ref TileDrawing.TileFlameData tileFlameData) {
			tileFlameData.flameTexture = flameTexture.Value;
			tileFlameData.flameColor = new Color(200, 200, 200, 0);
			tileFlameData.flameCount = 1;
		}
	}

	internal class ExampleAnimatedTileItem : ModItem
	{
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.FireflyinaBottle);
			Item.createTile = ModContent.TileType<ExampleAnimatedTile>();
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}
