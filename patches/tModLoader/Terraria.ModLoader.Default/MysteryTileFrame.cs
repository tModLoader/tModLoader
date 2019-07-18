namespace Terraria.ModLoader.Default
{
	internal class MysteryTileFrame
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

		public MysteryTileFrame(int value) {
			FrameID = value;
		}

		public MysteryTileFrame(short frameX, short frameY) {
			this.frameX = frameX;
			this.frameY = frameY;
		}
	}
}
