using System.Collections.Generic;

namespace Terraria.ModLoader;

public sealed class Conversion
{
	public sealed record class BlockConversion(int From, int To, bool IsTile)
	{
		public delegate ConversionRunCodeValues PreConversionDelegate(Tile tile, int i, int j, ConversionHandler.ConversionSettings settings);
		public delegate void OnConversionDelegate(Tile tile, int oldTileType, int i, int j, ConversionHandler.ConversionSettings settings);

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
