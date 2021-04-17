using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Terraria.ModLoader.Default
{
	public class UnloadedNonSolidTile : UnloadedTile {
		public override string Texture => "ModLoader/UnloadedNonSolidTile";

		public override void SetDefaults() {
			TileIO.Tiles.unloadedTypes.Add(Type);
			//common
			Main.tileFrameImportant[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;

			Main.tileSolid[Type] = false;
			Main.tileNoAttach[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1); // Disables hammering
			TileObjectData.addTile(Type);
		}
	}
}
