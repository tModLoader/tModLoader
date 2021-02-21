using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader.Default;

namespace Terraria.ModLoader.IO
{
	internal static partial class TileIO
	{
		public static void LoadMaster(TagCompound tag) {
			tileList.Clear(); //call at begin
			legacyLoad = false;
			saveKeyTiles.Clear();

			LoadUnloadedTiles(tag);
			LoadTiles(tag);

			if (legacyLoad) {
				LoadLegacy(tag);
			}
			else {

			}

			WorldIO.ValidateSigns(); //call this at end
		}

		internal static TagCompound SaveMaster() {
			return new TagCompound {
				["tileMap"] = SaveTiles(),
				["unloadedTileEntries"] = uTileList.Select(entry => entry?.Save() ?? new TagCompound()).ToList(),
			};
		}

		static bool legacyLoad = false;

		internal static bool canPurgeOldData => false; //for deleting unloaded mod data in a save; should point to UI flag; temp false

		static List<TileEntry> tileList = new List<TileEntry>();
		static List<TileEntry> uTileList = new List<TileEntry>();
		static List<TileEntry> rTileList = new List<TileEntry>();

		static List<PosIndexer.PosKey> tilePosMapList = new List<PosIndexer.PosKey>();
		static PosIndexer.PosKey[] tilePosMap;

		// Dictionairies are only used in save and/or load operations, mostly for legacy.
		internal static Dictionary<ushort, ushort> saveKeyTiles = new Dictionary<ushort, ushort>();
		internal static Dictionary<ushort, ushort> unloadedKeyTiles = new Dictionary<ushort, ushort>();
		internal static Dictionary<ushort, ushort> restoreKeyTiles = new Dictionary<ushort, ushort>();

		internal static ushort[] unloadedTileIDs => new ushort[5] {
			ModContent.Find<ModTile>("ModLoader/UnloadedTile").Type,
			ModContent.Find<ModTile>("ModLoader/UnloadedNonSolidTile").Type,
			ModContent.Find<ModTile>("ModLoader/UnloadedSemiSolidTile").Type,
			ModContent.Find<ModTile>("ModLoader/UnloadedChest").Type,
			ModContent.Find<ModTile>("ModLoader/UnloadedDresser").Type
		};

		internal static List<TagCompound> SaveTiles() {
			bool[] hasTile = new bool[TileLoader.TileCount];

			//TODO: Improve this for loop.
			for (int type = TileID.Count; type < hasTile.Length; type++) {
				// Skip Tile Types that aren't active in world
				if (!hasTile[type])
					continue;

				var modTile = TileLoader.GetTile(type);

				var entry = new TileEntry((ushort)type, modTile.Mod.Name, modTile.Name, modTile.vanillaFallbackOnModDeletion, Main.tileFrameImportant[type], GetUnloadedTileType(type));

				ushort key = (ushort) tileList.IndexOf(entry);
				if (key < 0) {
					key = (ushort)tileList.Count;
					tileList.Add(entry);
				}
				saveKeyTiles.Add((ushort)type, key);
			}

			// Return if null
			if (tileList.Count == 0)
				return null;

			return tileList.Select(entry => entry?.Save() ?? new TagCompound()).ToList();
		}

		internal static void LoadTiles(TagCompound tag) {
			// If there is no modded data saved, skip
			if (!tag.ContainsKey("tileMap"))
				return;

			// Retrieve Basic Tile Type Data from saved Tile Map, and store in table
			foreach (var tileTag in tag.GetList<TagCompound>("tileMap")) {
				var entry = new TileEntry(tileTag);
				ushort saveType = entry.id;

				ushort newID = ModContent.TryFind(entry.modName, entry.name, out ModTile tile) ? tile.Type : (ushort)0;

				if (newID == 0) {
					unloadedKeyTiles.Add(saveType, (ushort)uTileList.Count);
					uTileList.Add(entry);
				}
				else {
					entry.id = newID;
					saveKeyTiles.Add(saveType, (ushort)tileList.Count);
					tileList.Add(entry);
				}

				if (!legacyLoad && entry.unloadedType == null) {
					legacyLoad = true;
				}
			}
		}

		internal static void LoadUnloadedTiles(TagCompound tag) {
			// If there is no unloaded data saved, skip
			if (!tag.ContainsKey("unloadedTileEntries"))
				return;

			// infos and canRestore lists are same length so the indices match later for Restore()
			foreach (var tileTag in tag.GetList<TagCompound>("unloadedTileEntries")) {
				var entry = new TileEntry(tileTag);
				ushort restoreType = ModContent.TryFind(entry.modName, entry.name, out ModTile tile) ? tile.Type : (ushort)0;
				if (restoreType == 0 && canPurgeOldData)
					restoreType = entry.fallbackID;

				if (restoreType != 0) {
					rTileList.Add(entry);
					restoreKeyTiles.Add((ushort)uTileList.Count, (ushort)rTileList.Count);
				}

				uTileList.Add(entry);
			}
		}

		internal static string GetUnloadedTileType(int type) {
			if (TileID.Sets.BasicChest[type])
				return "ModLoader/UnloadedChest";

			if (TileID.Sets.BasicDresser[type])
				return "ModLoader/UnloadedDresser";

			if (Main.tileSolidTop[type])
				return "ModLoader/UnloadedSemiSolidTile";

			if (!Main.tileSolid[type])
				return "ModLoader/UnloadedNonSolidTile";

			return "ModLoader/UnloadedTile";
		}
	}
}
