namespace Terraria.ModLoader.Default;

internal partial class LegacyUnloadedTilesSystem
{
	private struct TileFrame
	{
		private short frameX;
		private short frameY;

		public short FrameX => frameX;
		public short FrameY => frameY;

		public int FrameID {
			get => frameY * (short.MaxValue + 1) + frameX;
			set {
				frameX = (short)(value % (short.MaxValue + 1));
				frameY = (short)(value / (short.MaxValue + 1));
			}
		}

		public TileFrame(int value)
		{
			frameX = 0;
			frameY = 0;
			FrameID = value;
		}

		public TileFrame(short frameX, short frameY)
		{
			this.frameX = frameX;
			this.frameY = frameY;
		}
	}

}