using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.ID;

namespace Terraria.ModLoader;
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
	internal static int TileIndex(int index, int type) => (TileLoader.TileCount * index) + type;
	internal static int WallIndex(int index, int type) => (TileLoader.TileCount * ConversionDatabase.conversions.Count) + (WallLoader.WallCount * index) + type;

	internal static void FillData()
	{
		if (data != null) {
			Array.Clear(data);
		}
		data = new Conversion.BlockConversion[(TileLoader.TileCount * ConversionDatabase.conversions.Count) + (TileLoader.TileCount * ConversionDatabase.conversions.Count)];
		foreach (var conv in ConversionDatabase.conversions.Values) {
			conv.Fill(data);
		}
	}

	#region Overload hell
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(Conversion conversion, Point16 coordinate, ConversionSettings settings = new()) => Convert(conversion.Index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(Conversion conversion, Point coordinate, ConversionSettings settings = new()) => Convert(conversion.Index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(Conversion conversion, int i, int j, ConversionSettings settings = new()) => Convert(conversion.Index, i, j, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(string conversion, Point16 coordinate, ConversionSettings settings = new()) => Convert(ConversionDatabase.Conversions[conversion].Index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(string conversion, Point coordinate, ConversionSettings settings = new()) => Convert(ConversionDatabase.Conversions[conversion].Index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(string conversion, int i, int j, ConversionSettings settings = new()) => Convert(ConversionDatabase.Conversions[conversion].Index, i, j, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(int index, Point16 coordinate, ConversionSettings settings = new()) => Convert(index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(int index, Point coordinate, ConversionSettings settings = new()) => Convert(index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(int index, int i, int j, ConversionSettings settings = new()) => ConvertInternal(index, i, j, settings, ref MemoryMarshal.GetArrayDataReference(data));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(Conversion conversion, Point16 coordinate, int size, ConversionSettings settings = new()) => Convert(conversion.Index, coordinate.X, coordinate.Y, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(Conversion conversion, Point coordinate, int size, ConversionSettings settings = new()) => Convert(conversion.Index, coordinate.X, coordinate.Y, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(Conversion conversion, int i, int j, int size, ConversionSettings settings = new()) => Convert(conversion.Index, i, j, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(string conversion, Point16 coordinate, int size, ConversionSettings settings = new()) => Convert(ConversionDatabase.Conversions[conversion].Index, coordinate.X, coordinate.Y, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(string conversion, Point coordinate, int size, ConversionSettings settings = new()) => Convert(ConversionDatabase.Conversions[conversion].Index, coordinate.X, coordinate.Y, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(string conversion, int i, int j, int size, ConversionSettings settings = new()) => Convert(ConversionDatabase.Conversions[conversion].Index, i, j, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(int index, Point16 coordinate, int size, ConversionSettings settings = new()) => Convert(index, coordinate.X, coordinate.Y, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(int index, Point coordinate, int size, ConversionSettings settings = new()) => Convert(index, coordinate.X, coordinate.Y, size, settings);
	public static unsafe void Convert(int index, int i, int j, int size, ConversionSettings settings = new()) => ConvertSizedInternal(index, i, j, size, settings, &ConvertInternal);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(Conversion conversion, Point16 coordinate, ConversionSettings settings = new()) => ConvertTile(conversion.Index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(Conversion conversion, Point coordinate, ConversionSettings settings = new()) => ConvertTile(conversion.Index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(Conversion conversion, int i, int j, ConversionSettings settings = new()) => ConvertTile(conversion.Index, i, j, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(string conversion, Point16 coordinate, ConversionSettings settings = new()) => ConvertTile(ConversionDatabase.Conversions[conversion].Index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(string conversion, Point coordinate, ConversionSettings settings = new()) => ConvertTile(ConversionDatabase.Conversions[conversion].Index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(string conversion, int i, int j, ConversionSettings settings = new()) => ConvertTile(ConversionDatabase.Conversions[conversion].Index, i, j, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(int index, Point16 coordinate, ConversionSettings settings = new()) => ConvertTile(index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(int index, Point coordinate, ConversionSettings settings = new()) => ConvertTile(index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(int index, int i, int j, ConversionSettings settings = new()) => ConvertTileInternal(index, i, j, settings, ref MemoryMarshal.GetArrayDataReference(data));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(Conversion conversion, Point16 coordinate, int size, ConversionSettings settings = new()) => ConvertTile(conversion.Index, coordinate.X, coordinate.Y, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(Conversion conversion, Point coordinate, int size, ConversionSettings settings = new()) => ConvertTile(conversion.Index, coordinate.X, coordinate.Y, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(Conversion conversion, int i, int j, int size, ConversionSettings settings = new()) => ConvertTile(conversion.Index, i, j, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(string conversion, Point16 coordinate, int size, ConversionSettings settings = new()) => ConvertTile(ConversionDatabase.Conversions[conversion].Index, coordinate.X, coordinate.Y, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(string conversion, Point coordinate, int size, ConversionSettings settings = new()) => ConvertTile(ConversionDatabase.Conversions[conversion].Index, coordinate.X, coordinate.Y, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(string conversion, int i, int j, int size, ConversionSettings settings = new()) => ConvertTile(ConversionDatabase.Conversions[conversion].Index, i, j, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(int index, Point16 coordinate, int size, ConversionSettings settings = new()) => ConvertTile(index, coordinate.X, coordinate.Y, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(int index, Point coordinate, int size, ConversionSettings settings = new()) => ConvertTile(index, coordinate.X, coordinate.Y, size, settings);
	public static unsafe void ConvertTile(int index, int i, int j, int size, ConversionSettings settings = new()) => ConvertSizedInternal(index, i, j, size, settings, &ConvertTileInternal);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(Conversion conversion, Point16 coordinate, ConversionSettings settings = new()) => ConvertWall(conversion.Index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(Conversion conversion, Point coordinate, ConversionSettings settings = new()) => ConvertWall(conversion.Index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(Conversion conversion, int i, int j, ConversionSettings settings = new()) => ConvertWall(conversion.Index, i, j, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(string conversion, Point16 coordinate, ConversionSettings settings = new()) => ConvertWall(ConversionDatabase.Conversions[conversion].Index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(string conversion, Point coordinate, ConversionSettings settings = new()) => ConvertWall(ConversionDatabase.Conversions[conversion].Index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(string conversion, int i, int j, ConversionSettings settings = new()) => ConvertWall(ConversionDatabase.Conversions[conversion].Index, i, j, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(int index, Point16 coordinate, ConversionSettings settings = new()) => ConvertWall(index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(int index, Point coordinate, ConversionSettings settings = new()) => ConvertWall(index, coordinate.X, coordinate.Y, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(int index, int i, int j, ConversionSettings settings = new()) => ConvertWallInternal(index, i, j, settings, ref MemoryMarshal.GetArrayDataReference(data));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(Conversion conversion, Point16 coordinate, int size, ConversionSettings settings = new()) => ConvertWall(conversion.Index, coordinate.X, coordinate.Y, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(Conversion conversion, Point coordinate, int size, ConversionSettings settings = new()) => ConvertWall(conversion.Index, coordinate.X, coordinate.Y, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(Conversion conversion, int i, int j, int size, ConversionSettings settings = new()) => ConvertWall(conversion.Index, i, j, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(string conversion, Point16 coordinate, int size, ConversionSettings settings = new()) => ConvertWall(ConversionDatabase.Conversions[conversion].Index, coordinate.X, coordinate.Y, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(string conversion, Point coordinate, int size, ConversionSettings settings = new()) => ConvertWall(ConversionDatabase.Conversions[conversion].Index, coordinate.X, coordinate.Y, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(string conversion, int i, int j, int size, ConversionSettings settings = new()) => ConvertWall(ConversionDatabase.Conversions[conversion].Index, i, j, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(int index, Point16 coordinate, int size, ConversionSettings settings = new()) => ConvertWall(index, coordinate.X, coordinate.Y, size, settings);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(int index, Point coordinate, int size, ConversionSettings settings = new()) => ConvertWall(index, coordinate.X, coordinate.Y, size, settings);
	public static unsafe void ConvertWall(int index, int i, int j, int size, ConversionSettings settings = new()) => ConvertSizedInternal(index, i, j, size, settings, &ConvertWallInternal);
	#endregion

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static unsafe void ConvertSizedInternal(int index, int i, int j, int size, ConversionSettings netSpam, delegate* managed<int, int, int, ConversionSettings, ref Conversion.BlockConversion, void> convertCall)
	{
		ref var arrayData = ref MemoryMarshal.GetArrayDataReference(data);

		int startX = i - size;
		int endX = i + size;
		int startY = j - size;
		int endY = j + size;
		for (int l = startX; l <= endX; l++) {
			int k = startY;
			for (; k <= endY - (endY % 4); k += 4) {
				if (WorldGen.InWorld(l, k, 1) && Math.Abs(l - i) + Math.Abs(k - j) < 6) {
					convertCall(index, i, j, netSpam, ref arrayData);
				}
				if (WorldGen.InWorld(l, k + 1, 1) && Math.Abs(l - i) + Math.Abs(k - j + 1) < 6) {
					convertCall(index, i, j, netSpam, ref arrayData);
				}
				if (WorldGen.InWorld(l, k + 2, 1) && Math.Abs(l - i) + Math.Abs(k - j + 2) < 6) {
					convertCall(index, i, j, netSpam, ref arrayData);
				}
				if (WorldGen.InWorld(l, k + 3, 1) && Math.Abs(l - i) + Math.Abs(k - j + 3) < 6) {
					convertCall(index, i, j, netSpam, ref arrayData);
				}
			}
			for (; k <= endY; k++) {
				if (WorldGen.InWorld(l, k, 1) && Math.Abs(l - i) + Math.Abs(k - j) < 6) {
					convertCall(index, i, j, netSpam, ref arrayData);
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static void ConvertTileInternal(int index, int i, int j, ConversionSettings netSpam, ref Conversion.BlockConversion arrayDataReference)
	{
		const int wasCalled = 0b0000_0001;
		const int breakTile = 0b0000_0010;
		const int replacedTile = 0b0000_0100;

		var tile = Main.tile[i, j];
		ushort oldTile = tile.TileType;
		var convertedTile = Unsafe.Add(ref arrayDataReference, TileIndex(index, oldTile));

		byte transformations = 0;

		ConvertInternal_RunHooks(ref convertedTile, ref tile, ref i, ref j, ref netSpam, out var preConvTileVal);

		const ConversionRunCodeValues mask = ConversionRunCodeValues.Break;
		transformations |= (byte)((int)(preConvTileVal & mask) << 1);

		ConvertInternal_ConvertIfRuns(ref convertedTile, ref preConvTileVal, ref tile, ref tile.TileType, ref oldTile, ref i, ref j, ref transformations, ref netSpam, wasCalled, breakTile, replacedTile);

		if (Main.netMode == NetmodeID.MultiplayerClient && (transformations & breakTile) > 0) {
			if ((transformations & breakTile) > 0) {
				WorldGen.KillTile(i, j);
			}
			NetMessage.SendData(MessageID.TileManipulation, number: i, number2: j);
		}
		if ((transformations & replacedTile) > 0) {
			if (netSpam.SquareTileFrame)
				WorldGen.SquareTileFrame(i, j);
			if (netSpam.NetSpam)
				NetMessage.SendTileSquare(-1, i, j);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static void ConvertWallInternal(int index, int i, int j, ConversionSettings netSpam, ref Conversion.BlockConversion arrayDataReference)
	{
		const int wasCalledW = 0b0001_0000;
		const int breakTileW = 0b0010_0000;
		const int replacedTileW = 0b0100_0000;

		var tile = Main.tile[i, j];
		ushort oldWall = tile.WallType;
		var convertedWall = Unsafe.Add(ref arrayDataReference, WallIndex(index, oldWall));

		byte transformations = 0;

		ConvertInternal_RunHooks(ref convertedWall, ref tile, ref i, ref j, ref netSpam, out var preConvWallVal);

		const ConversionRunCodeValues mask = ConversionRunCodeValues.Break;
		transformations |= (byte)((int)(preConvWallVal & mask) << 5);

		ConvertInternal_ConvertIfRuns(ref convertedWall, ref preConvWallVal, ref tile, ref tile.WallType, ref oldWall, ref i, ref j, ref transformations, ref netSpam, wasCalledW, breakTileW, replacedTileW);

		if (Main.netMode == NetmodeID.MultiplayerClient && (transformations & breakTileW) > 0) {
			if ((transformations & breakTileW) > 0) {
				WorldGen.KillWall(i, j);
			}
			if (netSpam.NetSpam)
				NetMessage.SendData(MessageID.TileManipulation, number: i, number2: j);
		}
		if ((transformations & replacedTileW) > 0) {
			if (netSpam.SquareWallFrame)
				WorldGen.SquareWallFrame(i, j);
			if (netSpam.NetSpam)
				NetMessage.SendTileSquare(-1, i, j);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static void ConvertInternal(int index, int i, int j, ConversionSettings settings, ref Conversion.BlockConversion arrayDataReference)
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

		ConvertInternal_RunHooks(ref convertedTile, ref tile, ref i, ref j, ref settings, out var preConvTileVal);
		ConvertInternal_RunHooks(ref convertedWall, ref tile, ref i, ref j, ref settings, out var preConvWallVal);

		const ConversionRunCodeValues mask = ConversionRunCodeValues.Break;
		transformations |= (byte)((int)(preConvTileVal & mask) << 1);
		transformations |= (byte)((int)(preConvWallVal & mask) << 5);

		ConvertInternal_ConvertIfRuns(ref convertedTile, ref preConvTileVal, ref tile, ref tile.TileType, ref oldTile, ref i, ref j, ref transformations, ref settings, wasCalled, breakTile, replacedTile);
		ConvertInternal_ConvertIfRuns(ref convertedWall, ref preConvWallVal, ref tile, ref tile.WallType, ref oldWall, ref i, ref j, ref transformations, ref settings, wasCalledW, breakTileW, replacedTileW);

		if (Main.netMode == NetmodeID.MultiplayerClient && (transformations & (breakTile | breakTileW)) > 0) {
			if ((transformations & breakTile) > 0) {
				WorldGen.KillTile(i, j);
			}
			if ((transformations & breakTileW) > 0) {
				WorldGen.KillWall(i, j);
			}
			if (settings.NetSpam)
				NetMessage.SendData(MessageID.TileManipulation, number: i, number2: j);
		}
		if ((transformations & (replacedTile | replacedTileW)) > 0) {
			if (settings.SquareTileFrame && (transformations & replacedTile) > 0)
				WorldGen.SquareTileFrame(i, j);
			if (settings.SquareWallFrame && (transformations & replacedTileW) > 0)
				WorldGen.SquareWallFrame(i, j);
			if (settings.NetSpam)
				NetMessage.SendTileSquare(-1, i, j);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static void ConvertInternal_RunHooks(ref Conversion.BlockConversion block, ref Tile tile, ref int i, ref int j, ref ConversionSettings settings, out ConversionRunCodeValues value)
	{
		value = ConversionRunCodeValues.Run;
		if (block?.PreConversionHooks != null) {
			foreach (var hook in block?.PreConversionHooks) {
				var hookValue = hook(tile, i, j, settings);
				if (hookValue != ConversionRunCodeValues.Run) {
					value = hookValue;
					break;
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static void ConvertInternal_ConvertIfRuns(ref Conversion.BlockConversion block, ref ConversionRunCodeValues value,
		ref Tile tile, ref ushort type, ref ushort oldType, ref int i, ref int j,
		ref byte transformations, ref ConversionSettings settings,
		byte wasCalled, byte breakBlock, byte replacedBlock)
	{
		if (block != null && value != ConversionRunCodeValues.DontRun) {
			transformations |= wasCalled;

			int conv = block.To;
			if (conv == Break) {
				transformations |= breakBlock;
			}
			else if (conv >= 0) {
				type = (ushort)conv;
				block.OnConversionHook?.Invoke(tile, oldType, i, j, settings);
				transformations |= replacedBlock;
			}
		}
	}
}
