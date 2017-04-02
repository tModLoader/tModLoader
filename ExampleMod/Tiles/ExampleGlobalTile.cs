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
		public override bool Drop(int i, int j, int type)
		{
			// Get mod player
			var modPlayer = Main.LocalPlayer.GetModPlayer<ExamplePlayer>(mod);

			if (modPlayer.ZoneExample && (type == TileID.Tombstones))
				return false;

			return true;
		}
	}
}
