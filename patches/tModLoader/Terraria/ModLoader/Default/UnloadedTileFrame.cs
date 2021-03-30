namespace Terraria.ModLoader.Default
{
	internal class UnloadedTileFrame
	{
		private short frameX;
		private short frameY;

		public short FrameX => frameX;
		public short FrameY => frameY;

		public int FrameID {
			get {
				return frameY * (short.MaxValue + 1) + frameX;
			}
			set {
				frameX = (short)(value % (short.MaxValue + 1));
				frameY = (short)(value / (short.MaxValue + 1));
			}
		}

		public UnloadedTileFrame(int value) {
			FrameID = value;
		}

		public UnloadedTileFrame(short frameX, short frameY) {
			this.frameX = frameX;
			this.frameY = frameY;
		}
	}
}
