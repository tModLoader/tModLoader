using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent.Tile_Entities;
using Terraria.ID;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.Exceptions;

namespace Terraria.ModLoader.IO
{
	internal static class TileIO
	{
		//*********** Tile, Walls, & Chests Save, Load, and Placeholder Implementations ***************************//
		internal static class TileIOFlags
		{
			internal const byte None = 0;
			internal const byte ModTile = 1;
			internal const byte FrameXInt16 = 2;
			internal const byte FrameYInt16 = 4;
			internal const byte TileColor = 8;
			internal const byte ModWall = 16;
			internal const byte WallColor = 32;
			internal const byte NextTilesAreSame = 64;
			internal const byte NextModTile = 128;
		}

		internal struct tableData
		{
			internal ushort id;
			internal string modName;
			internal string name;
			internal ushort fallbackID;

			internal static tableData Create() {
				return new tableData {
					id = 0,
					modName = null,
					name = null,
					fallbackID = 0
				};
			}
		}

		//in Terraria.IO.WorldFile.SaveWorldTiles add type check to tile.active() check and wall check
		internal struct TileTables
		{
			internal Dictionary<int, bool> frameImportant;
			internal Dictionary<int, tableData> tileData;
			internal Dictionary<int, tableData> wallData;
			
			internal static TileTables Create() {
				return new TileTables {
					frameImportant = new Dictionary<int, bool>(),
					tileData = new Dictionary<int, tableData>(),
					wallData = new Dictionary<int, tableData>()
				};
			}
		}

		internal struct posMap
		{
			public int posID;
			public ushort infoID;
		}

		/// These values are synced to match UpdateUnloadedInfos <see cref="UpdateUnloaded"/> 
		internal static byte TilesContext = 0;
		internal static byte WallsContext = 1;

		/// <summary>
		/// Tile-<see cref="UnloadedTileInfo"/>s that are not able to be restored in the current state of the world (and saved for the next world load)
		/// </summary>
		internal static List<UnloadedInfo> tileInfos = new List<UnloadedInfo>();

		/// <summary>
		/// Use a dictionary mapping coordinates of tile infos from <see cref="tileInfos"/>
		/// </summary>
		internal static List<posMap> tileInfoMap = new List<posMap>();
		internal static List<posMap> prevTileInfoMap = new List<posMap>();

		/// <summary>
		/// Wall-<see cref="UnloadedWallInfo"/>s that are not able to be restored in the current state of the world (and saved for the next world load)
		/// </summary>
		internal static List<UnloadedInfo> wallInfos = new List<UnloadedInfo>();

		/// <summary>
		/// Use a dictionary mapping coordinates of walls infos from <see cref="wallInfos"/>
		/// </summary>
		internal static List<posMap> wallInfoMap = new List<posMap>();
		internal static List<posMap> prevWallInfoMap = new List<posMap>();

		internal static int[] unloadedTileIDs = new int[5];

		internal static TagCompound SaveTiles() {
			bool[] hasTile = new bool[TileLoader.TileCount];
			bool[] hasWall = new bool[WallLoader.WallCount];

			using var ms = new MemoryStream();
			using var writer = new BinaryWriter(ms);

			// Store Locational-Specific Data
			WriteTileData(writer, hasTile, hasWall);

			// Store Basic Tile Type Data
			var tileList = new List<TagCompound>();

			for (int type = TileID.Count; type < hasTile.Length; type++) {
				// Skip Tile Types that aren't active in world
				if (!hasTile[type])
					continue;
						
				var modTile = TileLoader.GetTile(type);

				tileList.Add(new TagCompound {
					["value"] = (short)type,
					["mod"] = modTile.Mod.Name,
					["name"] = modTile.Name,
					["unloadedType"] = GetUnloadedTileType(type),
					["framed"] = Main.tileFrameImportant[type], 
					["fallbackType"] = modTile.vanillaFallbackOnModDeletion, 
				});
			}

			// Store Basic Wall Type Data
			var wallList = new List<TagCompound>();

			for (int wall = WallID.Count; wall < hasWall.Length; wall++) {
				// Skip Wall Types that aren't active in world
				if (!hasWall[wall])
					continue;

				var modWall = WallLoader.GetWall(wall);

				wallList.Add(new TagCompound {
					["value"] = (short)wall,
					["mod"] = modWall.Mod.Name,
					["name"] = modWall.Name,
					["fallbackType"] = modWall.vanillaFallbackOnModDeletion, 
				});
			}

			// Return compressed variant of all data
			if (tileList.Count == 0 && wallList.Count == 0)
				return null;

			return new TagCompound {
				["tileMap"] = tileList, // Lists of all active tile types and restoration mapping
				["tileInfoMap"] = tileInfoMap.Select(pair => new TagCompound {
					["posID"] = pair.posID,
					["infoID"] = pair.infoID
				}).ToList(),

				["wallMap"] = wallList, // Lists of all active wall types and restoration mapping
				["wallInfoMap"] = wallInfoMap.Select(pair => new TagCompound {
					["posID"] = pair.posID,
					["infoID"] = pair.infoID
				}).ToList(),

				["data"] = ms.ToArray(), // Array of locational-specific data
			};
		}

