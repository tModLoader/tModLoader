using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Terraria.ModLoader.Default;

public class UnloadedDresser : UnloadedTile
{
	public override string Texture => "ModLoader/UnloadedDresser";

	public override void SetStaticDefaults()
	{
		TileIO.Tiles.unloadedTypes.Add(Type);

		//common
		Main.tileFrameImportant[Type] = true;
		TileID.Sets.DisableSmartCursor[Type] = true;

		Main.tileNoAttach[Type] = true;
		Main.tileTable[Type] = true;
		Main.tileSolidTop[Type] = true;
		Main.tileContainer[Type] = true;
		TileID.Sets.BasicDresser[Type] = true;
		AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);

		TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2); // Disables hammering
		TileObjectData.addTile(Type);

		AdjTiles = new int[] { TileID.Dressers };
	}
}
