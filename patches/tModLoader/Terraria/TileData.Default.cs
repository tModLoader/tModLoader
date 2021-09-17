namespace Terraria
{
	public struct TileTypeData : ITileData
	{
		public ushort Type;
	}

	public struct WallTypeData : ITileData
	{
		public ushort Type;
	}

	public struct TileLiquidData : ITileData
	{
		public byte Amount;
	}

	public struct TileFramingData : ITileData
	{
		public short FrameX;
		public short FrameY;
	}

	public struct WallFramingData : ITileData
	{
		public short FrameX;
		public short FrameY;
	}

	public struct MiscellaneousTileData : ITileData
	{
		internal ushort UShortHeader; // TML: Changed from 'short' for easier bitwise manipulations
		internal byte ByteHeader1;
		internal byte ByteHeader2;
		internal byte ByteHeader3;
	}
}
