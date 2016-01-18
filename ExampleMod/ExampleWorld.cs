using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.World.Generation;

namespace ExampleMod
{
	public class ExampleWorld : ModWorld
	{
		public override void PostWorldGen()
		{
			for(int i = 0; i < Main.maxTilesX; i++)
			{
				Main.tile[i, Main.maxTilesY / 2].type = TileID.Chlorophyte;
			}
		}
	}
}
