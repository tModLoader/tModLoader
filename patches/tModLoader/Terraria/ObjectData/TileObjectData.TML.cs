using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Tile_Entities;
using Terraria.ModLoader;
using Terraria.Modules;

namespace Terraria.ObjectData;

public partial class TileObjectData
{
	public static void FixNewTile()
	{
		newTile = new TileObjectData(_baseObject);
	}

	/// <summary>
	/// Retrieves the tile style corresponding to the passed in Tile. Empty tiles and terrain tiles will return -1. Any Tile of the multitile works.
	/// <para/> This is most useful in replacing hard-coded math where the tile style is calculated from <see cref="Tile.TileFrameX"/> and <see cref="Tile.TileFrameY"/> directly, such as mouse over icons and other tile style specific behaviors.
	/// <para/> Other related methods include <see cref="GetTileData(Tile)"/>, <see cref="GetTileData(int, int, int)"/>, and <see cref="GetTileInfo(Tile, ref int, ref int)"/>.
	/// </summary>
	public static int GetTileStyle(Tile getTile)
	{
		if (getTile == null || !getTile.active())
			return -1;

		int type = getTile.type;

		if (type < 0 || type >= _data.Count)
			throw new ArgumentOutOfRangeException(nameof(getTile), "Function called with a bad tile type");

		TileObjectData tileObjectData = _data[type];

		if (tileObjectData == null)
			return -1;

		// Adapted from GetTileData
		int num = getTile.frameX / tileObjectData.CoordinateFullWidth;
		int num2 = getTile.frameY / tileObjectData.CoordinateFullHeight;
		int num3 = tileObjectData.StyleWrapLimit;
		if (num3 == 0)
			num3 = 1;

		int styleLineSkip = tileObjectData.StyleLineSkip;
		int num4 = (!tileObjectData.StyleHorizontal) ? (num / styleLineSkip * num3 + num2) : (num2 / styleLineSkip * num3 + num);
		int num5 = num4 / tileObjectData.StyleMultiplier;
		//int num6 = num4 % tileObjectData.StyleMultiplier;

		return num5;
	}

	/// <summary>
	/// Retrieves the tile <paramref name="style"/> and <paramref name="alternate"/> placement corresponding to the passed in Tile. Empty tiles and terrain tiles will return without setting the ref parameters. Any Tile of the multitile works.
	/// <para/> Other related methods include <see cref="GetTileData(Tile)"/>, <see cref="GetTileData(int, int, int)"/>, and <see cref="GetTileStyle(Tile)"/>.
	/// </summary>
	public static void GetTileInfo(Tile getTile, ref int style, ref int alternate)
	{
		if (getTile == null || !getTile.active())
			return;

		int type = getTile.type;

		if (type < 0 || type >= _data.Count)
			throw new ArgumentOutOfRangeException(nameof(getTile), "Function called with a bad tile type");

		TileObjectData tileObjectData = _data[type];

		if (tileObjectData == null)
			return;

		// Adapted from GetTileData
		int num = getTile.frameX / tileObjectData.CoordinateFullWidth;
		int num2 = getTile.frameY / tileObjectData.CoordinateFullHeight;
		int num3 = tileObjectData.StyleWrapLimit;
		if (num3 == 0)
			num3 = 1;

		int styleLineSkip = tileObjectData.StyleLineSkip;
		int num4 = (!tileObjectData.StyleHorizontal) ? (num / styleLineSkip * num3 + num2) : (num2 / styleLineSkip * num3 + num);
		int num5 = num4 / tileObjectData.StyleMultiplier;
		int num6 = num4 % tileObjectData.StyleMultiplier;

		style = num5;
		alternate = num6;

		return;
 	}

	/// <summary>
	/// Returns true if the <see cref="Tile"/> at the location provided has a placed tile and is the top left tile of a multitile (supports alternate placements and states as well). Returns false otherwise.
	/// <para/> Can be used for logic that should only run once per multitile, such as custom tile rendering.
	/// </summary>
	public static bool IsTopLeft(int i, int j) => IsTopLeft(Main.tile[i, j]);

	/// <summary>
	/// Returns true if the <see cref="Tile"/> provided has a placed tile and is the top left tile of a multitile (supports alternate placements and states as well). Returns false otherwise.
	/// <para/> Can be used for logic that should only run once per multitile, such as custom tile rendering.
	/// </summary>
	public static bool IsTopLeft(Tile tile)
	{
		if (!tile.HasTile)
		{
			return false;
		}
		var tileData = GetTileData(tile);
		if (tileData == null)
		{
			return false;
		}
		int partFrameX = tile.TileFrameX % tileData.CoordinateFullWidth;
		int partFrameY = tile.TileFrameY % tileData.CoordinateFullHeight;
		return partFrameX == 0 && partFrameY == 0;
	}

	/// <summary>
	/// Returns the coordinates of the top left tile of the multitile at the location provided. Returns <see cref="Point16.NegativeOne"/> if no tile exists at the coordinates. If the tile does not have a TileObjectData, such as if it were a normal terrain tile, the provided coordinates will be returned.
	/// </summary>
	/// <param name="i"></param>
	/// <param name="j"></param>
	/// <returns></returns>
	public static Point16 TopLeft(int i, int j)
	{
		Tile tile = Main.tile[i, j];

		if (!tile.HasTile) {
			return Point16.NegativeOne;
		}
		var tileData = GetTileData(tile);
		if (tileData == null) {
			return new Point16(i, j);
		}
		int partFrameX = tile.TileFrameX % tileData.CoordinateFullWidth;
		int partFrameY = tile.TileFrameY % tileData.CoordinateFullHeight;
		int partX = partFrameX / (tileData.CoordinateWidth + tileData.CoordinatePadding);
		int partY = 0;
		for (int remainingFrameY = partFrameY; partY + 1 < tileData.Height && remainingFrameY - tileData.CoordinateHeights[partY] - tileData.CoordinatePadding >= 0; partY++) {
			remainingFrameY -= tileData.CoordinateHeights[partY] + tileData.CoordinatePadding;
		}
		i -= partX;
		j -= partY;
		return new Point16(i, j);
	}

	/// <inheritdoc cref="TopLeft(int, int)"/>
	public static Point16 TopLeft(Point16 point) => TopLeft(point.X, point.Y);
}