		internal static void LoadTiles(TagCompound tag) {
			// If there is no modded data saved, skip
			if (!tag.ContainsKey("data"))
				return;

			// Create a Table to store working information
			var tables = TileTables.Create();

			unloadedTileIDs = new int[5] {
					ModContent.Find<ModTile>("ModLoader/UnloadedNonSolidTile").Type,
					ModContent.Find<ModTile>("ModLoader/UnloadedSemiSolidTile").Type,
					ModContent.Find<ModTile>("ModLoader/UnloadedChest").Type,
					ModContent.Find<ModTile>("ModLoader/UnloadedDresser").Type,
					ModContent.Find<ModTile>("ModLoader/UnloadedTile").Type
			};

			//TODO: Refactor the insides of two ForEach into one method call

			// Retrieve Basic Tile Type Data from saved Tile Map, and store in table
			foreach (var tileTag in tag.GetList<TagCompound>("tileMap")) {
				tableData tabData = tableData.Create();

				int saveType = tileTag.GetShort("value") - TileID.Count;
				string modName = tileTag.GetString("mod");
				string name = tileTag.GetString("name");
				
				tabData.id = ModContent.TryFind(modName, name, out ModTile tile) ? tile.Type : (ushort)0;

				// Handle unloaded tiles at the tile level by storing restoration data
				if (tabData.id == 0) { 
					tabData.modName = modName;
					tabData.name = name;

					string unloadedType = "ModLoader/UnloadedTile"; 
					ushort fallbackType = 0;

					if (tileTag.ContainsKey("unloadedType")) { // If statement is for legacy support
						unloadedType = tileTag.Get<string>("unloadedType");
						
						fallbackType = tileTag.Get<ushort>("fallbackType");
					}

					UpdateUnloaded pendingInfo = new UpdateUnloaded(tileInfos, TilesContext);
					pendingInfo.AddInfos(new UnloadedInfo(modName, name, fallbackType));

					tabData.fallbackID = fallbackType;
					tabData.id = ModContent.Find<ModTile>(unloadedType).Type;
				}

				tables.tileData[saveType] = tabData;
				tables.frameImportant[saveType] = tileTag.GetBool("framed");
			}

			// Retrieve Basic Wall Type Data from saved Wall Map, and store in table
			foreach (var wallTag in tag.GetList<TagCompound>("wallMap")) {
				tableData tabData = tableData.Create();

				int saveType = wallTag.GetShort("value") - WallID.Count;
				string modName = wallTag.GetString("mod");
				string name = wallTag.GetString("name");
								
				tabData.id = ModContent.TryFind(modName, name, out ModWall wall) ? wall.Type : (ushort)0;
				
				if (tabData.id == 0) {
					tabData.id = ModContent.Find<ModWall>("ModLoader/UnloadedWall").Type;
					tabData.modName = modName;
					tabData.name = name;
					
					ushort fallbackType = 0;
					if (wallTag.ContainsKey("fallbackType"))
						fallbackType = wallTag.Get<ushort>("fallbackType");

					tabData.fallbackID = fallbackType;

					UpdateUnloaded pendingInfo = new UpdateUnloaded(wallInfos, WallsContext);
					pendingInfo.AddInfos(new UnloadedInfo(modName, name, fallbackType));
				}

				tables.wallData[saveType] = tabData;
			}

			if (tag.ContainsKey("tileInfoMap")) {
				UpdateMaps(tag.GetList<TagCompound>("tileInfoMap"), prevTileInfoMap);
				UpdateMaps(tag.GetList<TagCompound>("wallInfoMap"), prevWallInfoMap);
			}

			// Retrieve Locational-Specific Data from 'Data' and apply
			using (var memoryStream = new MemoryStream(tag.GetByteArray("data")))
			using (var reader = new BinaryReader(memoryStream))
				ReadTileData(reader, tables);

			// Validate Load
			WorldIO.ValidateSigns();
		}

