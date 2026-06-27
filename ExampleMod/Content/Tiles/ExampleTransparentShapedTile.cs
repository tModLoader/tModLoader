using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Tiles;

// This tile has funny edges and internal holes. By giving the 'transparent' pixels in the mesh an alpha of 1/255, we can stop liquid from showing through
[LegacyName("ExampleCustomLiquidMaskedTile")]
public sealed class ExampleTransparentShapedTile : ModTile
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

		AddMapEntry(Color.Orange);
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
		// Let's make it glow since it is fiery looking and does fire damage
		r = 2f;
		g = 1.33f;
		b = 0.4f;
	}
}
public sealed class ExampleTransparentShapedTileItem : ModItem
{
	public override void SetDefaults() {
		Item.DefaultToPlaceableTile(ModContent.TileType<ExampleTransparentShapedTile>());
	}

	public override void AddRecipes() {
		CreateRecipe()
			.AddIngredient<Items.Placeable.ExampleBlock>()
			.Register();
	}
}

