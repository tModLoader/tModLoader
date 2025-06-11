using System.Runtime.CompilerServices;
using Terraria.ID;

#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable CA1822 // Mark members as static

namespace Terraria;

public readonly partial struct Tile
{
	// General state

	/// <summary>
	/// Legacy code, use <see cref="TileType"/> instead.
	/// </summary>
	internal ref ushort type => ref TileType;
	/// <summary>
	/// Legacy code, use <see cref="WallType"/> instead.
	/// </summary>
	internal ref ushort wall => ref WallType;

	/// <summary>
	/// Legacy code, use <see cref="HasTile"/> instead.
	/// </summary>
	/// <returns></returns>
	internal bool active() => HasTile;
	/// <inheritdoc cref="active()"/>
	internal void active(bool active) => HasTile = active;

	/// <summary>
	/// Legacy code, use <see cref="IsActuated"/> instead.
	/// </summary>
	/// <returns></returns>
	internal bool inActive() => IsActuated;
	/// <inheritdoc cref="inActive()"/>
	internal void inActive(bool inActive) => IsActuated = inActive;

	/// <summary>
	/// Legacy code, use <see cref="HasActuator"/> instead.
	/// </summary>
	/// <returns></returns>
	internal bool actuator() => HasActuator;
	/// <inheritdoc cref="actuator()"/>
	internal void actuator(bool actuator) => HasActuator = actuator;

	/// <summary>
	/// Legacy code, use <see cref="HasUnactuatedTile"/> instead.
	/// </summary>
	/// <returns></returns>
	internal bool nactive() => HasUnactuatedTile;

	// Slopes

	/// <summary>
	/// Legacy code, use <see cref="Slope"/> instead.
	/// </summary>
	/// <returns></returns>
	internal byte slope() => (byte)Slope;
	/// <inheritdoc cref="slope()"/>
	internal void slope(byte slope) => Slope = (SlopeType)slope;

	/// <summary>
	/// Legacy code, use <see cref="IsHalfBlock"/> instead.
	/// </summary>
	/// <returns></returns>
	internal bool halfBrick() => IsHalfBlock;
	/// <inheritdoc cref="halfBrick()"/>
	internal void halfBrick(bool halfBrick) => IsHalfBlock = halfBrick;

	/// <summary>
	/// Legacy code, use <c>tile1.Slope == tile2.Slope</c> instead.
	/// </summary>
	/// <param name="tile"></param>
	/// <returns></returns>
	internal bool HasSameSlope(Tile tile) => Slope == tile.Slope;

	// Framing

	/// <summary>
	/// Legacy code, use <see cref="TileFrameX"/> instead.
	/// </summary>
	internal ref short frameX => ref TileFrameX;
	/// <summary>
	/// Legacy code, use <see cref="TileFrameY"/> instead.
	/// </summary>
	internal ref short frameY => ref TileFrameY;

	/// <summary>
	/// Legacy code, use <see cref="WallFrameX"/> instead.
	/// </summary>
	/// <returns></returns>
	internal int wallFrameX() => WallFrameX;
	/// <inheritdoc cref="wallFrameX()"/>
	internal void wallFrameX(int wallFrameX) => WallFrameX = wallFrameX;

	/// <summary>
	/// Legacy code, use <see cref="WallFrameY"/> instead.
	/// </summary>
	/// <returns></returns>
	internal int wallFrameY() => WallFrameY;
	/// <inheritdoc cref="wallFrameY()"/>
	internal void wallFrameY(int wallFrameY) => WallFrameY = wallFrameY;

	/// <summary>
	/// Legacy code, use <see cref="TileFrameNumber"/> instead.
	/// </summary>
	/// <returns></returns>
	internal byte frameNumber() => (byte)TileFrameNumber;
	/// <inheritdoc cref="frameNumber()"/>
	internal void frameNumber(byte frameNumber) => TileFrameNumber = frameNumber;
	
	/// <summary>
	/// Legacy code, use <see cref="WallFrameNumber"/> instead.
	/// </summary>
	/// <returns></returns>
	internal byte wallFrameNumber() => (byte)WallFrameNumber;
	/// <inheritdoc cref="wallFrameNumber()"/>
	internal void wallFrameNumber(byte wallFrameNumber) => WallFrameNumber = wallFrameNumber;

	// Color

	/// <summary>
	/// Legacy code, use <see cref="TileColor"/> instead.
	/// </summary>
	/// <returns></returns>
	internal byte color() => TileColor;
	/// <inheritdoc cref="color()"/>
	internal void color(byte color) => TileColor = color;

	/// <summary>
	/// Legacy code, use <see cref="WallColor"/> instead.
	/// </summary>
	/// <returns></returns>
	internal byte wallColor() => WallColor;
	/// <inheritdoc cref="wallColor()"/>
	internal void wallColor(byte wallColor) => WallColor = wallColor;

	// Liquids

	/// <summary>
	/// Legacy code, use <see cref="LiquidAmount"/> instead.
	/// </summary>
	internal ref byte liquid => ref LiquidAmount;

	/// <summary>
	/// Legacy code, use <see cref="LiquidType"/> instead.
	/// </summary>
	/// <returns></returns>
	internal byte liquidType() => (byte)LiquidType;
	/// <inheritdoc cref="liquidType()"/>
	internal void liquidType(int liquidType) => LiquidType = liquidType;

	/// <summary>
	/// Legacy code, use <c>tile.LiquidType == LiquidID.Lava</c> instead.
	/// </summary>
	/// <returns></returns>
	internal bool lava() => LiquidType == LiquidID.Lava;
	/// <inheritdoc cref="lava()"/>
	internal void lava(bool lava) => SetIsLiquidType(LiquidID.Lava, lava);

	/// <summary>
	/// Legacy code, use <c>tile.LiquidType == LiquidID.Honey</c> instead.
	/// </summary>
	/// <returns></returns>
	internal bool honey() => LiquidType == LiquidID.Honey;
	/// <inheritdoc cref="honey()"/>
	internal void honey(bool honey) => SetIsLiquidType(LiquidID.Honey, honey);

	/// <summary>
	/// Legacy code, use <c>tile.LiquidType == LiquidID.Shimmer</c> instead.
	/// </summary>
	/// <returns></returns>
	internal bool shimmer() => LiquidType == LiquidID.Shimmer;
	/// <inheritdoc cref="shimmer()"/>
	internal void shimmer(bool shimmer) => SetIsLiquidType(LiquidID.Shimmer, shimmer);

	/// <summary>
	/// Legacy code, use <see cref="SkipLiquid"/> instead.
	/// </summary>
	/// <returns></returns>
	internal bool skipLiquid() => SkipLiquid;
	/// <inheritdoc cref="skipLiquid()"/>
	internal void skipLiquid(bool skipLiquid) => SkipLiquid = skipLiquid;

	/// <summary>
	/// Legacy code, use <see cref="CheckingLiquid"/> instead.
	/// </summary>
	/// <returns></returns>
	internal bool checkingLiquid() => CheckingLiquid;
	/// <inheritdoc cref="checkingLiquid()"/>
	internal void checkingLiquid(bool checkingLiquid) => CheckingLiquid = checkingLiquid;

	// Wires

	/// <summary>
	/// Legacy code, use <see cref="RedWire"/> instead.
	/// </summary>
	/// <returns></returns>
	internal bool wire() => RedWire;
	/// <inheritdoc cref="wire()"/>
	internal void wire(bool wire) => RedWire = wire;
	
	/// <summary>
	/// Legacy code, use <see cref="BlueWire"/> instead.
	/// </summary>
	/// <returns></returns>
	internal bool wire2() => BlueWire;
	/// <inheritdoc cref="wire2()"/>
	internal void wire2(bool wire2) => BlueWire = wire2;
	
	/// <summary>
	/// Legacy code, use <see cref="GreenWire"/> instead.
	/// </summary>
	/// <returns></returns>
	internal bool wire3() => GreenWire;
	/// <inheritdoc cref="wire3()"/>
	internal void wire3(bool wire3) => GreenWire = wire3;

	/// <summary>
	/// Legacy code, use <see cref="YellowWire"/> instead.
	/// </summary>
	/// <returns></returns>
	internal bool wire4() => YellowWire;
	/// <inheritdoc cref="wire4()"/>
	internal void wire4(bool wire4) => YellowWire = wire4;

	// Invisibility

	/// <summary>
	/// Legacy code, use <see cref="IsTileInvisible"/> instead.
	/// </summary>
	/// <returns></returns>
	internal bool invisibleBlock() => IsTileInvisible;
	/// <inheritdoc cref="invisibleBlock()"/>
	internal void invisibleBlock(bool invisibleBlock) => IsTileInvisible = invisibleBlock;
	
	/// <summary>
	/// Legacy code, use <see cref="IsWallInvisible"/> instead.
	/// </summary>
	/// <returns></returns>
	internal bool invisibleWall() => IsWallInvisible;
	/// <inheritdoc cref="invisibleWall()"/>
	internal void invisibleWall(bool invisibleWall) => IsWallInvisible = invisibleWall;

	// Fullbright

	/// <summary>
	/// Legacy code, use <see cref="IsTileFullbright"/> instead.
	/// </summary>
	/// <returns></returns>
	internal bool fullbrightBlock() => IsTileFullbright;
	/// <inheritdoc cref="fullbrightBlock()"/>
	internal void fullbrightBlock(bool fullbrightBlock) => IsTileFullbright = fullbrightBlock;

	/// <summary>
	/// Legacy code, use <see cref="IsWallFullbright"/> instead.
	/// </summary>
	/// <returns></returns>
	internal bool fullbrightWall() => IsWallFullbright;
	/// <inheritdoc cref="fullbrightWall()"/>
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