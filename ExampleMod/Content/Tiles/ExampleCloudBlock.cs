using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.WaterfallManager;

namespace ExampleMod.Content.Tiles
{
	public class ExampleCloudBlock : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileNoSunLight[Type] = false;
			Main.tileSolid[Type] = true;
			TileID.Sets.MergesWithClouds[Type] = true;
			TileID.Sets.Clouds[Type] = true;
			TileID.Sets.ChecksForMerge[Type] = true;
			TileID.Sets.NegatesFallDamage[Type] = true;
			DustType = DustID.RainCloud;
		}

		public override bool HasWalkDust() {
			return true;
		}

		public override void WalkDust(ref int dustType, ref bool makeDust, ref Color color) {
			dustType = DustID.RainCloud;
			color = new(100, 150, 130, 100);
		}

		//This method is used to spawn a waterfall after waterfalls have been processed
		//Liquids use this to spawn their waterfalls next to slabs/slopes, Cloud blocks use this to spawn their rain/snow effects (which are waterfalls)
		//Here we use this hook to spawn our rain waterfall beneith it.
		public override WaterfallData CreateWaterfall(int i, int j) {
			Tile below = Main.tile[i, j + 1];
			if (below.Slope == 0 && !WorldGen.SolidTile(below)) //as long as below us the slope is normal and not solid...
			{
				return new() //we spawn a new waterfall with the following data
				{
					type = ModContent.GetInstance<ExampleRainWaterfall>().Slot, //Honey Rain shows a customly drawn waterfall, render similarly to both to snow and rain
					x = i,
					y = j + 1
				};
			}
			return new WaterfallData() { type = -1, x = i, y = j }; //otherwise we return null. Null makes the tile not spawn any waterfall, defaulting to the normal beaviour of most tiles
		}
	}
}
