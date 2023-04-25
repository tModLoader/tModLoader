using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.ID;
using Terraria;
using static Terraria.GameContent.Animations.Actions.Sprites;

namespace Terraria.ModLoader;

public static class TileFrameCache
{
	internal static BitsByte[,] TileFramer;

	internal static LinkedList<Tuple<int, int, bool>> TileQueue = new();
	internal static LinkedList<Tuple<int, int, bool>> WallQueue = new();

	internal static void InitializeTileFramer()
	{
		TileFramer = new BitsByte[Main.maxTilesX, Main.maxTilesY];
	}

	public static void QueueSquareTileFrame(int x, int y, bool resetFrame = false)
	{
		if (x > 5 && y > 5 && x < Main.maxTilesX - 5 && y < Main.maxTilesY - 5 && Main.tile[x, y] != null) {

			QueueTileFrame(x - 1, y - 1);
			QueueTileFrame(x - 1, y);
			QueueTileFrame(x - 1, y + 1);
			QueueTileFrame(x, y - 1);
			QueueTileFrame(x, y, resetFrame);
			QueueTileFrame(x, y + 1);
			QueueTileFrame(x + 1, y - 1);
			QueueTileFrame(x + 1, y);
			QueueTileFrame(x + 1, y + 1);
		}
	}
	public static void QueueSquareWallFrame(int x, int y, bool resetFrame = false)
	{
		if (x > 5 && y > 5 && x < Main.maxTilesX - 5 && y < Main.maxTilesY - 5 && Main.tile[x, y] != null) {

			QueueWallFrame(x - 1, y - 1);
			QueueWallFrame(x - 1, y);
			QueueWallFrame(x - 1, y + 1);
			QueueWallFrame(x, y - 1);
			QueueWallFrame(x, y, resetFrame);
			QueueWallFrame(x, y + 1);
			QueueWallFrame(x + 1, y - 1);
			QueueWallFrame(x + 1, y);
			QueueWallFrame(x + 1, y + 1);
		}
	}
	public static void QueueTileFrame(int x, int y, bool resetFrame = false)
	{
		if (!TileFramer[x, y][0]) {
			TileFramer[x, y][0] = true;
			Tuple<int, int, bool> data = new(x, y, resetFrame);
			TileQueue.AddFirst(data);
		}
	}
	public static void QueueWallFrame(int x, int y, bool resetFrame = false)
	{
		if (!TileFramer[x, y][1]) {
			TileFramer[x, y][1] = true;
			Tuple<int, int, bool> data = new(x, y, resetFrame);
			WallQueue.AddFirst(data);
		}
	}

	public static void ResolveFrame()
	{
		Tuple<int, int, bool> item;
		for (LinkedListNode<Tuple<int, int, bool>> node = TileQueue.First; node != null; node = node.Next) {
			item = node.Value;
			TileFramer[item.Item1, item.Item2][0] = false;
			WorldGen.TileFrame(item.Item1, item.Item2, item.Item3);

		}
		TileQueue = new();
		for (LinkedListNode<Tuple<int, int, bool>> node = WallQueue.First; node != null; node = node.Next) {
			item = node.Value;
			TileFramer[item.Item1, item.Item2][1] = false;
			Framing.WallFrame(item.Item1, item.Item2, item.Item3);
		}
		WallQueue = new();
	}
}
public sealed class ConversionHandler
{
	public readonly record struct ConversionSettings(
		bool SquareTileFrame = true,
		bool SquareWallFrame = true,
		bool NetSpam = true
	) { }

	private static Conversion.BlockConversion[] data;
	public const int Keep = -1;
	public const int Break = -2;

	// Would be lovely to have a better way than this.
	internal static int TileIndex(int index, int type) => (TileLoader.TileCount + WallLoader.WallCount) * index + type;
	internal static int WallIndex(int index, int type) => (TileLoader.TileCount + WallLoader.WallCount) * index + TileLoader.TileCount + type;

	internal static void FillData()
	{
		if (data != null) {
			Array.Clear(data);
		}
		data = new Conversion.BlockConversion[(TileLoader.TileCount * ConversionDatabase.conversions.Count) + (TileLoader.TileCount * ConversionDatabase.conversions.Count)];
		for(int x = 0; x < data.Length; x++) {
		}
		foreach (var conv in ConversionDatabase.conversions.Values) {
			conv.Fill(data);
		}
	}

