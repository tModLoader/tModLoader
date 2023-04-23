using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
			PreConversionHooks.AddLast(preConversionHook);
			return this;
		}

		public BlockConversion OnConversion(OnConversionDelegate onConversionHook)
		{
			OnConversionHook += onConversionHook;
			return this;
		}
	}

	private readonly List<BlockConversion> blockConversions = new();

	public ushort Index { get; private set; }
	public string Name { get; }

	public Conversion(string name)
	{
		Name = name;
	}

	public BlockConversion ConvertTile(int from, int to)
	{
		blockConversions.Add(new BlockConversion(from, to, true));
		return blockConversions[^1];
	}

	public BlockConversion ConvertWall(int from, int to)
	{
		blockConversions.Add(new BlockConversion(from, to, false));
		return blockConversions[^1];
	}

	internal void Fill(BlockConversion[] data)
	{
		blockConversions.ForEach(conversion => {
			if (conversion.IsTile) {
				data[ConversionHandler.TileIndex(Index, conversion.To)] = conversion;
			}
			else {
				data[ConversionHandler.WallIndex(Index, conversion.To)] = conversion;
			}
		});
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
		foreach (var conv in ConversionDatabase.conversions.Values) {
			conv.Fill(data);
		}
	}

	public static void Convert(int index, int i, int j, int size)
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
					ConvertInternal(index, i, j, ref arrayData);
				}
				if (WorldGen.InWorld(l, k + 1, 1) && Math.Abs(l - i) + Math.Abs(k - j + 1) < 6) {
					ConvertInternal(index, i, j, ref arrayData);
				}
				if (WorldGen.InWorld(l, k + 2, 1) && Math.Abs(l - i) + Math.Abs(k - j + 2) < 6) {
					ConvertInternal(index, i, j, ref arrayData);
				}
				if (WorldGen.InWorld(l, k + 3, 1) && Math.Abs(l - i) + Math.Abs(k - j + 3) < 6) {
					ConvertInternal(index, i, j, ref arrayData);
				}
			}
			for (; k <= endY; k++) {
				if (WorldGen.InWorld(l, k, 1) && Math.Abs(l - i) + Math.Abs(k - j) < 6) {
					ConvertInternal(index, i, j, ref arrayData);
				}
			}
		}
	}

	private static void ConvertInternal(int index, int i, int j, ref Conversion.BlockConversion arrayDataReference)
	{
		// TODO: Use pointers here somehow to maybe optimize this much more (mostly delegates)
		// because they're ahhhhh slow :(

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

		var preConvTileVal = ConversionRunCodeValues.Run;
		var preConvWallVal = ConversionRunCodeValues.Run;
		if (convertedTile?.PreConversionHooks != null) {
			foreach (var hook in convertedTile?.PreConversionHooks) {
				var hookValue = hook(tile, i, j);
				if (hookValue != ConversionRunCodeValues.Run) {
					preConvTileVal = hookValue;
					break;
				}
			}
		}
		if (convertedWall?.PreConversionHooks != null) {
			foreach (var hook in convertedWall?.PreConversionHooks) {
				var hookValue = hook(tile, i, j);
				if (hookValue != ConversionRunCodeValues.Run) {
					preConvWallVal = hookValue;
					break;
				}
			}
		}

		const ConversionRunCodeValues mask = ConversionRunCodeValues.Break;
		// those match breakTile
		transformations |= (byte)((int)(preConvTileVal & mask) << 1);
		transformations |= (byte)((int)(preConvWallVal & mask) << 5);

		if (convertedTile != null && preConvTileVal != ConversionRunCodeValues.DontRun) {
			transformations |= wasCalled;

			int conv = convertedTile.To;
			if (conv == Break) {
				transformations |= breakTile;
			}
			else if (conv >= 0) {
				tile.TileType = (ushort)conv;
				convertedTile.OnConversionHook?.Invoke(tile, oldWall, i, j);
				transformations |= replacedTile;
			}
		}

		if (convertedWall != null && preConvWallVal != ConversionRunCodeValues.DontRun) {
			transformations |= wasCalledW;

			int conv = convertedWall.To;
			if (conv == Break) {
				transformations |= breakTileW;
			}
			else if (conv >= 0) {
				tile.WallType = (ushort)conv;
				convertedWall.OnConversionHook?.Invoke(tile, oldWall, i, j);
				transformations |= replacedTileW;
			}
		}

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
}

public static class ConversionDatabase
{
	internal static Dictionary<string, Conversion> conversions = new();

	public static IReadOnlyDictionary<string, Conversion> Conversions => conversions;

	public static Conversion GetByName(string name) => conversions[name];

	internal static void Load()
	{
		conversions.Clear();

		// TODO: Add vanilla conversions here.
	}
}