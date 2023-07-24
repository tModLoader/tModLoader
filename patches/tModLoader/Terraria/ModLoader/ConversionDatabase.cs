﻿using System.Collections.Generic;
using Terraria.ID;

namespace Terraria.ModLoader;

public static class ConversionDatabase
{
	internal static Dictionary<string, Conversion> conversions = new();

	public static IReadOnlyDictionary<string, Conversion> Conversions => conversions;

	public static Conversion GetByName(string name) => conversions[name];

	internal static void Load()
	{
		if (conversions.Count > 0)
			throw new System.Exception("Some mod added an conversion before vanilla conversion were added to the list! Please move your conversion into something alike PostSetupContent.");

		GreenSolution();
		PurpleSolution();
		LightBlueSolution();
		BlueSolution();
		CrimsonSolution();
		YellowSolution();
		WhiteSolution();
		DirtSolution();
	}

	private static void GreenSolution()
	{
		var conv = new Conversion("Terraria:Purity");

		for (int wall = 1; wall < WallLoader.WallCount; wall++) {
			if (wall == 69 || wall == 70 || wall == 81) {
				conv.ConvertWall(wall, 64).PreConversion((tile, k, l, settings) => {
					if (l < Main.worldSurface) {
						if (WorldGen.genRand.Next(10) == 0)
							tile.wall = 65;
						else
							tile.wall = 63;

						if (settings.SquareWallFrame)
							WorldGen.SquareWallFrame(k, l);
						if (settings.NetSpam)
							NetMessage.SendTileSquare(-1, k, l);
						return ConversionRunCodeValues.DontRun;
					}
					return ConversionRunCodeValues.Run;
				});
			}
			else if (WallID.Sets.Conversion.Stone[wall] && wall != 1 && wall != 262 && wall != 274 && wall != 61 && wall != 185) {
				conv.ConvertWall(wall, 1);
			}
			else if (WallID.Sets.Conversion.Stone[wall] && wall == 262) {
				conv.ConvertWall(wall, 61);
			}
			else if (WallID.Sets.Conversion.Stone[wall] && wall == 274) {
				conv.ConvertWall(wall, 185);
			}
			if (WallID.Sets.Conversion.NewWall1[wall] && wall != 212) {
				conv.ConvertWall(wall, 212);
			}
			else if (WallID.Sets.Conversion.NewWall2[wall] && wall != 213) {
				conv.ConvertWall(wall, 213);
			}
			else if (WallID.Sets.Conversion.NewWall3[wall] && wall != 214) {
				conv.ConvertWall(wall, 214);
			}
			else if (WallID.Sets.Conversion.NewWall4[wall] && wall != 215) {
				conv.ConvertWall(wall, 215);
			}
			else if (wall == 80) {
				conv.ConvertWall(wall, 64).PreConversion((tile, k, l, settings) => {
					if (l < Main.worldSurface + 4.0 + WorldGen.genRand.Next(3) || l > (Main.maxTilesY + Main.rockLayer) / 2.0 - 3.0 + WorldGen.genRand.Next(3)) {
						tile.wall = 15;

						if (settings.SquareWallFrame)
							WorldGen.SquareWallFrame(k, l);
						if (settings.NetSpam)
							NetMessage.SendTileSquare(-1, k, l);
						return ConversionRunCodeValues.DontRun;
					}
					return ConversionRunCodeValues.Run;
				});
			}
			else if (WallID.Sets.Conversion.HardenedSand[wall] && wall != 216) {
				conv.ConvertWall(wall, 216);
			}
			else if (WallID.Sets.Conversion.Sandstone[wall] && wall != 187) {
				conv.ConvertWall(wall, 187);
			}
		}
		for (int type = 0; type < TileLoader.TileCount; type++) {
			if (type == 492) {
				conv.ConvertTile(type, 477).OnConversion(TryKillingTreesAboveIfTheyWouldBecomeInvalid);
			}
			else if (TileID.Sets.Conversion.JungleGrass[type] && type != 60) {
				conv.ConvertTile(type, 60).OnConversion(TryKillingTreesAboveIfTheyWouldBecomeInvalid);
			}
			else if (TileID.Sets.Conversion.Grass[type] && type != 2 && type != 477) {
				conv.ConvertTile(type, 2).OnConversion(TryKillingTreesAboveIfTheyWouldBecomeInvalid);
			}
			else if (TileID.Sets.Conversion.Stone[type] && type != 1) {
				conv.ConvertTile(type, 1).OnConversion(TryKillingTreesAboveIfTheyWouldBecomeInvalid);
			}
			else if (TileID.Sets.Conversion.Sand[type] && type != 53) {
				conv.ConvertTile(type, 53).OnConversion(TryKillingTreesAboveIfTheyWouldBecomeInvalid);
			}
			else if (TileID.Sets.Conversion.HardenedSand[type] && type != 397) {
				conv.ConvertTile(type, 397);
			}
			else if (TileID.Sets.Conversion.Sandstone[type] && type != 396) {
				conv.ConvertTile(type, 396);
			}
			else if (TileID.Sets.Conversion.Ice[type] && type != 161) {
				conv.ConvertTile(type, 161).OnConversion(TryKillingTreesAboveIfTheyWouldBecomeInvalid);
			}
			else if (TileID.Sets.Conversion.MushroomGrass[type]) {
				conv.ConvertTile(type, 60).OnConversion(TryKillingTreesAboveIfTheyWouldBecomeInvalid);
			}
			else if (type == 32 || type == 352) {
				conv.ConvertTile(type, ConversionHandler.Break);
			}
		}

		conv.Register();
	}

