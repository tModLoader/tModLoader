using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ObjectData;

namespace Terraria.ModLoader
{
	//todo: further documentation
	/// <summary>
	/// This serves as the central class from which tile-related functions are supported and carried out.
	/// </summary>
	public static class TileLoader
	{
		//make Terraria.ObjectData.TileObjectData._data internal
		//make all static Terraria.ObjectData.TileObjectData.StyleName fields public
		//make Terraria.ObjectData.TileObjectData.LinkedAlternates public
		//make Terraria.ObjectData.TileObjectData.SubTiles and Alternates internal
		//at end of Terraria.ObjectData.TileObjectData.Initialize remove TileObjectData.readOnlyData = true;
		//at beginning of Terraria.WorldGen.PlaceTile remove type too high check
		//at beginning of Terraria.WorldGen.PlaceObject remove type too high check
		//in Terraria.WorldGen.Convert remove type too high checks
		//in Terraria.WorldGen.StartRoomCheck change 419 to WorldGen.houseTile.Length
		//at end of Terraria.WorldGen.KillWall remove type too high check
		//in Terraria.Player change adjTile and oldAdjTile size to TileLoader.TileCount()
		//in Terraria.Player.AdjTiles change 419 to adjTile.Length
		//in Terraria.Lighting for accOreFinder replace 419 with Main.tileValue.Length
		//make Terraria.WorldGen public
		//in Terraria.IO.WorldFile.SaveFileFormatHeader set initial num to TileLoader.TileCount
		private static int nextTile = TileID.Count;
		internal static readonly IList<ModTile> tiles = new List<ModTile>();
		internal static readonly IList<GlobalTile> globalTiles = new List<GlobalTile>();
		internal static readonly IDictionary<int, ModTree> trees = new Dictionary<int, ModTree>();
		internal static readonly IDictionary<int, ModPalmTree> palmTrees = new Dictionary<int, ModPalmTree>();
		internal static readonly IDictionary<int, ModCactus> cacti = new Dictionary<int, ModCactus>();
		private static bool loaded = false;
		private static readonly int vanillaChairCount = TileID.Sets.RoomNeeds.CountsAsChair.Length;
		private static readonly int vanillaTableCount = TileID.Sets.RoomNeeds.CountsAsTable.Length;
		private static readonly int vanillaTorchCount = TileID.Sets.RoomNeeds.CountsAsTorch.Length;
		private static readonly int vanillaDoorCount = TileID.Sets.RoomNeeds.CountsAsDoor.Length;

		private static Func<int, int, int, bool>[] HookKillSound;
		private delegate void DelegateNumDust(int i, int j, int type, bool fail, ref int num);
		private static DelegateNumDust[] HookNumDust;
		private delegate bool DelegateCreateDust(int i, int j, int type, ref int dustType);
		private static DelegateCreateDust[] HookCreateDust;
		private delegate void DelegateDropCritterChance(int i, int j, int type, ref int wormChance, ref int grassHopperChance, ref int jungleGrubChance);
		private static DelegateDropCritterChance[] HookDropCritterChance;
		private static Func<int, int, int, bool>[] HookDrop;
		private delegate bool DelegateCanKillTile(int i, int j, int type, ref bool blockDamaged);
		private static DelegateCanKillTile[] HookCanKillTile;
		private delegate void DelegateKillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem);
		private static DelegateKillTile[] HookKillTile;
		private static Func<int, int, int, bool>[] HookCanExplode;
		private static Action<int, int, int, bool>[] HookNearbyEffects;
		private delegate void DelegateModifyLight(int i, int j, int type, ref float r, ref float g, ref float b);
		private static DelegateModifyLight[] HookModifyLight;
		private static Func<int, int, int, Player, bool>[] HookDangersense;
		private delegate void DelegateSetSpriteEffects(int i, int j, int type, ref SpriteEffects spriteEffects);
		private static DelegateSetSpriteEffects[] HookSetSpriteEffects;
		private static Action[] HookAnimateTile;
		private static Func<int, int, int, SpriteBatch, bool>[] HookPreDraw;
		private delegate void DelegateDrawEffects(int i, int j, int type, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex);
		private static DelegateDrawEffects[] HookDrawEffects;
		private static Action<int, int, int, SpriteBatch>[] HookPostDraw;
		private static Action<int, int, int, SpriteBatch>[] HookSpecialDraw;
		private static Action<int, int, int>[] HookRandomUpdate;
		private delegate bool DelegateTileFrame(int i, int j, int type, ref bool resetFrame, ref bool noBreak);
		private static DelegateTileFrame[] HookTileFrame;
		private static Func<int, int, int, bool>[] HookCanPlace;
		private static Func<int, int[]>[] HookAdjTiles;
		private static Action<int, int, int>[] HookRightClick;
		private static Action<int, int, int>[] HookMouseOver;
		private static Action<int, int, int>[] HookMouseOverFar;
		private static Func<int, int, int, Item, bool>[] HookAutoSelect;
		private static Func<int, int, int, bool>[] HookPreHitWire;
		private static Action<int, int, int>[] HookHitWire;
		private static Func<int, int, int, bool>[] HookSlope;
		private static Action<int, Player>[] HookFloorVisuals;
		private delegate void DelegateChangeWaterfallStyle(int type, ref int style);
		private static DelegateChangeWaterfallStyle[] HookChangeWaterfallStyle;
		private delegate int DelegateSaplingGrowthType(int type, ref int style);
		private static DelegateSaplingGrowthType[] HookSaplingGrowthType;
		private static Action<int, int, Item>[] HookPlaceInWorld;

		internal static int ReserveTileID() {
			if (ModNet.AllowVanillaClients) throw new Exception("Adding tiles breaks vanilla client compatibility");

			int reserveID = nextTile;
			nextTile++;
			return reserveID;
		}

		public static int TileCount => nextTile;

		/// <summary>
		/// Gets the ModTile instance with the given type. If no ModTile with the given type exists, returns null.
		/// </summary>
		/// <param name="type">The type of the ModTile</param>
		/// <returns>The ModTile instance in the tiles array, null if not found.</returns>
		public static ModTile GetTile(int type) {
			return type >= TileID.Count && type < TileCount ? tiles[type - TileID.Count] : null;
		}

