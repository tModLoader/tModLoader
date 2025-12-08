using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

namespace Terraria.ModLoader;

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
	/// <summary> Maps Tile type and Tile style to the Item type that places the tile with the style. </summary>
	internal static readonly Dictionary<(int, int), int> tileTypeAndTileStyleToItemType = new();
	public delegate bool ConvertTile(int i, int j, int type, int conversionType);
	internal static List<ConvertTile>[][] tileConversionDelegates = null;
	internal static int[][] tileConversionFallbacks = null;
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
	private static Func<int, int, int, bool>[] HookCanDrop;
	private static Action<int, int, int>[] HookDrop;
	private delegate bool DelegateCanKillTile(int i, int j, int type, ref bool blockDamaged);
	private static DelegateCanKillTile[] HookCanKillTile;
	private delegate void DelegateKillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem);
	private static DelegateKillTile[] HookKillTile;
	private static Func<int, int, int, bool>[] HookCanExplode;
	private static Action<int, int, int, bool>[] HookNearbyEffects;
	private delegate void DelegateModifyLight(int i, int j, int type, ref float r, ref float g, ref float b);
	private static DelegateModifyLight[] HookModifyLight;
	private static Func<int, int, int, Player, bool?>[] HookIsTileDangerous;
	private delegate bool? DelegateIsTileBiomeSightable(int i, int j, int type, ref Color sightColor);
	private static DelegateIsTileBiomeSightable[] HookIsTileBiomeSightable;
	private static Func<int, int, int, bool?>[] HookIsTileSpelunkable;
	private delegate void DelegateSetSpriteEffects(int i, int j, int type, ref SpriteEffects spriteEffects);
	private static DelegateSetSpriteEffects[] HookSetSpriteEffects;
	private static Action[] HookAnimateTile;
	private static Func<int, int, int, SpriteBatch, bool>[] HookPreDraw;
	private delegate void DelegateDrawEffects(int i, int j, int type, SpriteBatch spriteBatch, ref TileDrawInfo drawData);
	private static DelegateDrawEffects[] HookDrawEffects;
	private static Action<int, int, Tile, ushort, short, short, Color, bool>[] HookEmitParticles;
	private static Action<int, int, int, SpriteBatch>[] HookPostDraw;
	private static Action<int, int, int, SpriteBatch>[] HookSpecialDraw;
	private delegate bool DelegatePreDrawPlacementPreview(int i, int j, int type, SpriteBatch spriteBatch, ref Rectangle frame, ref Vector2 position, ref Color color, bool validPlacement, ref SpriteEffects spriteEffects);
	private static DelegatePreDrawPlacementPreview[] HookPreDrawPlacementPreview;
	private static Action<int, int, int, SpriteBatch, Rectangle, Vector2, Color, bool, SpriteEffects>[] HookPostDrawPlacementPreview;
	private static Action<int, int, int>[] HookRandomUpdate;
	private delegate bool DelegateTileFrame(int i, int j, int type, ref bool resetFrame, ref bool noBreak);
	private static DelegateTileFrame[] HookTileFrame;
	private static Func<int, int, int, bool>[] HookCanPlace;
	private static Func<int, int, int, int, bool>[] HookCanReplace;
	private static Action<int, int, int, int, int>[] HookReplaceTile;
	private static Func<int, int[]>[] HookAdjTiles;
	private static Action<int, int, int>[] HookRightClick;
	private static Action<int, int, int>[] HookMouseOver;
	private static Action<int, int, int>[] HookMouseOverFar;
	private static Func<int, int, int, Item, bool>[] HookAutoSelect;
	private static Func<int, int, int, bool>[] HookPreHitWire;
	private static Action<int, int, int>[] HookHitWire;
	private static Func<int, int, int, bool>[] HookHitSwitch;
	private static Func<int, int, int, Entity, Vector2, int, int, Vector2, int, bool>[] HookSwitchTiles;
	private static Func<int, int, int, bool>[] HookSlope;
	private static Action<int, Player>[] HookFloorVisuals;
	private delegate void DelegateChangeWaterfallStyle(int type, ref int style);
	private static DelegateChangeWaterfallStyle[] HookChangeWaterfallStyle;
	private static Action<int, int, int, Item>[] HookPlaceInWorld;
	private static Action[] HookPostSetupTileMerge;
	private static Action<int, int, TreeTypes>[] HookPreShakeTree;
	private static Func<int, int, TreeTypes, bool>[] HookShakeTree;
	private static Action<int, int, int, int, int>[] HookOnTileConverted;

	internal static int ReserveTileID()
	{
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
	public static ModTile GetTile(int type)
	{
		return type >= TileID.Count && type < TileCount ? tiles[type - TileID.Count] : null;
	}

	private static void Resize2DArray<T>(ref T[,] array, int newSize)
	{
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

	internal static void ResizeArrays(bool unloading = false)
	{
		//Textures
		Array.Resize(ref TextureAssets.Tile, nextTile);
		Array.Resize(ref TextureAssets.HighlightMask, nextTile);

		//Sets
		LoaderUtils.ResetStaticMembers(typeof(TileID));

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

		tileConversionDelegates = new List<ConvertTile>[nextTile][];
		tileConversionFallbacks = new int[nextTile][];
		InitializeConversionFallbacks();

		//Hooks

		// .NET 6 SDK bug: https://github.com/dotnet/roslyn/issues/57517
		// Remove generic arguments once fixed.
		ModLoader.BuildGlobalHook(ref HookKillSound, globalTiles, g => g.KillSound);
		ModLoader.BuildGlobalHook<GlobalTile, DelegateNumDust>(ref HookNumDust, globalTiles, g => g.NumDust);
		ModLoader.BuildGlobalHook<GlobalTile, DelegateCreateDust>(ref HookCreateDust, globalTiles, g => g.CreateDust);
		ModLoader.BuildGlobalHook<GlobalTile, DelegateDropCritterChance>(ref HookDropCritterChance, globalTiles, g => g.DropCritterChance);
		ModLoader.BuildGlobalHook(ref HookCanDrop, globalTiles, g => g.CanDrop);
		ModLoader.BuildGlobalHook(ref HookDrop, globalTiles, g => g.Drop);
		ModLoader.BuildGlobalHook<GlobalTile, DelegateCanKillTile>(ref HookCanKillTile, globalTiles, g => g.CanKillTile);
		ModLoader.BuildGlobalHook<GlobalTile, DelegateKillTile>(ref HookKillTile, globalTiles, g => g.KillTile);
		ModLoader.BuildGlobalHook(ref HookCanExplode, globalTiles, g => g.CanExplode);
		ModLoader.BuildGlobalHook(ref HookNearbyEffects, globalTiles, g => g.NearbyEffects);
		ModLoader.BuildGlobalHook<GlobalTile, DelegateModifyLight>(ref HookModifyLight, globalTiles, g => g.ModifyLight);
		ModLoader.BuildGlobalHook(ref HookIsTileDangerous, globalTiles, g => g.IsTileDangerous);
		ModLoader.BuildGlobalHook<GlobalTile, DelegateIsTileBiomeSightable>(ref HookIsTileBiomeSightable, globalTiles, g => g.IsTileBiomeSightable);
		ModLoader.BuildGlobalHook(ref HookIsTileSpelunkable, globalTiles, g => g.IsTileSpelunkable);
		ModLoader.BuildGlobalHook<GlobalTile, DelegateSetSpriteEffects>(ref HookSetSpriteEffects, globalTiles, g => g.SetSpriteEffects);
		ModLoader.BuildGlobalHook(ref HookAnimateTile, globalTiles, g => g.AnimateTile);
		ModLoader.BuildGlobalHook(ref HookPreDraw, globalTiles, g => g.PreDraw);
		ModLoader.BuildGlobalHook<GlobalTile, DelegateDrawEffects>(ref HookDrawEffects, globalTiles, g => g.DrawEffects);
		ModLoader.BuildGlobalHook(ref HookEmitParticles, globalTiles, g => g.EmitParticles);
		ModLoader.BuildGlobalHook(ref HookPostDraw, globalTiles, g => g.PostDraw);
		ModLoader.BuildGlobalHook(ref HookSpecialDraw, globalTiles, g => g.SpecialDraw);
		ModLoader.BuildGlobalHook<GlobalTile, DelegatePreDrawPlacementPreview>(ref HookPreDrawPlacementPreview, globalTiles, g => g.PreDrawPlacementPreview);
		ModLoader.BuildGlobalHook(ref HookPostDrawPlacementPreview, globalTiles, g => g.PostDrawPlacementPreview);
		ModLoader.BuildGlobalHook(ref HookRandomUpdate, globalTiles, g => g.RandomUpdate);
		ModLoader.BuildGlobalHook<GlobalTile, DelegateTileFrame>(ref HookTileFrame, globalTiles, g => g.TileFrame);
		ModLoader.BuildGlobalHook(ref HookCanPlace, globalTiles, g => g.CanPlace);
		ModLoader.BuildGlobalHook(ref HookCanReplace, globalTiles, g => g.CanReplace);
		ModLoader.BuildGlobalHook(ref HookReplaceTile, globalTiles, g => g.ReplaceTile);
		ModLoader.BuildGlobalHook(ref HookAdjTiles, globalTiles, g => g.AdjTiles);
		ModLoader.BuildGlobalHook(ref HookRightClick, globalTiles, g => g.RightClick);
		ModLoader.BuildGlobalHook(ref HookMouseOver, globalTiles, g => g.MouseOver);
		ModLoader.BuildGlobalHook(ref HookMouseOverFar, globalTiles, g => g.MouseOverFar);
		ModLoader.BuildGlobalHook(ref HookAutoSelect, globalTiles, g => g.AutoSelect);
		ModLoader.BuildGlobalHook(ref HookPreHitWire, globalTiles, g => g.PreHitWire);
		ModLoader.BuildGlobalHook(ref HookHitWire, globalTiles, g => g.HitWire);
		ModLoader.BuildGlobalHook(ref HookHitSwitch, globalTiles, g => g.HitSwitch);
		ModLoader.BuildGlobalHook(ref HookSwitchTiles, globalTiles, g => g.SwitchTiles);
		ModLoader.BuildGlobalHook(ref HookSlope, globalTiles, g => g.Slope);
		ModLoader.BuildGlobalHook(ref HookFloorVisuals, globalTiles, g => g.FloorVisuals);
		ModLoader.BuildGlobalHook<GlobalTile, DelegateChangeWaterfallStyle>(ref HookChangeWaterfallStyle, globalTiles, g => g.ChangeWaterfallStyle);
		ModLoader.BuildGlobalHook(ref HookPlaceInWorld, globalTiles, g => g.PlaceInWorld);
		ModLoader.BuildGlobalHook(ref HookPostSetupTileMerge, globalTiles, g => g.PostSetupTileMerge);
		ModLoader.BuildGlobalHook(ref HookPreShakeTree, globalTiles, g => g.PreShakeTree);
		ModLoader.BuildGlobalHook(ref HookShakeTree, globalTiles, g => g.ShakeTree);
		ModLoader.BuildGlobalHook(ref HookOnTileConverted, globalTiles, g => g.OnTileConverted);

		if (!unloading) {
			loaded = true;
		}
	}

	internal static void PostSetupContent()
	{
		Main.SetupAllBlockMerge();
		PostSetupTileMerge();
	}

	internal static void Unload()
	{
		loaded = false;
		nextTile = TileID.Count;

		tiles.Clear();
		globalTiles.Clear();
		tileTypeAndTileStyleToItemType.Clear();
		Animation.Unload();
		tileConversionDelegates = null;

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
	public static void CheckModTile(int i, int j, int type)
	{
		if (type <= TileID.Count) {
			return;
		}
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
		int styleLineSkip = tileData.StyleLineSkip;
		int subTile = tileData.StyleHorizontal ? subY / styleLineSkip * wrap + subX : subX / styleLineSkip * wrap + subY;
		int style = subTile / tileData.StyleMultiplier;
		/*
		int alternate = subTile % tileData.StyleMultiplier;
		for (int k = 0; k < tileData.AlternatesCount; k++) {
			if (alternate >= tileData.Alternates[k].Style && alternate <= tileData.Alternates[k].Style + tileData.RandomStyleRange) {
				alternate = k;
				break;
			}
		}
		*/
		tileData = TileObjectData.GetTileData(Main.tile[i, j]);
		int partFrameX = frameX % tileData.CoordinateFullWidth;
		int partFrameY = frameY % tileData.CoordinateFullHeight;
		int partX = partFrameX / (tileData.CoordinateWidth + tileData.CoordinatePadding);
		int partY = 0;
		for (int remainingFrameY = partFrameY; partY + 1 < tileData.Height && remainingFrameY - tileData.CoordinateHeights[partY] - tileData.CoordinatePadding >= 0; partY++) {
			remainingFrameY -= tileData.CoordinateHeights[partY] + tileData.CoordinatePadding;
		}
		// We need to use the tile that trigger this, since it still has the tile type instead of air
		int originalI = i;
		int originalJ = j;
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
		// TODO: Placed modded tiles can't automatically reorient themselves to an alternate placement, like Torch and Sign do.
		if (partiallyDestroyed || !TileObject.CanPlace(originX, originY, type, style, 0, out TileObject objectData, onlyCheck: true, checkStay: true)) {
			WorldGen.destroyObject = true;
			// First the Items to drop are tallied and spawned, then Kill each tile, then KillMultiTile can clean up TileEntities or Chests
			// KillTile will handle calling DropItems for 1x1 tiles.
			if (tileData.Width != 1 || tileData.Height != 1)
				WorldGen.KillTile_DropItems(originalI, originalJ, Main.tile[originalI, originalJ], includeLargeObjectDrops: true, includeAllModdedLargeObjectDrops: true); // include all drops.
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
	public static int OpenDoorID(Tile tile)
	{
		ModTile modTile = GetTile(tile.type);
		if (modTile != null) {
			return TileID.Sets.OpenDoorID[modTile.Type];
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
	public static int CloseDoorID(Tile tile)
	{
		ModTile modTile = GetTile(tile.type);

		if (modTile != null) {
			return TileID.Sets.CloseDoorID[modTile.Type];
		}

		if (tile.type == TileID.OpenDoor) {
			return TileID.ClosedDoor;
		}

		return -1;
	}

	/// <inheritdoc cref="IsClosedDoor(int)"/>
	public static bool IsClosedDoor(Tile tile) => IsClosedDoor(tile.type);

	/// <summary>
	/// Returns true if the tile is a vanilla or modded closed door.
	/// </summary>
	public static bool IsClosedDoor(int type)
	{
		ModTile modTile = GetTile(type);

		if (modTile != null) {
			return TileID.Sets.OpenDoorID[type] > -1;
		}

		return type == TileID.ClosedDoor;
	}

	/// <summary> Returns the default name for a modded chest or dresser with the provided FrameX and FrameY values. </summary>
	public static string DefaultContainerName(int type, int frameX, int frameY) => GetTile(type)?.DefaultContainerName(frameX, frameY)?.Value ?? string.Empty;

	public static bool IsModMusicBox(Tile tile)
	{
		return MusicLoader.tileToMusic.ContainsKey(tile.type)
		&& MusicLoader.tileToMusic[tile.type].ContainsKey(tile.frameY / 36 * 36);
	}

	public static bool HasSmartInteract(int i, int j, int type, SmartInteractScanSettings settings)
	{
		return GetTile(type)?.HasSmartInteract(i, j, settings) ?? false;
	}

	public static void ModifySmartInteractCoords(int type, ref int width, ref int height, ref int frameWidth, ref int frameHeight, ref int extraY)
	{
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

	public static void ModifySittingTargetInfo(int i, int j, int type, ref TileRestingInfo info)
	{
		ModTile modTile = GetTile(type);
		if (modTile != null) {
			modTile.ModifySittingTargetInfo(i, j, ref info);
		}
		else {
			info.AnchorTilePosition.Y += 1; // Hardcoded vanilla offset from the bottom tile moved here (all chairs have height-1 offset)
		}
	}

	public static void ModifySleepingTargetInfo(int i, int j, int type, ref TileRestingInfo info)
	{
		ModTile modTile = GetTile(type);
		if (modTile != null) {
			// Because vanilla sets its own offset based on frameY, ignoring tile type, which might not be set to an expected default, reassign it
			info.VisualOffset = new Vector2(-9f, 1f); // Taken from default case of vanilla beds
			modTile.ModifySleepingTargetInfo(i, j, ref info);
		}
	}

	public static bool KillSound(int i, int j, int type, bool fail)
	{
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

	public static void NumDust(int i, int j, int type, bool fail, ref int numDust)
	{
		GetTile(type)?.NumDust(i, j, fail, ref numDust);

		foreach (var hook in HookNumDust) {
			hook(i, j, type, fail, ref numDust);
		}
	}

	public static bool CreateDust(int i, int j, int type, ref int dustType)
	{
		foreach (var hook in HookCreateDust) {
			if (!hook(i, j, type, ref dustType)) {
				return false;
			}
		}
		return GetTile(type)?.CreateDust(i, j, ref dustType) ?? true;
	}

	public static void DropCritterChance(int i, int j, int type, ref int wormChance, ref int grassHopperChance, ref int jungleGrubChance)
	{
		GetTile(type)?.DropCritterChance(i, j, ref wormChance, ref grassHopperChance, ref jungleGrubChance);

		foreach (var hook in HookDropCritterChance) {
			hook(i, j, type, ref wormChance, ref grassHopperChance, ref jungleGrubChance);
		}
	}

	// Reminders:
	// i and j are the coordinates being checked, not the top left.
	// Drop must be called before KillTile. Item.DisableNewItemMethod must be used after KillTile to prevent accidentally preventing drops from other tiles broken by KillTile->TileFrame chains. If NewItem code is above KillTile code in vanilla code, just use goto skipDrops.
	public static bool Drop(int i, int j, int type, bool includeLargeObjectDrops = true)
	{
		// Drop is called in TileFrame->CheckX methods with includeLargeObjectDrops true. Each individual tile is then killed in those methods where includeLargeObjectDrops will be false. Ignore those situations.
		bool isLarge = false;
		if (Main.tileFrameImportant[type]) {
			var tileData = TileObjectData.GetTileData(type, 0);
			if (tileData != null) {
				if (tileData.Width != 1 || tileData.Height != 1)
					isLarge = true;
			}
			else if (TileID.Sets.IsMultitile[type])
				isLarge = true;
		}
		if (!includeLargeObjectDrops && isLarge)
			return true;

		Tile t = Main.tile[i, j];
		// Comment out to debug: Main.NewText($"Drop: {i}, {j}, {type}, L: {includeLargeObjectDrops}, HasTile: {t.HasTile}, type: {t.TileType}, fX: {t.TileFrameX}, name: {TileID.Search.GetName(t.TileType)}");
		ModTile modTile = GetTile(type);
		bool dropItem = modTile?.CanDrop(i, j) ?? true;
		foreach (var hook in HookCanDrop) {
			dropItem &= hook(i, j, type);
		}
		if (!dropItem)
			return false;

		foreach (var hook in HookDrop) {
			hook(i, j, type);
		}

		return true;
	}

	public static void GetItemDrops(int x, int y, Tile tileCache, bool includeLargeObjectDrops = false, bool includeAllModdedLargeObjectDrops = false)
	{
		ModTile modTile = GetTile(tileCache.TileType);
		if (modTile == null)
			return;

		// Various call sites to WorldGen.KillTile_DropItems expect different sets of tile drops to be retrieved:
		// KillTile: All 1x1 tiles
		// ReplaceTile: All 1x1 tiles, all supported multi-tiles
		// CheckModTile: All modded tiles (except 1x1 tiles will drop from KillTile)
		bool needDrops = false;
		TileObjectData tileData = TileObjectData.GetTileData(tileCache.TileType, 0, 0);
		if (tileData == null) {
			// Terrain tile
			needDrops = true;
		}
		else if (tileData.Width == 1 && tileData.Height == 1) {
			// 1x1 tile, includeAllModdedLargeObjectDrops prevents double spawns from framing code calling CheckModTile, which calls KillTile_DropItems and KillTile. (Bars)
			needDrops = !includeAllModdedLargeObjectDrops;
		}
		else if (includeAllModdedLargeObjectDrops)
			needDrops = true;
		else if (includeLargeObjectDrops) {
			if (TileID.Sets.BasicChest[tileCache.type] || TileID.Sets.BasicDresser[tileCache.type] || TileID.Sets.Campfire[tileCache.type]) {
				needDrops = true;
			}
		}
		if (!needDrops) {
			return;
		}

		var itemDrops = modTile.GetItemDrops(x, y);
		if (itemDrops != null) {
			foreach (var item in itemDrops) {
				item.Prefix(-1); // Assign a random prefix, as expected
				int num = Item.NewItem(WorldGen.GetItemSource_FromTileBreak(x, y), x * 16, y * 16, 16, 16, item, noBroadcast: false);
				Main.item[num].TryCombiningIntoNearbyItems(num);
			}
		}
	}

	/// <summary>
	/// Retrieves the item type that would drop from a tile of the specified type and style. This method is only reliable for modded tile types. This method can be used in <see cref="ModTile.GetItemDrops(int, int)"/> for tiles that have custom tile style logic. If the specified style is not found, a fallback item will be returned if one has been registered through <see cref="ModTile.RegisterItemDrop(int, int[])"/> usage.<br/>
	/// Modders querying modded tile drops should use <see cref="ModTile.GetItemDrops(int, int)"/> directly rather that use this method so that custom drop logic is accounted for.
	/// <br/> A return of 0 indicates that no item would drop from the tile.
	/// </summary>
	/// <param name="type"></param>
	/// <param name="style"></param>
	/// <returns></returns>
	public static int GetItemDropFromTypeAndStyle(int type, int style = 0)
	{
		if (tileTypeAndTileStyleToItemType.TryGetValue((type, style), out int value) || tileTypeAndTileStyleToItemType.TryGetValue((type, -1), out value))
			return value;

		return 0;
	}

	public static bool CanKillTile(int i, int j, int type, ref bool blockDamaged)
	{
		foreach (var hook in HookCanKillTile) {
			if (!hook(i, j, type, ref blockDamaged)) {
				return false;
			}
		}
		return GetTile(type)?.CanKillTile(i, j, ref blockDamaged) ?? true;
	}

	public static void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
	{
		GetTile(type)?.KillTile(i, j, ref fail, ref effectOnly, ref noItem);

		foreach (var hook in HookKillTile) {
			hook(i, j, type, ref fail, ref effectOnly, ref noItem);
		}
	}

	public static void KillMultiTile(int i, int j, int frameX, int frameY, int type)
	{
		GetTile(type)?.KillMultiTile(i, j, frameX, frameY);
	}

	public static bool CanExplode(int i, int j)
	{
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

	public static void NearbyEffects(int i, int j, int type, bool closer)
	{
		GetTile(type)?.NearbyEffects(i, j, closer);

		foreach (var hook in HookNearbyEffects) {
			hook(i, j, type, closer);
		}
	}

	public static void ModifyTorchLuck(Player player, ref float positiveLuck, ref float negativeLuck)
	{
		foreach (int type in player.NearbyModTorch) {
			float f = GetTile(type).GetTorchLuck(player);
			if (f > 0)
				positiveLuck += f;
			else
				negativeLuck += -f;
		}
	}

	public static void ModifyLight(int i, int j, int type, ref float r, ref float g, ref float b)
	{
		if (!Main.tileLighted[type]) {
			return;
		}
		GetTile(type)?.ModifyLight(i, j, ref r, ref g, ref b);

		foreach (var hook in HookModifyLight) {
			hook(i, j, type, ref r, ref g, ref b);
		}
	}

	/// <summary>
	/// Registers a tile type as having custom biome conversion code for this specific <see cref="BiomeConversionID"/>. For modded tiles, you can directly use <see cref="Convert"/> <br/>
	/// If you need to register conversions that rely on <see cref="TileID.Sets.Conversion"/> being fully populated, consider doing it in <see cref="ModBiomeConversion.PostSetupContent"/>
	/// </summary>
	/// <param name="tileType">The tile type that has is affected by this custom conversion.</param>
	/// <param name="conversionType">The conversion type for which the tile should use custom conversion code.</param>
	/// <param name="conversionDelegate">Code to run when the tile attempts to get converted. Return false to signal that your custom conversion took place and that vanilla code shouldn't be ran.</param>
	public static void RegisterConversion(int tileType, int conversionType, ConvertTile conversionDelegate)
	{
		if (tileConversionDelegates == null)
			throw new Exception(Language.GetTextValue("tModLoader.LoadErrorCallDuringLoad", "TileLoader.RegisterConversion"));

		var conversions = tileConversionDelegates[tileType] ??= new List<ConvertTile>[BiomeConversionLoader.BiomeConversionCount];
		var list = conversions[conversionType] ??= new();
		list.Add(conversionDelegate);
	}

	/// <summary>
	/// Registers a tile type as having custom biome conversion code for this specific <see cref="BiomeConversionID"/>. For modded tiles, you can directly use <see cref="Convert"/> <br/>
	/// If you need to register conversions that rely on <see cref="TileID.Sets.Conversion"/> being fully populated, consider doing it in <see cref="ModBiomeConversion.PostSetupContent"/>
	/// </summary>
	/// <param name="tileType">The tile type that has is affected by this custom conversion.</param>
	/// <param name="conversionType">The conversion type for which the tile should use custom conversion code.</param>
	/// <param name="toType">What <paramref name="tileType"/> is converted into when it's hit with the <paramref name="conversionType"/>.</param>
	public static void RegisterConversion(int tileType, int conversionType, int toType)
	{
		RegisterConversion(tileType, conversionType, (int i, int j, int type, int conversionType) => {
			WorldGen.ConvertTile(i, j, toType);
			return false;
		});
	}

	/// <summary>
	/// Registers a conversion that replaces <paramref name="tileType"/> with <paramref name="toType"/> when touched by <paramref name="conversionType"/> <br/>
	/// Also registers <paramref name="tileType"/> as a fallback for <paramref name="toType"/> so that other conversions can convert <paramref name="toType"/> as if it was <paramref name="tileType"/>. <br/>
	/// If you need to register conversions that rely on <see cref="TileID.Sets.Conversion"/> being fully populated, consider doing it in <see cref="ModBiomeConversion.PostSetupContent"/>
	/// </summary>
	/// <param name="tileType">The tile type that has is affected by this conversion.</param>
	/// <param name="conversionType">The conversion type for which the tile should use this conversion.</param>
	/// <param name="toType">The tile type that this conversion should convert the tile to.</param>
	/// <param name="purification">If true, automatically registers purification conversions from toType to tileType as well.</param>
	public static void RegisterSimpleConversion(int tileType, int conversionType, int toType, bool purification = true)
	{
		RegisterConversion(tileType, conversionType, (int i, int j, int type, int conversionType) => {
			WorldGen.ConvertTile(i, j, toType);
			return false;
		});

		RegisterConversionFallback(toType, tileType, conversionType);

		if (purification) {
			bool Purify(int i, int j, int type, int conversionType)
			{
				WorldGen.ConvertTile(i, j, tileType);
				return false;
			}
			RegisterConversion(toType, BiomeConversionID.Purity, Purify);
			RegisterConversion(toType, BiomeConversionID.PurificationPowder, Purify);
			if (conversionType != BiomeConversionID.Hallow)
				RegisterConversion(toType, BiomeConversionID.Chlorophyte, Purify);
		}
	}

	private static void InitializeConversionFallbacks()
	{
		RegisterConversionFallback(TileID.Ebonstone, TileID.Stone, BiomeConversionID.Corruption);
		RegisterConversionFallback(TileID.Crimstone, TileID.Stone, BiomeConversionID.Crimson);
		RegisterConversionFallback(TileID.Pearlstone, TileID.Stone, BiomeConversionID.Hallow);

		RegisterConversionFallback(TileID.CorruptGrass, TileID.Grass, BiomeConversionID.Corruption);
		RegisterConversionFallback(TileID.CrimsonGrass, TileID.Grass, BiomeConversionID.Crimson);
		RegisterConversionFallback(TileID.HallowedGrass, TileID.Grass, BiomeConversionID.Hallow);

		RegisterConversionFallback(TileID.GolfGrassHallowed, TileID.GolfGrass, BiomeConversionID.Hallow);
		RegisterConversionFallback(TileID.GolfGrass, TileID.Grass, BiomeConversionID.Purity, BiomeConversionID.PurificationPowder, BiomeConversionID.Dirt);

		RegisterConversionFallback(TileID.CorruptJungleGrass, TileID.JungleGrass, BiomeConversionID.Corruption, BiomeConversionID.GlowingMushroom);
		RegisterConversionFallback(TileID.CrimsonJungleGrass, TileID.JungleGrass, BiomeConversionID.Crimson, BiomeConversionID.GlowingMushroom);
		RegisterConversionFallback(TileID.MushroomGrass, TileID.JungleGrass, BiomeConversionID.GlowingMushroom, BiomeConversionID.Corruption, BiomeConversionID.Crimson);

		RegisterConversionFallback(TileID.CorruptIce, TileID.IceBlock, BiomeConversionID.Corruption);
		RegisterConversionFallback(TileID.FleshIce, TileID.IceBlock, BiomeConversionID.Crimson);
		RegisterConversionFallback(TileID.HallowedIce, TileID.IceBlock, BiomeConversionID.Hallow);

		RegisterConversionFallback(TileID.Ebonsand, TileID.Sand, BiomeConversionID.Corruption);
		RegisterConversionFallback(TileID.Crimsand, TileID.Sand, BiomeConversionID.Crimson);
		RegisterConversionFallback(TileID.Pearlsand, TileID.Sand, BiomeConversionID.Hallow);

		RegisterConversionFallback(TileID.CorruptHardenedSand, TileID.HardenedSand, BiomeConversionID.Corruption);
		RegisterConversionFallback(TileID.CrimsonHardenedSand, TileID.HardenedSand, BiomeConversionID.Crimson);
		RegisterConversionFallback(TileID.HallowHardenedSand, TileID.HardenedSand, BiomeConversionID.Hallow);

		RegisterConversionFallback(TileID.CorruptSandstone, TileID.Sandstone, BiomeConversionID.Corruption);
		RegisterConversionFallback(TileID.CrimsonSandstone, TileID.Sandstone, BiomeConversionID.Crimson);
		RegisterConversionFallback(TileID.HallowSandstone, TileID.Sandstone, BiomeConversionID.Hallow);
	}

	private static int[] GetOrInitConversionFallbacks(int tileType)
	{
		if (tileConversionFallbacks == null)
			throw new Exception(Language.GetTextValue("tModLoader.LoadErrorCallDuringLoad", "TileLoader.RegisterConversionFallback"));

		ref var fallbacks = ref tileConversionFallbacks[tileType];
		if (fallbacks is null) {
			fallbacks = new int[BiomeConversionLoader.BiomeConversionCount];
			Array.Fill(fallbacks, -1);
		}

		return fallbacks;
	}

	/// <summary>
	/// Sets a fallback tile type for all conversion types except those in <paramref name="exceptForConversionTypes"/> <br/>
	/// When <see cref="WorldGen.Convert(int, int, int, int, bool, bool)"/> is called on the <paramref name="tileType"/> but there is no registsred conversion, the tile will be temporarily replaced with <paramref name="fallbackType"/> and conversion will be reattempted.<br/>
	/// If the <paramref name="fallbackType"/> also has no conversion, the tile remains unchanged. <br/>
	/// <br/>
	/// For example <see cref="TileID.Ebonstone"/> falls back to <see cref="TileID.Stone"/> so a modded conversion that affects Stone can convert Ebonstone without needing to register a conversion for Ebonstone directly.
	/// </summary>
	public static void RegisterConversionFallback(int tileType, int fallbackType, params int[] exceptForConversionTypes)
	{
		var fallbacks = GetOrInitConversionFallbacks(tileType);
		var backup = (int[])fallbacks.Clone();
		Array.Fill(fallbacks, fallbackType);
		foreach (var i in exceptForConversionTypes)
			fallbacks[i] = backup[i];
	}

	/// <summary>
	/// Sets an individual conversion fallback. For advanced uses only.
	/// </summary>
	public static void SetConversionFallback(int tileType, int conversionType, int fallbackType)
	{
		GetOrInitConversionFallbacks(tileType)[conversionType] = fallbackType;
	}

	/// <summary>
	/// Tries to retrieve the <paramref name="fallbackType"/> corresponding to the provided <paramref name="tileType"/> and <paramref name="conversionType"/> <br/>
	/// See also: <seealso cref="RegisterConversionFallback"/>
	/// </summary>
	/// <returns>True if the tile has a registered fallback for the given conversion type</returns>
	public static bool TryGetConversionFallback(int tileType, int conversionType, out int fallbackType)
	{
		if (tileConversionFallbacks == null)
			throw new Exception(Language.GetTextValue("tModLoader.LoadErrorCallDuringLoad", "TileLoader.TryGetConversionFallback"));

		fallbackType = tileConversionFallbacks[tileType]?[conversionType] ?? -1;
		return fallbackType >= 0;
	}

	public static bool Convert(int i, int j, int conversionType)
	{
		using var recursionCounter = new WorldGen.ConversionRecursion();
		var tile = Main.tile[i, j];
		int type = tile.TileType;
		var list = tileConversionDelegates[type]?[conversionType];
		if (list != null) {
			foreach (var hook in CollectionsMarshal.AsSpan(list)) {
				if (!hook(i, j, type, conversionType))
					return false;
			}
		}

		GetTile(type)?.Convert(i, j, conversionType);

		if (tile.TileType == type && TryGetConversionFallback(type, conversionType, out var fallback)) {
			tile.TileType = (ushort)fallback;
			WorldGen.Convert(i, j, conversionType, size: 0, walls: false);

			if (tile.TileType == fallback)
				tile.TileType = (ushort)type;
		}

		return true;
	}

	public static bool? IsTileDangerous(int i, int j, int type, Player player)
	{
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

	public static bool? IsTileBiomeSightable(int i, int j, int type, ref Color sightColor)
	{
		bool? retVal = null;

		ModTile modTile = GetTile(type);

		if (modTile != null && modTile.IsTileBiomeSightable(i, j, ref sightColor)) {
			retVal = true;
		}

		foreach (var hook in HookIsTileBiomeSightable) {
			bool? globalRetVal = hook(i, j, type, ref sightColor);
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

	public static bool? IsTileSpelunkable(int i, int j, int type)
	{
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

	public static void SetSpriteEffects(int i, int j, int type, ref SpriteEffects spriteEffects)
	{
		GetTile(type)?.SetSpriteEffects(i, j, ref spriteEffects);

		foreach (var hook in HookSetSpriteEffects) {
			hook(i, j, type, ref spriteEffects);
		}
	}

	public static void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
	{
		Tile tile = Main.tile[i, j];
		if (tile.type >= TileID.Count) {
			TileObjectData tileData = TileObjectData.GetTileData(tile.type, 0, 0);
			if (tileData != null) {
				int partY = 0;
				for (int remainingFrameY = tile.frameY % tileData.CoordinateFullHeight; partY + 1 < tileData.Height && remainingFrameY - tileData.CoordinateHeights[partY] - tileData.CoordinatePadding >= 0; partY++) {
					remainingFrameY -= tileData.CoordinateHeights[partY] + tileData.CoordinatePadding;
				}
				width = tileData.CoordinateWidth;
				offsetY = tileData.DrawYOffset;
				height = tileData.CoordinateHeights[partY];
			}
			GetTile(tile.type).SetDrawPositions(i, j, ref width, ref offsetY, ref height, ref tileFrameX, ref tileFrameY);
		}
	}

	public static void AnimateTiles()
	{
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
	public static void SetAnimationFrame(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
	{
		ModTile modTile = GetTile(type);
		if (modTile != null) {
			frameYOffset = modTile.AnimationFrameHeight * Main.tileFrame[type];
			modTile.AnimateIndividualTile(type, i, j, ref frameXOffset, ref frameYOffset);
		}
	}

	public static bool PreDraw(int i, int j, int type, SpriteBatch spriteBatch)
	{
		foreach (var hook in HookPreDraw) {
			if (!hook(i, j, type, spriteBatch)) {
				return false;
			}
		}
		return GetTile(type)?.PreDraw(i, j, spriteBatch) ?? true;
	}

	public static void DrawEffects(int i, int j, int type, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
	{
		GetTile(type)?.DrawEffects(i, j, spriteBatch, ref drawData);
		foreach (var hook in HookDrawEffects) {
			hook(i, j, type, spriteBatch, ref drawData);
		}
	}

	public static void EmitParticles(int i, int j, Tile tileCache, ushort typeCache, short tileFrameX, short tileFrameY, Color tileLight, bool visible)
	{
		foreach (var hook in HookEmitParticles) {
			hook(i, j, tileCache, typeCache, tileFrameX, tileFrameY, tileLight, visible);
		}
		GetTile(typeCache)?.EmitParticles(i, j, tileCache, tileFrameX, tileFrameY, tileLight, visible);
	}

	public static void PostDraw(int i, int j, int type, SpriteBatch spriteBatch)
	{
		// TODO: Pass in TileDrawInfo so mods don't need to replicate existing SetDrawPositions logic. For example, ExampleTorch repeated logic (SetDrawPositions/PostDraw)
		GetTile(type)?.PostDraw(i, j, spriteBatch);

		foreach (var hook in HookPostDraw) {
			hook(i, j, type, spriteBatch);
		}
	}

	/// <summary>
	/// Special Draw calls ModTile and GlobalTile SpecialDraw methods. Special Draw is called at the end of the DrawSpecialTilesLegacy loop, allowing for basically another layer above tiles. Use DrawEffects hook to queue for SpecialDraw.
	/// </summary>
	public static void SpecialDraw(int type, int specialTileX, int specialTileY, SpriteBatch spriteBatch)
	{
		GetTile(type)?.SpecialDraw(specialTileX, specialTileY, spriteBatch);

		foreach (var hook in HookSpecialDraw) {
			hook(specialTileX, specialTileY, type, spriteBatch);
		}
	}

	public static bool PreDrawPlacementPreview(int i, int j, int type, SpriteBatch spriteBatch, ref Rectangle frame, ref Vector2 position, ref Color color, bool validPlacement, ref SpriteEffects spriteEffects)
	{
		foreach (var hook in HookPreDrawPlacementPreview) {
			if (!hook(i, j, type, spriteBatch, ref frame, ref position, ref color, validPlacement, ref spriteEffects)) {
				return false;
			}
		}
		return GetTile(type)?.PreDrawPlacementPreview(i, j, spriteBatch, ref frame, ref position, ref color, validPlacement, ref spriteEffects) ?? true;
	}

	public static void PostDrawPlacementPreview(int i, int j, int type, SpriteBatch spriteBatch, Rectangle frame, Vector2 position, Color color, bool validPlacement, SpriteEffects spriteEffects)
	{
		GetTile(type)?.PostDrawPlacementPreview(i, j, spriteBatch, frame, position, color, validPlacement, spriteEffects);

		foreach (var hook in HookPostDrawPlacementPreview) {
			hook(i, j, type, spriteBatch, frame, position, color, validPlacement, spriteEffects);
		}
	}

	public static void RandomUpdate(int i, int j, int type)
	{
		if (!Main.tile[i, j].active()) {
			return;
		}
		GetTile(type)?.RandomUpdate(i, j);

		foreach (var hook in HookRandomUpdate) {
			hook(i, j, type);
		}
	}

	public static bool TileFrame(int i, int j, int type, ref bool resetFrame, ref bool noBreak)
	{
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

	public static void PostTileFrame(int type, int i, int j, int up, int down, int left, int right, int upLeft, int upRight, int downLeft, int downRight)
	{
		ModTile modTile = GetTile(type);
		if (modTile != null) {
			modTile.PostTileFrame(i, j, up, down, left, right, upLeft, upRight, downLeft, downRight);
		}
	}

	public static void ModifyFrameMerge(int type, int i, int j, ref int up, ref int down, ref int left, ref int right, ref int upLeft, ref int upRight, ref int downLeft, ref int downRight)
	{
		ModTile modTile = GetTile(type);
		if (modTile != null) {
			modTile.ModifyFrameMerge(i, j, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
		}
	}

	public static void PickPowerCheck(Tile target, int pickPower, ref int damage)
	{
		ModTile modTile = GetTile(target.type);
		if (modTile != null && pickPower < modTile.MinPick) {
			damage = 0;
		}
	}

	public static bool CanPlace(int i, int j, int type)
	{
		foreach (var hook in HookCanPlace) {
			if (!hook(i, j, type)) {
				return false;
			}
		}
		return GetTile(type)?.CanPlace(i, j) ?? true;
	}

	public static bool CanReplace(int i, int j, int type, int tileTypeBeingPlaced)
	{
		foreach (var hook in HookCanReplace) {
			if (!hook(i, j, type, tileTypeBeingPlaced)) {
				return false;
			}
		}
		return GetTile(type)?.CanReplace(i, j, tileTypeBeingPlaced) ?? true;
	}

	public static void ReplaceTile(int i, int j, int type, int targetType, int targetStyle)
	{
		foreach (var hook in HookReplaceTile) {
			hook(i, j, type, targetType, targetStyle);
		}
		GetTile(type)?.ReplaceTile(i, j, targetType, targetStyle); // Do we want the reverse as well?
	}

	public static void AdjTiles(Player player, int type)
	{
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

	public static bool RightClick(int i, int j)
	{
		bool returnValue = false;
		int type = Main.tile[i, j].type;

		if (GetTile(type)?.RightClick(i, j) ?? false)
			returnValue = true;

		foreach (var hook in HookRightClick) {
			hook(i, j, type);
		}
		return returnValue;
	}

	public static void MouseOver(int i, int j)
	{
		int type = Main.tile[i, j].type;
		GetTile(type)?.MouseOver(i, j);

		foreach (var hook in HookMouseOver) {
			hook(i, j, type);
		}
	}

	public static void MouseOverFar(int i, int j)
	{
		int type = Main.tile[i, j].type;
		GetTile(type)?.MouseOverFar(i, j);

		foreach (var hook in HookMouseOverFar) {
			hook(i, j, type);
		}
	}

	public static int AutoSelect(int i, int j, Player player)
	{
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

	public static bool PreHitWire(int i, int j, int type)
	{
		foreach (var hook in HookPreHitWire) {
			if (!hook(i, j, type)) {
				return false;
			}
		}
		return true;
	}

	public static void HitWire(int i, int j, int type)
	{
		GetTile(type)?.HitWire(i, j);

		foreach (var hook in HookHitWire) {
			hook(i, j, type);
		}
	}

	public static bool HitSwitch(int i, int j, int type)
	{
		foreach (var hook in HookHitSwitch) {
			if (!hook(i, j, type))
				return false;
		}
		GetTile(type)?.HitSwitch(i, j);
		return true;
	}

	public static bool SwitchTiles(int i, int j, int type, Entity entity, Vector2 position, int width, int height, Vector2 oldPosition, int objType)
	{
		bool returnValue = false;
		foreach (var hook in HookSwitchTiles) {
			returnValue |= hook(i, j, type, entity, position, width, height, oldPosition, objType);
		}
		returnValue |= GetTile(type)?.SwitchTiles(i, j, entity, position, width, height, oldPosition, objType) ?? false;
		return returnValue;
	}

	public static void FloorVisuals(int type, Player player)
	{
		GetTile(type)?.FloorVisuals(player);

		foreach (var hook in HookFloorVisuals) {
			hook(type, player);
		}
	}

	public static bool Slope(int i, int j, int type)
	{
		foreach (var hook in HookSlope) {
			if (!hook(i, j, type)) {
				return true;
			}
		}
		return !GetTile(type)?.Slope(i, j) ?? false;
	}

	public static bool HasWalkDust(int type)
	{
		return GetTile(type)?.HasWalkDust() ?? false;
	}

	public static void WalkDust(int type, ref int dustType, ref bool makeDust, ref Color color)
	{
		GetTile(type)?.WalkDust(ref dustType, ref makeDust, ref color);
	}

	public static void ChangeWaterfallStyle(int type, ref int style)
	{
		GetTile(type)?.ChangeWaterfallStyle(ref style);
		foreach (var hook in HookChangeWaterfallStyle) {
			hook(type, ref style);
		}
	}

	public static bool SaplingGrowthType(int soilType, ref int saplingType, ref int style)
	{
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

	public static bool CanGrowModTree(int type)
	{
		return PlantLoader.Exists(TileID.Trees, type);
	}

	public static void TreeDust(Tile tile, ref int dust)
	{
		if (!tile.active())
			return;

		var tree = PlantLoader.Get<ModTree>(TileID.Trees, tile.type);
		if (tree != null)
			dust = tree.CreateDust();
	}

	public static bool CanDropAcorn(int type)
	{
		var tree = PlantLoader.Get<ModTree>(TileID.Trees, type);
		if (tree == null)
			return false;

		return tree.CanDropAcorn();
	}

	public static void DropTreeWood(int type, ref int wood)
	{
		var tree = PlantLoader.Get<ModTree>(TileID.Trees, type);
		if (tree != null)
			wood = tree.DropWood();
	}

	public static bool CanGrowModPalmTree(int type)
	{
		return PlantLoader.Exists(TileID.PalmTree, type);
	}

	public static void PalmTreeDust(Tile tile, ref int dust)
	{
		if (!tile.active())
			return;

		var tree = PlantLoader.Get<ModPalmTree>(TileID.PalmTree, tile.type);
		if (tree != null)
			dust = tree.CreateDust();
	}

	public static void DropPalmTreeWood(int type, ref int wood)
	{
		var tree = PlantLoader.Get<ModPalmTree>(TileID.PalmTree, type);
		if (tree != null)
			wood = tree.DropWood();
	}

	public static bool CanGrowModCactus(int type)
	{
		return PlantLoader.Exists(TileID.Cactus, type) || TileIO.Tiles.unloadedTypes.Contains((ushort)type);
	}

	public static Texture2D GetCactusTexture(int type)
	{
		var tree = PlantLoader.Get<ModCactus>(TileID.Cactus, type);
		if (tree == null)
			return null;

		return tree.GetTexture().Value;
	}

	public static void PlaceInWorld(int i, int j, Item item)
	{
		Tile tile = Main.tile[i, j];
		int type = tile.TileType;
		if (!tile.HasTile)
			return;

		foreach (var hook in HookPlaceInWorld) {
			hook(i, j, type, item);
		}

		GetTile(type)?.PlaceInWorld(i, j, item);
	}

	public static void PostSetupTileMerge()
	{
		foreach (var hook in HookPostSetupTileMerge) {
			hook();
		}

		foreach (var modTile in tiles) {
			modTile.PostSetupTileMerge();
		}
	}

	public static bool IsLockedChest(int i, int j, int type)
	{
		return GetTile(type)?.IsLockedChest(i, j) ?? false;
	}

	public static bool UnlockChest(int i, int j, int type, ref short frameXAdjustment, ref int dustType, ref bool manual)
	{
		return GetTile(type)?.UnlockChest(i, j, ref frameXAdjustment, ref dustType, ref manual) ?? false;
	}

	public static bool LockChest(int i, int j, int type, ref short frameXAdjustment, ref bool manual)
	{
		return GetTile(type)?.LockChest(i, j, ref frameXAdjustment, ref manual) ?? false;
	}

	public static void RecountTiles(SceneMetrics metrics)
	{
		// reset every tile count
		metrics.HolyTileCount = metrics.EvilTileCount = metrics.BloodTileCount = metrics.SnowTileCount = metrics.JungleTileCount = metrics.MushroomTileCount = metrics.SandTileCount = metrics.DungeonTileCount = 0;

		// loop through all tiles, skipping ones not onscreen, and add each to the biome tile counts from their respective sets
		for (int i = 0; i < TileCount; i++) {

			int tileCount = metrics._tileCounts[i];

			if (tileCount == 0)
				continue;

			metrics.HolyTileCount += tileCount * TileID.Sets.HallowBiome[i];
			metrics.SnowTileCount += tileCount * TileID.Sets.SnowBiome[i];
			metrics.MushroomTileCount += tileCount * TileID.Sets.MushroomBiome[i];
			metrics.SandTileCount += tileCount * TileID.Sets.SandBiome[i];
			metrics.DungeonTileCount += tileCount * TileID.Sets.DungeonBiome[i];

			int crimson, corrupt, jungle = 0;

			// handles if the world is using the remix seed or not, which slightly changes which blocks are counted
			if (!Main.remixWorld) {
				corrupt = TileID.Sets.CorruptBiome[i];
				crimson = TileID.Sets.CrimsonBiome[i];
				jungle = TileID.Sets.JungleBiome[i];
			}

			else {
				corrupt = TileID.Sets.RemixCorruptBiome[i];
				crimson = TileID.Sets.RemixCrimsonBiome[i];
				jungle = TileID.Sets.RemixJungleBiome[i];
			}

			metrics.EvilTileCount += tileCount * corrupt;
			metrics.BloodTileCount += tileCount * crimson;
			metrics.JungleTileCount += tileCount * jungle;
		}
	}

	internal static void FinishSetup()
	{
		for (int k = 0; k < ItemLoader.ItemCount; k++) {
			Item item = ContentSamples.ItemsByType[k];
			if (!ItemID.Sets.DisableAutomaticPlaceableDrop[k]) {
				if (item.createTile > -1) {
					// TryAdd won't override existing value if present. Existing ModTile.RegisterItemDrop entries take precedence
					tileTypeAndTileStyleToItemType.TryAdd((item.createTile, item.placeStyle), item.type);
				}
			}
		}
	}

	public static bool GlobalShakeTree(int x, int y, TreeTypes treeType)
	{
		foreach (var hook in HookPreShakeTree) {
			hook(x, y, treeType);
		}

		foreach (var hook in HookShakeTree) {
			if (hook(x, y, treeType))
				return true;
		}
		return false;
	}

	public static void OnTileConverted(int i, int j, int fromType, int toType, int conversionType)
	{
		foreach (var hook in HookOnTileConverted) {
			hook(i, j, fromType, toType, conversionType);
		}

		GetTile(fromType)?.OnTileConverted(i, j, fromType, toType, conversionType);
		GetTile(toType)?.OnTileConverted(i, j, fromType, toType, conversionType);
	}
}
