using ExampleMod.Content.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExampleMod.Content.Tiles
{
	public class ExampleAnimatedGlowmaskTile : ModTile
	{
		private Asset<Texture2D> glowTexture;

		// If you want to know more about tiles, please follow this link
		// https://github.com/tModLoader/tModLoader/wiki/Basic-Tile
		public override void SetStaticDefaults() {
			// This changes a Framed tile to a FrameImportant tile
			// For modders, just remember to set this to true when you make a tile that uses a TileObjectData
			// Or basically all tiles that aren't like dirt, ores, or other basic building tiles
			Main.tileFrameImportant[Type] = true;
			// Use this to utilize an existing template
			// The names of styles are self explanatory usually (you can see all existing templates at the link mentioned earlier)
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			// Before adding the new tile you can make some changes to newTile like height, origin and etc.
			// Changing the Height because the template is for 1x2 not 1x3
			TileObjectData.newTile.Height = 3;
			// Modifies which part of the tile is centered on the mouse, in tile coordinates, from the top right corner
			TileObjectData.newTile.Origin = new Point16(1, 2);
			// Setting the height of the tiles individually for each
			TileObjectData.newTile.CoordinateHeights = [16, 16, 18];
			// Finally adding newTile
			TileObjectData.addTile(Type);

			// AddMapEntry is for setting the color and optional text associated with the Tile when viewed on the map
			AddMapEntry(new Color(75, 139, 166));

			// The height of a group of animation frames for this tile
			// Defaults to 0, which disables animations
			AnimationFrameHeight = 56;

			glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
		}

		public override void AnimateTile(ref int frame, ref int frameCounter) {
			// We can change frames manually, but since we are just simulating a different tile, we can just use the same value
			frame = Main.tileFrame[TileID.LunarMonolith];
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			Tile tile = Main.tile[i, j];

			// If you are using ModTile.SpecialDraw or PostDraw or PreDraw, use this snippet and add zero to all calls to spriteBatch.Draw
			// The reason for this is to accommodate the shift in drawing coordinates that occurs when using the different Lighting modes
			// While at 100% world zoom, press Shift+F9 to change lighting modes quickly to verify your code works for all lighting modes
			Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);

			// Because height of third tile is different we change it
			int height = tile.TileFrameY % AnimationFrameHeight == 36 ? 18 : 16;

			// Offset along the Y axis depending on the current frame
			int frameYOffset = (Main.tileFrame[Type] + 1) * AnimationFrameHeight;

			// Firstly we draw the original texture and then glow mask texture
			spriteBatch.Draw(
				TextureAssets.Tile[Type].Value,
				new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero,
				new Rectangle(tile.TileFrameX, tile.TileFrameY + frameYOffset, 16, height),
				Lighting.GetColor(i, j), 0f, default, 1f, SpriteEffects.None, 0f);
			// Make sure to draw with Color.White or at least a color that is fully opaque
			// Achieve opaqueness by increasing the alpha channel closer to 255. (lowering closer to 0 will achieve transparency)
			spriteBatch.Draw(
				glowTexture.Value,
				new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero,
				new Rectangle(tile.TileFrameX, tile.TileFrameY + frameYOffset, 16, height),
				Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

			// Return false to stop vanilla draw
			return false;
		}
	}

	internal class ExampleAnimatedGlowmaskTileItem : ModItem
	{
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.VoidMonolith);
			Item.createTile = ModContent.TileType<ExampleAnimatedGlowmaskTile>();
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}
