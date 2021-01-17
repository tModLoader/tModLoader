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
		internal static ushort PendingChest => ModContent.Find<ModTile>("ModLoader/PendingChest").Type;

		internal static ushort PendingDresser => ModContent.Find<ModTile>("ModLoader/PendingDresser").Type;

		internal static ushort PendingWallType => ModContent.Find<ModWall>("ModLoader/PendingWall").Type;

		internal static ushort PendingTile => ModContent.Find<ModTile>("ModLoader/PendingTile").Type;

		internal static ushort PendingNonSolidTile => ModContent.Find<ModTile>("ModLoader/PendingNonSolidTile").Type;

		internal static ushort PendingSemiSolidTile => ModContent.Find<ModTile>("ModLoader/PendingSemiSolidTile").Type;

		/// These values are synced to match UpdateUnloadedInfos <see cref="UpdateUnloadedInfos"/> 
		/// and synced to UnloadedPosIndexing <see cref="UnloadedPosIndexing"/>
		internal static byte TilesIndex = 0;
		internal static byte WallsIndex = 1;
		internal static byte ChestIndex = 2;

		//*********** Tile, Walls, & Chests Save, Load, and Placeholder Implementations ***************************//
		//Should add liquids data to implementation in locational data

		//in Terraria.IO.WorldFile.SaveWorldTiles add type check to tile.active() check and wall check
		internal struct TileTables
		{
			internal IDictionary<ushort, ushort> tiles;
			internal IDictionary<ushort, bool> frameImportant;
			internal IDictionary<ushort, string> tileModNames;
			internal IDictionary<ushort, string> tileNames;
			internal IDictionary<ushort, ushort> tileFallback;

			internal IDictionary<ushort, ushort> walls;
			internal IDictionary<ushort, string> wallModNames;
			internal IDictionary<ushort, string> wallNames;
			internal IDictionary<ushort, ushort> wallFallback;

			internal static TileTables Create() {
				TileTables tables = new TileTables {
					tiles = new Dictionary<ushort, ushort>(),
					frameImportant = new Dictionary<ushort, bool>(),
					tileModNames = new Dictionary<ushort, string>(),
					tileNames = new Dictionary<ushort, string>(),
					tileFallback = new Dictionary<ushort, ushort>(),

					walls = new Dictionary<ushort, ushort>(),
					wallModNames = new Dictionary<ushort, string>(),
					wallNames = new Dictionary<ushort, string>(),
					wallFallback = new Dictionary<ushort, ushort>(),
				};
				return tables;
			}
		}

		internal static class TileVariantFlags
		{
			internal const byte Standard = 0;
			internal const byte NonSolid = 1;
			internal const byte SemiSolid = 2;
			internal const byte Planter = 4;
			internal const byte Container = 8;
			internal const byte Dresser = 16;
			internal const byte Chest = 32;
			internal const byte Framed = 64;
		}

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
		
		internal static class TileIOFlags2
		{
			internal const byte None = 0;
			internal const byte ChestStyle = 1;
			internal const byte LiquidAmount = 2;
			internal const byte CustomData = 128;
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

					ushort tileVariants = TileVariantFlags.Standard;
					if (TileID.Sets.BasicChest[type])
						tileVariants |= TileVariantFlags.Chest; // Flag Chests for preserving Chest Tile data 
					if (TileID.Sets.BasicDresser[type])
						tileVariants |= TileVariantFlags.Dresser; // Flag Dressers for preserving Dresser shape
					if (TileID.Sets.IsAContainer[type] && !TileID.Sets.BasicDresser[type] && !TileID.Sets.BasicChest[type])
						tileVariants |= TileVariantFlags.Container; // Flag non-standard Containers 
					if (!Main.tileSolid[type])
						tileVariants |= TileVariantFlags.NonSolid; // Flag non-solid blocks for liquids, player movement, etc.
					if (Main.tileSolidTop[type])
						tileVariants |= TileVariantFlags.SemiSolid; // Flag semi-solid blocks for platforms, tables
					if (Main.tileFrameImportant[type])
						tileVariants |= TileVariantFlags.Framed; // Flag tiles where framing is important

					var modTile = TileLoader.GetTile(type);
					tileList.Add(new TagCompound { 
						["value"] = (short)type,
						["mod"] = modTile.Mod.Name,
						["name"] = modTile.Name,
						["variants"] = tileVariants,
						["framed"] = Main.tileFrameImportant[type], // Just here for legacy
						["fallbackType"] = (ushort)1, //Unintelligent fallback type prototyping
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
						["fallbackType"] = (ushort)1, //Unintelligent fallback type prototyping
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
			ushort pendingTile = PendingTile;
			ushort pendingNSTile = PendingNonSolidTile;
			ushort pendingChest = PendingChest;
			ushort pendingSSTile = PendingSemiSolidTile;
			ushort pendingDresser = PendingDresser;
			ushort pendingWallType = PendingWallType;
			// Create a flag to support legacy (pre-unloaded rework change) world loading
			bool unloadedLegacyCompat = false;

			// Retrieve Basic Tile Type Data from saved Tile Map, and store in table
			foreach (var tileTag in tag.GetList<TagCompound>("tileMap")) {
				ushort type = (ushort)tileTag.GetShort("value");
				string modName = tileTag.GetString("mod");
				string name = tileTag.GetString("name");
				ushort tileVariants = tileTag.Get<ushort>("variants");

				tables.tiles[type] = ModContent.TryFind(modName, name, out ModTile tile) ? tile.Type : (ushort)0;
				if (tables.tiles[type] == 0) {
					tables.tileModNames[type] = modName;
					tables.tileNames[type] = name;

					ushort workingType = pendingTile;
					ushort fallbackType = 0;
					if (tileTag.ContainsKey("variants")) {
						if ((tileVariants & TileVariantFlags.NonSolid) == TileVariantFlags.NonSolid) {
							workingType = pendingNSTile;
						}
						if ((tileVariants & TileVariantFlags.Chest) == TileVariantFlags.Chest) { // Order matters, Chest should override NonSolid
							workingType = pendingChest;
						}
						if ((tileVariants & TileVariantFlags.SemiSolid) == TileVariantFlags.SemiSolid)
							workingType = pendingSSTile;
						if ((tileVariants & TileVariantFlags.Dresser) == TileVariantFlags.Dresser) // Order matters, Dresser should override SemiSolid
							workingType = pendingDresser;

						fallbackType = tileTag.Get<ushort>("fallbackType");
					}
					tables.tileFallback[type] = fallbackType;
					tables.tiles[type] = workingType;
				}
				tables.frameImportant[type] = tileTag.GetBool("framed");

				// Check if loading locational data requires legacy support:  
				//Note: legacy loading will not work if the world contains no modded tiles, but does contatin modded walls. Treated as edge case
				if (!unloadedLegacyCompat && !tileTag.ContainsKey("variants")) {
					unloadedLegacyCompat = true;
				}
			}

			// Retrieve Basic Wall Type Data from saved Wall Map, and store in table
			foreach (var wallTag in tag.GetList<TagCompound>("wallMap")) {
				ushort type = (ushort)wallTag.GetShort("value");
				string modName = wallTag.GetString("mod");
				string name = wallTag.GetString("name");
				tables.walls[type] = ModContent.TryFind(modName, name, out ModWall wall) ? wall.Type : (ushort)0;
				if (tables.walls[type] == 0) {
					tables.walls[type] = pendingWallType;
					tables.wallModNames[type] = modName;
					tables.wallNames[type] = name;
					if (!unloadedLegacyCompat) {
						tables.wallFallback[type] = wallTag.Get<ushort>("fallbackType");
					}
					else
						tables.wallFallback[type] = 0;
				}
			}

			// Retrieve Locational-Specific Data from 'Data' and apply
			using (var memoryStream = new MemoryStream(tag.GetByteArray("data")))
			using (var reader = new BinaryReader(memoryStream))
				ReadTileData(reader, tables, unloadedLegacyCompat);

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
					skip++; //Skip over vanilla tiles
					if (skip == 255) {
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

		internal static void ReadTileData(BinaryReader reader, TileTables tables, bool legacyCompatFlag) {
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
				ReadModTile(ref i, ref j, tables, reader, ref nextModTile, legacyCompatFlag);
			}
			while (NextTile(ref i, ref j));
		}

		internal static void WriteModTile(ref int i, ref int j, BinaryWriter writer, ref bool nextModTile, bool[] hasTile, bool[] hasWall) {
			Tile tile = Main.tile[i, j];
			byte flags = TileIOFlags.None;
			byte flags2 = TileIOFlags2.None;
			byte[] data = new byte[13]; //data[0-1] will be filled with the flags, hence why index starts with 2 below
			int index = 2;

			// Write Tiles
			if (tile.active() && tile.type >= TileID.Count) {
				// Atleast one Tile is placed of that Type, so record Type as 16 bit uint
				hasTile[tile.type] = true;
				flags |= TileIOFlags.ModTile;
				//Converts an Int16 into two bytes, reversed by reader.ReadInt16
				data[index] = (byte)tile.type;
				index++;
				data[index] = (byte)(tile.type >> 8);
				index++;

				// Checks if Tile Framing is important, if so records x and y data as 16 bit uint
				if (Main.tileFrameImportant[tile.type]) {
					data[index] = (byte)tile.frameX;
					index++;
					if (tile.frameX >= 256) {
						flags |= TileIOFlags.FrameXInt16;
						data[index] = (byte)(tile.frameX >> 8);
						index++;
					}
					data[index] = (byte)tile.frameY;
					index++;
					if (tile.frameY >= 256) {
						flags |= TileIOFlags.FrameYInt16;
						data[index] = (byte)(tile.frameY >> 8);
						index++;
					}
				}

				// Checks if Tile has been Coloured 
				if (tile.color() != 0) {
					data[index] = tile.color();
					flags |= TileIOFlags.TileColor;
					index++;
				}

				// Check if Tile is a chest, and record style as uint8
				if (TileID.Sets.BasicChest[tile.type]) {
					data[index] = (byte) Math.Truncate((decimal)(tile.frameX / 36));
					flags2 |= TileIOFlags2.ChestStyle;
					index++;
				}
			}

			// Write Walls
			if (tile.wall >= WallID.Count) {
				// Atleast one Wall is placed of that ID, so record ID as 16 bit uint
				hasWall[tile.wall] = true;
				flags |= TileIOFlags.ModWall;
				//Converts a UInt16 into two bytes, reversed by reader.ReadUInt16
				data[index] = (byte)tile.wall;
				index++;
				data[index] = (byte)(tile.wall >> 8);
				index++;

				// Checks if Wall has been Coloured 
				if (tile.wallColor() != 0) {
					flags |= TileIOFlags.WallColor;
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
				data[index] = sameCount;
				index++;
			}

			// Output result Data array to stream
			data[0] = flags;
			data[1] = flags2;
			writer.Write(data, 0, index);
		}

		internal static void ReadModTile(ref int i, ref int j, TileTables tables, BinaryReader reader, ref bool nextModTile,bool legacyCompatFlag) {
			// Access Stored 8bit Flags
			byte flags;
			flags = reader.ReadByte();
			// Apply legacy compatibility for saves following legacy 8bit flag system
			byte flags2 = TileIOFlags2.None;
			if (!legacyCompatFlag) {
				flags2 = reader.ReadByte();
			}

			UnloadedPosIndexing posIndexer = new UnloadedPosIndexing(i, j);
			UnloadedInfo info = null;

			// Read Tiles
			Tile tile = Main.tile[i, j];
			if ((flags & TileIOFlags.ModTile) == TileIOFlags.ModTile) {
				tile.active(true);
				ushort saveType = reader.ReadUInt16();
				tile.type = tables.tiles[saveType];

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

				byte chestStyle = 0;
				if ((flags2 & TileIOFlags2.ChestStyle) == TileIOFlags2.ChestStyle){
					chestStyle = reader.ReadByte();
				}

				
				if (tables.tileNames.ContainsKey(saveType)) {
					// Handle Disabled Mods and Implement Pending Tile
					bool nonSolidChk = tile.type == PendingNonSolidTile;
					bool semiSolidChk = tile.type == PendingSemiSolidTile;

					if (tile.type == PendingTile || nonSolidChk || semiSolidChk) {
						// Load saved Basic Tile Type Data into UnloadedTileInfo and index
						info = new UnloadedInfo(tables.tileModNames[saveType], tables.tileNames[saveType],tables.tileFallback[saveType]);
						posIndexer.SaveInfoToPos(info,TilesIndex);
						tile.type = UnloadedTilesSystem.UnloadedTile;
						if (nonSolidChk)
							tile.type = UnloadedTilesSystem.UnloadedNonSolidTile;
						if (semiSolidChk)
							tile.type = UnloadedTilesSystem.UnloadedSemiSolidTile;
					}
					else if (tile.type == PendingChest) {
						bool accountedFor = ( //Extremely janky check that not sure why is needed and works
							Main.tile[i - 1, j].type == UnloadedTilesSystem.UnloadedChest ||
							Main.tile[i, j - 1].type == UnloadedTilesSystem.UnloadedChest ||
							Main.tile[i - 1, j - 1].type == UnloadedTilesSystem.UnloadedChest);
						if (!accountedFor) { // Only care about top-left tile of 2x2 for Chests
							TagCompound customData = new TagCompound {
								["chestStyle"] = chestStyle,
							};
							// Load saved Basic Chest Type Data into UnloadedChestInfo and index
							info = new UnloadedInfo(tables.tileModNames[saveType], tables.tileNames[saveType],tables.tileFallback[saveType],customData);
							posIndexer.SaveInfoToPos(info,ChestIndex);
							// Place UnloadedChest (required to preserve the inventory and re-namings
							WorldGen.PlaceChestDirect(i, j + 1, UnloadedTilesSystem.UnloadedChest, 0, -1);
						}
					}
					else if (tile.type == PendingDresser) {
						bool accountedFor = ( //Extremely janky check that not sure why is needed and works
							Main.tile[i - 1, j].type == UnloadedTilesSystem.UnloadedDresser || //bottom middle
							Main.tile[i, j - 1].type == UnloadedTilesSystem.UnloadedDresser || //top right
							Main.tile[i - 1, j - 1].type == UnloadedTilesSystem.UnloadedDresser || //top middle
							Main.tile[i - 2, j - 1].type == UnloadedTilesSystem.UnloadedDresser || //top left
							Main.tile[i - 2, j].type == UnloadedTilesSystem.UnloadedDresser); //bottom left
						if (!accountedFor) { // Only care about top-left tile of 3x2 for Dressers
							// Load saved Basic Dresser Type Data into UnloadedChestInfo and index
							info = new UnloadedInfo(tables.tileModNames[saveType], tables.tileNames[saveType], tables.tileFallback[saveType]);
							posIndexer.SaveInfoToPos(info,ChestIndex);
							// Place UnloadedChest (required to preserve the inventory and re-namings
							WorldGen.PlaceDresserDirect(i + 1, j + 1, UnloadedTilesSystem.UnloadedDresser, 0, -1);
						}
					}
				}
				// Ready Tile
				WorldGen.tileCounts[tile.type] += j <= Main.worldSurface ? 5 : 1;
			}
			ushort saveWallType = WallID.None; //This value is used later, in handling reoccurence
			if ((flags & TileIOFlags.ModWall) == TileIOFlags.ModWall) {
				saveWallType = reader.ReadUInt16();
				tile.wall = tables.walls[saveWallType];
				if (tile.wall == PendingWallType) {
					info = new UnloadedInfo(tables.wallModNames[saveWallType], tables.wallNames[saveWallType],tables.wallFallback[saveWallType]);
					posIndexer.SaveInfoToPos(info,WallsIndex);
					tile.wall = UnloadedTilesSystem.UnloadedWallType;
				}
				if ((flags & TileIOFlags.WallColor) == TileIOFlags.WallColor) {
					tile.wallColor(reader.ReadByte());
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
