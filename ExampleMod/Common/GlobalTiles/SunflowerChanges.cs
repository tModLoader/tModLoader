using ExampleMod.Content.Tiles;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExampleMod.Common.GlobalTiles
{
	// This example uses a GlobalTile to affect the properties of an existing tile.
	// Tweaking existing tiles is doable to some degree.
	public class SunflowerChanges : GlobalTile
	{
		public override void SetStaticDefaults() {
			// This allows the Sunflower tile to be placed on ExampleBlock
			TileObjectData tileObjectData = TileObjectData.GetTileData(TileID.Sunflower, 0);
			tileObjectData.AnchorValidTiles = tileObjectData.AnchorValidTiles.Append(ModContent.TileType<ExampleBlock>()).ToArray();
		}

		public override void Unload() {
			// TileObjectData for existing tiles will not automatically be reset when a mod is unloaded. It is up to the modder to properly undo changes such as these.
			TileObjectData tileObjectData = TileObjectData.GetTileData(TileID.Sunflower, 0);
			tileObjectData.AnchorValidTiles = tileObjectData.AnchorValidTiles.Except([ModContent.TileType<ExampleBlock>()]).ToArray();
		}
	}
}