	private static void PurpleSolution()
	{
		var conv = new Conversion("Terraria:Corrupt");

		for (int wall = 1; wall < WallLoader.WallCount; wall++) {
			if (WallID.Sets.Conversion.Grass[wall] && wall != 69) {
				conv.ConvertWall(wall, 69);
			}
			else if (WallID.Sets.Conversion.Stone[wall] && wall != 3) {
				conv.ConvertWall(wall, 3);
			}
			else if (WallID.Sets.Conversion.HardenedSand[wall] && wall != 217) {
				conv.ConvertWall(wall, 217);
			}
			else if (WallID.Sets.Conversion.Sandstone[wall] && wall != 220) {
				conv.ConvertWall(wall, 220);
			}
			else if (WallID.Sets.Conversion.NewWall1[wall] && wall != 188) {
				conv.ConvertWall(wall, 188);
			}
			else if (WallID.Sets.Conversion.NewWall2[wall] && wall != 189) {
				conv.ConvertWall(wall, 189);
			}
			else if (WallID.Sets.Conversion.NewWall3[wall] && wall != 190) {
				conv.ConvertWall(wall, 190);
			}
			else if (WallID.Sets.Conversion.NewWall4[wall] && wall != 191) {
				conv.ConvertWall(wall, 191);
			}
		}
		for (int type = 0; type < TileLoader.TileCount; type++) {
			if (TileID.Sets.Conversion.JungleGrass[type] && type != 661) {
				conv.ConvertTile(type, 661);
			}
			if ((Main.tileMoss[type] || TileID.Sets.Conversion.Stone[type]) && type != 25) {
				conv.ConvertTile(type, 25).OnConversion(TryKillingTreesAboveIfTheyWouldBecomeInvalid);
			}
			else if (TileID.Sets.Conversion.Grass[type] && type != 23) {
				conv.ConvertTile(type, 23).OnConversion(TryKillingTreesAboveIfTheyWouldBecomeInvalid);
			}
			else if (TileID.Sets.Conversion.Ice[type] && type != 163) {
				conv.ConvertTile(type, 163).OnConversion(TryKillingTreesAboveIfTheyWouldBecomeInvalid);
			}
			else if (TileID.Sets.Conversion.Sand[type] && type != 112) {
				conv.ConvertTile(type, 112).OnConversion(TryKillingTreesAboveIfTheyWouldBecomeInvalid);
			}
			else if (TileID.Sets.Conversion.HardenedSand[type] && type != 398) {
				conv.ConvertTile(type, 398);
			}
			else if (TileID.Sets.Conversion.Sandstone[type] && type != 400) {
				conv.ConvertTile(type, 400);
			}
			else if (TileID.Sets.Conversion.Thorn[type] && type != 32) {
				conv.ConvertTile(type, 32);
			}
		}

		conv.Register();
	}

