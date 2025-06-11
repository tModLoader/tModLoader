using System;
using System.Runtime.CompilerServices;
using Terraria.ID;

#pragma warning disable IDE0060 // Remove unused parameter

namespace Terraria;

/// <summary>
/// A data structure used for accessing information about tiles, walls, wires, and liquids at a single position in the world.<para/>
/// Vanilla tile code and a mods tile code will be quite different, since tModLoader reworked how tiles function to improve performance. This means that copying vanilla code will leave you with many errors. Running the code through tModPorter will fix most of the issues, however.<para/>
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
	/// The <see cref="TileID"/> of the tile at this position.<br/>
	/// This value is only valid if <see cref="HasTile"/> is true.<br/>
	/// Legacy/vanilla equivalent is <see cref="type"/>.
	/// </summary>
	public ref ushort TileType => ref Get<TileTypeData>().Type;
	/// <summary>
	/// The <see cref="WallID"/> of the wall at this position.<br/>
	/// A value of 0 indicates no wall.<br/>
	/// Legacy/vanilla equivalent is <see cref="wall"/>.
	/// </summary>
	public ref ushort WallType => ref Get<WallTypeData>().Type;

	/// <summary>
	/// Whether there is a tile at this position. Check this whenever you are accessing data from a tile to avoid getting data from an empty tile.<br/>
	/// Legacy/vanilla equivalent is <see cref="active()"/> or <see cref="active(bool)"/>.
	/// </summary>
	/// <remarks>
	/// Actuated tiles are not solid, so use <see cref="HasUnactuatedTile"/> instead of <see cref="HasTile"/> for collision checks.<br/>
	/// This only corresponds to whether a tile exists, however, a wall can exist without a tile. To check if a wall exists, use <c>tile.WallType != WallID.None</c>.
	/// </remarks>
	public bool HasTile { get => Get<TileWallWireStateData>().HasTile; set => Get<TileWallWireStateData>().HasTile = value; }
	/// <summary>
	/// Whether the tile at this position is actuated by an actuator.<br/>
	/// Legacy/vanilla equivalent is <see cref="inActive()"/> or <see cref="inActive(bool)"/>.
	/// </summary>
	/// <remarks>
	/// Actuated tiles are <strong>not</strong> solid.
	/// </remarks>
	public bool IsActuated { get => Get<TileWallWireStateData>().IsActuated; set => Get<TileWallWireStateData>().IsActuated = value; }
	/// <summary>
	/// Whether there is an actuator at this position.<br/>
	/// Legacy/vanilla equivalent is <see cref="actuator()"/> or <see cref="actuator(bool)"/>.
	/// </summary>
	public bool HasActuator { get => Get<TileWallWireStateData>().HasActuator; set => Get<TileWallWireStateData>().HasActuator = value; }
	/// <summary>
	/// Whether there is a tile at this position that isn't actuated.<br/>
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
	/// If the top side of a tile is sloped (<see cref="Slope"/>), meaning the bottom side is solid. (<see cref="SlopeType.SlopeDownLeft"/> or <see cref="SlopeType.SlopeDownRight"/>).<br/>
	/// Legacy/vanilla equivalent is <see cref="topSlope"/>.
	/// </summary>
	public bool TopSlope => Slope == SlopeType.SlopeDownLeft || Slope == SlopeType.SlopeDownRight;
	/// <summary>
	/// If the bottom side of a tile is sloped (<see cref="Slope"/>), meaning the top side is solid. (<see cref="SlopeType.SlopeUpLeft"/> or <see cref="SlopeType.SlopeUpRight"/>).<br/>
	/// Legacy/vanilla equivalent is <see cref="bottomSlope"/>.
	/// </summary>
	public bool BottomSlope => Slope == SlopeType.SlopeUpLeft || Slope == SlopeType.SlopeUpRight;
	/// <summary>
	/// If the left side of a tile is sloped (<see cref="Slope"/>), meaning the right side is solid. (<see cref="SlopeType.SlopeDownRight"/> or <see cref="SlopeType.SlopeUpRight"/>).<br/>
	/// Legacy/vanilla equivalent is <see cref="leftSlope"/>.
	/// </summary>
	public bool LeftSlope => Slope == SlopeType.SlopeDownRight || Slope == SlopeType.SlopeUpRight;
	/// <summary>
	/// If the right side of a tile is sloped (<see cref="Slope"/>), meaning the left side is solid. (<see cref="SlopeType.SlopeDownLeft"/> or <see cref="SlopeType.SlopeUpLeft"/>).<br/>
	/// Legacy/vanilla equivalent is <see cref="rightSlope"/>.
	/// </summary>
	public bool RightSlope => Slope == SlopeType.SlopeDownLeft || Slope == SlopeType.SlopeUpLeft;

	// Framing

	/// <summary>
	/// The X coordinate of the top left corner of the area in the spritesheet for the <see cref="TileType"/> to be used to draw the tile at this position.<para/>
	/// For a Framed tile, this value is set automatically according to the framing logic as the world loads or other tiles are placed or mined nearby. See <see href="https://github.com/tModLoader/tModLoader/wiki/Basic-Tile#framed-vs-frameimportant-tiles">Framed vs FrameImportant</see> for more info. For <see cref="Main.tileFrameImportant"/> tiles, this value will not change due to tile framing and will be saved and synced in Multiplayer. In either case, <see cref="TileFrameX"/> and <see cref="TileFrameY"/> correspond to the coordinates of the top left corner of the area in the spritesheet corresponding to the <see cref="TileType"/> that should be drawn at this position. Custom drawing logic can adjust these values.<para/>
	/// Some tiles such as Christmas Tree and Weapon Rack use the higher bits of these fields to do tile-specific behaviors. Modders should not attempt to do similar approaches, but should use <see cref="ModLoader.ModTileEntity"/>s.<para/>
	/// Legacy/vanilla equivalent is <see cref="frameX"/>.
	/// </summary>
	public ref short TileFrameX => ref Get<TileWallWireStateData>().TileFrameX;
	/// <summary>
	/// The Y coordinate of the top left corner of the area in the spritesheet for the <see cref="TileType"/> to be used to draw the tile at this position.<para/>
	/// For a Framed tile, this value is set automatically according to the framing logic as the world loads or other tiles are placed or mined nearby. See <see href="https://github.com/tModLoader/tModLoader/wiki/Basic-Tile#framed-vs-frameimportant-tiles">Framed vs FrameImportant</see> for more info. For <see cref="Main.tileFrameImportant"/> tiles, this value will not change due to tile framing and will be saved and synced in Multiplayer. In either case, <see cref="TileFrameX"/> and <see cref="TileFrameY"/> correspond to the coordinates of the top left corner of the area in the spritesheet corresponding to the <see cref="TileType"/> that should be drawn at this position. Custom drawing logic can adjust these values.<para/>
	/// Some tiles such as Christmas Tree and Weapon Rack use the higher bits of these fields to do tile-specific behaviors. Modders should not attempt to do similar approaches, but should use <see cref="ModLoader.ModTileEntity"/>s.<para/>
	/// Legacy/vanilla equivalent is <see cref="frameY"/>.
	/// </summary>
	public ref short TileFrameY => ref Get<TileWallWireStateData>().TileFrameY;

	/// <summary>
	/// The X coordinate of the top left corner of the area in the spritesheet for the <see cref="WallType"/> to be used to draw the wall at this position.<para/>
	/// Legacy/vanilla equivalent is <see cref="wallFrameX()"/> or <see cref="wallFrameX(int)"/>.
	/// </summary>
	public int WallFrameX { get => Get<TileWallWireStateData>().WallFrameX; set => Get<TileWallWireStateData>().WallFrameX = value; }
	/// <summary>
	/// The Y coordinate of the top left corner of the area in the spritesheet for the <see cref="WallType"/> to be used to draw the wall at this position.<para/>
	/// Legacy/vanilla equivalent is <see cref="wallFrameY()"/> or <see cref="wallFrameY(int)"/>.
	/// </summary>
	public int WallFrameY { get => Get<TileWallWireStateData>().WallFrameY; set => Get<TileWallWireStateData>().WallFrameY = value; }
	/// <summary>
	/// The random style number the tile at this position has, which is random number between 0 and 2 (inclusive).<br/>
	/// This is used in non-<see cref="Main.tileFrameImportant"/> tiles (aka "Terrain" tiles) to provide visual variation and is not synced in multiplayer nor will it be preserved when saving and loading the world.<br/>
	/// Legacy/vanilla equivalent is <see cref="frameNumber()"/> or <see cref="frameNumber(byte)"/>.
	/// </summary>
	public int TileFrameNumber { get => Get<TileWallWireStateData>().TileFrameNumber; set => Get<TileWallWireStateData>().TileFrameNumber = value; }
	/// <summary>
	/// The random style number the wall at this position has, which is a random number between 0 and 2 (inclusive).<br/>
	/// This is used to provide visual variation and is not synced in multiplayer nor will it be preserved when saving and loading the world.<br/>
	/// Legacy/vanilla equivalent is <see cref="wallFrameNumber()"/> or <see cref="wallFrameNumber(byte)"/>.
	/// </summary>
	public int WallFrameNumber { get => Get<TileWallWireStateData>().WallFrameNumber; set => Get<TileWallWireStateData>().WallFrameNumber = value; }

	// Color

	/// <summary>
	/// The <see cref="PaintID"/> the tile at this position is painted with. Is <see cref="PaintID.None"/> if not painted.<br/>
	/// Legacy/vanilla equivalent is <see cref="color()"/> or <see cref="color(byte)"/>.
	/// </summary>
	public byte TileColor { get => Get<TileWallWireStateData>().TileColor; set => Get<TileWallWireStateData>().TileColor = value; }
	/// <summary>
	/// The <see cref="PaintID"/> the wall at this position is painted with. Is <see cref="PaintID.None"/> if not painted.<br/>
	/// Legacy/vanilla equivalent is <see cref="wallColor()"/> or <see cref="wallColor(byte)"/>.
	/// </summary>
	public byte WallColor { get => Get<TileWallWireStateData>().WallColor; set => Get<TileWallWireStateData>().WallColor = value; }

	// Liquids

	/// <summary>
	/// The amount of liquid at this position.<br/>
	/// Ranges from 0, no liquid, to 255, filled with liquid.<br/>
	/// Legacy/vanilla equivalent is <see cref="liquid"/>.
	/// </summary>
	public ref byte LiquidAmount => ref Get<LiquidData>().Amount;

	/// <summary>
	/// The <see cref="LiquidID"/> of the liquid at this position.<br/>
	/// Make sure to check that <see cref="LiquidAmount"/> is greater than 0.<br/>
	/// Legacy/vanilla equivalent is <see cref="liquidType()"/> or <see cref="liquidType(int)"/>.
	/// </summary>
	public int LiquidType { get => Get<LiquidData>().LiquidType; set => Get<LiquidData>().LiquidType = value; }
	/// <summary>
	/// Whether the liquid at this position should skip updating for 1 tick.<br/>
	/// Legacy/vanilla equivalent is <see cref="skipLiquid()"/> or <see cref="skipLiquid(bool)"/>.
	/// </summary>
	public bool SkipLiquid { get => Get<LiquidData>().SkipLiquid; set => Get<LiquidData>().SkipLiquid = value; }
	/// <summary>
	/// Whether there is liquid at this position.<br/>
	/// Legacy/vanilla equivalent is <see cref="checkingLiquid()"/> or <see cref="checkingLiquid(bool)"/>.
	/// </summary>
	public bool CheckingLiquid { get => Get<LiquidData>().CheckingLiquid; set => Get<LiquidData>().CheckingLiquid = value; }

	// Wires

	/// <summary>
	/// Whether there is red wire at this position.<br/>
	/// Legacy/vanilla equivalent is <see cref="wire()"/> or <see cref="wire(bool)"/>.
	/// </summary>
	public bool RedWire { get => Get<TileWallWireStateData>().RedWire; set => Get<TileWallWireStateData>().RedWire = value; }
	/// <summary>
	/// Whether there is green wire at this position.<br/>
	/// Legacy/vanilla equivalent is <see cref="wire3()"/> or <see cref="wire3(bool)"/>.
	/// </summary>
	public bool GreenWire { get => Get<TileWallWireStateData>().GreenWire; set => Get<TileWallWireStateData>().GreenWire = value; }
	/// <summary>
	/// Whether there is blue wire at this position.<br/>
	/// Legacy/vanilla equivalent is <see cref="wire2()"/> or <see cref="wire2(bool)"/>.
	/// </summary>
	public bool BlueWire { get => Get<TileWallWireStateData>().BlueWire; set => Get<TileWallWireStateData>().BlueWire = value; }
	/// <summary>
	/// Whether there is yellow wire at this position.<br/>
	/// Legacy/vanilla equivalent is <see cref="wire4()"/> or <see cref="wire4(bool)"/>.
	/// </summary>
	public bool YellowWire { get => Get<TileWallWireStateData>().YellowWire; set => Get<TileWallWireStateData>().YellowWire = value; }

	// Invisibility

	/// <summary>
	/// Whether the tile at this position is invisible. Used by <see cref="ItemID.EchoCoating"/>.<br/>
	/// Legacy/vanilla equivalent is <see cref="invisibleBlock()"/> or <see cref="invisibleBlock(bool)"/>.
	/// </summary>
	public bool IsTileInvisible {
		get => Get<TileWallBrightnessInvisibilityData>().IsTileInvisible;
		set => Get<TileWallBrightnessInvisibilityData>().IsTileInvisible = value;
	}
	/// <summary>
	/// Whether the wall at this position is invisible. Used by <see cref="ItemID.EchoCoating"/>.<br/>
	/// Legacy/vanilla equivalent is <see cref="invisibleWall()"/> or <see cref="invisibleWall(bool)"/>.
	/// </summary>
	public bool IsWallInvisible {
		get => Get<TileWallBrightnessInvisibilityData>().IsWallInvisible;
		set => Get<TileWallBrightnessInvisibilityData>().IsWallInvisible = value;
	}

	// Fullbright

	/// <summary>
	/// Whether the tile at this position is fully illuminated. Used by <see cref="ItemID.GlowPaint"/>.<br/>
	/// Legacy/vanilla equivalent is <see cref="fullbrightBlock()"/> or <see cref="fullbrightBlock(bool)"/>.
	/// </summary>
	public bool IsTileFullbright {
		get => Get<TileWallBrightnessInvisibilityData>().IsTileFullbright;
		set => Get<TileWallBrightnessInvisibilityData>().IsTileFullbright = value;
	}
	/// <summary>
	/// Whether the wall at this position is fully illuminated. Used by <see cref="ItemID.GlowPaint"/>.<br/>
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
	/// Used to get a reference to the <see cref="ITileData"/> at this position.
	/// </summary>
	/// <typeparam name="T">The <see cref="ITileData"/> to get.</typeparam>
	/// <returns>The <see cref="ITileData"/> of type <typeparamref name="T"/> at this position.</returns>
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