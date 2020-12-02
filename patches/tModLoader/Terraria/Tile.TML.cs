using System;

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

		[Flags]
		public enum WireColor
		{
			Red = 1,
			Blue = 2,
			Green = 4,
			Yellow = 8,
			None = 0
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

		public WireColor Wire {
			get {
				WireColor wire = WireColor.None;
				if (IsBitSet(sTileHeader, 7)) wire |= WireColor.Red;
				if (IsBitSet(sTileHeader, 8)) wire |= WireColor.Blue;
				if (IsBitSet(sTileHeader, 9)) wire |= WireColor.Green;
				if (IsBitSet(bTileHeader, 7)) wire |= WireColor.Yellow;
				return wire;
			}
			set {
				if ((value & WireColor.Red) == WireColor.Red) SetBit(ref sTileHeader, 7);
				else ResetBit(ref sTileHeader, 7);

				if ((value & WireColor.Blue) == WireColor.Blue) SetBit(ref sTileHeader, 8);
				else ResetBit(ref sTileHeader, 8);

				if ((value & WireColor.Green) == WireColor.Green) SetBit(ref sTileHeader, 9);
				else ResetBit(ref sTileHeader, 9);

				if ((value & WireColor.Yellow) == WireColor.Yellow) SetBit(ref bTileHeader, 7);
				else ResetBit(ref bTileHeader, 7);
			}
		}
		
		// todo: wall frame, color, wall color, slope
	}
}