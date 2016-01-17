using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.World.Generation;

namespace ExampleMod
{
	public class ExampleWorldGen : ModWorld
	{
		public override void WorldGenModifyTaskList(List<GenPass> list)
		{
			list.RemoveRange(10, list.Count - 10);
		}

		public override void WorldGenPostGen()
		{
			for(int i = 0; i<Main.maxTilesX; i++)
			{
				Main.tile[i, Main.maxTilesY / 2].type = TileID.Chlorophyte;
			}
		}
	}
}
