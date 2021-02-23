using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	[Autoload(false)] // need multiple versions, all subclassed
	public class UnloadedTile : ModTile { 
		public override void MouseOver(int i, int j) {
			DisplayInfos(i, j);
		}

		public override void MouseOverFar(int i, int j) {
			MouseOver(i, j);

			DisplayInfos(i, j);
		}

		private void DisplayInfos(int i, int j) {
			var tile = Main.tile[i, j];

			if (tile == null || tile.type != Type) {
				return;
			}

			Player player = Main.LocalPlayer; 

			//NOTE: Onwards only works in singleplayer, as the lists aren't synced afaik.
			int infoID = PosIndexer.FloorGetKeyFromPos(TileIO.uTilePosMap, i, j);
			var infos = TileIO.uTileList;

			if (infoID < infos.Count) {
				var info = infos[infoID];

				if (info != null) {
					player.cursorItemIconEnabled = true;
					player.cursorItemIconID = -1;
					player.cursorItemIconText = $"{info.modName}: {info.name}";
				}
			}
		}
	}
}
