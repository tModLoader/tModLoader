using System;
using System.Runtime.CompilerServices;
using Terraria.ID;

namespace Terraria
{
	public readonly partial struct Tile
	{
		public ref ushort TileType => ref Get<TileTypeData>().Type;
		public ref ushort WallType => ref Get<WallTypeData>().Type;
		public ref short TileFrameX => ref Get<TileWallWireStateData>().TileFrameX;
		public ref short TileFrameY => ref Get<TileWallWireStateData>().TileFrameY;

		public ref byte LiquidAmount => ref Get<LiquidData>().Amount;
		public int LiquidType { get => Get<LiquidData>().LiquidType; set => Get<LiquidData>().LiquidType = value; }
		public bool CheckingLiquid { get => Get<LiquidData>().CheckingLiquid; set => Get<LiquidData>().CheckingLiquid = value; }
		public bool SkipLiquid { get => Get<LiquidData>().SkipLiquid; set => Get<LiquidData>().SkipLiquid = value; }

		public bool HasTile { get => Get<TileWallWireStateData>().HasTile; set => Get<TileWallWireStateData>().HasTile = value; }
		public bool IsActuated { get => Get<TileWallWireStateData>().IsActuated; set => Get<TileWallWireStateData>().IsActuated = value; }
		public bool HasActuator { get => Get<TileWallWireStateData>().HasActuator; set => Get<TileWallWireStateData>().HasActuator = value; }
		public bool IsHalfBlock { get => Get<TileWallWireStateData>().IsHalfBlock; set => Get<TileWallWireStateData>().IsHalfBlock = value; }
		public bool RedWire { get => Get<TileWallWireStateData>().RedWire; set => Get<TileWallWireStateData>().RedWire = value; }
		public bool GreenWire { get => Get<TileWallWireStateData>().GreenWire; set => Get<TileWallWireStateData>().GreenWire = value; }
		public bool BlueWire { get => Get<TileWallWireStateData>().BlueWire; set => Get<TileWallWireStateData>().BlueWire = value; }
		public bool YellowWire { get => Get<TileWallWireStateData>().YellowWire; set => Get<TileWallWireStateData>().YellowWire = value; }
		public byte TileColor { get => Get<TileWallWireStateData>().TileColor; set => Get<TileWallWireStateData>().TileColor = value; }
		public byte WallColor { get => Get<TileWallWireStateData>().WallColor; set => Get<TileWallWireStateData>().WallColor = value; }
		public int WallFrameX { get => Get<TileWallWireStateData>().WallFrameX; set => Get<TileWallWireStateData>().WallFrameX = value; }
		public int WallFrameY { get => Get<TileWallWireStateData>().WallFrameY; set => Get<TileWallWireStateData>().WallFrameY = value; }
		public SlopeType Slope { get => Get<TileWallWireStateData>().Slope; set => Get<TileWallWireStateData>().Slope = value; }
		public int TileFrameNumber { get => Get<TileWallWireStateData>().TileFrameNumber; set => Get<TileWallWireStateData>().TileFrameNumber = value; }
		public int WallFrameNumber { get => Get<TileWallWireStateData>().WallFrameNumber; set => Get<TileWallWireStateData>().WallFrameNumber = value; }

		public bool HasUnactuatedTile => HasTile && !IsActuated;

		public BlockType BlockType { get => Get<TileWallWireStateData>().BlockType; set => Get<TileWallWireStateData>().BlockType = value; }

		public override int GetHashCode() => (int)TileId;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe ref T Get<T>() where T : unmanaged, ITileData
			=> ref TileData<T>.ptr[TileId];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Tile tile, Tile tile2)
			=> tile.TileId == tile2.TileId;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(Tile tile, Tile tile2)
			=> tile.TileId != tile2.TileId;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Tile tile, ArgumentException justSoYouCanCompareWithNull) => false;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(Tile tile, ArgumentException justSoYouCanCompareWithNull) => true; }
}