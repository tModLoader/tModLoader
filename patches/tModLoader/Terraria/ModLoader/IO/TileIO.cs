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
		//Should add liquids data to implementation in locational data

		//in Terraria.IO.WorldFile.SaveWorldTiles add type check to tile.active() check and wall check
		internal struct TileTables
		{
			internal IDictionary<ushort, ushort> tiles;
			internal IDictionary<ushort, bool> frameImportant;
			internal IDictionary<ushort, bool> IsSolid;
			internal IDictionary<ushort, bool> IsChest;
			internal IDictionary<ushort, string> tileModNames;
			internal IDictionary<ushort, string> tileNames;

			internal IDictionary<ushort, ushort> walls;
			internal IDictionary<ushort, string> WallModNames;
			internal IDictionary<ushort, string> WallNames;
			


			internal static TileTables Create() {
				TileTables tables = new TileTables {
					tiles = new Dictionary<ushort, ushort>(),
					frameImportant = new Dictionary<ushort, bool>(),
					IsSolid = new Dictionary<ushort, bool>(),
					IsChest = new Dictionary<ushort, bool>(),
					tileModNames = new Dictionary<ushort, string>(),
					tileNames = new Dictionary<ushort, string>(),

					walls = new Dictionary<ushort, ushort>(),
					WallModNames = new Dictionary<ushort, string>(),
					WallNames = new Dictionary<ushort, string>(),
				};
				return tables;
			}
		}

		internal static TagCompound SaveTiles() {
			var hasTile = new bool[TileLoader.TileCount];
			var hasWall = new bool[WallLoader.WallCount];
			using (var ms = new MemoryStream())
			using (var writer = new BinaryWriter(ms)) {
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
						["framed"] = Main.tileFrameImportant[type],
						["IsChest"] = TileID.Sets.BasicChest[type], // Flag Chests for preserving Chest Tile data 
						["IsSolid"] = Main.tileSolid[type], // Flag non-solid blocks for liquids, player movement, etc.
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
					});
				}

				// Return compressed variant of all data
				if (tileList.Count == 0 && wallList.Count == 0)
					return null;

				return new TagCompound {
					["tileMap"] = tileList, // List of all active tile types
					["wallMap"] = wallList, // List of all active wall types
					["data"] = ms.ToArray() // Array of locational-specific data
				};
			}
		}

		internal static void LoadTiles(TagCompound tag) {
			// If there is no modded data saved, skip
			if (!tag.ContainsKey("data"))
				return;

			// Create a Table to store working information
			var tables = TileTables.Create();

			// Retrieve Basic Tile Type Data from saved Tile Map, and store in table
			foreach (var tileTag in tag.GetList<TagCompound>("tileMap")) {
				ushort type = (ushort)tileTag.GetShort("value");
				string modName = tileTag.GetString("mod");
				string name = tileTag.GetString("name");
				tables.tiles[type] = ModContent.TryFind(modName, name, out ModTile tile) ? tile.Type : (ushort)0;
				if (tables.tiles[type] == 0) {
					tables.tiles[type] = ModContent.Find<ModTile>("ModLoader/PendingUnloadedTile").Type; //where is this tile?
					tables.tileModNames[type] = modName;
					tables.tileNames[type] = name;
				}
				tables.frameImportant[type] = tileTag.GetBool("framed");
				tables.IsChest[type]= tileTag.GetBool("IsChest");
				tables.IsSolid[type] = tileTag.GetBool("IsSolid");
			}

			// Retrieve Basic Wall Type Data from saved Wall Map, and store in table
			foreach (var wallTag in tag.GetList<TagCompound>("wallMap")) {
				ushort type = (ushort)wallTag.GetShort("value");
				string modName = wallTag.GetString("mod");
				string name = wallTag.GetString("name");
				tables.walls[type] = ModContent.TryFind(modName, name, out ModWall wall) ? wall.Type : (ushort)0;
				/*if (tables.walls[type] == 0) {
					tables.walls[type] = ModContent.Find<ModTile>("ModLoader/PendingUnloadedWall").Type;
					tables.WallModNames[type] = modName;
					tables.WallNames[type] = name;
				}*/
			}

			// Retrieve Locational-Specific Data from 'Data' and apply
			using (var memoryStream = new MemoryStream(tag.GetByteArray("data")))
			using (var reader = new BinaryReader(memoryStream))
				ReadTileData(reader, tables);

			// Validate Load
			WorldIO.ValidateSigns();
		}

		internal static void WriteTileData(BinaryWriter writer, bool[] hasTile, bool[] hasWall) {
			byte skip = 0;
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
					skip++;
					if (skip == 255) {
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
								return;
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
			byte flags = 0;
			byte[] data = new byte[11];
			int index = 1;

			// Write Tiles
			if (tile.active() && tile.type >= TileID.Count) {
				// Atleast one Tile is placed of that Type, so record Type as 16 bit uint
				hasTile[tile.type] = true;
				flags |= 1; // Bit 0
				data[index] = (byte)tile.type;
				index++;
				data[index] = (byte)(tile.type >> 8);
				index++;

				// Checks if Tile Framing is important, if so records x and y data as 16 bit uint
				if (Main.tileFrameImportant[tile.type]) {
					data[index] = (byte)tile.frameX;
					index++;
					if (tile.frameX >= 256) {
						flags |= 2; // Bit 1
						data[index] = (byte)(tile.frameX >> 8);
						index++;
					}
					data[index] = (byte)tile.frameY;
					index++;
					if (tile.frameY >= 256) {
						flags |= 4; // Bit 2
						data[index] = (byte)(tile.frameY >> 8);
						index++;
					}
				}

				// Checks if Tile has been Coloured 
				if (tile.color() != 0) {
					flags |= 8; // Bit 3
					data[index] = tile.color();
					index++;
				}
			}

			// Write Walls
			if (tile.wall >= WallID.Count) {
				// Atleast one Wall is placed of that ID, so record ID as 16 bit uint
				hasWall[tile.wall] = true;
				flags |= 16; // Bit 4
				data[index] = (byte)tile.wall;
				index++;
				data[index] = (byte)(tile.wall >> 8);
				index++;

				// Checks if Wall has been Coloured 
				if (tile.wallColor() != 0) {
					flags |= 32; // Bit 5
					data[index] = tile.wallColor();
					index++;
				}
			}

			/* Check for re-occurence of exact same tile (includes walls, liquids, wires, etc), and compress data by 
			// skipping to next occurence of a new modded tile. Maximum of 256 grouped tiles. */
			int nextI = i;
			int nextJ = j;
			byte sameCount = 0;
			while (NextTile(ref nextI, ref nextJ)) {
				if (tile.isTheSameAs(Main.tile[nextI, nextJ]) && sameCount < 255) {
					sameCount++;
					i = nextI;
					j = nextJ;
				}
				else if (HasModData(Main.tile[nextI, nextJ])) {
					flags |= 128; // Bit 7, marks that next tile is still modded, for use during loading
					nextModTile = true;
					break;
				}
				else {
					break;
				}
			}
			if (sameCount > 0) {
				flags |= 64; // Bit 6
				data[index] = sameCount;
				index++;
			}

			// Output result Data array to stream
			data[0] = flags;
			writer.Write(data, 0, index);
		}

		internal static void ReadModTile(ref int i, ref int j, TileTables tables, BinaryReader reader, ref bool nextModTile) {
			// Access Stored Flags
			byte flags;
			flags = reader.ReadByte();

			// Read Tiles
			Tile tile = Main.tile[i, j];
			if ((flags & 1) == 1) { // If is modded tile
				// Initialize tile
				tile.active(true);
				ushort saveType = reader.ReadUInt16();
				tile.type = tables.tiles[saveType];

				// Implement tile frames
				if (tables.frameImportant[saveType]) {
					if ((flags & 2) == 2) {
						tile.frameX = reader.ReadInt16();
					}
					else {
						tile.frameX = reader.ReadByte();
					}
					if ((flags & 4) == 4) {
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

				// Handle Disabled Mods and Implement a Placeholder Tile
				if (tile.type == ModContent.Find<ModTile>("ModLoader/PendingUnloadedTile").Type
					&& tables.tileNames.ContainsKey(saveType)) {
					// Load saved Basic Tile Type Data into UnloadedTileInfo
					UnloadedTileInfo info;
					if (tables.frameImportant[saveType]) {
						info = new UnloadedTileInfo(tables.tileModNames[saveType], tables.tileNames[saveType],
							tile.frameX, tile.frameY, tables.IsChest[saveType],tables.IsSolid[saveType]);
					}
					else {
						info = new UnloadedTileInfo(tables.tileModNames[saveType], tables.tileNames[saveType], 
							tables.IsChest[saveType], tables.IsSolid[saveType]);
					}

					// Create and apply placeholder tile frame
					UnloadedTilesWorld modWorld = ModContent.GetInstance<UnloadedTilesWorld>();
					int pendingFrameID = modWorld.pendingInfos.IndexOf(info);
					if (pendingFrameID < 0) {
						pendingFrameID = modWorld.pendingInfos.Count;
						modWorld.pendingInfos.Add(info);
					}
					UnloadedTileFrame pendingFrame = new UnloadedTileFrame(pendingFrameID);
					tile.frameX = pendingFrame.FrameX;
					tile.frameY = pendingFrame.FrameY;
				}

				// Apply Colouring to Tile
				if ((flags & 8) == 8) {
					tile.color(reader.ReadByte());
				}

				// Ready Tile
				WorldGen.tileCounts[tile.type] += j <= Main.worldSurface ? 5 : 1;
			}

			// Read Walls 
			if ((flags & 16) == 16) { // if is modded wall
				// Initialize Wall
				ushort saveWall = reader.ReadUInt16();
				tile.wall = tables.walls[saveWall];

				/* Handle Disabled Mods and Implement a Placeholder Wall
				if (tile.type == ModContent.Find<ModTile>("ModLoader/PendingUnloadedWall").Type
					&& tables.WallNames.ContainsKey(saveWall)) {
					// Load saved Basic Tile Type Data into UnloadedTileInfo
					UnloadedWallInfo info;
					info = new UnloadedWallInfo(tables.WallModNames[saveWall], tables.WallNames[saveWall]);
				}*/

				// Apply Colouring to Wall
				if ((flags & 32) == 32) {
					tile.wallColor(reader.ReadByte());
				}
			}

			// Handle re-occurence, up to 256 counts.
			if ((flags & 64) == 64) {
				byte sameCount = reader.ReadByte();
				for (byte k = 0; k < sameCount; k++) {
					NextTile(ref i, ref j);
					Main.tile[i, j].CopyFrom(tile);
					WorldGen.tileCounts[tile.type] += j <= Main.worldSurface ? 5 : 1;
				}
			}

			// Set flag if next tile in indexed shortlist is a modded tile
			if ((flags & 128) == 128) {
				nextModTile = true;
			}
		}

		private static bool HasModData(Tile tile) {
			return (tile.active() && tile.type >= TileID.Count) || tile.wall >= WallID.Count;
		}

		private static bool NextTile(ref int i, ref int j) {
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
					tables.headSlots[reader.ReadUInt16()] = ModContent.TryFind(reader.ReadString(), reader.ReadString(), out ModItem item) ? item.item.headSlot : 0;
				}

				count = reader.ReadUInt16();

				for (int k = 0; k < count; k++) {
					tables.bodySlots[reader.ReadUInt16()] = ModContent.TryFind(reader.ReadString(), reader.ReadString(), out ModItem item) ? item.item.bodySlot : 0;
				}

				count = reader.ReadUInt16();

				for (int k = 0; k < count; k++) {
					tables.legSlots[reader.ReadUInt16()] = ModContent.TryFind(reader.ReadString(), reader.ReadString(), out ModItem item) ? item.item.legSlot : 0;
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
					//Find its type, defaulting to unloaded.
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
					//Do not create an UnloadedTileEntity on failure to do so.
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