	private static void CrimsonSolution()
	{
		var conv = new Conversion("Terraria:Crimson");

		for (int wall = 1; wall < WallLoader.WallCount; wall++) {
			if (WallID.Sets.Conversion.Grass[wall] && wall != 81) {
				conv.ConvertWall(wall, 81);
			}
			else if (WallID.Sets.Conversion.Stone[wall] && wall != 83) {
				conv.ConvertWall(wall, 83);
			}
			else if (WallID.Sets.Conversion.HardenedSand[wall] && wall != 218) {
				conv.ConvertWall(wall, 218);
			}
			else if (WallID.Sets.Conversion.Sandstone[wall] && wall != 221) {
				conv.ConvertWall(wall, 221);
			}
			else if (WallID.Sets.Conversion.NewWall1[wall] && wall != 192) {
				conv.ConvertWall(wall, 192);
			}
			else if (WallID.Sets.Conversion.NewWall2[wall] && wall != 193) {
				conv.ConvertWall(wall, 193);
			}
			else if (WallID.Sets.Conversion.NewWall3[wall] && wall != 194) {
				conv.ConvertWall(wall, 194);
			}
			else if (WallID.Sets.Conversion.NewWall4[wall] && wall != 195) {
				conv.ConvertWall(wall, 195);
			}
		}
		for (int type = 0; type < TileLoader.TileCount; type++) {
			if ((Main.tileMoss[type] || TileID.Sets.Conversion.Stone[type]) && type != 203) {
				conv.ConvertTile(type, 203).OnConversion(TryKillingTreesAboveIfTheyWouldBecomeInvalid);
			}
			else if (TileID.Sets.Conversion.JungleGrass[type] && type != 662) {
				conv.ConvertTile(type, 662).OnConversion(TryKillingTreesAboveIfTheyWouldBecomeInvalid);
			}
			else if (TileID.Sets.Conversion.Grass[type] && type != 199) {
				conv.ConvertTile(type, 199).OnConversion(TryKillingTreesAboveIfTheyWouldBecomeInvalid);
			}
			else if (TileID.Sets.Conversion.Ice[type] && type != 200) {
				conv.ConvertTile(type, 200).OnConversion(TryKillingTreesAboveIfTheyWouldBecomeInvalid);
			}
			else if (TileID.Sets.Conversion.Sand[type] && type != 234) {
				conv.ConvertTile(type, 234).OnConversion(TryKillingTreesAboveIfTheyWouldBecomeInvalid);
			}
			else if (TileID.Sets.Conversion.HardenedSand[type] && type != 399) {
				conv.ConvertTile(type, 399);
			}
			else if (TileID.Sets.Conversion.Sandstone[type] && type != 401) {
				conv.ConvertTile(type, 401);
			}
			else if (TileID.Sets.Conversion.Thorn[type] && type != 352) {
				conv.ConvertTile(type, 352);
			}
		}

		conv.Register();
	}

