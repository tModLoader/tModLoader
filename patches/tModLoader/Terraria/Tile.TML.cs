using System;
using System.Runtime.CompilerServices;
using Terraria.ID;

#pragma warning disable IDE0060 // Remove unused parameter

namespace Terraria;

#if TILE_X_Y
[StructLayout(LayoutKind.Sequential, Pack = 2)]
#endif
public readonly partial struct Tile
{
	internal readonly uint TileId;
#if TILE_X_Y
	public readonly ushort X;
	public readonly ushort Y;
#endif

	// General state

	public ref ushort TileType => ref Get<TileTypeData>().Type;
	public ref ushort WallType => ref Get<WallTypeData>().Type;

	public bool HasTile { get => Get<TileWallWireStateData>().HasTile; set => Get<TileWallWireStateData>().HasTile = value; }
	public bool IsActuated { get => Get<TileWallWireStateData>().IsActuated; set => Get<TileWallWireStateData>().IsActuated = value; }
	public bool HasActuator { get => Get<TileWallWireStateData>().HasActuator; set => Get<TileWallWireStateData>().HasActuator = value; }
	public bool HasUnactuatedTile => HasTile && !IsActuated;

	// Slopes

	public SlopeType Slope { get => Get<TileWallWireStateData>().Slope; set => Get<TileWallWireStateData>().Slope = value; }
	public BlockType BlockType { get => Get<TileWallWireStateData>().BlockType; set => Get<TileWallWireStateData>().BlockType = value; }
	public bool IsHalfBlock { get => Get<TileWallWireStateData>().IsHalfBlock; set => Get<TileWallWireStateData>().IsHalfBlock = value; }

	public bool TopSlope => Slope == SlopeType.SlopeDownLeft || Slope == SlopeType.SlopeDownRight;
	public bool BottomSlope => Slope == SlopeType.SlopeUpLeft || Slope == SlopeType.SlopeUpRight;
	public bool LeftSlope => Slope == SlopeType.SlopeDownRight || Slope == SlopeType.SlopeUpRight;
	public bool RightSlope => Slope == SlopeType.SlopeDownLeft || Slope == SlopeType.SlopeUpLeft;

	// Framing

	public ref short TileFrameX => ref Get<TileWallWireStateData>().TileFrameX;
	public ref short TileFrameY => ref Get<TileWallWireStateData>().TileFrameY;

	public int WallFrameX { get => Get<TileWallWireStateData>().WallFrameX; set => Get<TileWallWireStateData>().WallFrameX = value; }
	public int WallFrameY { get => Get<TileWallWireStateData>().WallFrameY; set => Get<TileWallWireStateData>().WallFrameY = value; }
	public int TileFrameNumber { get => Get<TileWallWireStateData>().TileFrameNumber; set => Get<TileWallWireStateData>().TileFrameNumber = value; }
	public int WallFrameNumber { get => Get<TileWallWireStateData>().WallFrameNumber; set => Get<TileWallWireStateData>().WallFrameNumber = value; }

	// Color

	public byte TileColor { get => Get<TileWallWireStateData>().TileColor; set => Get<TileWallWireStateData>().TileColor = value; }
	public byte WallColor { get => Get<TileWallWireStateData>().WallColor; set => Get<TileWallWireStateData>().WallColor = value; }

	// Liquids

	public ref byte LiquidAmount => ref Get<LiquidData>().Amount;

	public int LiquidType { get => Get<LiquidData>().LiquidType; set => Get<LiquidData>().LiquidType = value; }
	public bool SkipLiquid { get => Get<LiquidData>().SkipLiquid; set => Get<LiquidData>().SkipLiquid = value; }
	public bool CheckingLiquid { get => Get<LiquidData>().CheckingLiquid; set => Get<LiquidData>().CheckingLiquid = value; }

	internal void SetLiquid(int liquidId, bool value) => SetIsLiquidType(liquidId, value);

	// Wires

	public bool RedWire { get => Get<TileWallWireStateData>().RedWire; set => Get<TileWallWireStateData>().RedWire = value; }
	public bool GreenWire { get => Get<TileWallWireStateData>().GreenWire; set => Get<TileWallWireStateData>().GreenWire = value; }
	public bool BlueWire { get => Get<TileWallWireStateData>().BlueWire; set => Get<TileWallWireStateData>().BlueWire = value; }
	public bool YellowWire { get => Get<TileWallWireStateData>().YellowWire; set => Get<TileWallWireStateData>().YellowWire = value; }

	// Invisibility

	public bool IsTileInvisible {
		get => Get<TileWallBrightnessInvisibilityData>().IsTileInvisible;
		set => Get<TileWallBrightnessInvisibilityData>().IsTileInvisible = value;
	}
	public bool IsWallInvisible {
		get => Get<TileWallBrightnessInvisibilityData>().IsWallInvisible;
		set => Get<TileWallBrightnessInvisibilityData>().IsWallInvisible = value;
	}

	// Fullbright

	public bool IsTileFullbright {
		get => Get<TileWallBrightnessInvisibilityData>().IsTileFullbright;
		set => Get<TileWallBrightnessInvisibilityData>().IsTileFullbright = value;
	}
	public bool IsWallFullbright {
		get => Get<TileWallBrightnessInvisibilityData>().IsWallFullbright;
		set => Get<TileWallBrightnessInvisibilityData>().IsWallFullbright = value;
	}

	// Implementations

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#if TILE_X_Y
	internal Tile(ushort x, ushort y, uint tileId) {
		X = x;
		Y = y;
		TileId = tileId;
	}
#else
	internal Tile(uint tileId)
	{
		TileId = tileId;
	}
#endif

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe ref T Get<T>() where T : unmanaged, ITileData
		=> ref TileData<T>.ptr[TileId];

	public override int GetHashCode() => (int)TileId;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(Tile tile, Tile tile2) => tile.TileId == tile2.TileId;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(Tile tile, Tile tile2) => tile.TileId != tile2.TileId;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(Tile tile, ArgumentException justSoYouCanCompareWithNull) => false;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(Tile tile, ArgumentException justSoYouCanCompareWithNull) => true;
}