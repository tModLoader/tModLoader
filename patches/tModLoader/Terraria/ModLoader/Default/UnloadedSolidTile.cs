using Terraria.ID;
using Terraria.ObjectData;

namespace Terraria.ModLoader.Default
{
	[Autoload(false)] // need multiple versions, all subclassed
	public class UnloadedSolidTile : UnloadedTile {
		public override string Texture => "ModLoader/UnloadedSolidTile";

		public override void SetDefaults() {
			Main.tileFrameImportant[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;

			Main.tileSolid[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1); // Disables hammering
			TileObjectData.addTile(Type);
		}
	}
}
