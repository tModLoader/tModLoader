using Terraria.ID;
using Terraria.ObjectData;

namespace Terraria.ModLoader.Default
{
	[Autoload(false)] // need two named versions
	public class UnloadedTile : ModTile
	{
		public override string Name{get;}
		internal bool IsSolid;
		internal bool IsSemi;

		public override string Texture => "ModLoader/UnloadedTile";

		public UnloadedTile(string name = null,bool isSolid = true, bool isSemi = false) {
			Name = name ?? base.Name;
			this.IsSolid = isSolid;
			this.IsSemi = isSemi;
		}

		public override void SetDefaults() {
			Main.tileSolid[Type] = IsSolid;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = (!IsSolid || IsSemi);
			Main.tileTable[Type] =IsSemi;
			Main.tileSolidTop[Type] = IsSemi;
			TileID.Sets.Platforms[Type] = true;
			adjTiles = new int[] { TileID.Platforms };

			// Placement
			TileObjectData.newTile.CoordinateHeights = new[] { 16 };
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.StyleMultiplier = 27;
			TileObjectData.newTile.StyleWrapLimit = 27;
			TileObjectData.newTile.UsesCustomCanPlace = false;
			TileObjectData.newTile.LavaDeath = true;
			TileObjectData.addTile(Type);
		}

		public override void MouseOver(int i, int j)
		{
			var tile = Main.tile[i, j];
			if(tile != null && tile.type == Type) {
				UnloadedTilesSystem modWorld = ModContent.GetInstance<UnloadedTilesSystem>();
				int posID = new UnloadedPosIndexing(i, j).PosID;
				modWorld.tileInfoMap.TryGetValue(posID, out int infoID);
				if (infoID >= 0) { // This only works in SP
					var info = modWorld.tileInfos[infoID];
					if (info != null) {
						Main.LocalPlayer.cursorItemIconEnabled = true;
						Main.LocalPlayer.cursorItemIconID = -1;
						Main.LocalPlayer.cursorItemIconText = $"{info.modName}: {info.name}";
					}
				}
			}
		}
	}
}
