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
}
