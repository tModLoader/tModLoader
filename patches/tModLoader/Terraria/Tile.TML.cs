using System;
using System.Runtime.CompilerServices;
using Terraria.ID;

#pragma warning disable IDE0060 // Remove unused parameter

namespace Terraria;

/// <summary>
/// A data structure used for accessing information about a single cell in the world.<br/>
/// Vanilla tile code and a mods tile code will be quite different, since tModLoader reworked how tiles function to improve performance. This means that copying vanilla code will leave you with many errors.<br/>
/// For your sanity, all of the changes are well documented to make it easier to port vanilla code.
/// </summary>
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

	/// <summary>
	/// The <see cref="TileID"/> of the cell.<br/>
	/// Legacy/vanilla equivalent is <see cref="type"/>.
	/// </summary>
	public ref ushort TileType => ref Get<TileTypeData>().Type;
	/// <summary>
	/// The <see cref="WallID"/> of the cell.<br/>
	/// Legacy/vanilla equivalent is <see cref="wall"/>.
	/// </summary>
	public ref ushort WallType => ref Get<WallTypeData>().Type;

	/// <summary>
	/// Whether a cell contains a tile. Check this whenever you are accessing data from a tile to avoid getting data from an empty tile.<br/>
	/// Legacy/vanilla equivalent is <see cref="active()"/> or <see cref="active(bool)"/>.
	/// </summary>
	/// <remarks>
	/// Actuated tiles are not solid, so use <see cref="HasUnactuatedTile"/> instead of <see cref="HasTile"/> for collision checks.<br/>
	/// This only corresponds to a tile in a cell, which means a wall can exist without a tile. To check if a wall exists, use <c>tile.WallType != WallID.None</c>.
	/// </remarks>
	public bool HasTile { get => Get<TileWallWireStateData>().HasTile; set => Get<TileWallWireStateData>().HasTile = value; }
	/// <summary>
	/// Whether a cell is actuated by an actuator.<br/>
	/// Legacy/vanilla equivalent is <see cref="inActive()"/> or <see cref="inActive(bool)"/>.
	/// </summary>
	/// <remarks>
	/// Actuated tiles are <strong>not</strong> solid.
	/// </remarks>
	public bool IsActuated { get => Get<TileWallWireStateData>().IsActuated; set => Get<TileWallWireStateData>().IsActuated = value; }
	/// <summary>
	/// Whether a cell contains an actuator.<br/>
	/// Legacy/vanilla equivalent is <see cref="actuator()"/> or <see cref="actuator(bool)"/>.
	/// </summary>
	public bool HasActuator { get => Get<TileWallWireStateData>().HasActuator; set => Get<TileWallWireStateData>().HasActuator = value; }
	/// <summary>
	/// Whether a cell contains a tile that isn't actuated.<br/>
	/// Legacy/vanilla equivalent is <see cref="nactive"/>.
	/// </summary>
	/// <remarks>
	/// Actuated tiles are not solid, so use <see cref="HasUnactuatedTile"/> instead of <see cref="HasTile"/> for collision checks.<br/>
	/// When checking if a tile exists, use <see cref="HasTile"/> instead of <see cref="HasUnactuatedTile"/>.
	/// </remarks>
	public bool HasUnactuatedTile => HasTile && !IsActuated;

	// Slopes

	/// <summary>
	/// The slope shape of the tile, which can be changed by hammering.<br/>
	/// Used by <see cref="WorldGen.SlopeTile(int, int, int, bool)"/> and <see cref="BlockType"/>.<br/>
	/// Legacy/vanilla equivalent is <see cref="slope()"/> or <see cref="slope(byte)"/>.
	/// </summary>
	public SlopeType Slope { get => Get<TileWallWireStateData>().Slope; set => Get<TileWallWireStateData>().Slope = value; }
	/// <summary>
	/// The <see cref="Slope"/> and <see cref="IsHalfBlock"/> of this tile combined, which can be changed by hammering.<br/>
	/// Legacy/vanilla equivalent is <see cref="blockType"/>.
	/// </summary>
	public BlockType BlockType { get => Get<TileWallWireStateData>().BlockType; set => Get<TileWallWireStateData>().BlockType = value; }
	/// <summary>
	/// Whether a tile is a half block shape, which can be changed by hammering.<br/>
	/// Used by <see cref="WorldGen.PoundTile(int, int)"/> and <see cref="BlockType"/>.<br/>
	/// Legacy/vanilla equivalent is <see cref="halfBrick()"/> or <see cref="halfBrick(bool)"/>.
	/// </summary>
	public bool IsHalfBlock { get => Get<TileWallWireStateData>().IsHalfBlock; set => Get<TileWallWireStateData>().IsHalfBlock = value; }

	/// <summary>
	/// Whether a tile's <see cref="Slope"/> has a solid top side (<see cref="SlopeType.SlopeDownLeft"/> or <see cref="SlopeType.SlopeDownRight"/>).<br/>
	/// Legacy/vanilla equivalent is <see cref="topSlope"/>.
	/// </summary>
	public bool TopSlope => Slope == SlopeType.SlopeDownLeft || Slope == SlopeType.SlopeDownRight;
	/// <summary>
	/// Whether a tile's <see cref="Slope"/> has a solid bottom side (<see cref="SlopeType.SlopeUpLeft"/> or <see cref="SlopeType.SlopeUpRight"/>).<br/>
	/// Legacy/vanilla equivalent is <see cref="bottomSlope"/>.
	/// </summary>
	public bool BottomSlope => Slope == SlopeType.SlopeUpLeft || Slope == SlopeType.SlopeUpRight;
	/// <summary>
	/// Whether a tile's <see cref="Slope"/> has a solid left side (<see cref="SlopeType.SlopeDownRight"/> or <see cref="SlopeType.SlopeUpRight"/>).<br/>
	/// Legacy/vanilla equivalent is <see cref="leftSlope"/>.
	/// </summary>
	public bool LeftSlope => Slope == SlopeType.SlopeDownRight || Slope == SlopeType.SlopeUpRight;
	/// <summary>
	/// Whether a tile's <see cref="Slope"/> has a solid right side (<see cref="SlopeType.SlopeDownLeft"/> or <see cref="SlopeType.SlopeUpLeft"/>).<br/>
	/// Legacy/vanilla equivalent is <see cref="rightSlope"/>.
	/// </summary>
	public bool RightSlope => Slope == SlopeType.SlopeDownLeft || Slope == SlopeType.SlopeUpLeft;

	// Framing

	/// <summary>
	/// The x frame coordinate of the tile.<br/>
	/// Legacy/vanilla equivalent is <see cref="frameX"/>.
	/// </summary>
	public ref short TileFrameX => ref Get<TileWallWireStateData>().TileFrameX;
	/// <summary>
	/// The y frame coordinate of the tile.<br/>
	/// Legacy/vanilla equivalent is <see cref="frameY"/>.
	/// </summary>
	public ref short TileFrameY => ref Get<TileWallWireStateData>().TileFrameY;

	/// <summary>
	/// The x frame coordinate of the wall.<br/>
	/// Legacy/vanilla equivalent is <see cref="wallFrameX()"/> or <see cref="wallFrameX(int)"/>.
	/// </summary>
	public int WallFrameX { get => Get<TileWallWireStateData>().WallFrameX; set => Get<TileWallWireStateData>().WallFrameX = value; }
	/// <summary>
	/// The y frame coordinate of the wall.<br/>
	/// Legacy/vanilla equivalent is <see cref="wallFrameY()"/> or <see cref="wallFrameY(int)"/>.
	/// </summary>
	public int WallFrameY { get => Get<TileWallWireStateData>().WallFrameY; set => Get<TileWallWireStateData>().WallFrameY = value; }
	/// <summary>
	/// The frame number of the tile.<br/>
	/// Legacy/vanilla equivalent is <see cref="frameNumber()"/> or <see cref="frameNumber(byte)"/>.
	/// </summary>
	public int TileFrameNumber { get => Get<TileWallWireStateData>().TileFrameNumber; set => Get<TileWallWireStateData>().TileFrameNumber = value; }
	/// <summary>
	/// The frame number of the wall.<br/>
	/// Legacy/vanilla equivalent is <see cref="wallFrameNumber()"/> or <see cref="wallFrameNumber(byte)"/>.
	/// </summary>
	public int WallFrameNumber { get => Get<TileWallWireStateData>().WallFrameNumber; set => Get<TileWallWireStateData>().WallFrameNumber = value; }

	// Color

	/// <summary>
	/// The <see cref="PaintID"/> this tile is painted with. Is <see cref="PaintID.None"/> if not painted.<br/>
	/// Legacy/vanilla equivalent is <see cref="color()"/> or <see cref="color(byte)"/>.
	/// </summary>
	public byte TileColor { get => Get<TileWallWireStateData>().TileColor; set => Get<TileWallWireStateData>().TileColor = value; }
	/// <summary>
	/// The <see cref="PaintID"/> this wall is painted with. Is <see cref="PaintID.None"/> if not painted.<br/>
	/// Legacy/vanilla equivalent is <see cref="wallColor()"/> or <see cref="wallColor(byte)"/>.
	/// </summary>
	public byte WallColor { get => Get<TileWallWireStateData>().WallColor; set => Get<TileWallWireStateData>().WallColor = value; }

	// Liquids

	/// <summary>
	/// The amount of liquid in the cell.<br/>
	/// Legacy/vanilla equivalent is <see cref="liquid"/>.
	/// </summary>
	public ref byte LiquidAmount => ref Get<LiquidData>().Amount;

	/// <summary>
	/// The <see cref="LiquidID"/> of the liquid in the cell.<br/>
	/// Legacy/vanilla equivalent is <see cref="liquidType()"/> or <see cref="liquidType(int)"/>.
	/// </summary>
	public int LiquidType { get => Get<LiquidData>().LiquidType; set => Get<LiquidData>().LiquidType = value; }
	public bool SkipLiquid { get => Get<LiquidData>().SkipLiquid; set => Get<LiquidData>().SkipLiquid = value; }
	public bool CheckingLiquid { get => Get<LiquidData>().CheckingLiquid; set => Get<LiquidData>().CheckingLiquid = value; }

	// Wires

	/// <summary>
	/// Whether a cell contains red wire.<br/>
	/// Legacy/vanilla equivalent is <see cref="wire()"/> or <see cref="wire(bool)"/>.
	/// </summary>
	public bool RedWire { get => Get<TileWallWireStateData>().RedWire; set => Get<TileWallWireStateData>().RedWire = value; }
	/// <summary>
	/// Whether a cell contains green wire.<br/>
	/// Legacy/vanilla equivalent is <see cref="wire3()"/> or <see cref="wire3(bool)"/>.
	/// </summary>
	public bool GreenWire { get => Get<TileWallWireStateData>().GreenWire; set => Get<TileWallWireStateData>().GreenWire = value; }
	/// <summary>
	/// Whether a cell contains blue wire.<br/>
	/// Legacy/vanilla equivalent is <see cref="wire2()"/> or <see cref="wire2(bool)"/>.
	/// </summary>
	public bool BlueWire { get => Get<TileWallWireStateData>().BlueWire; set => Get<TileWallWireStateData>().BlueWire = value; }
	/// <summary>
	/// Whether a cell contains yellow wire.<br/>
	/// Legacy/vanilla equivalent is <see cref="wire4()"/> or <see cref="wire4(bool)"/>.
	/// </summary>
	public bool YellowWire { get => Get<TileWallWireStateData>().YellowWire; set => Get<TileWallWireStateData>().YellowWire = value; }

	// Invisibility

	/// <summary>
	/// Whether a tile is invisible. Used by <see cref="ItemID.EchoCoating"/>.<br/>
	/// Legacy/vanilla equivalent is <see cref="invisibleBlock()"/> or <see cref="invisibleBlock(bool)"/>.
	/// </summary>
	public bool IsTileInvisible {
		get => Get<TileWallBrightnessInvisibilityData>().IsTileInvisible;
		set => Get<TileWallBrightnessInvisibilityData>().IsTileInvisible = value;
	}
	/// <summary>
	/// Whether a wall is invisible. Used by <see cref="ItemID.EchoCoating"/>.<br/>
	/// Legacy/vanilla equivalent is <see cref="invisibleWall()"/> or <see cref="invisibleWall(bool)"/>.
	/// </summary>
	public bool IsWallInvisible {
		get => Get<TileWallBrightnessInvisibilityData>().IsWallInvisible;
		set => Get<TileWallBrightnessInvisibilityData>().IsWallInvisible = value;
	}

	// Fullbright

	/// <summary>
	/// Whether a tile is fully illuminated. Used by <see cref="ItemID.GlowPaint"/>.<br/>
	/// Legacy/vanilla equivalent is <see cref="fullbrightBlock()"/> or <see cref="fullbrightBlock(bool)"/>.
	/// </summary>
	public bool IsTileFullbright {
		get => Get<TileWallBrightnessInvisibilityData>().IsTileFullbright;
		set => Get<TileWallBrightnessInvisibilityData>().IsTileFullbright = value;
	}
	/// <summary>
	/// Whether a wall is fully illuminated. Used by <see cref="ItemID.GlowPaint"/>.<br/>
	/// Legacy/vanilla equivalent is <see cref="fullbrightWall()"/> or <see cref="fullbrightWall(bool)"/>.
	/// </summary>
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

	/// <summary>
	/// Used to get a reference to a cell's <see cref="ITileData"/> .
	/// </summary>
	/// <typeparam name="T">The <see cref="ITileData"/> to get.</typeparam>
	/// <returns>The <see cref="ITileData"/> of type <typeparamref name="T"/> that this cell contains.</returns>
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