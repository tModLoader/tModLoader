using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Terraria.ModLoader.Default;

public class UnloadedSemiSolidTile : UnloadedTile
{
	public override string Texture => "ModLoader/UnloadedSemiSolidTile";

	public override void SetStaticDefaults()
	{
		TileIO.Tiles.unloadedTypes.Add(Type);
		//common
		Main.tileFrameImportant[Type] = true;
		TileID.Sets.DisableSmartCursor[Type] = true;

		Main.tileNoAttach[Type] = true;
		Main.tileTable[Type] = true;
		Main.tileSolidTop[Type] = true;

		TileID.Sets.Platforms[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1); // Disables hammering
		TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.None, 0, 0);
		TileObjectData.addTile(Type);

		AdjTiles = new int[] { TileID.Platforms };
	}
}
