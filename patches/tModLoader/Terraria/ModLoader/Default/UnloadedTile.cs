using Terraria.ID;
using Terraria.ObjectData;

namespace Terraria.ModLoader.Default
{
	[Autoload(false)] // need two named versions
	public class UnloadedTile : ModTile
	{
		public override string Name { get; }

		internal bool isSolid;
		internal bool isSemi;

		public override string Texture => "ModLoader/UnloadedTile";

		public UnloadedTile(string name = null, bool isSolid = true, bool isSemi = false) {
			Name = name ?? base.Name;

			this.isSolid = isSolid;
			this.isSemi = isSemi;
		}

		public override void SetDefaults() {
			Main.tileSolid[Type] = isSolid;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = (!isSolid || isSemi);
			Main.tileTable[Type] = isSemi;
			Main.tileSolidTop[Type] = isSemi;
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

		public override void MouseOver(int i, int j) {
			var tile = Main.tile[i, j];

			if (tile == null || tile.type != Type) {
				return;
			}

			var modSystem = ModContent.GetInstance<UnloadedTilesSystem>();
			var posIndex = new UnloadedPosIndexing(i, j);
			int infoID = posIndex.FloorGetValue(modSystem.tileInfoMap);
			var infos = modSystem.tileInfos;

			if (infoID < infos.Count) { // This only works in SP
				var info = infos[infoID];

				if (info != null) {
					Main.LocalPlayer.cursorItemIconEnabled = true;
					Main.LocalPlayer.cursorItemIconID = -1;
					Main.LocalPlayer.cursorItemIconText = $"{info.modName}: {info.name}";
				}
			}
		}
	}
}
