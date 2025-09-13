using ExampleMod.Content.Items.Placeable;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.Liquid;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Tiles;

public sealed class ExampleCustomLiquidMaskedTile : ModTile
{
	public override void SetStaticDefaults() {
		Main.tileSolid[Type] = true;

		Main.tileLighted[Type] = true;
		Main.tileBlockLight[Type] = false;

		// The following lines make the tile dangerous to touch, like spikes
		TileID.Sets.TouchDamageImmediate[Type] = 30; 
		TileID.Sets.TouchDamageHot[Type] = true;
		TileID.Sets.CanBeSloped[Type] = false;

		// Show walls since the block is transparent
		TileID.Sets.DrawsWalls[Type] = true;


		// The following set is required for a tile to draw a liquid mask
		// While old rendering is enabled this will make water behind it disappear instead
		TileID.Sets.BlocksWaterDrawingBehindSelf[Type] = true;

		// This tile has funny edges and internal holes, so we can load a custom liquid mask to make it look better next to liquids.
		// The mask texture is fully white as a convention, and the parts we do not want liquid to show through are filled in.
		// Note: The tile will still render into the tileTarget, so it will already hide liquids where the normal texture is solid.
		LiquidEdgeRenderer.TileLiquidMasks[Type] = ModContent.Request<Texture2D>(Texture + "_Mask");
		AddMapEntry(Color.Orange);
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
		// Let's make it glow since it is fiery looking and does fire damage
		r = 2f;
		g = 1.33f;
		b = 0.4f;
	}
}
