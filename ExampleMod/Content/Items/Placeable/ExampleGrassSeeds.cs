using ExampleMod.Content.Tiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable
{
	// This is an example of an item that places a grass tile.
	// Grass seed items are unique in that they don't place a tile at an empty location, rather they place a tile over an existing tile to transform it into the new tile.
	// This is handled by TileID.Sets.Grass, TileID.Sets.GrassSpecial, or TileID.Sets.NeedsGrassFraming being set in the ModTile.
	public class ExampleGrassSeeds : ModItem
	{
		public override void SetStaticDefaults() {
			ItemID.Sets.GrassSeeds[Type] = true;
			ItemID.Sets.GrassSeedDirtTiles[Type] = [TileID.Dirt, ModContent.TileType<Tiles.ExampleBlock>()];

			ItemID.Sets.DisableAutomaticPlaceableDrop[Type] = true; // Seeds are not be recovered when grass tiles are destroyed.
		}

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<ExampleGrass_Dirt>());
			Item.value = Item.buyPrice(copper: 20);
		}

		// Note: The OverridePlacementTile hook exists on both ModItem and ModTile. ExampleGrass.OverridePlacementTile handles changing the grass tile placed by this item depending on the existing "dirt" tile. We could have implemented that here, but there are some cases where OverridePlacementTile is called without any Item context, so in this case it is better to implement that logic in the ModTile instead of the ModItem.
	}
}
