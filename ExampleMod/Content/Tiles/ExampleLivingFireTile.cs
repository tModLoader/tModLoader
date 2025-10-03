using ExampleMod.Content.Dusts;
using ExampleMod.Content.Items.Placeable;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Tiles
{
	// This tile showcases the unique tile placement property of tiles like Living Fire
	// This tile also serves as an example of an animated terrain tile.
	public class ExampleLivingFireTile : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileLighted[Type] = true; // This tells the game that our tile produces light.

			// Normally, non-solid tiles cannot be placed on other non-solid tiles. This set allows that.
			// This set includes Cobwebs, Coin Piles, Living Fire Blocks, Smoke Blocks, and Bubble Blocks.
			TileID.Sets.CanPlaceNextToNonSolidTile[Type] = true;

			DustType = ModContent.DustType<Sparkle>(); // Set the dust type.

			// Here we set the map color to the same color as the light color.
			// We are accessing a variable that we defined inside of the item so we don't have to repeat entering the values.
			AddMapEntry(new Color(ExampleLivingFire.LightColor));

			// There are 4 frames of animation for our texture.
			// The texture 360 pixels tall / 4 frames of animation = 90.
			AnimationFrameHeight = 90;
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			// Here we set the strength of the light that the tile produces.
			// We are accessing a variable that we defined inside of the item so we don't have to repeat entering the values.
			r = ExampleLivingFire.LightColor.X;
			g = ExampleLivingFire.LightColor.Y;
			b = ExampleLivingFire.LightColor.Z;
		}
		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			// The Living Fire Blocks are drawn 2 pixels lower so that they sink into the tile below it.
			offsetY = 2;
		}

		public override void AnimateTile(ref int frame, ref int frameCounter) {
			// Here is where the tiles are animated.
			// Since we are just mimicking an existing tile, we can just use the same frame value.
			frame = Main.tileFrame[TileID.LivingFire];

			/* This is how it would be done manually, spending 5 ticks on each of 4 frames, looping.
			if (++frameCounter >= 5) {
				frameCounter = 0;
				frame = ++frame % 4;
			}
			*/
		}
	}
}