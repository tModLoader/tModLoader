using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Biomes.CaveHouse;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.IO;
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
		private static bool loaded = false;
		private static readonly int vanillaChairCount = TileID.Sets.RoomNeeds.CountsAsChair.Length;
		private static readonly int vanillaTableCount = TileID.Sets.RoomNeeds.CountsAsTable.Length;
		private static readonly int vanillaTorchCount = TileID.Sets.RoomNeeds.CountsAsTorch.Length;
		private static readonly int vanillaDoorCount = TileID.Sets.RoomNeeds.CountsAsDoor.Length;

		private static Func<int, int, int, bool, bool>[] HookKillSound;
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
		private static Func<int, int, int, Player, bool?>[] HookIsTileDangerous;
		private static Func<int, int, int, bool?>[] HookIsTileSpelunkable;
		private delegate void DelegateSetSpriteEffects(int i, int j, int type, ref SpriteEffects spriteEffects);
		private static DelegateSetSpriteEffects[] HookSetSpriteEffects;
		private static Action[] HookAnimateTile;
		private static Func<int, int, int, SpriteBatch, bool>[] HookPreDraw;
		private delegate void DelegateDrawEffects(int i, int j, int type, SpriteBatch spriteBatch, ref TileDrawInfo drawData);
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
		private static Action<int, int, int, Item>[] HookPlaceInWorld;

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
			//Textures
			Array.Resize(ref TextureAssets.Tile, nextTile);
			Array.Resize(ref TextureAssets.HighlightMask, nextTile);

			//Sets
			LoaderUtils.ResetStaticMembers(typeof(TileID), true);

			//Etc
			Array.Resize(ref Main.SceneMetrics._tileCounts, nextTile);
			Array.Resize(ref Main.PylonSystem._sceneMetrics._tileCounts, nextTile);
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
			Array.Resize(ref Main.tileContainer, nextTile);
			Array.Resize(ref Main.tileSign, nextTile);
			Array.Resize(ref Main.tileSand, nextTile);
			Array.Resize(ref Main.tileFlame, nextTile);
			Array.Resize(ref Main.tileFrame, nextTile);
			Array.Resize(ref Main.tileFrameCounter, nextTile);
			Array.Resize(ref Main.tileMerge, nextTile);
			Array.Resize(ref Main.tileOreFinderPriority, nextTile);
			Array.Resize(ref Main.tileGlowMask, nextTile);
			Array.Resize(ref Main.tileCracked, nextTile);

			Array.Resize(ref WorldGen.tileCounts, nextTile);
			Array.Resize(ref WorldGen.houseTile, nextTile);
			//Array.Resize(ref GameContent.Biomes.CaveHouseBiome._blacklistedTiles, nextTile);
			Array.Resize(ref GameContent.Biomes.CorruptionPitBiome.ValidTiles, nextTile);
			Array.Resize(ref GameContent.Metadata.TileMaterials.MaterialsByTileId, nextTile);
			Array.Resize(ref HouseUtils.BlacklistedTiles, nextTile);
			Array.Resize(ref HouseUtils.BeelistedTiles, nextTile);

			for (int i = 0; i < nextTile; i++) { //oh dear
				Array.Resize(ref Main.tileMerge[i], nextTile);
			}

			for (int i = TileID.Count; i < nextTile; i++) {
				Main.tileGlowMask[i] = -1; //If we don't this, every modded tile will have a glowmask by default.
				GameContent.Metadata.TileMaterials.MaterialsByTileId[i] = GameContent.Metadata.TileMaterials._materialsByName["Default"]; //Set this so golf balls know how to interact with modded tiles physics-wise. If not set, then golf balls vanish when touching modded tiles.
			}

			while (TileObjectData._data.Count < nextTile) {
				TileObjectData._data.Add(null);
			}

			//Hooks

			// .NET 6 SDK bug: https://github.com/dotnet/roslyn/issues/57517
			// Remove generic arguments once fixed.
			ModLoader.BuildGlobalHook(ref HookKillSound, globalTiles, g => g.KillSound);
			ModLoader.BuildGlobalHook<GlobalTile, DelegateNumDust>(ref HookNumDust, globalTiles, g => g.NumDust);
			ModLoader.BuildGlobalHook<GlobalTile, DelegateCreateDust>(ref HookCreateDust, globalTiles, g => g.CreateDust);
			ModLoader.BuildGlobalHook<GlobalTile, DelegateDropCritterChance>(ref HookDropCritterChance, globalTiles, g => g.DropCritterChance);
			ModLoader.BuildGlobalHook(ref HookDrop, globalTiles, g => g.Drop);
			ModLoader.BuildGlobalHook<GlobalTile, DelegateCanKillTile>(ref HookCanKillTile, globalTiles, g => g.CanKillTile);
			ModLoader.BuildGlobalHook<GlobalTile, DelegateKillTile>(ref HookKillTile, globalTiles, g => g.KillTile);
			ModLoader.BuildGlobalHook(ref HookCanExplode, globalTiles, g => g.CanExplode);
			ModLoader.BuildGlobalHook(ref HookNearbyEffects, globalTiles, g => g.NearbyEffects);
			ModLoader.BuildGlobalHook<GlobalTile, DelegateModifyLight>(ref HookModifyLight, globalTiles, g => g.ModifyLight);
			ModLoader.BuildGlobalHook(ref HookIsTileDangerous, globalTiles, g => g.IsTileDangerous);
			ModLoader.BuildGlobalHook(ref HookIsTileSpelunkable, globalTiles, g => g.IsTileSpelunkable);
			ModLoader.BuildGlobalHook<GlobalTile, DelegateSetSpriteEffects>(ref HookSetSpriteEffects, globalTiles, g => g.SetSpriteEffects);
			ModLoader.BuildGlobalHook(ref HookAnimateTile, globalTiles, g => g.AnimateTile);
			ModLoader.BuildGlobalHook(ref HookPreDraw, globalTiles, g => g.PreDraw);
			ModLoader.BuildGlobalHook<GlobalTile, DelegateDrawEffects>(ref HookDrawEffects, globalTiles, g => g.DrawEffects);
			ModLoader.BuildGlobalHook(ref HookPostDraw, globalTiles, g => g.PostDraw);
			ModLoader.BuildGlobalHook(ref HookSpecialDraw, globalTiles, g => g.SpecialDraw);
			ModLoader.BuildGlobalHook(ref HookRandomUpdate, globalTiles, g => g.RandomUpdate);
			ModLoader.BuildGlobalHook<GlobalTile, DelegateTileFrame>(ref HookTileFrame, globalTiles, g => g.TileFrame);
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
			ModLoader.BuildGlobalHook<GlobalTile, DelegateChangeWaterfallStyle>(ref HookChangeWaterfallStyle, globalTiles, g => g.ChangeWaterfallStyle);
			ModLoader.BuildGlobalHook(ref HookPlaceInWorld, globalTiles, g => g.PlaceInWorld);

			if (!unloading) {
				loaded = true;
			}
		}

		internal static void Unload() {
			loaded = false;
			nextTile = TileID.Count;

			tiles.Clear();
			globalTiles.Clear();

			// Has to be ran on the main thread, since this may dispose textures.
			Main.QueueMainThreadAction(() => {
				Main.instance.TilePaintSystem.Reset();
			});

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
			for (int remainingFrameY = partFrameY; partY < tileData.Height && remainingFrameY - tileData.CoordinateHeights[partY] + tileData.CoordinatePadding >= 0; partY++) {
				remainingFrameY -= tileData.CoordinateHeights[partY] + tileData.CoordinatePadding;
			}
			i -= partX;
			j -= partY;
			int originX = i + tileData.Origin.X;
			int originY = j + tileData.Origin.Y;
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
			if (partiallyDestroyed || !TileObject.CanPlace(originX, originY, type, style, 0, out TileObject objectData, true, true)) {
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

		//in Terraria.WorldGen.OpenDoor replace bad type check with TileLoader.OpenDoorID(Main.tile[i, j]) < 0
		//in Terraria.WorldGen.OpenDoor replace 11 with (ushort)TileLoader.OpenDoorID
		//replace all type checks before WorldGen.OpenDoor
		public static int OpenDoorID(Tile tile) {
			ModTile modTile = GetTile(tile.type);
			if (modTile != null) {
				return modTile.OpenDoorID;
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
				return modTile.CloseDoorID;
			}

			if (tile.type == TileID.OpenDoor) {
				return TileID.ClosedDoor;
			}

			return -1;
		}

		/// <summary>
		/// Returns true if the tile is a vanilla or modded closed door.
		/// </summary>
		public static bool IsClosedDoor(Tile tile) {
			ModTile modTile = GetTile(tile.type);

			if (modTile != null) {
				return modTile.OpenDoorID > -1;
			}

			return tile.type == TileID.ClosedDoor;
		}

		public static string ContainerName(int type) => GetTile(type)?.ContainerName?.GetTranslation(Language.ActiveCulture) ?? string.Empty;

		public static bool IsModMusicBox(Tile tile) {
			return MusicLoader.tileToMusic.ContainsKey(tile.type)
			&& MusicLoader.tileToMusic[tile.type].ContainsKey(tile.frameY / 36 * 36);
		}

		public static bool HasSmartInteract(int i, int j, int type, SmartInteractScanSettings settings) {
			return GetTile(type)?.HasSmartInteract(i, j, settings) ?? false;
		}

		public static void ModifySmartInteractCoords(int type, ref int width, ref int height, ref int frameWidth, ref int frameHeight, ref int extraY) {
			ModTile modTile = GetTile(type);
			if (modTile == null)
				return;

			TileObjectData data = TileObjectData.GetTileData(type, 0);
			if (data == null)
				return;

			width = data.Width;
			height = data.Height;
			frameWidth = data.CoordinateWidth + data.CoordinatePadding;
			frameHeight = data.CoordinateHeights[0] + data.CoordinatePadding;
			extraY = data.CoordinateFullHeight % frameHeight;

			modTile.ModifySmartInteractCoords(ref width, ref height, ref frameWidth, ref frameHeight, ref extraY);
		}

		public static void ModifySittingTargetInfo(int i, int j, int type, ref TileRestingInfo info) {
			ModTile modTile = GetTile(type);
			if (modTile != null) {
				modTile.ModifySittingTargetInfo(i, j, ref info);
			}
			else {
				info.AnchorTilePosition.Y += 1; // Hardcoded vanilla offset from the bottom tile moved here (all chairs have height-1 offset)
			}
		}

		public static void ModifySleepingTargetInfo(int i, int j, int type, ref TileRestingInfo info) {
			ModTile modTile = GetTile(type);
			if (modTile != null) {
				// Because vanilla sets its own offset based on frameY, ignoring tile type, which might not be set to an expected default, reassign it
				info.VisualOffset = new Vector2(-9f, 1f); // Taken from default case of vanilla beds 
				modTile.ModifySleepingTargetInfo(i, j, ref info);
			}
		}

		public static bool KillSound(int i, int j, int type, bool fail) {
			foreach (var hook in HookKillSound) {
				if (!hook(i, j, type, fail))
					return false;
			}
			
			var modTile = GetTile(type);

			if (modTile != null) {
				if (!modTile.KillSound(i, j, fail))
					return false;
				
				SoundEngine.PlaySound(modTile.HitSound, new Vector2(i * 16, j * 16));
				
				return false;
			}
			
			return true;
		}
		
		public static void NumDust(int i, int j, int type, bool fail, ref int numDust) {
			GetTile(type)?.NumDust(i, j, fail, ref numDust);

			foreach (var hook in HookNumDust) {
				hook(i, j, type, fail, ref numDust);
			}
		}
		
		public static bool CreateDust(int i, int j, int type, ref int dustType) {
			foreach (var hook in HookCreateDust) {
				if (!hook(i, j, type, ref dustType)) {
					return false;
				}
			}
			return GetTile(type)?.CreateDust(i, j, ref dustType) ?? true;
		}
		
		public static void DropCritterChance(int i, int j, int type, ref int wormChance, ref int grassHopperChance, ref int jungleGrubChance) {
			GetTile(type)?.DropCritterChance(i, j, ref wormChance, ref grassHopperChance, ref jungleGrubChance);

			foreach (var hook in HookDropCritterChance) {
				hook(i, j, type, ref wormChance, ref grassHopperChance, ref jungleGrubChance);
			}
		}
		
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

				if (modTile.ItemDrop > 0) {
					Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, modTile.ItemDrop, 1, false, -1);
				}

				return false;
			}

			return true;
		}
		
		public static bool CanKillTile(int i, int j, int type, ref bool blockDamaged) {
			foreach (var hook in HookCanKillTile) {
				if (!hook(i, j, type, ref blockDamaged)) {
					return false;
				}
			}
			return GetTile(type)?.CanKillTile(i, j, ref blockDamaged) ?? true;
		}
		
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

		public static void NearbyEffects(int i, int j, int type, bool closer) {
			GetTile(type)?.NearbyEffects(i, j, closer);

			foreach (var hook in HookNearbyEffects) {
				hook(i, j, type, closer);
			}
		}

		public static void ModifyTorchLuck(Player player, ref float positiveLuck, ref float negativeLuck) {
			foreach (int type in player.NearbyModTorch) {
				float f = GetTile(type).GetTorchLuck(player);
				if (f > 0)
					positiveLuck += f;
				else
					negativeLuck += -f;
			}
		}

		public static void ModifyLight(int i, int j, int type, ref float r, ref float g, ref float b) {
			if (!Main.tileLighted[type]) {
				return;
			}
			GetTile(type)?.ModifyLight(i, j, ref r, ref g, ref b);

			foreach (var hook in HookModifyLight) {
				hook(i, j, type, ref r, ref g, ref b);
			}
		}

		public static bool? IsTileDangerous(int i, int j, int type, Player player) {
			bool? retVal = null;

			ModTile modTile = GetTile(type);

			if (modTile != null && modTile.IsTileDangerous(i, j, player)) {
				retVal = true;
			}

			foreach (var hook in HookIsTileDangerous) {
				bool? globalRetVal = hook(i, j, type, player);
				if (globalRetVal.HasValue) {
					if (globalRetVal.Value) {
						retVal = true;
					}
					else {
						return false;
					}
				}
			}

			return retVal;
		}

		public static bool? IsTileSpelunkable(int i, int j, int type) {
			bool? retVal = null;

			ModTile modTile = GetTile(type);

			if (!Main.tileSpelunker[type] && modTile != null && modTile.IsTileSpelunkable(i, j)) {
				retVal = true;
			}

			foreach (var hook in HookIsTileSpelunkable) {
				bool? globalRetVal = hook(i, j, type);
				if (globalRetVal.HasValue) {
					if (globalRetVal.Value) {
						retVal = true;
					}
					else {
						return false;
					}
				}
			}

			return retVal;
		}

		public static void SetSpriteEffects(int i, int j, int type, ref SpriteEffects spriteEffects) {
			GetTile(type)?.SetSpriteEffects(i, j, ref spriteEffects);

			foreach (var hook in HookSetSpriteEffects) {
				hook(i, j, type, ref spriteEffects);
			}
		}

		public static void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			Tile tile = Main.tile[i, j];
			if (tile.type >= TileID.Count) {
				TileObjectData tileData = TileObjectData.GetTileData(tile.type, 0, 0);
				if (tileData != null) {
					int partY = 0;
					for (int remainingFrameY = tile.frameY % tileData.CoordinateFullHeight; partY < tileData.Height && remainingFrameY - tileData.CoordinateHeights[partY] + tileData.CoordinatePadding >= 0; partY++) {
						remainingFrameY -= tileData.CoordinateHeights[partY] + tileData.CoordinatePadding;
					}
					width = tileData.CoordinateWidth;
					offsetY = tileData.DrawYOffset;
					height = tileData.CoordinateHeights[partY];
				}
				GetTile(tile.type).SetDrawPositions(i, j, ref width, ref offsetY, ref height, ref tileFrameX, ref tileFrameY);
			}
		}
		
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
				frameYOffset = modTile.AnimationFrameHeight * Main.tileFrame[type];
				modTile.AnimateIndividualTile(type, i, j, ref frameXOffset, ref frameYOffset);
			}
		}

		public static bool PreDraw(int i, int j, int type, SpriteBatch spriteBatch) {
			foreach (var hook in HookPreDraw) {
				if (!hook(i, j, type, spriteBatch)) {
					return false;
				}
			}
			return GetTile(type)?.PreDraw(i, j, spriteBatch) ?? true;
		}

		public static void DrawEffects(int i, int j, int type, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			GetTile(type)?.DrawEffects(i, j, spriteBatch, ref drawData);
			foreach (var hook in HookDrawEffects) {
				hook(i, j, type, spriteBatch, ref drawData);
			}
		}

		public static void PostDraw(int i, int j, int type, SpriteBatch spriteBatch) {
			GetTile(type)?.PostDraw(i, j, spriteBatch);

			foreach (var hook in HookPostDraw) {
				hook(i, j, type, spriteBatch);
			}
		}

		/// <summary>
		/// Special Draw calls ModTile and GlobalTile SpecialDraw methods. Special Draw is called at the end of the DrawSpecialTilesLegacy loop, allowing for basically another layer above tiles. Use DrawEffects hook to queue for SpecialDraw.
		/// </summary>
		public static void SpecialDraw(int type, int specialTileX, int specialTileY, SpriteBatch spriteBatch) {
			GetTile(type)?.SpecialDraw(specialTileX, specialTileY, spriteBatch);

			foreach (var hook in HookSpecialDraw) {
				hook(specialTileX, specialTileY, type, spriteBatch);
			}
		}

		public static void RandomUpdate(int i, int j, int type) {
			if (!Main.tile[i, j].active()) {
				return;
			}
			GetTile(type)?.RandomUpdate(i, j);

			foreach (var hook in HookRandomUpdate) {
				hook(i, j, type);
			}
		}

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

		public static void MineDamage(int minePower, ref int damage) {
			Tile target = Main.tile[Player.tileTargetX, Player.tileTargetY];
			ModTile modTile = GetTile(target.type);
			damage += modTile != null ? (int)(1.2f * minePower / modTile.MineResist) : (int)(1.2f * minePower);
		}

		public static void PickPowerCheck(Tile target, int pickPower, ref int damage) {
			ModTile modTile = GetTile(target.type);
			if (modTile != null && pickPower < modTile.MinPick) {
				damage = 0;
			}
		}

		public static bool CanPlace(int i, int j, int type) {
			foreach (var hook in HookCanPlace) {
				if (!hook(i, j, type)) {
					return false;
				}
			}
			return GetTile(type)?.CanPlace(i, j) ?? true;
		}

		public static void AdjTiles(Player player, int type) {
			ModTile modTile = GetTile(type);
			if (modTile != null) {
				foreach (int k in modTile.AdjTiles) {
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

		public static bool RightClick(int i, int j) {
			bool returnValue = false;
			int type = Main.tile[i, j].type;

			if (GetTile(type)?.RightClick(i, j) ?? false)
				returnValue = true;

			foreach (var hook in HookRightClick) {
				hook(i, j, type);
			}
			return returnValue;
		}

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

		public static bool SaplingGrowthType(int soilType, ref int saplingType, ref int style) {
			int originalType = saplingType;
			int originalStyle = style;

			var treeGrown = PlantLoader.Get<ModTree>(TileID.Trees, soilType);

			if (treeGrown == null) {
				var palmGrown = PlantLoader.Get<ModPalmTree>(TileID.PalmTree, soilType);

				if (palmGrown != null)
					saplingType = palmGrown.SaplingGrowthType(ref style);
				else
					return false;
			}
			else
				saplingType = treeGrown.SaplingGrowthType(ref style);

			if (TileID.Sets.TreeSapling[saplingType])
				return true;

			saplingType = originalType;
			style = originalStyle;
			return false;
		}

		public static bool CanGrowModTree(int type) {
			return PlantLoader.Exists(TileID.Trees, type);
		}

		public static void TreeDust(Tile tile, ref int dust) {
			if (!tile.active())
				return;

			var tree = PlantLoader.Get<ModTree>(TileID.Trees, tile.type);
			if (tree != null)
				dust = tree.CreateDust();
		}

		public static bool CanDropAcorn(int type) {
			var tree = PlantLoader.Get<ModTree>(TileID.Trees, type);
			if (tree == null)
				return false;

			return tree.CanDropAcorn();
		}

		public static void DropTreeWood(int type, ref int wood) {
			var tree = PlantLoader.Get<ModTree>(TileID.Trees, type);
			if (tree != null)
				wood = tree.DropWood();
		}

		public static bool CanGrowModPalmTree(int type) {
			return PlantLoader.Exists(TileID.PalmTree, type);
		}

		public static void PalmTreeDust(Tile tile, ref int dust) {
			if (!tile.active())
				return;

			var tree = PlantLoader.Get<ModPalmTree>(TileID.PalmTree, tile.type);
			if (tree != null)
				dust = tree.CreateDust();
		}

		public static void DropPalmTreeWood(int type, ref int wood) {
			var tree = PlantLoader.Get<ModPalmTree>(TileID.PalmTree, type);
			if (tree != null)
				wood = tree.DropWood();
		}

		public static bool CanGrowModCactus(int type) {
			return PlantLoader.Exists(TileID.Cactus, type) || TileIO.Tiles.unloadedTypes.Contains((ushort)type);
		}

		public static Texture2D GetCactusTexture(int type) {
			var tree = PlantLoader.Get<ModCactus>(TileID.Cactus, type);
			if (tree == null)
				return null;

			return tree.GetTexture().Value;
		}

		public static void PlaceInWorld(int i, int j, Item item) {
			int type = item.createTile;
			if (type < 0)
				return;

			foreach (var hook in HookPlaceInWorld) {
				hook(i, j, type, item);
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
