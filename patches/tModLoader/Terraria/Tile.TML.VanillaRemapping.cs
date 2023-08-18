using System.Runtime.CompilerServices;
using Terraria.ID;

#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable CA1822 // Mark members as static

namespace Terraria;

public readonly partial struct Tile
{
	// General state

	internal ref ushort type => ref TileType;
	internal ref ushort wall => ref WallType;

	internal bool active() => HasTile;
	internal void active(bool active) => HasTile = active;

	internal bool inActive() => IsActuated;
	internal void inActive(bool inActive) => IsActuated = inActive;

	internal bool actuator() => HasActuator;
	internal void actuator(bool actuator) => HasActuator = actuator;

	internal bool nactive() => HasUnactuatedTile;

	// Slopes

	internal byte slope() => (byte)Slope;
	internal void slope(byte slope) => Slope = (SlopeType)slope;

	internal bool halfBrick() => IsHalfBlock;
	internal void halfBrick(bool halfBrick) => IsHalfBlock = halfBrick;

	internal bool HasSameSlope(Tile tile) => Slope == tile.Slope;

	// Framing

	internal ref short frameX => ref TileFrameX;
	internal ref short frameY => ref TileFrameY;

	internal int wallFrameX() => WallFrameX;
	internal void wallFrameX(int wallFrameX) => WallFrameX = wallFrameX;

	internal int wallFrameY() => WallFrameY;
	internal void wallFrameY(int wallFrameY) => WallFrameY = wallFrameY;

	internal byte frameNumber() => (byte)TileFrameNumber;
	internal void frameNumber(byte frameNumber) => TileFrameNumber = frameNumber;
	
	internal byte wallFrameNumber() => (byte)WallFrameNumber;
	internal void wallFrameNumber(byte wallFrameNumber) => WallFrameNumber = wallFrameNumber;

	// Color

	internal byte color() => TileColor;
	internal void color(byte color) => TileColor = color;

	internal byte wallColor() => WallColor;
	internal void wallColor(byte wallColor) => WallColor = wallColor;

	// Liquids

	internal ref byte liquid => ref LiquidAmount;

	internal ushort liquidType() => (ushort)LiquidType;
	internal void liquidType(int liquidType) => LiquidType = liquidType;

	internal bool lava() => LiquidType == LiquidID.Lava;
	internal void lava(bool lava) => SetIsLiquidType(LiquidID.Lava, lava);

	internal bool honey() => LiquidType == LiquidID.Honey;
	internal void honey(bool honey) => SetIsLiquidType(LiquidID.Honey, honey);

	internal bool shimmer() => LiquidType == LiquidID.Shimmer;
	internal void shimmer(bool shimmer) => SetIsLiquidType(LiquidID.Shimmer, shimmer);

	internal bool skipLiquid() => SkipLiquid;
	internal void skipLiquid(bool skipLiquid) => SkipLiquid = skipLiquid;

	internal bool checkingLiquid() => CheckingLiquid;
	internal void checkingLiquid(bool checkingLiquid) => CheckingLiquid = checkingLiquid;

	// Wires

	internal bool wire() => RedWire;
	internal void wire(bool wire) => RedWire = wire;
	
	internal bool wire2() => BlueWire;
	internal void wire2(bool wire2) => BlueWire = wire2;
	
	internal bool wire3() => GreenWire;
	internal void wire3(bool wire3) => GreenWire = wire3;

	internal bool wire4() => YellowWire;
	internal void wire4(bool wire4) => YellowWire = wire4;

	// Invisibility

	internal bool invisibleBlock() => IsTileInvisible;
	internal void invisibleBlock(bool invisibleBlock) => IsTileInvisible = invisibleBlock;
	
	internal bool invisibleWall() => IsWallInvisible;
	internal void invisibleWall(bool invisibleWall) => IsWallInvisible = invisibleWall;

	// Fullbright

	internal bool fullbrightBlock() => IsTileFullbright;
	internal void fullbrightBlock(bool fullbrightBlock) => IsTileFullbright = fullbrightBlock;

	internal bool fullbrightWall() => IsWallFullbright;
	internal void fullbrightWall(bool fullbrightWall) => IsWallFullbright = fullbrightWall;

	// Utilities

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SetIsLiquidType(int liquidId, bool value)
	{
		if (value)
			LiquidType = liquidId;
		else if (LiquidType == liquidId)
			LiquidType = LiquidID.Water;
	}
}