	private static void LightBlueSolution()
	{
		var conv = new Conversion("Terraria:Hallow");

		for (int wall = 1; wall < WallLoader.WallCount; wall++) {
			if (WallID.Sets.Conversion.Grass[wall] && wall != 70) {
				conv.ConvertWall(wall, 70);
			}
			else if (WallID.Sets.Conversion.Stone[wall] && wall != 28) {
				conv.ConvertWall(wall, 28);
			}
			else if (WallID.Sets.Conversion.HardenedSand[wall] && wall != 219) {
				conv.ConvertWall(wall, 219);
			}
			else if (WallID.Sets.Conversion.Sandstone[wall] && wall != 222) {
				conv.ConvertWall(wall, 222);
			}
			else if (WallID.Sets.Conversion.NewWall1[wall] && wall != 200) {
				conv.ConvertWall(wall, 200);
			}
			else if (WallID.Sets.Conversion.NewWall2[wall] && wall != 201) {
				conv.ConvertWall(wall, 201);
			}
			else if (WallID.Sets.Conversion.NewWall3[wall] && wall != 202) {
				conv.ConvertWall(wall, 202);
			}
			else if (WallID.Sets.Conversion.NewWall4[wall] && wall != 203) {
				conv.ConvertWall(wall, 203);
			}
		}
		for (int type = 0; type < TileLoader.TileCount; type++) {

			if ((Main.tileMoss[type] || TileID.Sets.Conversion.Stone[type]) && type != 117) {
				conv.ConvertTile(type, 117).OnConversion(TryKillingTreesAboveIfTheyWouldBecomeInvalid);
			}
			else if (TileID.Sets.Conversion.GolfGrass[type] && type != 492) {
				conv.ConvertTile(type, 492).OnConversion(TryKillingTreesAboveIfTheyWouldBecomeInvalid);
			}
			else if (TileID.Sets.Conversion.Grass[type] && type != 109 && type != 492) {
				conv.ConvertTile(type, 109).OnConversion(TryKillingTreesAboveIfTheyWouldBecomeInvalid);
			}
			else if (TileID.Sets.Conversion.Ice[type] && type != 164) {
				conv.ConvertTile(type, 164).OnConversion(TryKillingTreesAboveIfTheyWouldBecomeInvalid);
			}
			else if (TileID.Sets.Conversion.Sand[type] && type != 116) {
				conv.ConvertTile(type, 116).OnConversion(TryKillingTreesAboveIfTheyWouldBecomeInvalid);
			}
			else if (TileID.Sets.Conversion.HardenedSand[type] && type != 402) {
				conv.ConvertTile(type, 402);
			}
			else if (TileID.Sets.Conversion.Sandstone[type] && type != 403) {
				conv.ConvertTile(type, 403);
			}
			else if (TileID.Sets.Conversion.Thorn[type]) {
				conv.ConvertTile(type, ConversionHandler.Break);
			}
			if (type == 59/* && (Main.tile[k - 1, l].type == 109 || Main.tile[k + 1, l].type == 109 || Main.tile[k, l - 1].type == 109 || Main.tile[k, l + 1].type == 109)*/) {
				conv.ConvertTile(type, 0).PreConversion((tile, k, l, settings) => {
					if (Main.tile[k - 1, l].type == 109 || Main.tile[k + 1, l].type == 109 || Main.tile[k, l - 1].type == 109 || Main.tile[k, l + 1].type == 109) {
						return ConversionRunCodeValues.Run;
					}
					return ConversionRunCodeValues.DontRun;
				});
			}
		}

		conv.Register();
	}

	private static void BlueSolution()
	{
		var conv = new Conversion("Terraria:Mushroom");

		for (int wall = 1; wall < WallLoader.WallCount; wall++) {
			if (WallID.Sets.CanBeConvertedToGlowingMushroom[wall]) {
				conv.ConvertWall(wall, 80);
			}
		}
		conv.ConvertTile(60, 70).OnConversion(TryKillingTreesAboveIfTheyWouldBecomeInvalid);
		for (int type = 0; type < TileLoader.TileCount; type++) {
			if (TileID.Sets.Conversion.Thorn[type]) {
				conv.ConvertTile(type, ConversionHandler.Break);
			}
		}

		conv.Register();
	}

