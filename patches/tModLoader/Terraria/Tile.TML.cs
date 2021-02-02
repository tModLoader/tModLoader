using System;
using Terraria.ID;

namespace Terraria
{
	public partial class Tile
	{
		private static void SetBit(ref byte header, int position, bool value) {
			if (value) header = (byte)(header | (1 << position));
			else header = (byte)(header | ~(1 << position));
		}

		private static void SetBit(ref ushort header, int position, bool value) {
			if (value) header = (ushort)(header | (1 << position));
			else header = (ushort)(header | ~(1 << position));
		}

		private static bool IsBitSet(ushort value, int pos) {
			return (value & (1 << pos)) != 0;
		}

		public int LiquidType {
			get => (bTileHeader & 0x60) >> 5;
			set {
				if (value >= LiquidID.Count) throw new Exception($"The liquid with type {value} does not exist");

				bTileHeader &= (byte)((bTileHeader & 0x9F) | (32 * value));
			}
		}

		public byte LiquidAmount {
			get => liquid;
			set {
				liquid = value;
				if (liquid == 0)
					LiquidType = LiquidID.Water;
			}
		}

		public bool IsActive {
			get => IsBitSet(sTileHeader, 5);
			set => SetBit(ref sTileHeader, 5, value);
		}

		public bool IsActuated {
			get => IsBitSet(sTileHeader, 6);
			set => SetBit(ref sTileHeader, 6, value);
		}

		public bool HasActuator {
			get => IsBitSet(sTileHeader, 11);
			set => SetBit(ref sTileHeader, 11, value);
		}

		public bool IsHalfBrick {
			get => IsBitSet(sTileHeader, 10);
			set => SetBit(ref sTileHeader, 10, value);
		}

		public bool RedWire {
			get => IsBitSet(sTileHeader, 7);
			set => SetBit(ref sTileHeader, 7, value);
		}

		public bool GreenWire {
			get => IsBitSet(sTileHeader, 9);
			set => SetBit(ref sTileHeader, 9, value);
		}

		public bool BlueWire {
			get => IsBitSet(sTileHeader, 8);
			set => SetBit(ref sTileHeader, 8, value);
		}

		public bool YellowWire {
			get => IsBitSet(bTileHeader, 7);
			set => SetBit(ref bTileHeader, 7, value);
		}

		public byte Color {
			get => (byte)(sTileHeader & 0x1F);
			set => sTileHeader = (ushort)((sTileHeader & 0xFFE0) | value);
		}

		public byte WallColor {
			get => (byte)(bTileHeader & 0x1F);
			set => bTileHeader = (byte)((bTileHeader & 0xE0) | value);
		}

		public int WallFrameX {
			get => (bTileHeader2 & 0xF) * 36;
			set => bTileHeader2 = (byte)((bTileHeader2 & 0xF0) | ((value / 36) & 0xF));
		}

		public int WallFrameY {
			get => (bTileHeader3 & 7) * 36;
			set => bTileHeader3 = (byte)((bTileHeader3 & 0xF8) | ((value / 36) & 7));
		}

		public SlopeID Slope {
			get => (SlopeID)((sTileHeader & 0x7000) >> 12);
			set => sTileHeader = (ushort)((sTileHeader & 0x8FFF) | (((byte)value & 7) << 12));
		}

		public byte FrameNumber {
			get => (byte)((bTileHeader2 & 0x30) >> 4);
			set => bTileHeader2 = (byte)((bTileHeader2 & 0xCF) | ((value & 3) << 4));
		}

		public byte WallFrameNumber {
			get => (byte)((bTileHeader2 & 0xC0) >> 6);
			set => bTileHeader2 = (byte)((bTileHeader2 & 0x3F) | ((value & 3) << 6));
		}
	}
}