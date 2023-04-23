using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.ID;

namespace Terraria.ModLoader;

public enum ConversionRunCodeValues : sbyte
{
	DontRun = -2,
	Run = 0,
	Break = 1,
}

public sealed class Conversion
{
	public sealed record class BlockConversion(int From, int To, bool IsTile)
	{
		public delegate ConversionRunCodeValues PreConversionDelegate(Tile tile, int i, int j);
		public delegate void OnConversionDelegate(Tile tile, int oldTileType, int i, int j);

		public LinkedList<PreConversionDelegate> PreConversionHooks;
		public OnConversionDelegate OnConversionHook;

		public BlockConversion PreConversion(PreConversionDelegate preConversionHook)
		{
			PreConversionHooks ??= new();
			PreConversionHooks.AddLast(preConversionHook);
			return this;
		}

		public BlockConversion OnConversion(OnConversionDelegate onConversionHook)
		{
			OnConversionHook += onConversionHook;
			return this;
		}
	}

	private readonly LinkedList<BlockConversion> blockConversions = new();

	public IReadOnlyCollection<BlockConversion> BlockConversions => blockConversions;

	public ushort Index { get; private set; }
	public string Name { get; }

	public Conversion(string name)
	{
		Name = name;
	}

	public BlockConversion ConvertTile(int from, int to)
	{
		var blockConversion = new BlockConversion(from, to, true);
		blockConversions.AddLast(blockConversion);
		return blockConversion;
	}
	public BlockConversion ConvertTile<TFrom>(int to) where TFrom : ModTile => ConvertTile(ModContent.TileType<TFrom>(), to);
	public BlockConversion ConvertTile<TFrom, TTo>() where TFrom : ModTile where TTo : ModTile => ConvertTile(ModContent.TileType<TFrom>(), ModContent.TileType<TTo>());

	public BlockConversion ConvertWall(int from, int to)
	{
		var blockConversion = new BlockConversion(from, to, false);
		blockConversions.AddLast(blockConversion);
		return blockConversion;
	}
	public BlockConversion ConvertWall<TFrom>(int to) where TFrom : ModWall => ConvertWall(ModContent.WallType<TFrom>(), to);
	public BlockConversion ConvertWall<TFrom, TTo>() where TFrom : ModWall where TTo : ModWall => ConvertWall(ModContent.WallType<TFrom>(), ModContent.WallType<TTo>());

	internal void Fill(BlockConversion[] data)
	{
		foreach (var conversion in blockConversions) {
			if (conversion.IsTile) {
				data[ConversionHandler.TileIndex(Index, conversion.From)] = conversion;
			}
			else {
				data[ConversionHandler.WallIndex(Index, conversion.From)] = conversion;
			}
		}
	}

