using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Terraria.ModLoader.Default
{
	public class UnloadedSolidTile : UnloadedTile {
		public override string Texture => "ModLoader/UnloadedSolidTile";

		public override void SetDefaults() {
			TileIO.IsUnloadedTile.Add(Type);

			Main.tileFrameImportant[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;

			Main.tileSolid[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1); // Disables hammering
			TileObjectData.addTile(Type);
		}
	}
}