		internal static void WriteTileData(BinaryWriter writer, bool[] hasTile, bool[] hasWall) {
			byte skip = 0; //Track amount of tiles that don't contain mod data
			bool nextModTile = false;
			int i = 0;
			int j = 0;

			// Index a shortlist all tile locations with mod data on either of wall or tiles
			do {
				Tile tile = Main.tile[i, j];

				if (HasModData(tile)) {
					if (!nextModTile) {
						writer.Write(skip);
						skip = 0;
					}
					else {
						nextModTile = false;
					}

					WriteModTile(ref i, ref j, writer, ref nextModTile, hasTile, hasWall);
				}
				// Skip vanilla tiles, and record number of skips
				else {
					//Skip over vanilla tiles
					if (++skip == 255) {
						//Write additional skip
						writer.Write(skip);
						skip = 0;
					}
				}
			}
			while (NextTile(ref i, ref j));

			if (skip > 0) {
				writer.Write(skip);
			}
		}

		internal static void ReadTileData(BinaryReader reader, TileTables tables) {
			int i = 0;
			int j = 0;
			byte skip;
			bool nextModTile = false;

			// Access indexed shortlist of all tile locations with mod data on either of wall or tiles
			do {
				// Skip vanilla tiles
				if (!nextModTile) {
					skip = reader.ReadByte();
					
					while (skip == 255) {
						for (byte k = 0; k < 255; k++) {
							if (!NextTile(ref i, ref j)) {
								return; //Skip over vanilla tiles
							}
						}

						skip = reader.ReadByte();
					}

					for (byte k = 0; k < skip; k++) {
						if (!NextTile(ref i, ref j)) {
							return;
						}
					}
				}
				else {
					nextModTile = false;
				}

				// Load modded tiles
				ReadModTile(ref i, ref j, tables, reader, ref nextModTile);
			}
			while (NextTile(ref i, ref j));
		}

		internal static void WriteModTile(ref int i, ref int j, BinaryWriter writer, ref bool nextModTile, bool[] hasTile, bool[] hasWall) {
			Tile tile = Main.tile[i, j];
			byte flags = TileIOFlags.None;
			byte[] data = new byte[11]; //data[0] will be filled with the flags, hence why index starts with 1 below
			int index = 1;

			// Write Tiles
			if (tile.active() && tile.type >= TileID.Count) {
				// Atleast one Tile is placed of that Type, so record Type as 16 bit uint
				hasTile[tile.type] = true;
				flags |= TileIOFlags.ModTile;

				//Converts an Int16 into two bytes, reversed by reader.ReadInt16
				data[index++] = (byte)tile.type;
				data[index++] = (byte)(tile.type >> 8);

				// Checks if Tile Framing is important, if so records x and y data as 16 bit uint
				if (Main.tileFrameImportant[tile.type]) {
					data[index++] = (byte)tile.frameX;

					if (tile.frameX >= 256) {
						flags |= TileIOFlags.FrameXInt16;
						data[index++] = (byte)(tile.frameX >> 8);
					}

					data[index++] = (byte)tile.frameY;

					if (tile.frameY >= 256) {
						flags |= TileIOFlags.FrameYInt16;
						data[index++] = (byte)(tile.frameY >> 8);
					}
				}

				// Checks if Tile has been Coloured 
				if (tile.color() != 0) {
					data[index++] = tile.color();
					flags |= TileIOFlags.TileColor;
				}
			}

			// Write Walls
			if (tile.wall >= WallID.Count) {
				// Atleast one Wall is placed of that ID, so record ID as 16 bit uint
				hasWall[tile.wall] = true;
				flags |= TileIOFlags.ModWall;

				//Converts a UInt16 into two bytes, reversed by reader.ReadUInt16
				data[index++] = (byte)tile.wall;
				data[index++] = (byte)(tile.wall >> 8);

				// Checks if Wall has been Coloured 
				if (tile.wallColor() != 0) {
					flags |= TileIOFlags.WallColor;
					data[index++] = tile.wallColor();
				}
			}

			/* Check for re-occurence of exact same tile (includes walls, liquids, wires, etc), and compress data by 
			// skipping to next occurence of a new modded tile. Maximum of 256 grouped tiles. */
			int nextI = i;
			int nextJ = j;
			byte sameCount = 0;

			while (NextTile(ref nextI, ref nextJ)) {
				Tile nextTile = Main.tile[nextI, nextJ];

				if (tile.isTheSameAs(nextTile) && sameCount < 255) {
					//Optimization by not writing potentially duplicate data: simply write the amount of tiles that are identical
					sameCount++;
					i = nextI;
					j = nextJ;
				}
				else if (HasModData(nextTile)) {
					flags |= TileIOFlags.NextModTile;
					nextModTile = true;
					break;
				}
				else {
					break;
				}
			}

			if (sameCount > 0) {
				flags |= TileIOFlags.NextTilesAreSame;
				data[index++] = sameCount;
			}

			// Output result Data array to stream
			data[0] = flags;
			writer.Write(data, 0, index);
		}

