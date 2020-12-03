using Terraria.ID;

namespace Terraria
{
	public partial class Tile
	{
		public enum Liquid
		{
			Water = 0,
			Lava = 1,
			Honey = 2,
			None = 3
		}

		private static void SetBit(ref byte value, int pos) {
			value = (byte)(value | (1 << pos));
		}

		private static void SetBit(ref ushort value, int pos) {
			value = (ushort)(value | (1 << pos));
		}

		private static void ResetBit(ref byte value, int pos) {
			value = (byte)(value & ~(1 << pos));
		}

		private static void ResetBit(ref ushort value, int pos) {
			value = (ushort)(value & ~(1 << pos));
		}

		private static bool IsBitSet(ushort value, int pos) {
			return (value & (1 << pos)) != 0;
		}

		public Liquid LiquidType {
			get => (Liquid)((bTileHeader & 0b0110_0000) >> 5);
			set {
				switch (value) {
					case Liquid.Water:
						bTileHeader &= 0b1001_1111;
						break;
					case Liquid.Lava:
						bTileHeader = (byte)((bTileHeader & 0b1001_1111) | 0b0010_0000);
						break;
					case Liquid.Honey:
						bTileHeader = (byte)((bTileHeader & 0b1001_1111) | 0b0100_0000);
						break;
					case Liquid.None:
						bTileHeader = (byte)((bTileHeader & 0b1001_1111) | 0b0110_0000);
						break;
				}
			}
		}

		public byte LiquidAmount {
			get => liquid;
			set => liquid = value;
		}

		public bool IsAir {
			get => !IsBitSet(sTileHeader, 5);
			set {
				if (value)
					ResetBit(ref sTileHeader, 5);
				else
					SetBit(ref sTileHeader, 5);
			}
		}

		public bool IsActuated {
			get => IsBitSet(sTileHeader, 6);
			set {
				if (value)
					SetBit(ref sTileHeader, 6);
				else
					ResetBit(ref sTileHeader, 6);
			}
		}

		public bool HasActuator {
			get => IsBitSet(sTileHeader, 11);
			set {
				if (value)
					SetBit(ref sTileHeader, 11);
				else
					ResetBit(ref sTileHeader, 11);
			}
		}

		public bool IsHalfBrick {
			get => IsBitSet(sTileHeader, 10);
			set {
				if (value)
					SetBit(ref sTileHeader, 10);
				else
					ResetBit(ref sTileHeader, 10);
			}
		}

		public bool RedWire {
			get => IsBitSet(sTileHeader, 7);
			set {
				if (value)
					SetBit(ref sTileHeader, 7);
				else
					ResetBit(ref sTileHeader, 7);
			}
		}

		public bool GreenWire {
			get => IsBitSet(sTileHeader, 9);
			set {
				if (value)
					SetBit(ref sTileHeader, 9);
				else
					ResetBit(ref sTileHeader, 9);
			}
		}

		public bool BlueWire {
			get => IsBitSet(sTileHeader, 8);
			set {
				if (value)
					SetBit(ref sTileHeader, 8);
				else
					ResetBit(ref sTileHeader, 8);
			}
		}

		public bool YellowWire {
			get => IsBitSet(bTileHeader, 7);
			set {
				if (value)
					SetBit(ref bTileHeader, 7);
				else
					ResetBit(ref bTileHeader, 7);
			}
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