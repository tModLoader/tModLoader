using System.Linq;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using System.Collections.Generic;
using System;
using Terraria.DataStructures;

namespace Terraria.ModLoader.Default;

internal class PurgeUnloadedCommand : ModCommand
{
	public override string Command => "purgeunloaded";
	public override CommandType Type => CommandType.Chat; // complicated to implement correctly in MP
	public override string Description => Language.GetTextValue("tModLoader.CommandPurgeUnloadedDescription");
	public override string Usage => Language.GetTextValue("tModLoader.CommandPurgeUnloadedUsage");

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		if (Main.netMode != NetmodeID.SinglePlayer) {
			caller.Reply("This command can only be called in Single Player mode.");
			return;
		}

		bool purgeTiles = false;
		bool purgeWalls = false;
		bool actuallyPurge = false; // "dry run" otherwise

		string modName = null;
		for (int i = 0; i < args.Length; i++) {
			if (args[i] == "-h") {
				caller.Reply(Usage);
				return;
			}
			else if (args[i] == "-t") {
				purgeTiles = true;
			}
			else if (args[i] == "-w") {
				purgeWalls = true;
			}
			// TODO: We can purge other content in the future as well with other flags
			else if (args[i] == "-p") {
				actuallyPurge = true;
			}
			else {
				modName = args[i];
			}
		}

		if (!purgeTiles && !purgeWalls) {
			caller.Reply("At least one of '-w' or '-t' must be set");
			caller.Reply(Usage);
			return;
		}

		Dictionary<string, int> purgedTiles = [];
		Dictionary<string, int> purgedWalls = [];
		var unloadedTileTypes = TileID.Sets.Factory.CreateBoolSet(TileIO.Tiles.unloadedTypes.Select(x => (int)x).ToArray());
		var unloadedWallType = ModContent.WallType<UnloadedWall>();

		for (int i = 0; i < Main.maxTilesX; i++) {
			for (int j = 0; j < Main.maxTilesY; j++) {
				Tile t = Main.tile[i, j];
				if (purgeTiles && t.HasTile && unloadedTileTypes[t.TileType]) {
					ushort type = TileIO.Tiles.unloadedEntryLookup.Lookup(i, j);
					var info = TileIO.Tiles.entries[type];
					if (modName == null || info.modName.Equals(modName, StringComparison.OrdinalIgnoreCase)) {
						string key = $"{info.modName}/{info.name}";
						purgedTiles.TryGetValue(key, out var currentCount);
						purgedTiles[key] = currentCount + 1;
						if (actuallyPurge) {
							t.TileType = info.vanillaReplacementType;
						}
					}
				}
				if (purgeWalls && t.WallType == unloadedWallType) {
					ushort type = TileIO.Walls.unloadedEntryLookup.Lookup(i, j);
					var info = TileIO.Walls.entries[type];
					if (modName == null || info.modName.Equals(modName, StringComparison.OrdinalIgnoreCase)) {
						string key = $"{info.modName}/{info.name}";
						purgedWalls.TryGetValue(key, out var currentCount);
						purgedWalls[key] = currentCount + 1;
						if (actuallyPurge)
							t.WallType = info.vanillaReplacementType;
					}
				}
			}
		}

		if (actuallyPurge && (purgedTiles.Count != 0 || purgedWalls.Count != 0))
			Main.sectionManager.SetAllSectionsLoaded();

		if (modName != null)
			caller.Reply($"Only removing unloaded content belonging to the mod \"{modName}\".");
		else
			caller.Reply($"Removing unloaded content from any mod.");
		if (purgedTiles.Count != 0)
			caller.Reply($"Unloaded tiles: {string.Join(", ", purgedTiles.Select(x => $"{x.Key} ({x.Value})"))}");
		if (purgedWalls.Count != 0)
			caller.Reply($"Unloaded walls: {string.Join(", ", purgedWalls.Select(x => $"{x.Key} ({x.Value})"))}");
		if (!actuallyPurge)
			caller.Reply("The '-p' flag was not set, this is a dry run, no tiles or walls were actually removed.");
		if (purgedTiles.Count == 0 && purgedWalls.Count == 0)
			caller.Reply($"No unloaded content found to remove.");
		else if (actuallyPurge)
			caller.Reply("If this was unintentional please restore to a backup or force close the game right not to prevent the world changes from being saved.");
	}
}