		internal static void ReadModTile(ref int i, ref int j, TileTables tables, BinaryReader reader, ref bool nextModTile) {
			// Access Stored 8bit Flags
			byte flags;
			flags = reader.ReadByte();

			UnloadedInfo info;
			var posIndexer = new UnloadedPosIndexing(i, j);

			// Read Tiles
			Tile tile = Main.tile[i, j];

			if ((flags & TileIOFlags.ModTile) == TileIOFlags.ModTile) {
				tile.active(true);

				ushort saveType = (ushort) (reader.ReadUInt16() - TileID.Count);
				tile.type = tables.tileData[saveType].id;

				// Implement tile frames
				if (tables.frameImportant[saveType]) {
					if ((flags & TileIOFlags.FrameXInt16) == TileIOFlags.FrameXInt16) {
						tile.frameX = reader.ReadInt16();
					}
					else {
						tile.frameX = reader.ReadByte();
					}
					if ((flags & TileIOFlags.FrameYInt16) == TileIOFlags.FrameYInt16) {
						tile.frameY = reader.ReadInt16();
					}
					else {
						tile.frameY = reader.ReadByte();
					}
				}
				else {
					tile.frameX = -1;
					tile.frameY = -1;
				}

				if ((flags & TileIOFlags.TileColor) == TileIOFlags.TileColor) {
					tile.color(reader.ReadByte());
				}

				

				if (unloadedTileIDs.Contains(tile.type)) {   
					tableData tabData = tables.tileData[saveType];

					// Update Unloaded Tile Position Map
					if (tabData.name != null) {
						info = new UnloadedInfo(tabData.modName, tabData.name, tabData.fallbackID);
						posIndexer.MapPosToInfo(tileInfos, tileInfoMap, info: info);
					}
					else {
						posIndexer.MapPosToInfo(tileInfos, tileInfoMap, prevTileInfoMap); 
					}
				}

				WorldGen.tileCounts[tile.type] += j <= Main.worldSurface ? 5 : 1;
			}

			if ((flags & TileIOFlags.ModWall) == TileIOFlags.ModWall) {
				ushort saveWallType = (ushort) (reader.ReadUInt16() - WallID.Count);
				tile.wall = tables.wallData[saveWallType].id;

				if ((flags & TileIOFlags.WallColor) == TileIOFlags.WallColor) {
					tile.wallColor(reader.ReadByte());
				}

				if (tile.wall == ModContent.Find<ModWall>("ModLoader/UnloadedWall").Type) {
					tableData tabData = tables.wallData[saveWallType];

					// Update Unloaded Wall Position Map
					if (tabData.name != null) {
						info = new UnloadedInfo(tabData.modName, tabData.name, tabData.fallbackID);
						posIndexer.MapPosToInfo(wallInfos, wallInfoMap, info: info);
					}
					else {
						posIndexer.MapPosToInfo(wallInfos, wallInfoMap, prevWallInfoMap);
					}
				}
			}

			// Handle re-occurence, up to 256 counts.
			if ((flags & TileIOFlags.NextTilesAreSame) == TileIOFlags.NextTilesAreSame) { 
				byte sameCount = reader.ReadByte(); //how many are the same

				for (byte k = 0; k < sameCount; k++) { // for all copy-paste tiles
					NextTile(ref i, ref j); // move i,j to the next tile, with vertical being priority
					
					Main.tile[i, j].CopyFrom(tile); 
					WorldGen.tileCounts[tile.type] += j <= Main.worldSurface ? 5 : 1;
				}
			}

			if ((flags & TileIOFlags.NextModTile) == TileIOFlags.NextModTile) {
				nextModTile = true;
			}
		}