		private static void Resize2DArray<T>(ref T[,] array, int newSize) {
			int dim1 = array.GetLength(0);
			int dim2 = array.GetLength(1);
			T[,] newArray = new T[newSize, dim2];
			for (int j = 0; j < newSize && j < dim1; j++) {
				for (int k = 0; k < dim2; k++) {
					newArray[j, k] = array[j, k];
				}
			}
			array = newArray;
		}

		internal static void ResizeArrays(bool unloading = false) {
			Array.Resize(ref Main.tileSetsLoaded, nextTile);
			for (int k = TileID.Count; k < nextTile; k++) {
				Main.tileSetsLoaded[k] = true;
			}
			Array.Resize(ref Main.highlightMaskTexture, nextTile);
			Resize2DArray(ref Main.tileAltTexture, nextTile);
			Resize2DArray(ref Main.tileAltTextureInit, nextTile);
			Resize2DArray(ref Main.tileAltTextureDrawn, nextTile);
			Array.Resize(ref Main.tileTexture, nextTile);
			Array.Resize(ref Main.tileLighted, nextTile);
			Array.Resize(ref Main.tileMergeDirt, nextTile);
			Array.Resize(ref Main.tileCut, nextTile);
			Array.Resize(ref Main.tileAlch, nextTile);
			Array.Resize(ref Main.tileShine, nextTile);
			Array.Resize(ref Main.tileShine2, nextTile);
			Array.Resize(ref Main.tileStone, nextTile);
			Array.Resize(ref Main.tileAxe, nextTile);
			Array.Resize(ref Main.tileHammer, nextTile);
			Array.Resize(ref Main.tileWaterDeath, nextTile);
			Array.Resize(ref Main.tileLavaDeath, nextTile);
			Array.Resize(ref Main.tileTable, nextTile);
			Array.Resize(ref Main.tileBlockLight, nextTile);
			Array.Resize(ref Main.tileNoSunLight, nextTile);
			Array.Resize(ref Main.tileDungeon, nextTile);
			Array.Resize(ref Main.tileSpelunker, nextTile);
			Array.Resize(ref Main.tileSolidTop, nextTile);
			Array.Resize(ref Main.tileSolid, nextTile);
			Array.Resize(ref Main.tileBouncy, nextTile);
			Array.Resize(ref Main.tileValue, nextTile);
			Array.Resize(ref Main.tileLargeFrames, nextTile);
			Array.Resize(ref Main.tileRope, nextTile);
			Array.Resize(ref Main.tileBrick, nextTile);
			Array.Resize(ref Main.tileMoss, nextTile);
			Array.Resize(ref Main.tileNoAttach, nextTile);
			Array.Resize(ref Main.tileNoFail, nextTile);
			Array.Resize(ref Main.tileObsidianKill, nextTile);
			Array.Resize(ref Main.tileFrameImportant, nextTile);
			Array.Resize(ref Main.tilePile, nextTile);
			Array.Resize(ref Main.tileBlendAll, nextTile);
			Array.Resize(ref Main.tileGlowMask, nextTile);
			Array.Resize(ref Main.tileContainer, nextTile);
			Array.Resize(ref Main.tileSign, nextTile);
			Array.Resize(ref Main.tileMerge, nextTile);
			for (int k = 0; k < nextTile; k++) //oh dear
			{
				Array.Resize(ref Main.tileMerge[k], nextTile);
			}
			Array.Resize(ref Main.tileSand, nextTile);
			Array.Resize(ref Main.tileFlame, nextTile);
			Array.Resize(ref Main.tileFrame, nextTile);
			Array.Resize(ref Main.tileFrameCounter, nextTile);
			Array.Resize(ref Main.screenTileCounts, nextTile);
			Array.Resize(ref WorldGen.tileCounts, nextTile);
			Array.Resize(ref WorldGen.houseTile, nextTile);
			Array.Resize(ref GameContent.Biomes.CaveHouseBiome._blacklistedTiles, nextTile);
			Array.Resize(ref GameContent.Biomes.CorruptionPitBiome.ValidTiles, nextTile);
			Array.Resize(ref TileID.Sets.Conversion.Grass, nextTile);
			Array.Resize(ref TileID.Sets.Conversion.Stone, nextTile);
			Array.Resize(ref TileID.Sets.Conversion.Ice, nextTile);
			Array.Resize(ref TileID.Sets.Conversion.Sand, nextTile);
			Array.Resize(ref TileID.Sets.Conversion.HardenedSand, nextTile);
			Array.Resize(ref TileID.Sets.Conversion.Sandstone, nextTile);
			Array.Resize(ref TileID.Sets.Conversion.Thorn, nextTile);
			Array.Resize(ref TileID.Sets.Conversion.Moss, nextTile);
			Array.Resize(ref TileID.Sets.ForAdvancedCollision.ForSandshark, nextTile);
			Array.Resize(ref TileID.Sets.Platforms, nextTile);
			Array.Resize(ref TileID.Sets.DrawsWalls, nextTile);
			Array.Resize(ref TileID.Sets.GemsparkFramingTypes, nextTile);
			Array.Resize(ref TileID.Sets.TeamTiles, nextTile);
			Array.Resize(ref TileID.Sets.ConveyorDirection, nextTile);
			Array.Resize(ref TileID.Sets.HasSlopeFrames, nextTile);
			Array.Resize(ref TileID.Sets.TileInteractRead, nextTile);
			Array.Resize(ref TileID.Sets.HasOutlines, nextTile);
			Array.Resize(ref TileID.Sets.AllTiles, nextTile);
			Array.Resize(ref TileID.Sets.Stone, nextTile);
			Array.Resize(ref TileID.Sets.Grass, nextTile);
			Array.Resize(ref TileID.Sets.Mud, nextTile);
			Array.Resize(ref TileID.Sets.Snow, nextTile);
			Array.Resize(ref TileID.Sets.Ices, nextTile);
			Array.Resize(ref TileID.Sets.IcesSlush, nextTile);
			Array.Resize(ref TileID.Sets.IcesSnow, nextTile);
			Array.Resize(ref TileID.Sets.GrassSpecial, nextTile);
			Array.Resize(ref TileID.Sets.JungleSpecial, nextTile);
			Array.Resize(ref TileID.Sets.HellSpecial, nextTile);
			Array.Resize(ref TileID.Sets.Leaves, nextTile);
			Array.Resize(ref TileID.Sets.GeneralPlacementTiles, nextTile);
			Array.Resize(ref TileID.Sets.BasicChest, nextTile);
			Array.Resize(ref TileID.Sets.BasicChestFake, nextTile);
			Array.Resize(ref TileID.Sets.CanBeClearedDuringGeneration, nextTile);
			Array.Resize(ref TileID.Sets.CanBeClearedDuringOreRunner, nextTile);
			Array.Resize(ref TileID.Sets.Corrupt, nextTile);
			Array.Resize(ref TileID.Sets.Hallow, nextTile);
			Array.Resize(ref TileID.Sets.Crimson, nextTile);
			Array.Resize(ref TileID.Sets.BlocksStairs, nextTile);
			Array.Resize(ref TileID.Sets.BlocksStairsAbove, nextTile);
			Array.Resize(ref TileID.Sets.NotReallySolid, nextTile);
			Array.Resize(ref TileID.Sets.NeedsGrassFraming, nextTile);
			Array.Resize(ref TileID.Sets.NeedsGrassFramingDirt, nextTile);
			Array.Resize(ref TileID.Sets.ChecksForMerge, nextTile);
			Array.Resize(ref TileID.Sets.FramesOnKillWall, nextTile);
			Array.Resize(ref TileID.Sets.AvoidedByNPCs, nextTile);
			Array.Resize(ref TileID.Sets.InteractibleByNPCs, nextTile);
			Array.Resize(ref TileID.Sets.HousingWalls, nextTile);
			Array.Resize(ref TileID.Sets.BreakableWhenPlacing, nextTile);
			Array.Resize(ref TileID.Sets.TouchDamageVines, nextTile);
			Array.Resize(ref TileID.Sets.TouchDamageSands, nextTile);
			Array.Resize(ref TileID.Sets.TouchDamageHot, nextTile);
			Array.Resize(ref TileID.Sets.TouchDamageOther, nextTile);
			Array.Resize(ref TileID.Sets.Falling, nextTile);
			Array.Resize(ref TileID.Sets.Ore, nextTile);
			Array.Resize(ref TileID.Sets.ForceObsidianKill, nextTile);
			for (int k = TileID.Count; k < nextTile; k++) {
				TileID.Sets.AllTiles[k] = true;
				TileID.Sets.GeneralPlacementTiles[k] = true;
				TileID.Sets.CanBeClearedDuringGeneration[k] = true;
			}
			while (TileObjectData._data.Count < nextTile) {
				TileObjectData._data.Add(null);
			}

			ModLoader.BuildGlobalHook(ref HookKillSound, globalTiles, g => g.KillSound);
			ModLoader.BuildGlobalHook(ref HookNumDust, globalTiles, g => g.NumDust);
			ModLoader.BuildGlobalHook(ref HookCreateDust, globalTiles, g => g.CreateDust);
			ModLoader.BuildGlobalHook(ref HookDropCritterChance, globalTiles, g => g.DropCritterChance);
			ModLoader.BuildGlobalHook(ref HookDrop, globalTiles, g => g.Drop);
			ModLoader.BuildGlobalHook(ref HookCanKillTile, globalTiles, g => g.CanKillTile);
			ModLoader.BuildGlobalHook(ref HookKillTile, globalTiles, g => g.KillTile);
			ModLoader.BuildGlobalHook(ref HookCanExplode, globalTiles, g => g.CanExplode);
			ModLoader.BuildGlobalHook(ref HookNearbyEffects, globalTiles, g => g.NearbyEffects);
			ModLoader.BuildGlobalHook(ref HookModifyLight, globalTiles, g => g.ModifyLight);
			ModLoader.BuildGlobalHook(ref HookDangersense, globalTiles, g => g.Dangersense);
			ModLoader.BuildGlobalHook(ref HookSetSpriteEffects, globalTiles, g => g.SetSpriteEffects);
			ModLoader.BuildGlobalHook(ref HookAnimateTile, globalTiles, g => g.AnimateTile);
			ModLoader.BuildGlobalHook(ref HookPreDraw, globalTiles, g => g.PreDraw);
			ModLoader.BuildGlobalHook(ref HookDrawEffects, globalTiles, g => g.DrawEffects);
			ModLoader.BuildGlobalHook(ref HookPostDraw, globalTiles, g => g.PostDraw);
			ModLoader.BuildGlobalHook(ref HookSpecialDraw, globalTiles, g => g.SpecialDraw);
			ModLoader.BuildGlobalHook(ref HookRandomUpdate, globalTiles, g => g.RandomUpdate);
			ModLoader.BuildGlobalHook(ref HookTileFrame, globalTiles, g => g.TileFrame);
			ModLoader.BuildGlobalHook(ref HookCanPlace, globalTiles, g => g.CanPlace);
			ModLoader.BuildGlobalHook(ref HookAdjTiles, globalTiles, g => g.AdjTiles);
			ModLoader.BuildGlobalHook(ref HookRightClick, globalTiles, g => g.RightClick);
			ModLoader.BuildGlobalHook(ref HookMouseOver, globalTiles, g => g.MouseOver);
			ModLoader.BuildGlobalHook(ref HookMouseOverFar, globalTiles, g => g.MouseOverFar);
			ModLoader.BuildGlobalHook(ref HookAutoSelect, globalTiles, g => g.AutoSelect);
			ModLoader.BuildGlobalHook(ref HookPreHitWire, globalTiles, g => g.PreHitWire);
			ModLoader.BuildGlobalHook(ref HookHitWire, globalTiles, g => g.HitWire);
			ModLoader.BuildGlobalHook(ref HookSlope, globalTiles, g => g.Slope);
			ModLoader.BuildGlobalHook(ref HookFloorVisuals, globalTiles, g => g.FloorVisuals);
			ModLoader.BuildGlobalHook(ref HookChangeWaterfallStyle, globalTiles, g => g.ChangeWaterfallStyle);
			ModLoader.BuildGlobalHook(ref HookSaplingGrowthType, globalTiles, g => g.SaplingGrowthType);
			ModLoader.BuildGlobalHook(ref HookPlaceInWorld, globalTiles, g => g.PlaceInWorld);

			if (!unloading) {
				loaded = true;
			}
		}

