using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Terraria.ModLoader.Default
{
	public class UnloadedSupremeFurniture : UnloadedTile
	{
		public override string Texture => "ModLoader/UnloadedSupremeFurniture";

		public override void SetStaticDefaults() {
			TileIO.Tiles.unloadedTypes.Add(Type);
			//common
			Main.tileFrameImportant[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;

			Main.tileSolidTop[Type] = true;
			Main.tileSolid[Type] = true;
			Main.tileTable[Type] = true;
			Main.tileNoAttach[Type] = true;

			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsChair);
			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);
			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1); // Disables hammering
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.None, 0, 0);
			TileObjectData.addTile(Type);
		}
	}
}