		private static bool HasModData(Tile tile) => (tile.active() && tile.type >= TileID.Count) || tile.wall >= WallID.Count;

		internal static bool NextTile(ref int i, ref int j) {
			j++;
			if (j >= Main.maxTilesY) {
				j = 0;
				i++;
				if (i >= Main.maxTilesX) {
					return false;
				}
			}
			return true;
		}

		//TODO: Figure out where to put this
		public static string GetUnloadedTileType(int type) {
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

		public static void UpdateMaps(IList<TagCompound> list, List<posMap> prevPosMap) {
			prevPosMap.Clear();
			foreach (var posTag in list) {
				prevPosMap.Add(new posMap {
					posID = posTag.Get<int>("posID"),
					infoID = posTag.Get<ushort>("infoID")
				});
			}
		}

		//*********** Containers (*annequin) Save, Load, and Placeholder Implementations ***********************************//

		//in Terraria.IO.WorldFile.SaveWorldTiles for saving tile frames add
		//  short frameX = tile.frameX; TileIO.VanillaSaveFrames(tile, ref frameX);
		//  and replace references to tile.frameX with frameX
		internal static void VanillaSaveFrames(Tile tile, ref short frameX) {
			if (tile.type == TileID.Mannequin || tile.type == TileID.Womannequin) {
				int slot = tile.frameX / 100;
				int position = tile.frameY / 18;
				if (HasModArmor(slot, position)) {
					frameX %= 100;
				}
			}
		}

		internal struct ContainerTables
		{
			internal IDictionary<int, int> headSlots;
			internal IDictionary<int, int> bodySlots;
			internal IDictionary<int, int> legSlots;

			internal static ContainerTables Create() {
				ContainerTables tables = new ContainerTables {
					headSlots = new Dictionary<int, int>(),
					bodySlots = new Dictionary<int, int>(),
					legSlots = new Dictionary<int, int>()
				};
				return tables;
			}
		}
		//in Terraria.GameContent.Tile_Entities.TEItemFrame.WriteExtraData
		//  if item is a mod item write 0 as the ID
		internal static TagCompound SaveContainers() {
			var ms = new MemoryStream();
			var writer = new BinaryWriter(ms);
			byte[] flags = new byte[1];
			byte numFlags = 0;
			ISet<int> headSlots = new HashSet<int>();
			ISet<int> bodySlots = new HashSet<int>();
			ISet<int> legSlots = new HashSet<int>();
			IDictionary<int, int> itemFrames = new Dictionary<int, int>();
			for (int i = 0; i < Main.maxTilesX; i++) {
				for (int j = 0; j < Main.maxTilesY; j++) {
					Tile tile = Main.tile[i, j];
					if (tile.active() && (tile.type == TileID.Mannequin || tile.type == TileID.Womannequin)) {
						int slot = tile.frameX / 100;
						int position = tile.frameY / 18;
						if (HasModArmor(slot, position)) {
							if (position == 0) {
								headSlots.Add(slot);
							}
							else if (position == 1) {
								bodySlots.Add(slot);
							}
							else if (position == 2) {
								legSlots.Add(slot);
							}
							flags[0] |= 1;
							numFlags = 1;
						}
					}
				}
			}
			int tileEntity = 0;
			foreach (KeyValuePair<int, TileEntity> entity in TileEntity.ByID) {
				TEItemFrame itemFrame = entity.Value as TEItemFrame;
				if (itemFrame != null && ItemLoader.NeedsModSaving(itemFrame.item)) {
					itemFrames.Add(itemFrame.ID, tileEntity);
					//flags[0] |= 2; legacy
					numFlags = 1;
				}
				if(!(entity.Value is ModTileEntity))
					tileEntity++;
			}
			if (numFlags == 0) {
				return null;
			}
			writer.Write(numFlags);
			writer.Write(flags, 0, numFlags);
			if ((flags[0] & 1) == 1) {
				writer.Write((ushort)headSlots.Count);
				foreach (int slot in headSlots) {
					writer.Write((ushort)slot);
					ModItem item = ItemLoader.GetItem(EquipLoader.slotToId[EquipType.Head][slot]);
					writer.Write(item.Mod.Name);
					writer.Write(item.Name);
				}
				writer.Write((ushort)bodySlots.Count);
				foreach (int slot in bodySlots) {
					writer.Write((ushort)slot);
					ModItem item = ItemLoader.GetItem(EquipLoader.slotToId[EquipType.Body][slot]);
					writer.Write(item.Mod.Name);
					writer.Write(item.Name);
				}
				writer.Write((ushort)legSlots.Count);
				foreach (int slot in legSlots) {
					writer.Write((ushort)slot);
					ModItem item = ItemLoader.GetItem(EquipLoader.slotToId[EquipType.Legs][slot]);
					writer.Write(item.Mod.Name);
					writer.Write(item.Name);
				}
				WriteContainerData(writer);
			}
			var tag = new TagCompound();
			tag.Set("data", ms.ToArray());

			if (itemFrames.Count > 0) {
				tag.Set("itemFrames", itemFrames.Select(entry =>
					new TagCompound {
						["id"] = entry.Value,
						["item"] = ItemIO.Save(((TEItemFrame)TileEntity.ByID[entry.Key]).item)
					}
				).ToList());
			}
			return tag;
		}

		internal static void LoadContainers(TagCompound tag) {
			if (tag.ContainsKey("data"))
				ReadContainers(new BinaryReader(new MemoryStream(tag.GetByteArray("data"))));

			foreach (var frameTag in tag.GetList<TagCompound>("itemFrames")) {
				if (TileEntity.ByID.TryGetValue(frameTag.GetInt("id"), out TileEntity tileEntity) && tileEntity is TEItemFrame itemFrame)
					ItemIO.Load(itemFrame.item, frameTag.GetCompound("item"));
				else
					Logging.tML.Warn($"Due to a bug in previous versions of tModLoader, the following ItemFrame data has been lost: {frameTag.ToString()}");
			}
		}

		internal static void ReadContainers(BinaryReader reader) {
			byte[] flags = new byte[1];

			reader.Read(flags, 0, reader.ReadByte());

			if ((flags[0] & 1) == 1) {
				var tables = ContainerTables.Create();
				int count = reader.ReadUInt16();

				for (int k = 0; k < count; k++) {
					tables.headSlots[reader.ReadUInt16()] = ModContent.TryFind(reader.ReadString(), reader.ReadString(), out ModItem item) ? item.Item.headSlot : 0;
				}

				count = reader.ReadUInt16();

				for (int k = 0; k < count; k++) {
					tables.bodySlots[reader.ReadUInt16()] = ModContent.TryFind(reader.ReadString(), reader.ReadString(), out ModItem item) ? item.Item.bodySlot : 0;
				}

				count = reader.ReadUInt16();

				for (int k = 0; k < count; k++) {
					tables.legSlots[reader.ReadUInt16()] = ModContent.TryFind(reader.ReadString(), reader.ReadString(), out ModItem item) ? item.Item.legSlot : 0;
				}

				ReadContainerData(reader, tables);
			}

			//legacy load //Let's not care anymore.
			/*if ((flags[0] & 2) == 2) {
				int count = reader.ReadInt32();
				for (int k = 0; k < count; k++) {
					int id = reader.ReadInt32();
					TEItemFrame itemFrame = TileEntity.ByID[id] as TEItemFrame;
					ItemIO.LoadLegacy(itemFrame.item, reader, true);
				}
			}*/
		}

		internal static void WriteContainerData(BinaryWriter writer) {
			for (int i = 0; i < Main.maxTilesX; i++) {
				for (int j = 0; j < Main.maxTilesY; j++) {
					Tile tile = Main.tile[i, j];
					if (tile.active() && (tile.type == TileID.Mannequin || tile.type == TileID.Womannequin)) {
						int slot = tile.frameX / 100;
						int frameX = tile.frameX % 100;
						int position = tile.frameY / 18;
						if (HasModArmor(slot, position) && frameX % 36 == 0) {
							writer.Write(i);
							writer.Write(j);
							writer.Write((byte)position);
							writer.Write((ushort)slot);
						}
					}
				}
			}
			writer.Write(-1);
		}

		internal static void ReadContainerData(BinaryReader reader, ContainerTables tables) {
			int i = reader.ReadInt32();
			while (i > 0) {
				int j = reader.ReadInt32();
				int position = reader.ReadByte();
				int slot = reader.ReadUInt16();
				Tile left = Main.tile[i, j];
				Tile right = Main.tile[i + 1, j];
				if (left.active() && right.active() && (left.type == TileID.Mannequin || left.type == TileID.Womannequin)
					&& left.type == right.type && (left.frameX == 0 || left.frameX == 36) && right.frameX == left.frameX + 18
					&& left.frameY / 18 == position && left.frameY == right.frameY) {
					if (position == 0) {
						slot = tables.headSlots[slot];
					}
					else if (position == 1) {
						slot = tables.bodySlots[slot];
					}
					else if (position == 2) {
						slot = tables.legSlots[slot];
					}
					left.frameX += (short)(100 * slot);
				}
				i = reader.ReadInt32();
			}
		}

		private static bool HasModArmor(int slot, int position) {
			if (position == 0) {
				return slot >= Main.numArmorHead;
			}
			else if (position == 1) {
				return slot >= Main.numArmorBody;
			}
			else if (position == 2) {
				return slot >= Main.numArmorLegs;
			}
			return false;
		}

		//*********** Tile Entities Save, Load, and Placeholder Implementations ***********************************//
		internal static List<TagCompound> SaveTileEntities() {
			var list = new List<TagCompound>();

			foreach (KeyValuePair<int, TileEntity> pair in TileEntity.ByID) {
				var tileEntity = pair.Value;
				var modTileEntity = tileEntity as ModTileEntity;

				list.Add(new TagCompound {
					["mod"] = modTileEntity?.Mod.Name ?? "Terraria",
					["name"] = modTileEntity?.Name ?? tileEntity.GetType().Name,
					["X"] = tileEntity.Position.X,
					["Y"] = tileEntity.Position.Y,
					["data"] = tileEntity.Save()
				});
			}

			return list;
		}

		internal static void LoadTileEntities(IList<TagCompound> list) {
			foreach (TagCompound tag in list) {
				string modName = tag.GetString("mod");
				string name = tag.GetString("name");
				var point = new Point16(tag.GetShort("X"), tag.GetShort("Y"));

				ModTileEntity baseModTileEntity = null;
				TileEntity tileEntity = null;

				//If the TE is modded
				if (modName != "Terraria") {
					//Find its type, defaulting to pending.
					if (!ModContent.TryFind(modName, name, out baseModTileEntity)) {
						baseModTileEntity = ModContent.GetInstance<UnloadedTileEntity>();
					}

					tileEntity = ModTileEntity.ConstructFromBase(baseModTileEntity);
					tileEntity.type = (byte)baseModTileEntity.Type;
					tileEntity.Position = point;

					(tileEntity as UnloadedTileEntity)?.SetData(tag);
				}
				//Otherwise, if the TE is vanilla, try to find its existing instance for the current coordinate.
				else if (!TileEntity.ByPosition.TryGetValue(point, out tileEntity)) {
					//Do not create an PendingTileEntity on failure to do so.
					continue;
				}

				//Load TE data.
				if (tag.ContainsKey("data")) {
					try {
						tileEntity.Load(tag.GetCompound("data"));

						if (tileEntity is ModTileEntity modTileEntity) {
							(tileEntity as UnloadedTileEntity)?.TryRestore(ref modTileEntity);

							tileEntity = modTileEntity;
						}
					}
					catch (Exception e) {
						throw new CustomModDataException((tileEntity as ModTileEntity)?.Mod, $"Error in reading {name} tile entity data for {modName}", e);
					}
				}

				//Check mods' TEs for being valid. If they are, register them to TE collections.
				if (baseModTileEntity != null && baseModTileEntity.ValidTile(tileEntity.Position.X, tileEntity.Position.Y)) {
					tileEntity.ID = TileEntity.AssignNewID();
					TileEntity.ByID[tileEntity.ID] = tileEntity;

					if (TileEntity.ByPosition.TryGetValue(tileEntity.Position, out TileEntity other)) {
						TileEntity.ByID.Remove(other.ID);
					}

					TileEntity.ByPosition[tileEntity.Position] = tileEntity;
				}
			}
		}
	}
}
