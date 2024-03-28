using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Tiles
{
	public class ExamplePlanterBox : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileTable[Type] = true;
			Main.tileSolid[Type] = true;
			Main.tileSolidTop[Type] = true;
			AddMapEntry(new Color(200, 200, 200));
			TileID.Sets.DisableSmartCursor[Type] = true;
			TileID.Sets.IgnoresNearbyHalfbricksWhenDrawn[Type] = true; //All vanilla planter boxes have this
			TileID.Sets.PlanterBoxes[Type] = true; //This does a majority of the work for us,
												   //both allowing vanilla herbs to attach to our planterbox amongst a list of other things
			AdjTiles = new int[] { TileID.PlanterBox };
		}

		public override bool Slope(int i, int j) {
			return false; //We dont want our planter box to be slope-able so we return false
		}
	}
}