	#region Overload hell
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(Conversion conversion, Point16 coordinate, ConversionSettings? settings = default) => Convert(conversion.Index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(Conversion conversion, Point coordinate, ConversionSettings? settings = default) => Convert(conversion.Index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(Conversion conversion, int i, int j, ConversionSettings? settings = default) => Convert(conversion.Index, i, j, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(string conversion, Point16 coordinate, ConversionSettings? settings = default) => Convert(ConversionDatabase.Conversions[conversion].Index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(string conversion, Point coordinate, ConversionSettings? settings = default) => Convert(ConversionDatabase.Conversions[conversion].Index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(string conversion, int i, int j, ConversionSettings? settings = default) => Convert(ConversionDatabase.Conversions[conversion].Index, i, j, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(int index, Point16 coordinate, ConversionSettings? settings = default) => Convert(index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(int index, Point coordinate, ConversionSettings? settings = default) => Convert(index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(int index, int i, int j, ConversionSettings? settings = default)
	{
		settings ??= new(true, true, true);
		ConvertInternal(index, i, j, settings.Value, ref MemoryMarshal.GetArrayDataReference(data));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(Conversion conversion, Point16 coordinate, int size, ConversionSettings? settings = default) => Convert(conversion.Index, coordinate.X, coordinate.Y, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(Conversion conversion, Point coordinate, int size, ConversionSettings? settings = default) => Convert(conversion.Index, coordinate.X, coordinate.Y, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(Conversion conversion, int i, int j, int size, ConversionSettings? settings = default) => Convert(conversion.Index, i, j, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(string conversion, Point16 coordinate, int size, ConversionSettings? settings = default) => Convert(ConversionDatabase.Conversions[conversion].Index, coordinate.X, coordinate.Y, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(string conversion, Point coordinate, int size, ConversionSettings? settings = default) => Convert(ConversionDatabase.Conversions[conversion].Index, coordinate.X, coordinate.Y, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(string conversion, int i, int j, int size, ConversionSettings? settings = default) => Convert(ConversionDatabase.Conversions[conversion].Index, i, j, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(int index, Point16 coordinate, int size, ConversionSettings? settings = default) => Convert(index, coordinate.X, coordinate.Y, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(int index, Point coordinate, int size, ConversionSettings? settings = default) => Convert(index, coordinate.X, coordinate.Y, size, settings);
	public static unsafe void Convert(int index, int i, int j, int size, ConversionSettings? settings = default) => ConvertSizedInternal(index, i, j, size, settings, &ConvertInternal);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(Conversion conversion, Point16 coordinate, ConversionSettings? settings = default) => ConvertTile(conversion.Index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(Conversion conversion, Point coordinate, ConversionSettings? settings = default) => ConvertTile(conversion.Index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(Conversion conversion, int i, int j, ConversionSettings? settings = default) => ConvertTile(conversion.Index, i, j, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(string conversion, Point16 coordinate, ConversionSettings? settings = default) => ConvertTile(ConversionDatabase.Conversions[conversion].Index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(string conversion, Point coordinate, ConversionSettings? settings = default) => ConvertTile(ConversionDatabase.Conversions[conversion].Index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(string conversion, int i, int j, ConversionSettings? settings = default) => ConvertTile(ConversionDatabase.Conversions[conversion].Index, i, j, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(int index, Point16 coordinate, ConversionSettings? settings = default) => ConvertTile(index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(int index, Point coordinate, ConversionSettings? settings = default) => ConvertTile(index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(int index, int i, int j, ConversionSettings? settings = default) => ConvertTileInternal(index, i, j, settings, ref MemoryMarshal.GetArrayDataReference(data));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(Conversion conversion, Point16 coordinate, int size, ConversionSettings? settings = default) => ConvertTile(conversion.Index, coordinate.X, coordinate.Y, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(Conversion conversion, Point coordinate, int size, ConversionSettings? settings = default) => ConvertTile(conversion.Index, coordinate.X, coordinate.Y, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(Conversion conversion, int i, int j, int size, ConversionSettings? settings = default) => ConvertTile(conversion.Index, i, j, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(string conversion, Point16 coordinate, int size, ConversionSettings? settings = default) => ConvertTile(ConversionDatabase.Conversions[conversion].Index, coordinate.X, coordinate.Y, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(string conversion, Point coordinate, int size, ConversionSettings? settings = default) => ConvertTile(ConversionDatabase.Conversions[conversion].Index, coordinate.X, coordinate.Y, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(string conversion, int i, int j, int size, ConversionSettings? settings = default) => ConvertTile(ConversionDatabase.Conversions[conversion].Index, i, j, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(int index, Point16 coordinate, int size, ConversionSettings? settings = default) => ConvertTile(index, coordinate.X, coordinate.Y, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(int index, Point coordinate, int size, ConversionSettings? settings = default) => ConvertTile(index, coordinate.X, coordinate.Y, size, settings);
	public static unsafe void ConvertTile(int index, int i, int j, int size, ConversionSettings? settings = default) => ConvertSizedInternal(index, i, j, size, settings, &ConvertTileInternal);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(Conversion conversion, Point16 coordinate, ConversionSettings? settings = default) => ConvertWall(conversion.Index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(Conversion conversion, Point coordinate, ConversionSettings? settings = default) => ConvertWall(conversion.Index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(Conversion conversion, int i, int j, ConversionSettings? settings = default) => ConvertWall(conversion.Index, i, j, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(string conversion, Point16 coordinate, ConversionSettings? settings = default) => ConvertWall(ConversionDatabase.Conversions[conversion].Index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(string conversion, Point coordinate, ConversionSettings? settings = default) => ConvertWall(ConversionDatabase.Conversions[conversion].Index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(string conversion, int i, int j, ConversionSettings? settings = default) => ConvertWall(ConversionDatabase.Conversions[conversion].Index, i, j, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(int index, Point16 coordinate, ConversionSettings? settings = default) => ConvertWall(index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(int index, Point coordinate, ConversionSettings? settings = default) => ConvertWall(index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(int index, int i, int j, ConversionSettings? settings = default) => ConvertWallInternal(index, i, j, settings, ref MemoryMarshal.GetArrayDataReference(data));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(Conversion conversion, Point16 coordinate, int size, ConversionSettings? settings = default) => ConvertWall(conversion.Index, coordinate.X, coordinate.Y, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(Conversion conversion, Point coordinate, int size, ConversionSettings? settings = default) => ConvertWall(conversion.Index, coordinate.X, coordinate.Y, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(Conversion conversion, int i, int j, int size, ConversionSettings? settings = default) => ConvertWall(conversion.Index, i, j, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(string conversion, Point16 coordinate, int size, ConversionSettings? settings = default) => ConvertWall(ConversionDatabase.Conversions[conversion].Index, coordinate.X, coordinate.Y, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(string conversion, Point coordinate, int size, ConversionSettings? settings = default) => ConvertWall(ConversionDatabase.Conversions[conversion].Index, coordinate.X, coordinate.Y, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(string conversion, int i, int j, int size, ConversionSettings? settings = default) => ConvertWall(ConversionDatabase.Conversions[conversion].Index, i, j, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(int index, Point16 coordinate, int size, ConversionSettings? settings = default) => ConvertWall(index, coordinate.X, coordinate.Y, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(int index, Point coordinate, int size, ConversionSettings? settings = default) => ConvertWall(index, coordinate.X, coordinate.Y, size, settings);
	public static unsafe void ConvertWall(int index, int i, int j, int size, ConversionSettings? settings = default) => ConvertSizedInternal(index, i, j, size, settings, &ConvertWallInternal);
	#endregion

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static unsafe void ConvertSizedInternal(int index, int i, int j, int size, ConversionSettings? settings, delegate* managed<int, int, int, ConversionSettings?, ref Conversion.BlockConversion, void> convertCall)
	{
		settings ??= new ConversionSettings(true, true, true);

		ref var arrayData = ref MemoryMarshal.GetArrayDataReference(data);

		int startX = i - size;
		int endX = i + size;
		int startY = j - size;
		int endY = j + size;
		for (int l = startX; l <= endX; l++) {
			int k = startY;
			for (; k <= endY - (endY % 4); k += 4) {
				if (WorldGen.InWorld(l, k, 1) && Math.Abs(l - i) + Math.Abs(k - j) < 6) {
					convertCall(index, l, k, settings.Value, ref arrayData);
				}
				if (WorldGen.InWorld(l, k + 1, 1) && Math.Abs(l - i) + Math.Abs(k - j + 1) < 6) {
					convertCall(index, l, k + 1, settings.Value, ref arrayData);
				}
				if (WorldGen.InWorld(l, k + 2, 1) && Math.Abs(l - i) + Math.Abs(k - j + 2) < 6) {
					convertCall(index, l, k + 2, settings.Value, ref arrayData);
				}
				if (WorldGen.InWorld(l, k + 3, 1) && Math.Abs(l - i) + Math.Abs(k - j + 3) < 6) {
					convertCall(index, l, k + 3, settings.Value, ref arrayData);
				}
			}
			for (; k <= endY; k++) {
				if (WorldGen.InWorld(l, k, 1) && Math.Abs(l - i) + Math.Abs(k - j) < 6) {
					convertCall(index, l, k, settings.Value, ref arrayData);
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static void ConvertTileInternal(int index, int i, int j, ConversionSettings? settings, ref Conversion.BlockConversion arrayDataReference)
	{
		settings ??= new ConversionSettings(true, true, true);

		const int wasCalled = 0b0000_0001;
		const int breakTile = 0b0000_0010;
		const int replacedTile = 0b0000_0100;

		var tile = Main.tile[i, j];
		ushort oldTile = tile.TileType;
		var convertedTile = Unsafe.Add(ref arrayDataReference, TileIndex(index, oldTile));

		byte transformations = 0;

		ConvertInternal_RunHooks(ref convertedTile, ref tile, ref i, ref j, settings.Value, out var preConvTileVal);

		const ConversionRunCodeValues mask = ConversionRunCodeValues.Break;
		transformations |= (byte)((int)(preConvTileVal & mask) << 1);

		ConvertInternal_ConvertIfRuns(ref convertedTile, ref preConvTileVal, ref tile, ref tile.TileType, ref oldTile, ref i, ref j, ref transformations, settings.Value, wasCalled, breakTile, replacedTile);

		if (Main.netMode == NetmodeID.MultiplayerClient && (transformations & breakTile) > 0) {
			if ((transformations & breakTile) > 0) {
				WorldGen.KillTile(i, j);
			}
			NetMessage.SendData(MessageID.TileManipulation, number: i, number2: j);
		}
		if ((transformations & replacedTile) > 0) {
			if (settings.Value.SquareTileFrame)
				TileFrameCache.QueueSquareTileFrame(i, j);
			if (settings.Value.NetSpam)
				NetMessage.SendTileSquare(-1, i, j);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static void ConvertWallInternal(int index, int i, int j, ConversionSettings? settings, ref Conversion.BlockConversion arrayDataReference)
	{
		settings ??= new ConversionSettings(true, true, true);

		const int wasCalledW = 0b0001_0000;
		const int breakTileW = 0b0010_0000;
		const int replacedTileW = 0b0100_0000;

		var tile = Main.tile[i, j];
		ushort oldWall = tile.WallType;
		var convertedWall = Unsafe.Add(ref arrayDataReference, WallIndex(index, oldWall));

		byte transformations = 0;

		ConvertInternal_RunHooks(ref convertedWall, ref tile, ref i, ref j, settings.Value, out var preConvWallVal);

		const ConversionRunCodeValues mask = ConversionRunCodeValues.Break;
		transformations |= (byte)((int)(preConvWallVal & mask) << 5);

		ConvertInternal_ConvertIfRuns(ref convertedWall, ref preConvWallVal, ref tile, ref tile.WallType, ref oldWall, ref i, ref j, ref transformations, settings.Value, wasCalledW, breakTileW, replacedTileW);

		if (Main.netMode == NetmodeID.MultiplayerClient && (transformations & breakTileW) > 0) {
			if ((transformations & breakTileW) > 0) {
				WorldGen.KillWall(i, j);
			}
			if (settings.Value.NetSpam)
				NetMessage.SendData(MessageID.TileManipulation, number: i, number2: j);
		}
		if ((transformations & replacedTileW) > 0) {
			if (settings.Value.SquareWallFrame)
				WorldGen.SquareWallFrame(i, j);
			if (settings.Value.NetSpam)
				NetMessage.SendTileSquare(-1, i, j);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static void ConvertInternal(int index, int i, int j, ConversionSettings? settings, ref Conversion.BlockConversion arrayDataReference)
	{
		const int wasCalled =		0b0000_0001;
		const int breakTile =		0b0000_0010;
		const int replacedTile =	0b0000_0100;
		const int wasCalledW =		0b0001_0000;
		const int breakTileW =		0b0010_0000;
		const int replacedTileW =	0b0100_0000;

		var tile = Main.tile[i, j];
		ushort oldTile = tile.TileType;
		ushort oldWall = tile.WallType;
		var convertedTile = Unsafe.Add(ref arrayDataReference, TileIndex(index, oldTile));
		var convertedWall = Unsafe.Add(ref arrayDataReference, WallIndex(index, oldWall));

		byte transformations = 0;

		ConvertInternal_RunHooks(ref convertedTile, ref tile, ref i, ref j, settings.Value, out var preConvTileVal);
		ConvertInternal_RunHooks(ref convertedWall, ref tile, ref i, ref j, settings.Value, out var preConvWallVal);

		const ConversionRunCodeValues mask = ConversionRunCodeValues.Break;
		transformations |= (byte)((int)(preConvTileVal & mask) << 1);
		transformations |= (byte)((int)(preConvWallVal & mask) << 5);

		ConvertInternal_ConvertIfRuns(ref convertedTile, ref preConvTileVal, ref tile, ref tile.TileType, ref oldTile, ref i, ref j, ref transformations, settings.Value, wasCalled, breakTile, replacedTile);
		ConvertInternal_ConvertIfRuns(ref convertedWall, ref preConvWallVal, ref tile, ref tile.WallType, ref oldWall, ref i, ref j, ref transformations, settings.Value, wasCalledW, breakTileW, replacedTileW);

		if (Main.netMode == NetmodeID.MultiplayerClient && (transformations & (breakTile | breakTileW)) > 0) {
			if ((transformations & breakTile) > 0) {
				WorldGen.KillTile(i, j);
			}
			if ((transformations & breakTileW) > 0) {
				WorldGen.KillWall(i, j);
			}
			if (settings.Value.NetSpam)
				NetMessage.SendData(MessageID.TileManipulation, number: i, number2: j);
		}
		if ((transformations & (replacedTile | replacedTileW)) > 0) {
			if (settings.Value.SquareTileFrame && (transformations & replacedTile) > 0)
				TileFrameCache.QueueSquareTileFrame(i, j);
			if (settings.Value.SquareWallFrame && (transformations & replacedTileW) > 0)
				TileFrameCache.QueueSquareWallFrame(i, j);
			if (settings.Value.NetSpam)
				NetMessage.SendTileSquare(-1, i, j);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static void ConvertInternal_RunHooks(ref Conversion.BlockConversion block, ref Tile tile, ref int i, ref int j, ConversionSettings settings, out ConversionRunCodeValues value)
	{
		value = ConversionRunCodeValues.Run;
		/*if (block?.PreConversionHooks != null) {
			foreach (var hook in block?.PreConversionHooks) {
				var hookValue = hook(tile, i, j, settings);
				if (hookValue != ConversionRunCodeValues.Run) {
					value = hookValue;
					break;
				}
			}
		}*/
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static void ConvertInternal_ConvertIfRuns(ref Conversion.BlockConversion block, ref ConversionRunCodeValues value,
		ref Tile tile, ref ushort type, ref ushort oldType, ref int i, ref int j,
		ref byte transformations, ConversionSettings settings,
		byte wasCalled, byte breakBlock, byte replacedBlock)
	{
		if (block.From != block.To && value != ConversionRunCodeValues.DontRun) {
			transformations |= wasCalled;

			int conv = block.To;
			if (conv == Break) {
				transformations |= breakBlock;
			}
			else if (conv >= 0) {
				type = (ushort)conv;
				//block.OnConversionHook?.Invoke(tile, oldType, i, j, settings);
				transformations |= replacedBlock;
			}
		}
	}
}