		internal static void Unload() {
			loaded = false;
			tiles.Clear();
			nextTile = TileID.Count;
			globalTiles.Clear();
			trees.Clear();
			palmTrees.Clear();
			cacti.Clear();
			Array.Resize(ref TileID.Sets.RoomNeeds.CountsAsChair, vanillaChairCount);
			Array.Resize(ref TileID.Sets.RoomNeeds.CountsAsTable, vanillaTableCount);
			Array.Resize(ref TileID.Sets.RoomNeeds.CountsAsTorch, vanillaTorchCount);
			Array.Resize(ref TileID.Sets.RoomNeeds.CountsAsDoor, vanillaDoorCount);
			while (TileObjectData._data.Count > TileID.Count) {
				TileObjectData._data.RemoveAt(TileObjectData._data.Count - 1);
			}
		}
		//in Terraria.WorldGen.TileFrame after if else chain inside frameImportant if statement before return add
		//  else { TileLoader.CheckModTile(i, j, num); }
		//in Terraria.TileObject.CanPlace add optional checkStay parameter as false to end
		//  and add && !checkStay to if statement that sets flag4
		public static void CheckModTile(int i, int j, int type) {
			if (WorldGen.destroyObject) {
				return;
			}
			TileObjectData tileData = TileObjectData.GetTileData(type, 0, 0);
			if (tileData == null) {
				return;
			}
			int frameX = Main.tile[i, j].frameX;
			int frameY = Main.tile[i, j].frameY;
			int subX = frameX / tileData.CoordinateFullWidth;
			int subY = frameY / tileData.CoordinateFullHeight;
			int wrap = tileData.StyleWrapLimit;
			if (wrap == 0) {
				wrap = 1;
			}
			int subTile = tileData.StyleHorizontal ? subY * wrap + subX : subX * wrap + subY;
			int style = subTile / tileData.StyleMultiplier;
			int alternate = subTile % tileData.StyleMultiplier;
			for (int k = 0; k < tileData.AlternatesCount; k++) {
				if (alternate >= tileData.Alternates[k].Style && alternate <= tileData.Alternates[k].Style + tileData.RandomStyleRange) {
					alternate = k;
					break;
				}
			}
			tileData = TileObjectData.GetTileData(type, style, alternate + 1);
			int partFrameX = frameX % tileData.CoordinateFullWidth;
			int partFrameY = frameY % tileData.CoordinateFullHeight;
			int partX = partFrameX / (tileData.CoordinateWidth + tileData.CoordinatePadding);
			int partY = 0;
			int remainingFrameY = partFrameY;
			while (remainingFrameY > 0) {
				remainingFrameY -= tileData.CoordinateHeights[partY] + tileData.CoordinatePadding;
				partY++;
			}
			i -= partX;
			j -= partY;
			int originX = i + tileData.Origin.X;
			int originY = j + tileData.Origin.Y;
			TileObject objectData;
			bool partiallyDestroyed = false;
			for (int x = i; x < i + tileData.Width; x++) {
				for (int y = j; y < j + tileData.Height; y++) {
					if (!Main.tile[x, y].active() || Main.tile[x, y].type != type) {
						partiallyDestroyed = true;
						break;
					}
				}
				if (partiallyDestroyed) {
					break;
				}
			}
			if (partiallyDestroyed || !TileObject.CanPlace(originX, originY, type, style, 0, out objectData, true, true)) {
				WorldGen.destroyObject = true;
				for (int x = i; x < i + tileData.Width; x++) {
					for (int y = j; y < j + tileData.Height; y++) {
						if (Main.tile[x, y].type == type && Main.tile[x, y].active()) {
							WorldGen.KillTile(x, y, false, false, false);
						}
					}
				}
				KillMultiTile(i, j, frameX - partFrameX, frameY - partFrameY, type);
				WorldGen.destroyObject = false;
				for (int x = i - 1; x < i + tileData.Width + 2; x++) {
					for (int y = j - 1; y < j + tileData.Height + 2; y++) {
						WorldGen.TileFrame(x, y, false, false);
					}
				}
			}
			TileObject.objectPreview.Active = false;
		}

