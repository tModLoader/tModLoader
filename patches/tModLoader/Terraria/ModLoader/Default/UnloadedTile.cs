using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	public abstract class UnloadedTile : ModTile { 
		public override void MouseOver(int i, int j) {
			var tile = Main.tile[i, j];

			if (tile == null || tile.type != Type) {
				return;
			}

			Player player = Main.LocalPlayer;

			//NOTE: Onwards only works in singleplayer, as the lists aren't synced afaik.
			if (TileIO.unloadedTileLookup.Lookup(i, j, out var type)) {
				var info = TileIO.tileEntries[type];
				player.cursorItemIconEnabled = true;
				player.cursorItemIconID = -1;
				player.cursorItemIconText = $"{info.modName}: {info.name}";
			}
		}

		public override void MouseOverFar(int i, int j) => MouseOver(i, j);
	}
}