	public void Register()
	{
		Index = (ushort)ConversionDatabase.conversions.Count;
		ConversionDatabase.conversions[Name] = this;
	}
}
public sealed class ConversionHandler
{
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
	public static void Convert(Conversion conversion, Point16 coordinate) => Convert(conversion.Index, coordinate.X, coordinate.Y);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(Conversion conversion, Point coordinate) => Convert(conversion.Index, coordinate.X, coordinate.Y);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(Conversion conversion, int i, int j) => Convert(conversion.Index, i, j);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(int index, Point16 coordinate) => Convert(index, coordinate.X, coordinate.Y);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(int index, Point coordinate) => Convert(index, coordinate.X, coordinate.Y);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(int index, int i, int j) => ConvertInternal(index, i, j, ref MemoryMarshal.GetArrayDataReference(data));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(Conversion conversion, Point16 coordinate, int size) => Convert(conversion.Index, coordinate.X, coordinate.Y, size);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(Conversion conversion, Point coordinate, int size) => Convert(conversion.Index, coordinate.X, coordinate.Y, size);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(Conversion conversion, int i, int j, int size) => Convert(conversion.Index, i, j, size);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(int index, Point16 coordinate, int size) => Convert(index, coordinate.X, coordinate.Y, size);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Convert(int index, Point coordinate, int size) => Convert(index, coordinate.X, coordinate.Y, size);
	public static unsafe void Convert(int index, int i, int j, int size) => ConvertSizedInternal(index, i, j, size, &ConvertInternal);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(Conversion conversion, Point16 coordinate) => ConvertTile(conversion.Index, coordinate.X, coordinate.Y);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(Conversion conversion, Point coordinate) => ConvertTile(conversion.Index, coordinate.X, coordinate.Y);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(Conversion conversion, int i, int j) => ConvertTile(conversion.Index, i, j);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(int index, Point16 coordinate) => ConvertTile(index, coordinate.X, coordinate.Y);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(int index, Point coordinate) => ConvertTile(index, coordinate.X, coordinate.Y);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(int index, int i, int j) => ConvertTileInternal(index, i, j, ref MemoryMarshal.GetArrayDataReference(data));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(Conversion conversion, Point16 coordinate, int size) => ConvertTile(conversion.Index, coordinate.X, coordinate.Y, size);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(Conversion conversion, Point coordinate, int size) => ConvertTile(conversion.Index, coordinate.X, coordinate.Y, size);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(Conversion conversion, int i, int j, int size) => ConvertTile(conversion.Index, i, j, size);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(int index, Point16 coordinate, int size) => ConvertTile(index, coordinate.X, coordinate.Y, size);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertTile(int index, Point coordinate, int size) => ConvertTile(index, coordinate.X, coordinate.Y, size);
	public static unsafe void ConvertTile(int index, int i, int j, int size) => ConvertSizedInternal(index, i, j, size, &ConvertTileInternal);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(Conversion conversion, Point16 coordinate) => ConvertWall(conversion.Index, coordinate.X, coordinate.Y);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(Conversion conversion, Point coordinate) => ConvertWall(conversion.Index, coordinate.X, coordinate.Y);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(Conversion conversion, int i, int j) => ConvertWall(conversion.Index, i, j);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(int index, Point16 coordinate) => ConvertWall(index, coordinate.X, coordinate.Y);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(int index, Point coordinate) => ConvertWall(index, coordinate.X, coordinate.Y);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(int index, int i, int j) => ConvertWallInternal(index, i, j, ref MemoryMarshal.GetArrayDataReference(data));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(Conversion conversion, Point16 coordinate, int size) => ConvertWall(conversion.Index, coordinate.X, coordinate.Y, size);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(Conversion conversion, Point coordinate, int size) => ConvertWall(conversion.Index, coordinate.X, coordinate.Y, size);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(Conversion conversion, int i, int j, int size) => ConvertWall(conversion.Index, i, j, size);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(int index, Point16 coordinate, int size) => ConvertWall(index, coordinate.X, coordinate.Y, size);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConvertWall(int index, Point coordinate, int size) => ConvertWall(index, coordinate.X, coordinate.Y, size);
	public static unsafe void ConvertWall(int index, int i, int j, int size) => ConvertSizedInternal(index, i, j, size, &ConvertWallInternal);
	#endregion

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static unsafe void ConvertSizedInternal(int index, int i, int j, int size, delegate* managed<int, int, int, ref Conversion.BlockConversion, void> convertCall)
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
					convertCall(index, i, j, ref arrayData);
				}
				if (WorldGen.InWorld(l, k + 1, 1) && Math.Abs(l - i) + Math.Abs(k - j + 1) < 6) {
					convertCall(index, i, j, ref arrayData);
				}
				if (WorldGen.InWorld(l, k + 2, 1) && Math.Abs(l - i) + Math.Abs(k - j + 2) < 6) {
					convertCall(index, i, j, ref arrayData);
				}
				if (WorldGen.InWorld(l, k + 3, 1) && Math.Abs(l - i) + Math.Abs(k - j + 3) < 6) {
					convertCall(index, i, j, ref arrayData);
				}
			}
			for (; k <= endY; k++) {
				if (WorldGen.InWorld(l, k, 1) && Math.Abs(l - i) + Math.Abs(k - j) < 6) {
					convertCall(index, i, j, ref arrayData);
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static void ConvertTileInternal(int index, int i, int j, ref Conversion.BlockConversion arrayDataReference)
	{
		const int wasCalled = 0b0000_0001;
		const int breakTile = 0b0000_0010;
		const int replacedTile = 0b0000_0100;

		var tile = Main.tile[i, j];
		ushort oldTile = tile.TileType;
		var convertedTile = Unsafe.Add(ref arrayDataReference, TileIndex(index, oldTile));

		byte transformations = 0;

		ConvertInternal_RunHooks(ref convertedTile, ref tile, ref i, ref j, out var preConvTileVal);

		const ConversionRunCodeValues mask = ConversionRunCodeValues.Break;
		transformations |= (byte)((int)(preConvTileVal & mask) << 1);

		ConvertInternal_ConvertIfRuns(ref convertedTile, ref preConvTileVal, ref tile, ref tile.TileType, ref oldTile, ref i, ref j, ref transformations, wasCalled, breakTile, replacedTile);

		if (Main.netMode == NetmodeID.MultiplayerClient && (transformations & breakTile) > 0) {
			if ((transformations & breakTile) > 0) {
				WorldGen.KillTile(i, j);
			}
			NetMessage.SendData(MessageID.TileManipulation, number: i, number2: j);
		}
		if ((transformations & replacedTile) > 0) {
			WorldGen.SquareTileFrame(i, j);
			NetMessage.SendTileSquare(-1, i, j);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static void ConvertWallInternal(int index, int i, int j, ref Conversion.BlockConversion arrayDataReference)
	{
		const int wasCalledW = 0b0001_0000;
		const int breakTileW = 0b0010_0000;
		const int replacedTileW = 0b0100_0000;

		var tile = Main.tile[i, j];
		ushort oldWall = tile.WallType;
		var convertedWall = Unsafe.Add(ref arrayDataReference, WallIndex(index, oldWall));

		byte transformations = 0;

		ConvertInternal_RunHooks(ref convertedWall, ref tile, ref i, ref j, out var preConvWallVal);

		const ConversionRunCodeValues mask = ConversionRunCodeValues.Break;
		transformations |= (byte)((int)(preConvWallVal & mask) << 5);

		ConvertInternal_ConvertIfRuns(ref convertedWall, ref preConvWallVal, ref tile, ref tile.WallType, ref oldWall, ref i, ref j, ref transformations, wasCalledW, breakTileW, replacedTileW);

		if (Main.netMode == NetmodeID.MultiplayerClient && (transformations & breakTileW) > 0) {
			if ((transformations & breakTileW) > 0) {
				WorldGen.KillWall(i, j);
			}
			NetMessage.SendData(MessageID.TileManipulation, number: i, number2: j);
		}
		if ((transformations & replacedTileW) > 0) {
			WorldGen.SquareTileFrame(i, j);
			NetMessage.SendTileSquare(-1, i, j);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static void ConvertInternal(int index, int i, int j, ref Conversion.BlockConversion arrayDataReference)
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

		ConvertInternal_RunHooks(ref convertedTile, ref tile, ref i, ref j, out var preConvTileVal);
		ConvertInternal_RunHooks(ref convertedWall, ref tile, ref i, ref j, out var preConvWallVal);

		const ConversionRunCodeValues mask = ConversionRunCodeValues.Break;
		transformations |= (byte)((int)(preConvTileVal & mask) << 1);
		transformations |= (byte)((int)(preConvWallVal & mask) << 5);

		ConvertInternal_ConvertIfRuns(ref convertedTile, ref preConvTileVal, ref tile, ref tile.TileType, ref oldTile, ref i, ref j, ref transformations, wasCalled, breakTile, replacedTile);
		ConvertInternal_ConvertIfRuns(ref convertedWall, ref preConvWallVal, ref tile, ref tile.WallType, ref oldWall, ref i, ref j, ref transformations, wasCalledW, breakTileW, replacedTileW);

		if (Main.netMode == NetmodeID.MultiplayerClient && (transformations & (breakTile | breakTileW)) > 0) {
			if ((transformations & breakTile) > 0) {
				WorldGen.KillTile(i, j);
			}
			if ((transformations & breakTileW) > 0) {
				WorldGen.KillWall(i, j);
			}
			NetMessage.SendData(MessageID.TileManipulation, number: i, number2: j);
		}
		if ((transformations & (replacedTile | replacedTileW)) > 0) {
			WorldGen.SquareTileFrame(i, j);
			NetMessage.SendTileSquare(-1, i, j);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static void ConvertInternal_RunHooks(ref Conversion.BlockConversion block, ref Tile tile, ref int i, ref int j, out ConversionRunCodeValues value)
	{
		value = ConversionRunCodeValues.Run;
		if (block?.PreConversionHooks != null) {
			foreach (var hook in block?.PreConversionHooks) {
				var hookValue = hook(tile, i, j);
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
		ref byte transformations, byte wasCalled, byte breakBlock, byte replacedBlock)
	{
		if (block != null && value != ConversionRunCodeValues.DontRun) {
			transformations |= wasCalled;

			int conv = block.To;
			if (conv == Break) {
				transformations |= breakBlock;
			}
			else if (conv >= 0) {
				type = (ushort)conv;
				block.OnConversionHook?.Invoke(tile, oldType, i, j);
				transformations |= replacedBlock;
			}
		}
	}
}

public static class ConversionDatabase
{
	internal static Dictionary<string, Conversion> conversions = new();

	public static IReadOnlyDictionary<string, Conversion> Conversions => conversions;

	public static Conversion GetByName(string name) => conversions[name];

	internal static void Load()
	{
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
				conv.ConvertWall(wall, 64).PreConversion((tile, k, l) => {
					if (l < Main.worldSurface) {
						if (WorldGen.genRand.Next(10) == 0)
							tile.wall = 65;
						else
							tile.wall = 63;

						WorldGen.SquareWallFrame(k, l);
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
				conv.ConvertWall(wall, 64).PreConversion((tile, k, l) => {
					if (l < Main.worldSurface + 4.0 + WorldGen.genRand.Next(3) || l > (Main.maxTilesY + Main.rockLayer) / 2.0 - 3.0 + WorldGen.genRand.Next(3)) {
						tile.wall = 15;
						WorldGen.SquareWallFrame(k, l);
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
				conv.ConvertTile(type, 0).PreConversion((tile, k, l) => {
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
				conv.ConvertTile(type, 53).PreConversion((tile, k, l) => {
					if (WorldGen.BlockBelowMakesSandConvertIntoHardenedSand(k, l)) {
						tile.TileType = 397;
						WorldGen.SquareTileFrame(k, l);
						NetMessage.SendTileSquare(-1, k, l);
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
				conv.ConvertTile(type, 0).PreConversion((tile, k, l) => {
					if (WorldGen.TileIsExposedToAir(k, l)) {
						tile.TileType = 2;
						WorldGen.SquareTileFrame(k, l);
						NetMessage.SendTileSquare(-1, k, l);
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

	private static void TryKillingTreesAboveIfTheyWouldBecomeInvalid(Tile tile, int oldTileType, int i, int j)
	{
		WorldGen.TryKillingTreesAboveIfTheyWouldBecomeInvalid(i, j, tile.TileType);
	}
}