	private static void YellowSolution()
	{
		var conv = new Conversion("Terraria:Desert");

		for (int wall = 1; wall < WallLoader.WallCount; wall++) {
			if ((WallID.Sets.Conversion.Stone[wall] || WallID.Sets.Conversion.NewWall1[wall] || WallID.Sets.Conversion.NewWall2[wall] || WallID.Sets.Conversion.NewWall3[wall] || WallID.Sets.Conversion.NewWall4[wall] || WallID.Sets.Conversion.Ice[wall] || WallID.Sets.Conversion.Sandstone[wall]) && wall != 187) {
				conv.ConvertWall(wall, 187);
			}
			else if ((WallID.Sets.Conversion.HardenedSand[wall] || WallID.Sets.Conversion.Dirt[wall] || WallID.Sets.Conversion.Snow[wall]) && wall != 216) {
				conv.ConvertWall(wall, 216);
			}
		}
		for (int type = 0; type < TileLoader.TileCount; type++) {

			if ((TileID.Sets.Conversion.Grass[type] || TileID.Sets.Conversion.Sand[type] || TileID.Sets.Conversion.Snow[type] || TileID.Sets.Conversion.Dirt[type]) && type != 53) {
				conv.ConvertTile(type, 53).PreConversion((tile, k, l, settings) => {
					if (WorldGen.BlockBelowMakesSandConvertIntoHardenedSand(k, l)) {
						tile.TileType = 397;

						if (settings.SquareTileFrame)
							WorldGen.SquareTileFrame(k, l);
						if (settings.NetSpam)
							NetMessage.SendTileSquare(-1, k, l);
						return ConversionRunCodeValues.DontRun;
					}
					return ConversionRunCodeValues.Run;
				}).OnConversion(TryKillingTreesAboveIfTheyWouldBecomeInvalid);
			}
			else if (TileID.Sets.Conversion.HardenedSand[type] && type != 397) {
				conv.ConvertTile(type, 397);
			}
			else if ((Main.tileMoss[type] || TileID.Sets.Conversion.Stone[type] || TileID.Sets.Conversion.Ice[type] || TileID.Sets.Conversion.Sandstone[type]) && type != 396) {
				conv.ConvertTile(type, 396).OnConversion(TryKillingTreesAboveIfTheyWouldBecomeInvalid);
			}
			else if (TileID.Sets.Conversion.Thorn[type] && type != 69) {
				conv.ConvertTile(type, ConversionHandler.Break);
			}
		}

		conv.Register();
	}

	private static void WhiteSolution()
	{
		var conv = new Conversion("Terraria:Snow");

		for (int wall = 1; wall < WallLoader.WallCount; wall++) {
			if ((WallID.Sets.Conversion.Stone[wall] || WallID.Sets.Conversion.NewWall1[wall] || WallID.Sets.Conversion.NewWall2[wall] || WallID.Sets.Conversion.NewWall3[wall] || WallID.Sets.Conversion.NewWall4[wall] || WallID.Sets.Conversion.Ice[wall] || WallID.Sets.Conversion.Sandstone[wall]) && wall != 71) {
				conv.ConvertWall(wall, 71);
			}
			else if ((WallID.Sets.Conversion.HardenedSand[wall] || WallID.Sets.Conversion.Dirt[wall] || WallID.Sets.Conversion.Snow[wall]) && wall != 40) {
				conv.ConvertWall(wall, 40);
			}
		}
		for (int type = 0; type < TileLoader.TileCount; type++) {
			if ((TileID.Sets.Conversion.Grass[type] || TileID.Sets.Conversion.Sand[type] || TileID.Sets.Conversion.HardenedSand[type] || TileID.Sets.Conversion.Snow[type] || TileID.Sets.Conversion.Dirt[type]) && type != 147) {
				conv.ConvertTile(type, 147).OnConversion(TryKillingTreesAboveIfTheyWouldBecomeInvalid);
			}
			else if ((Main.tileMoss[type] || TileID.Sets.Conversion.Stone[type] || TileID.Sets.Conversion.Ice[type] || TileID.Sets.Conversion.Sandstone[type]) && type != 161) {
				conv.ConvertTile(type, 161).OnConversion(TryKillingTreesAboveIfTheyWouldBecomeInvalid);
			}
			else if (TileID.Sets.Conversion.Thorn[type] && type != 69) {
				conv.ConvertTile(type, ConversionHandler.Break);
			}
		}

		conv.Register();
	}

