using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Tiles
{
	internal sealed class ExampleGlobalTile : GlobalTile
	{
		public override bool Drop(int i, int j, int type, TileStyle dropStyle)
		{
			// Get mod player
			var modPlayer = Main.LocalPlayer.GetModPlayer<ExamplePlayer>(mod);

			// If player is in example zone, any tile of these styles will not drop
			// This is includes signs, tombstones
			if (modPlayer.ZoneExample
				&& (dropStyle == TileStyle.Style2x2
				|| dropStyle == TileStyle.Style3x3
				|| dropStyle == TileStyle.Style2x3
				|| dropStyle == TileStyle.Style1x1))
				return false;

			// For TileDrop.StyleXxX types, you will need to check X (which is not set) manually
			// For example: if tileDrop == TileDrop.Style2xX, then you will need to check for height manually (height will be 0)

			return true;
		}
	}
}
