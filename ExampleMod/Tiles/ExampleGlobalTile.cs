using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Tiles
{
	internal sealed class ExampleGlobalTile : GlobalTile
	{
		public override bool Drop(int i, int j, int type) {
			// Get mod player
			var modPlayer = Main.LocalPlayer.GetModPlayer<ExamplePlayer>();

			if (modPlayer.ZoneExample && type == TileID.Tombstones) {
				return false;
			}

			return true;
		}
	}
}
