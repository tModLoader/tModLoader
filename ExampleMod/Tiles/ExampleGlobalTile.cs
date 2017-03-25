using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Tiles
{
	internal class ExampleGlobalTile : GlobalTile
	{
		public override bool Drop(int i, int j, int type, TileDrop tileDrop)
		{
			// Get mod player
			var modPlayer = Main.LocalPlayer.GetModPlayer<ExamplePlayer>(mod);

			// If player is in example zone, any 2x2 tile will not drop
			if (modPlayer.ZoneExample
				&& tileDrop == TileDrop.Style2x2)
			{
				return false;
			}

			// For TileDrop.StyleXxX types, you will need to check X (which is not set) manually
			// For example: if tileDrop == TileDrop.Style2xX, then you will need to check for height manually (height will be 0)

			return true;
		}
	}
}
