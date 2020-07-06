using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Tiles
{
	public class AnimatedLoom : GlobalTile
	{
		public override void AnimateTile() {
			if (++Main.tileFrameCounter[TileID.Loom] >= 16) {
				Main.tileFrameCounter[TileID.Loom] = 0;
				if (++Main.tileFrame[TileID.Loom] >= 4) {
					Main.tileFrame[TileID.Loom] = 0;
				}
			}
		}
	}
}
