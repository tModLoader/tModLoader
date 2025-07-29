using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExampleMod.Content.Tiles
{
	public class ExampleBar : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileShine[Type] = 1100;
			Main.tileSolid[Type] = true;
			Main.tileSolidTop[Type] = true;
			Main.tileFrameImportant[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.addTile(Type);

			VanillaFallbackOnModDeletion = TileID.MetalBars;

			AddMapEntry(new Color(200, 200, 200), Language.GetText("MapObject.MetalBar")); // localized text for "Metal Bar"
		}

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			// This check will destroy this tile if the tile below has become sloped such that it doesn't have a solid top side.
			// This is necessary in this case because Bar tiles can be placed on top of each other but can also be hammered to be half bricks despite being tileSolidTop.
			if (!WorldGen.SolidTileAllowBottomSlope(i, j + 1)) {
				WorldGen.KillTile(i, j);
			}
			return true;
		}
	}
}