		public static void DisableSmartCursor(Tile tile, ref bool disable) {
			if (tile.active()) {
				ModTile modTile = GetTile(tile.type);
				if (modTile != null) {
					disable = modTile.disableSmartCursor;
				}
			}
		}

		public static void DisableSmartInteract(Tile tile, ref bool disable) {
			if (tile.active()) {
				ModTile modTile = GetTile(tile.type);
				if (modTile != null) {
					disable = modTile.disableSmartInteract;
				}
			}
		}
		//in Terraria.WorldGen.OpenDoor replace bad type check with TileLoader.OpenDoorID(Main.tile[i, j]) < 0
		//in Terraria.WorldGen.OpenDoor replace 11 with (ushort)TileLoader.OpenDoorID
		//replace all type checks before WorldGen.OpenDoor
		public static int OpenDoorID(Tile tile) {
			ModTile modTile = GetTile(tile.type);
			if (modTile != null) {
				return modTile.openDoorID;
			}
			if (tile.type == TileID.ClosedDoor && (tile.frameY < 594 || tile.frameY > 646 || tile.frameX >= 54)) {
				return TileID.OpenDoor;
			}
			return -1;
		}
		//in Terraria.WorldGen.CloseDoor replace bad type check with TileLoader.CloseDoorID(Main.tile[i, j]) < 0
		//in Terraria.WorldGen.CloseDoor replace 10 with (ushort)TileLoader.CloseDoorID
		//replace all type checks before WorldGen.CloseDoor
		//replace type check in WorldGen.CheckRoom
		public static int CloseDoorID(Tile tile) {
			ModTile modTile = GetTile(tile.type);
			if (modTile != null) {
				return modTile.closeDoorID;
			}
			if (tile.type == TileID.OpenDoor) {
				return TileID.ClosedDoor;
			}
			return -1;
		}
		public static bool IsClosedDoor(Tile tile) {
			ModTile modTile = GetTile(tile.type);
			if (modTile != null) {
				return modTile.openDoorID > -1;
			}
			return tile.type == TileID.ClosedDoor;
		}
		//in Terraria.UI.ChestUI add this to Lang lookups
		public static string ModChestName(int type) {
			ModTile modTile = GetTile(type);
			if (modTile != null) {
				return modTile.chest;
			}
			return "";
		}

