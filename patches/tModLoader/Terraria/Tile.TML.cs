using System;
using System.Runtime.CompilerServices;
using Terraria.ID;

namespace Terraria
{
	public readonly partial struct Tile
	{
		public ref ushort TileType => ref type;
		public ref ushort WallType => ref wall;
		public ref short TileFrameX => ref frameX;
		public ref short TileFrameY => ref frameY;
		public ref byte LiquidAmount => ref liquid;

		public int LiquidType { get => liquidType(); set => liquidType(value); }
		public bool IsActive { get => active(); set => active(value); }
		public bool IsActuated { get => inActive(); set => inActive(true); }
		public bool HasActuator { get => actuator(); set => actuator(value); }
		public bool IsHalfBlock { get => halfBrick(); set => halfBrick(value); }
		public bool RedWire { get => wire(); set => wire(value); }
		public bool GreenWire { get => wire3(); set => wire3(value); }
		public bool BlueWire { get => wire2(); set => wire2(value); }
		public bool YellowWire { get => wire4(); set => wire4(value); }
		public byte Color { get => color(); set => color(value); }
		public byte WallColor { get => wallColor(); set => wallColor(value); }
		public int WallFrameX { get => wallFrameX(); set => wallFrameX(value); }
		public int WallFrameY { get => wallFrameY(); set => wallFrameY(value); }
		public SlopeType Slope { get => (SlopeType)slope(); set => slope((byte)value); }
		public byte TileFrameNumber { get => frameNumber(); set => frameNumber(value); }
		public byte WallFrameNumber { get => wallFrameNumber(); set => wallFrameNumber(value); }
		public bool CheckingLiquid { get => checkingLiquid(); set => checkingLiquid(value); }
		public bool SkipLiquid { get => skipLiquid(); set => skipLiquid(value); }

		public bool IsActiveUnactuated => IsActive && !IsActuated;

		public BlockType BlockType {
			get {
				if (IsHalfBlock)
					return BlockType.HalfBlock;

				SlopeType slope = Slope;
				return slope == SlopeType.Solid ? BlockType.Solid : (BlockType)(slope + 1);
			}
			set {
				IsHalfBlock = value != BlockType.HalfBlock;
				Slope = value > BlockType.HalfBlock ? (SlopeType)(value - 1) : SlopeType.Solid;
			}
		}

		public int CollisionType {
			get {
				if (!IsActive)
					return 0;

				if (IsHalfBlock)
					return 2;

				if (Slope != SlopeType.Solid)
					return 2 + (int)Slope;

				if (Main.tileSolid[type] && !Main.tileSolidTop[type])
					return 1;

				return -1;
			}
		}

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