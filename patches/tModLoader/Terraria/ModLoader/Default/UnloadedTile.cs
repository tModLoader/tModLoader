using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default;

public abstract class UnloadedTile : ModTile
{
	public override void MouseOver(int i, int j)
	{
		if (Main.netMode != NetmodeID.SinglePlayer) {
			return;
		}

		var tile = Main.tile[i, j];

		if (tile == null || tile.type != Type) {
			return;
		}

		Player player = Main.LocalPlayer;

		//NOTE: Onwards only works in singleplayer
		ushort type = TileIO.Tiles.unloadedEntryLookup.Lookup(i, j);
		var info = TileIO.Tiles.entries[type];
		player.cursorItemIconEnabled = true;
		player.cursorItemIconID = -1;
		player.cursorItemIconText = $"{info.modName}: {info.name}";
	}
}