		public static bool IsDresser(int type) {
			if (type == TileID.Dressers) {
				return true;
			}
			return ModDresserName(type).Length > 0;
		}

		public static string ModDresserName(int type) {
			ModTile modTile = GetTile(type);
			if (modTile != null) {
				return modTile.dresser;
			}
			return "";
		}
		//in Terraria.Player.CheckSpawn add this to bed type check
		public static bool IsModBed(int type) {
			ModTile modTile = GetTile(type);
			if (modTile == null) {
				return false;
			}
			return modTile.bed;
		}

		public static bool IsTorch(int type) {
			ModTile modTile = GetTile(type);
			if (modTile == null) {
				return type == TileID.Torches;
			}
			return modTile.torch;
		}

		public static bool IsSapling(int type) {
			ModTile modTile = GetTile(type);
			if (modTile == null) {
				return type == TileID.Saplings;
			}
			return modTile.sapling;
		}

		public static bool IsModMusicBox(Tile tile) {
			return SoundLoader.tileToMusic.ContainsKey(tile.type)
			&& SoundLoader.tileToMusic[tile.type].ContainsKey(tile.frameY / 36 * 36);
		}
		//in Terraria.ObjectData.TileObject data make the following public:
		//  newTile, newSubTile, newAlternate, addTile, addSubTile, addAlternate
		internal static void SetDefaults(ModTile tile) {
			tile.SetDefaults();
			if (TileObjectData.newTile.Width > 1 || TileObjectData.newTile.Height > 1) {
				TileObjectData.FixNewTile();
				throw new Exception("It appears that you have an error surrounding TileObjectData.AddTile in " + tile.GetType().FullName) { HelpLink = "https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-FAQ#tileobjectdataaddtile-issues" };
			}
			if (Main.tileLavaDeath[tile.Type]) {
				Main.tileObsidianKill[tile.Type] = true;
			}
			if (Main.tileSolid[tile.Type]) {
				Main.tileNoSunLight[tile.Type] = true;
			}
			tile.PostSetDefaults();
		}

		public static bool HasSmartInteract(int type) {
			return GetTile(type)?.HasSmartInteract() ?? false;
		}