	private static void DirtSolution()
	{
		var conv = new Conversion("Terraria:Forest");

		for (int wall = 1; wall < WallLoader.WallCount; wall++) {
			if ((WallID.Sets.Conversion.Stone[wall] || WallID.Sets.Conversion.Ice[wall] || WallID.Sets.Conversion.Sandstone[wall]) && wall != 1) {
				conv.ConvertWall(wall, 1);
			}
			else if ((WallID.Sets.Conversion.HardenedSand[wall] || WallID.Sets.Conversion.Snow[wall] || WallID.Sets.Conversion.Dirt[wall]) && wall != 2) {
				conv.ConvertWall(wall, 2);
			}
			else if (WallID.Sets.Conversion.NewWall1[wall] && wall != 196) {
				conv.ConvertWall(wall, 196);
			}
			else if (WallID.Sets.Conversion.NewWall2[wall] && wall != 197) {
				conv.ConvertWall(wall, 197);
			}
			else if (WallID.Sets.Conversion.NewWall3[wall] && wall != 198) {
				conv.ConvertWall(wall, 198);
			}
			else if (WallID.Sets.Conversion.NewWall4[wall] && wall != 199) {
				conv.ConvertWall(wall, 199);
			}
		}
		for (int type = 0; type < TileLoader.TileCount; type++) {
			if ((TileID.Sets.Conversion.Stone[type] || TileID.Sets.Conversion.Ice[type] || TileID.Sets.Conversion.Sandstone[type]) && type != 1) {
				conv.ConvertTile(type, 1).OnConversion(TryKillingTreesAboveIfTheyWouldBecomeInvalid);
			}
			else if (TileID.Sets.Conversion.GolfGrass[type] && type != 477) {
				conv.ConvertTile(type, 477).OnConversion(TryKillingTreesAboveIfTheyWouldBecomeInvalid);
			}
			else if (TileID.Sets.Conversion.Grass[type] && type != 2 && type != 477) {
				conv.ConvertTile(type, 2).OnConversion(TryKillingTreesAboveIfTheyWouldBecomeInvalid);
			}
			else if ((TileID.Sets.Conversion.Sand[type] || TileID.Sets.Conversion.HardenedSand[type] || TileID.Sets.Conversion.Snow[type] || TileID.Sets.Conversion.Dirt[type]) && type != 0) {
				conv.ConvertTile(type, 0).PreConversion((tile, k, l, settings) => {
					if (WorldGen.TileIsExposedToAir(k, l)) {
						tile.TileType = 2;

						if (settings.SquareTileFrame)
							WorldGen.SquareTileFrame(k, l);
						if (settings.NetSpam)
							NetMessage.SendTileSquare(-1, k, l);
						return ConversionRunCodeValues.DontRun;
					}
					return ConversionRunCodeValues.Run;
				}).OnConversion(TryKillingTreesAboveIfTheyWouldBecomeInvalid);
			}
			else if (TileID.Sets.Conversion.Thorn[type] && type != 69) {
				conv.ConvertTile(type, ConversionHandler.Break);
			}
		}

		conv.Register();
	}

	private static void TryKillingTreesAboveIfTheyWouldBecomeInvalid(Tile tile, int oldTileType, int i, int j, ConversionHandler.ConversionSettings settings)
	{
		WorldGen.TryKillingTreesAboveIfTheyWouldBecomeInvalid(i, j, tile.TileType, settings);
	}
}