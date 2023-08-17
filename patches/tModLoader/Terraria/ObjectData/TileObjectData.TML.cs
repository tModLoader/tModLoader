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

		int num4 = ((!tileObjectData.StyleHorizontal) ? (num * num3 + num2) : (num2 * num3 + num));
		int num5 = num4 / tileObjectData.StyleMultiplier;
		//int num6 = num4 % tileObjectData.StyleMultiplier;
		int styleLineSkip = tileObjectData.StyleLineSkip;
		if (styleLineSkip > 1) {
			if (tileObjectData.StyleHorizontal) {
				num5 = num2 / styleLineSkip * num3 + num;
				//num6 = num2 % styleLineSkip;
			}
			else {
				num5 = num / styleLineSkip * num3 + num2;
				//num6 = num % styleLineSkip;
			}
		}

		return num5;
	}

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

		int num4 = ((!tileObjectData.StyleHorizontal) ? (num * num3 + num2) : (num2 * num3 + num));
		int num5 = num4 / tileObjectData.StyleMultiplier;
		int num6 = num4 % tileObjectData.StyleMultiplier;
		int styleLineSkip = tileObjectData.StyleLineSkip;
		if (styleLineSkip > 1) {
			if (tileObjectData.StyleHorizontal) {
				num5 = num2 / styleLineSkip * num3 + num;
				num6 = num2 % styleLineSkip;
			}
			else {
				num5 = num / styleLineSkip * num3 + num2;
				num6 = num % styleLineSkip;
			}
		}

		style = num5;
		alternate = num6;

		return;
 	}
}
