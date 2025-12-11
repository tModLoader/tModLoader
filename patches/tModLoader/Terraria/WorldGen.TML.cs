using MonoMod.Cil;
using System.Collections.Generic;
using Terraria.GameContent.Generation;
using Terraria.WorldBuilding;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ID;
using System;

namespace Terraria;

public partial class WorldGen
{
	internal static void ClearGenerationPasses()
	{
		_generator?._passes.Clear();
	}

	internal static Dictionary<string, GenPass> _vanillaGenPasses = new();
	public static IReadOnlyDictionary<string, GenPass> VanillaGenPasses => _vanillaGenPasses;

	public static void ModifyPass(PassLegacy pass, ILContext.Manipulator callback)
	{
		MonoModHooks.Modify(pass._method.Method, callback);
	}

	// The self reference has to be object, because the actual type is a compiler generated closure class
	// The self reference isn't useful anyway, since the closure doesn't capture any method locals or an enclosing class instance
	// We might think to omit the self parameter from mod delegates, and register a wrapper which propogates self via a closure, but then MonoModHooks will attribute the hook to tModLoader rather than the original mod.
	public delegate void GenPassDetour(orig_GenPassDetour orig, object self, GenerationProgress progress, GameConfiguration configuration);
	public delegate void orig_GenPassDetour(object self, GenerationProgress progress, GameConfiguration configuration);

	public static void DetourPass(PassLegacy pass, GenPassDetour hookDelegate)
	{
		MonoModHooks.Add(pass._method.Method, hookDelegate);
	}

	/// <inheritdoc cref="ConvertTile(int, int, int)"/>
	[OriginalOverload]
	[Obsolete("Use ConvertTile(int, int, int) instead")]
	public static void ConvertTile(int i, int j, int newType, bool tryBreakTrees = false) => ConvertTile(i, j, newType);

	/// <summary>
	/// Converts the single tile at the given coordinate into a specified new tile type<br/>
	/// Automatically handles tile framing and multiplayer syncing.
	/// </summary>
	/// <param name="i">The X coordinate of the target tile.</param>
	/// <param name="j">The Y coordinate of the target tile.</param>
	/// <param name="newType">The new type to convert the tile into</param>
	public static void ConvertTile(int i, int j, int newType)
	{
		Tile tile = Main.tile[i, j];
		if (tile.type == (ushort)newType)
			return;

		TryKillingTreesAboveIfTheyWouldBecomeInvalid(i, j, newType);

		tile.type = (ushort)newType;
		SquareTileFrame(i, j);
		if (Main.netMode != 0)
			NetMessage.SendTileSquare(-1, i, j);
	}

	/// <summary>
	/// Converts the single wall at the given coordinate into a specified new wall type<br/>
	/// Automatically handles wall framing and multiplayer syncing.
	/// </summary>
	/// <param name="i">The X coordinate of the target tile.</param>
	/// <param name="j">The Y coordinate of the target tile.</param>
	/// <param name="newType">The new type to convert the wall into</param>
	public static void ConvertWall(int i, int j, int newType)
	{
		Tile tile = Main.tile[i, j];
		if (tile.wall == (ushort)newType)
			return;

		tile.wall = (ushort)newType;
		SquareWallFrame(i, j);
		if (Main.netMode != 0)
			NetMessage.SendTileSquare(-1, i, j);
	}

	/// <summary>
	/// Utility method that mimics vanilla behavior for biome spread in hardmode. Call this in <see cref="ModBlockType.RandomUpdate"/> to let your infectious tiles spread around<br/>
	/// Automatically checks for the journey mode biome spread toggle, hardmode, spread speed decrease after plantera, chlorophyte protection and sunflower protection.
	/// </summary>
	/// <param name="x">X coordinate of the spreading tile</param>
	/// <param name="y">Y coordinate of the spreading tile</param>
	/// <param name="conversionType">The <see cref="BiomeConversionID"/> of the spreading tile</param>
	/// <param name="range">Tile range for potential conversion targets</param>
	public static void SpreadInfectionToNearbyTile(int x, int y, int conversionType, int range = 3)
	{
		if (!AllowedToSpreadInfections)
			return;

		if (!Main.hardMode || (NPC.downedPlantBoss && genRand.NextBool(2)))
			return;

		// Vanilla keeps trying to spread the infection to neighboring tiles, with a 1/2 chance to stop after each converted tile
		bool keepSpreading = true;
		while (keepSpreading) {
			keepSpreading = false;
			int testX = x + genRand.Next(-range, range + 1);
			int testY = y + genRand.Next(-range, range + 1);
			if (!InWorld(testX, testY, 10))
				return;

			int preConversionType = Main.tile[testX, testY].type;
			if (!TileID.Sets.Infectable[preConversionType])
				return;

			if (nearbyChlorophyte(testX, testY) && conversionType != BiomeConversionID.Hallow) {
				ChlorophyteDefense(testX, testY);
				return;
			}

			if (CountNearBlocksTypes(testX, testY, 2, 1, TileID.Sunflower) > 0)
				return;

			Convert(testX, testY, conversionType, size: 0, tiles: true, walls: false);
			if (preConversionType != Main.tile[testX, testY].type)
				keepSpreading = genRand.NextBool(2); // 1 in 2 chance to attempt to spread to another tile if we successfuly converted the first one
		}
	}

}
