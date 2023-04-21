using System;
using Terraria.ID;
using static Terraria.TileDataPacking;

namespace Terraria;

public static class TileDataPacking
{
	public static int Unpack(int bits, int offset, int width)
		=> bits >> offset & ((1 << width) - 1);

	// we also & the incoming value with the bit mask as a high performance safeguard against invalid values spilling over into adjacent bits
	public static int Pack(int value, int bits, int offset, int width)
		=> bits & ~(((1 << width) - 1) << offset) | (value & ((1 << width) - 1)) << offset;

	public static bool GetBit(int bits, int offset)
		=> (bits & 1 << offset) != 0;

	public static int SetBit(bool value, int bits, int offset)
		=> value ? bits | 1 << offset : bits & ~(1 << offset);
}

public struct TileTypeData : ITileData
{
	public ushort Type;
}

public struct WallTypeData : ITileData
{
	public ushort Type;
}	

public struct LiquidData : ITileData
{
	public byte Amount;

	// c = checking liquid
	// s = skip liquid
	// l = liquid id
	private byte typeAndFlags;

	public int LiquidType		{ get => Unpack(typeAndFlags, 0, 6); set => typeAndFlags = (byte)Pack(value, typeAndFlags, 0, 6); }
	public bool SkipLiquid		{ get => GetBit(typeAndFlags, 6); set => typeAndFlags = (byte)SetBit(value, typeAndFlags, 6); }
	public bool CheckingLiquid	{ get => GetBit(typeAndFlags, 7); set => typeAndFlags = (byte)SetBit(value, typeAndFlags, 7); }
}

public struct TileWallBrightnessInvisibilityData : ITileData
{
	private BitsByte bitpack;

	public byte Data => bitpack;

	public bool IsTileInvisible { get => bitpack[0]; set => bitpack[0] = value; }
	public bool IsWallInvisible { get => bitpack[1]; set => bitpack[1] = value; }
	public bool IsTileFullbright { get => bitpack[2]; set => bitpack[2] = value; }
	public bool IsWallFullbright { get => bitpack[3]; set => bitpack[3] = value; }
}

public struct TileWallWireStateData : ITileData
{
	public short TileFrameX;
	public short TileFrameY;

	// t = HasTile
	// i = IsActuated (inActive)
	// a = HasActuator
	// c = TileColor
	// C = WallColor
	// n = TileFrameNumber
	// N = WallFrameNumber
	// X = WallFrameX / 36
	// Y = WallFrameY / 36
	// h = IsHalfBrick
	// s = Slope
	// w = Wire 1-4

	// wwwwsssh YYYXXXXN NnnCCCCC cccccait
	private int bitpack;

	public bool HasTile			{ get => GetBit(bitpack, 0); set => bitpack = SetBit(value, bitpack, 0); }
	public bool IsActuated		{ get => GetBit(bitpack, 1); set => bitpack = SetBit(value, bitpack, 1); }
	public bool HasActuator		{ get => GetBit(bitpack, 2); set => bitpack = SetBit(value, bitpack, 2); }

	public byte TileColor		{ get => (byte)Unpack(bitpack, 3, 5); set => bitpack = Pack(value, bitpack, 3, 5); }
	public byte WallColor		{ get => (byte)Unpack(bitpack, 8, 5); set => bitpack = Pack(value, bitpack, 8, 5); }

	public int TileFrameNumber	{ get => Unpack(bitpack, 13, 2); set => bitpack = Pack(value, bitpack, 13, 2); }
	public int WallFrameNumber	{ get => Unpack(bitpack, 15, 2); set => bitpack = Pack(value, bitpack, 15, 2); }

	public int WallFrameX		{ get => Unpack(bitpack, 17, 4) * 36; set => bitpack = Pack(value / 36, bitpack, 17, 4); }
	public int WallFrameY		{ get => Unpack(bitpack, 21, 3) * 36; set => bitpack = Pack(value / 36, bitpack, 21, 3); }

	public bool IsHalfBlock		{ get => GetBit(bitpack, 24); set => bitpack = SetBit(value, bitpack, 24); }
	public SlopeType Slope		{ get => (SlopeType)Unpack(bitpack, 25, 3); set => bitpack = Pack((int)value, bitpack, 25, 3); }

	public int WireData			{ get => Unpack(bitpack, 28, 4); set => bitpack = Pack(value, bitpack, 28, 4); }
	public bool RedWire			{ get => GetBit(bitpack, 28); set => bitpack = SetBit(value, bitpack, 28); }
	public bool BlueWire		{ get => GetBit(bitpack, 29); set => bitpack = SetBit(value, bitpack, 29); }
	public bool GreenWire		{ get => GetBit(bitpack, 30); set => bitpack = SetBit(value, bitpack, 30); }
	public bool YellowWire		{ get => GetBit(bitpack, 31); set => bitpack = SetBit(value, bitpack, 31); }

	public int NonFrameBits => (int)(bitpack & 0xFF001FFF);

	/// <summary>
	/// Intended to be used to set all the persistent data about a tile. For example, when loading a schematic from serialized NonFrameBits.
	/// </summary>
	public void SetAllBitsClearFrame(int nonFrameBits)
	{
		bitpack = (int)(nonFrameBits & 0xFF001FFF);
	}

	public BlockType BlockType {
		get {
			if (IsHalfBlock)
				return BlockType.HalfBlock;

			SlopeType slope = Slope;
			return slope == SlopeType.Solid ? BlockType.Solid : (BlockType)(slope + 1);
		}
		set {
			IsHalfBlock = value == BlockType.HalfBlock;
			Slope = value > BlockType.HalfBlock ? (SlopeType)(value - 1) : SlopeType.Solid;
		}
	}
}