		public static void FixSmartInteractCoords(int type, ref int width, ref int height, ref int frameWidth, ref int frameHeight, ref int extraX, ref int extraY) {
			ModTile modTile = GetTile(type);
			if (modTile != null) {
				TileObjectData data = TileObjectData.GetTileData(type, 0);
				width = data.Width;
				height = data.Height;
				frameWidth = data.CoordinateWidth + data.CoordinatePadding;
				frameHeight = data.CoordinateHeights[0] + data.CoordinatePadding;
				extraY = data.CoordinateFullHeight % frameHeight;
			}
		}
		//in Terraria.WorldGen.KillTile inside if (!effectOnly && !WorldGen.stopDrops) for playing sounds
		//  add if(!TileLoader.KillSound(i, j, tile.type)) { } to beginning of if/else chain and turn first if into else if
		public static bool KillSound(int i, int j, int type) {
			foreach (var hook in HookKillSound) {
				if (!hook(i, j, type)) {
					return false;
				}
			}
			ModTile modTile = GetTile(type);
			if (modTile != null) {
				if (!modTile.KillSound(i, j)) {
					return false;
				}
				Main.PlaySound(modTile.soundType, i * 16, j * 16, modTile.soundStyle);
				return false;
			}
			return true;
		}
		//in Terraria.WorldGen.KillTile before num14 (num dust iteration) is declared, add
		//  TileLoader.NumDust(i, j, tile.type, ref num13);
		public static void NumDust(int i, int j, int type, bool fail, ref int numDust) {
			GetTile(type)?.NumDust(i, j, fail, ref numDust);

			foreach (var hook in HookNumDust) {
				hook(i, j, type, fail, ref numDust);
			}
		}
		//in Terraria.WorldGen.KillTile replace if (num15 >= 0) with
		//  if(TileLoader.CreateDust(i, j, tile.type, ref num15) && num15 >= 0)
		public static bool CreateDust(int i, int j, int type, ref int dustType) {
			foreach (var hook in HookCreateDust) {
				if (!hook(i, j, type, ref dustType)) {
					return false;
				}
			}
			return GetTile(type)?.CreateDust(i, j, ref dustType) ?? true;
		}
		//in Terraria.WorldGen.KillTile before if statement checking num43 call
		//  TileLoader.DropCritterChance(i, j, tile.type, ref num43, ref num44, ref num45);
		public static void DropCritterChance(int i, int j, int type, ref int wormChance, ref int grassHopperChance, ref int jungleGrubChance) {
			GetTile(type)?.DropCritterChance(i, j, ref wormChance, ref grassHopperChance, ref jungleGrubChance);

			foreach (var hook in HookDropCritterChance) {
				hook(i, j, type, ref wormChance, ref grassHopperChance, ref jungleGrubChance);
			}
		}
		//in Terraria.WorldGen.KillTile before if statements checking num49 and num50
		//  add bool vanillaDrop = TileLoader.Drop(i, j, tile.type);
		//  add "vanillaDrop && " to beginning of these if statements
		public static bool Drop(int i, int j, int type) {
			foreach (var hook in HookDrop) {
				if (!hook(i, j, type)) {
					return false;
				}
			}
			ModTile modTile = GetTile(type);
			if (modTile != null) {
				if (!modTile.Drop(i, j)) {
					return false;
				}
				if (modTile.drop > 0) {
					Item.NewItem(i * 16, j * 16, 16, 16, modTile.drop, 1, false, -1);
				}
				return false;
			}
			return true;
		}
		//in Terraria.WorldGen.CanKillTile after check for tile.active() add
		//  if(!TileLoader.CanKillTile(i, j, tile.type, ref blockDamaged)) { return false; }
		public static bool CanKillTile(int i, int j, int type, ref bool blockDamaged) {
			foreach (var hook in HookCanKillTile) {
				if (!hook(i, j, type, ref blockDamaged)) {
					return false;
				}
			}
			return GetTile(type)?.CanKillTile(i, j, ref blockDamaged) ?? true;
		}
		//in Terraria.WorldGen.KillTile before if (!effectOnly && !WorldGen.stopDrops) add
		//  TileLoader.KillTile(i, j, tile.type, ref fail, ref effectOnly, ref noItem);
		public static void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem) {
			GetTile(type)?.KillTile(i, j, ref fail, ref effectOnly, ref noItem);

			foreach (var hook in HookKillTile) {
				hook(i, j, type, ref fail, ref effectOnly, ref noItem);
			}
		}

		public static void KillMultiTile(int i, int j, int frameX, int frameY, int type) {
			GetTile(type)?.KillMultiTile(i, j, frameX, frameY);
		}

		public static bool CanExplode(int i, int j) {
			int type = Main.tile[i, j].type;
			ModTile modTile = GetTile(type);
			if (modTile != null && !modTile.CanExplode(i, j)) {
				return false;
			}
			foreach (var hook in HookCanExplode) {
				if (!hook(i, j, type)) {
					return false;
				}
			}
			return true;
		}
		//in Terraria.Lighting.PreRenderPhase add local closer variable and after setting music box
		//  call TileLoader.NearbyEffects(n, num17, type, closer);
		public static void NearbyEffects(int i, int j, int type, bool closer) {
			GetTile(type)?.NearbyEffects(i, j, closer);

			foreach (var hook in HookNearbyEffects) {
				hook(i, j, type, closer);
			}
		}
		//in Terraria.Lighting.PreRenderPhase after label after if statement checking Main.tileLighted call
		//  TileLoader.ModifyLight(n, num17, tile.type, ref num18, ref num19, ref num20);
		public static void ModifyLight(int i, int j, int type, ref float r, ref float g, ref float b) {
			if (!Main.tileLighted[type]) {
				return;
			}
			GetTile(type)?.ModifyLight(i, j, ref r, ref g, ref b);

			foreach (var hook in HookModifyLight) {
				hook(i, j, type, ref r, ref g, ref b);
			}
		}

		public static bool Dangersense(int i, int j, int type, Player player) {
			ModTile modTile = GetTile(type);
			if (modTile != null && modTile.Dangersense(i, j, player)) {
				return true;
			}
			foreach (var hook in HookDangersense) {
				if (hook(i, j, type, player)) {
					return true;
				}
			}
			return false;
		}
		//in Terraria.Main.DrawTiles after if statement setting effects call
		//  TileLoader.SetSpriteEffects(j, i, type, ref effects);
		public static void SetSpriteEffects(int i, int j, int type, ref SpriteEffects spriteEffects) {
			GetTile(type)?.SetSpriteEffects(i, j, ref spriteEffects);

			foreach (var hook in HookSetSpriteEffects) {
				hook(i, j, type, ref spriteEffects);
			}
		}
		//in Terraria.Main.DrawTiles after if statements setting num11 and num12 call
		//  TileLoader.SetDrawPositions(j, i, ref num9, ref num11, ref num12);
		public static void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height) {
			Tile tile = Main.tile[i, j];
			if (tile.type >= TileID.Count) {
				TileObjectData tileData = TileObjectData.GetTileData(tile.type, 0, 0);
				if (tileData != null) {
					int partFrameY = tile.frameY % tileData.CoordinateFullHeight;
					int partY = 0;
					while (partFrameY > 0) {
						partFrameY -= tileData.CoordinateHeights[partY] + tileData.CoordinatePadding;
						partY++;
					}
					width = tileData.CoordinateWidth;
					offsetY = tileData.DrawYOffset;
					height = tileData.CoordinateHeights[partY];
				}
				GetTile(tile.type).SetDrawPositions(i, j, ref width, ref offsetY, ref height);
			}
		}
		//in Terraria.Main.Update after vanilla tile animations call TileLoader.AnimateTiles();
		public static void AnimateTiles() {
			if (loaded) {
				for (int i = 0; i < tiles.Count; i++) {
					ModTile modTile = tiles[i];
					modTile.AnimateTile(ref Main.tileFrame[modTile.Type], ref Main.tileFrameCounter[modTile.Type]);
				}
				foreach (var hook in HookAnimateTile) {
					hook();
				}
			}
		}

		//in Terraria.Main.Draw after small if statements setting num15 call
		//  TileLoader.SetAnimationFrame(type, ref num15);
		/// <summary>
		/// Sets the animation frame. Sets frameYOffset = modTile.animationFrameHeight * Main.tileFrame[type]; and then calls ModTile.AnimateIndividualTile
		/// </summary>
		/// <param name="type">The tile type.</param>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		/// <param name="frameXOffset">The offset to frameX.</param>
		/// <param name="frameYOffset">The offset to frameY.</param>
		public static void SetAnimationFrame(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
			ModTile modTile = GetTile(type);
			if (modTile != null) {
				frameYOffset = modTile.animationFrameHeight * Main.tileFrame[type];
				modTile.AnimateIndividualTile(type, i, j, ref frameXOffset, ref frameYOffset);
			}
		}

		//in Terraria.Main.Draw after calling SetAnimationFrame call
		//  if(!TileLoader.PreDraw(j, i, type, Main.spriteBatch))
		//  { TileLoader.PostDraw(j, i, type, Main.spriteBatch); continue; }
		public static bool PreDraw(int i, int j, int type, SpriteBatch spriteBatch) {
			foreach (var hook in HookPreDraw) {
				if (!hook(i, j, type, spriteBatch)) {
					return false;
				}
			}
			return GetTile(type)?.PreDraw(i, j, spriteBatch) ?? true;
		}

		public static void DrawEffects(int i, int j, int type, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex) {
			GetTile(type)?.DrawEffects(i, j, spriteBatch, ref drawColor, ref nextSpecialDrawIndex);
			foreach (var hook in HookDrawEffects) {
				hook(i, j, type, spriteBatch, ref drawColor, ref nextSpecialDrawIndex);
			}
		}
		//in Terraria.Main.Draw after if statement checking whether texture2D is null call
		//  TileLoader.PostDraw(j, i, type, Main.spriteBatch);
		public static void PostDraw(int i, int j, int type, SpriteBatch spriteBatch) {
			GetTile(type)?.PostDraw(i, j, spriteBatch);

			foreach (var hook in HookPostDraw) {
				hook(i, j, type, spriteBatch);
			}
		}

		/// <summary>
		/// Special Draw calls ModTile and GlobalTile SpecialDraw methods. Special Draw is called from DrawTiles after the draw loop, allowing for basically another layer above tiles.  Main.specX and Main.specY are used to specify tiles to call SpecialDraw on. Use DrawEffects hook to queue for SpecialDraw. 
		/// </summary>
		public static void SpecialDraw(int type, int specX, int specY, SpriteBatch spriteBatch) {
			GetTile(type)?.SpecialDraw(specX, specY, spriteBatch);

			foreach (var hook in HookSpecialDraw) {
				hook(specX, specY, type, spriteBatch);
			}
		}

		//in Terraria.WorldGen.UpdateWorld in the while loops updating certain numbers of tiles at end of null check if statements
		//  add TileLoader.RandomUpdate(num7, num8, Main.tile[num7, num8].type; for the first loop
		//  add TileLoader.RandomUpdate(num64, num65, Main.tile[num64, num65].type; for the second loop
		public static void RandomUpdate(int i, int j, int type) {
			if (!Main.tile[i, j].active()) {
				return;
			}
			GetTile(type)?.RandomUpdate(i, j);

			foreach (var hook in HookRandomUpdate) {
				hook(i, j, type);
			}
		}
		//in Terraria.WorldGen.TileFrame at beginning of block of if(tile.active()) add
		//  if(!TileLoader.TileFrame(i, j, tile.type, ref resetFrame, ref noBreak)) { return; }
		public static bool TileFrame(int i, int j, int type, ref bool resetFrame, ref bool noBreak) {
			ModTile modTile = GetTile(type);
			bool flag = true;
			if (modTile != null) {
				flag = modTile.TileFrame(i, j, ref resetFrame, ref noBreak);
			}
			foreach (var hook in HookTileFrame) {
				flag &= hook(i, j, type, ref resetFrame, ref noBreak);
			}
			return flag;
		}
		//in Terraria.Player.ItemCheck in if statements for mining
		//  replace num222 += item.hammer; with TileLoader.MineDamage(item.hammer, ref num222);
		//  replace num222 += item.axe; with TileLoader.MineDamage(item.axe, ref num222);
		//in Terraria.Player.PickTile replace num += pickPower; with TileLoader.MineDamage(pickPower, ref num);
		public static void MineDamage(int minePower, ref int damage) {
			Tile target = Main.tile[Player.tileTargetX, Player.tileTargetY];
			ModTile modTile = GetTile(target.type);
			damage += modTile != null ? (int)(minePower / modTile.mineResist) : minePower;
		}
		//in Terraria.Player.ItemCheck at end of else if chain setting num to 0 add
		//  else { TileLoader.PickPowerCheck(tile, pickPower, ref num); }
		public static void PickPowerCheck(Tile target, int pickPower, ref int damage) {
			ModTile modTile = GetTile(target.type);
			if (modTile != null && pickPower < modTile.minPick) {
				damage = 0;
			}
		}
		//in Terraria.Player.PlaceThing after tileObject is initalized add else to if statement and before add
		//  if(!TileLoader.CanPlace(Player.tileTargetX, Player.tileTargetY)) { }
		public static bool CanPlace(int i, int j, int type) {
			foreach (var hook in HookCanPlace) {
				if (!hook(i, j, type)) {
					return false;
				}
			}
			return GetTile(type)?.CanPlace(i, j) ?? true;
		}
		//in Terraria.Player.AdjTiles in end of if statement checking for tile's active
		//  add TileLoader.AdjTiles(this, Main.tile[j, k].type);
		public static void AdjTiles(Player player, int type) {
			ModTile modTile = GetTile(type);
			if (modTile != null) {
				foreach (int k in modTile.adjTiles) {
					player.adjTile[k] = true;
				}
			}
			foreach (var hook in HookAdjTiles) {
				int[] adjTiles = hook(type);
				foreach (int k in adjTiles) {
					player.adjTile[k] = true;
				}
			}
		}
		//in Terraria.Player.Update in if statements involving controluseTile and releaseUseTile
		//  at end of type-check if else chain add TileLoader.RightClick(Player.tileTargetX, Player.tileTargetY);
		public static bool RightClick(int i, int j) {
			bool returnValue = false;
			int type = Main.tile[i, j].type;
			GetTile(type)?.RightClick(i, j);
			if (GetTile(type)?.NewRightClick(i, j) ?? false)
				returnValue = true;

			foreach (var hook in HookRightClick) {
				hook(i, j, type);
			}
			return returnValue;
		}
		//in Terraria.Player.Update after if statements setting showItemIcon call
		//  TileLoader.MouseOver(Player.tileTargetX, Player.tileTargetY);
		public static void MouseOver(int i, int j) {
			int type = Main.tile[i, j].type;
			GetTile(type)?.MouseOver(i, j);

			foreach (var hook in HookMouseOver) {
				hook(i, j, type);
			}
		}

		public static void MouseOverFar(int i, int j) {
			int type = Main.tile[i, j].type;
			GetTile(type)?.MouseOverFar(i, j);

			foreach (var hook in HookMouseOverFar) {
				hook(i, j, type);
			}
		}

		public static int AutoSelect(int i, int j, Player player) {
			if (!Main.tile[i, j].active()) {
				return -1;
			}
			int type = Main.tile[i, j].type;
			ModTile modTile = GetTile(type);
			for (int k = 0; k < 50; k++) {
				Item item = player.inventory[k];
				if (item.type == 0 || item.stack == 0) {
					continue;
				}
				if (modTile != null && modTile.AutoSelect(i, j, item)) {
					return k;
				}
				foreach (var hook in HookAutoSelect) {
					if (hook(i, j, type, item)) {
						return k;
					}
				}
			}
			return -1;
		}

		public static bool PreHitWire(int i, int j, int type) {
			foreach (var hook in HookPreHitWire) {
				if (!hook(i, j, type)) {
					return false;
				}
			}
			return true;
		}
		//in Terraria.Wiring make the following public:
		//  _wireList, _toProcess, _teleport, _inPumpX, _inPumpY, _numInPump, _outPumpX, _outPumpY, _numOutPump CheckMech, TripWire
		//at end of Terraria.Wiring.HitWireSingle inside if statement checking for tile active add
		//  TileLoader.HitWire(i, j, type);
		public static void HitWire(int i, int j, int type) {
			GetTile(type)?.HitWire(i, j);

			foreach (var hook in HookHitWire) {
				hook(i, j, type);
			}
		}

		public static void FloorVisuals(int type, Player player) {
			GetTile(type)?.FloorVisuals(player);

			foreach (var hook in HookFloorVisuals) {
				hook(type, player);
			}
		}
		//in Terraria.Player.ItemCheck in poundRelease if statement before sloping if statements add
		//  if(TileLoader.Slope(num223, num224, Main.tile[num223, num224].type)) { } else
		public static bool Slope(int i, int j, int type) {
			foreach (var hook in HookSlope) {
				if (!hook(i, j, type)) {
					return true;
				}
			}
			return !GetTile(type)?.Slope(i, j) ?? false;
		}

		public static bool HasWalkDust(int type) {
			return GetTile(type)?.HasWalkDust() ?? false;
		}

		public static void WalkDust(int type, ref int dustType, ref bool makeDust, ref Color color) {
			GetTile(type)?.WalkDust(ref dustType, ref makeDust, ref color);
		}

		public static void ChangeWaterfallStyle(int type, ref int style) {
			GetTile(type)?.ChangeWaterfallStyle(ref style);
			foreach (var hook in HookChangeWaterfallStyle) {
				hook(type, ref style);
			}
		}

		public static bool SaplingGrowthType(int type, ref int saplingType, ref int style) {
			int originalType = saplingType;
			int originalStyle = style;
			bool flag = false;
			ModTile modTile = GetTile(type);
			if (modTile != null) {
				saplingType = modTile.SaplingGrowthType(ref style);
				if (IsSapling(saplingType)) {
					originalType = saplingType;
					originalStyle = style;
					flag = true;
				}
				else {
					saplingType = originalType;
					style = originalStyle;
				}
			}
			foreach (var hook in HookSaplingGrowthType) {
				saplingType = hook(type, ref style);
				if (IsSapling(saplingType)) {
					originalType = saplingType;
					originalStyle = style;
					flag = true;
				}
				else {
					saplingType = originalType;
					style = originalStyle;
				}
			}
			return flag;
		}

		public static bool CanGrowModTree(int type) {
			return trees.ContainsKey(type);
		}

		public static void TreeDust(Tile tile, ref int dust) {
			if (tile.active() && trees.ContainsKey(tile.type)) {
				dust = trees[tile.type].CreateDust();
			}
		}

		public static void TreeGrowthFXGore(int type, ref int gore) {
			if (trees.ContainsKey(type)) {
				gore = trees[type].GrowthFXGore();
			}
		}

		public static bool CanDropAcorn(int type) {
			return trees.ContainsKey(type) ? trees[type].CanDropAcorn() : false;
		}

		public static void DropTreeWood(int type, ref int wood) {
			if (trees.ContainsKey(type)) {
				wood = trees[type].DropWood();
			}
		}

		public static Texture2D GetTreeTexture(Tile tile) {
			return tile.active() && trees.ContainsKey(tile.type) ? trees[tile.type].GetTexture() : null;
		}

		public static Texture2D GetTreeTopTextures(int type, int i, int j, ref int frame,
			ref int frameWidth, ref int frameHeight, ref int xOffsetLeft, ref int yOffset) {
			return trees.ContainsKey(type) ? trees[type].GetTopTextures(i, j, ref frame,
				ref frameWidth, ref frameHeight, ref xOffsetLeft, ref yOffset) : null;
		}

		public static Texture2D GetTreeBranchTextures(int type, int i, int j, int trunkOffset, ref int frame) {
			return trees.ContainsKey(type) ? trees[type].GetBranchTextures(i, j, trunkOffset, ref frame) : null;
		}

		public static bool CanGrowModPalmTree(int type) {
			return palmTrees.ContainsKey(type);
		}

		public static void PalmTreeDust(Tile tile, ref int dust) {
			if (tile.active() && palmTrees.ContainsKey(tile.type)) {
				dust = palmTrees[tile.type].CreateDust();
			}
		}

		public static void PalmTreeGrowthFXGore(int type, ref int gore) {
			if (palmTrees.ContainsKey(type)) {
				gore = palmTrees[type].GrowthFXGore();
			}
		}

		public static void DropPalmTreeWood(int type, ref int wood) {
			if (palmTrees.ContainsKey(type)) {
				wood = palmTrees[type].DropWood();
			}
		}

		public static Texture2D GetPalmTreeTexture(Tile tile) {
			return tile.active() && palmTrees.ContainsKey(tile.type) ? palmTrees[tile.type].GetTexture() : null;
		}

		public static Texture2D GetPalmTreeTopTextures(int type) {
			return palmTrees.ContainsKey(type) ? palmTrees[type].GetTopTextures() : null;
		}

		public static bool CanGrowModCactus(int type) {
			return cacti.ContainsKey(type);
		}

		public static Texture2D GetCactusTexture(int type) {
			return cacti.ContainsKey(type) ? cacti[type].GetTexture() : null;
		}

		public static void PlaceInWorld(int i, int j, Item item) {
			int type = item.createTile;
			if (type < 0)
				return;

			foreach (var hook in HookPlaceInWorld) {
				hook(i, j, item);
			}

			GetTile(type)?.PlaceInWorld(i, j, item);
		}

		public static bool IsLockedChest(int i, int j, int type) {
			return GetTile(type)?.IsLockedChest(i, j) ?? false;
		}

		public static bool UnlockChest(int i, int j, int type, ref short frameXAdjustment, ref int dustType, ref bool manual) {
			return GetTile(type)?.UnlockChest(i, j, ref frameXAdjustment, ref dustType, ref manual) ?? false;
		}
	}
}
