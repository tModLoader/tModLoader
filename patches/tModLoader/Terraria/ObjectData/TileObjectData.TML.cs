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
			throw new ArgumentOutOfRangeException(nameof(getTile), "Function called with a bad tile type", null);

		TileObjectData tileObjectData = _data[type];

		if (tileObjectData == null)
			return -1;

		//TODO: Make sense of these locals.
		int num = getTile.frameX / tileObjectData.CoordinateFullWidth;
		int num2 = getTile.frameY / tileObjectData.CoordinateFullHeight;
		int styleWrapLimit = tileObjectData.StyleWrapLimit;

		if (styleWrapLimit == 0)
			styleWrapLimit = 1;

		int num4;

		if (tileObjectData.StyleHorizontal)
			num4 = num2 * styleWrapLimit + num;
		else
			num4 = num * styleWrapLimit + num2;

		int num5 = num4 / tileObjectData.StyleMultiplier;
		//int num6 = num4 % tileObjectData.StyleMultiplier;

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

		//TODO: Make sense of these locals.
		int num = getTile.frameX / tileObjectData.CoordinateFullWidth;
		int num2 = getTile.frameY / tileObjectData.CoordinateFullHeight;
		int styleWrapLimit = tileObjectData.StyleWrapLimit;

		if (styleWrapLimit == 0)
			styleWrapLimit = 1;

		int num4;

		if (tileObjectData.StyleHorizontal)
			num4 = num2 * styleWrapLimit + num;
		else
			num4 = num * styleWrapLimit + num2;

		int num5 = num4 / tileObjectData.StyleMultiplier;
		int num6 = num4 % tileObjectData.StyleMultiplier;

		style = num5;

		if (tileObjectData._alternates != null)
			alternate = num6;

		return;
 	}
}
