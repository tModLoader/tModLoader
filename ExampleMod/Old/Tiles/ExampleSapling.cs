using ExampleMod.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Tiles
{
	public class ExampleSapling : ModTile
	{
		public override void SetDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;

			TileObjectData.newTile.Width = 1;
			TileObjectData.newTile.Height = 2;
			TileObjectData.newTile.Origin = new Point16(0, 1);
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.AnchorValidTiles = new[] { TileType<ExampleBlock>() };
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.DrawFlipHorizontal = true;
			TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
			TileObjectData.newTile.LavaDeath = true;
			TileObjectData.newTile.RandomStyleRange = 3;
			TileObjectData.newTile.StyleMultiplier = 3;
			TileObjectData.newSubTile.CopyFrom(TileObjectData.newTile);
			TileObjectData.newSubTile.AnchorValidTiles = new int[] { TileType<ExampleSand>() };
			TileObjectData.addSubTile(1);
			TileObjectData.addTile(Type);

			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Example Sapling");
			AddMapEntry(new Color(200, 200, 200), name);

			sapling = true;
			dustType = DustType<Sparkle>();
			adjTiles = new int[] { TileID.Saplings };
		}

		public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

		public override void RandomUpdate(int i, int j) {
			// A random chance to slow down growth
			if (WorldGen.genRand.Next(20) == 0) {
				Tile tile = Framing.GetTileSafely(i, j); // Safely get the tile at the given coordinates
				bool growSucess; // A bool to see if the tree growing was sucessful.

				// Style 0 is for the ExampleTree sapling, and style 1 is for ExamplePalmTree, so here we check frameX to call the correct method.
				// Any pixels before 54 on the tilesheet are for ExampleTree while any pixels above it are for ExamplePalmTree
				if (tile.frameX < 54)
					growSucess = WorldGen.GrowTree(i, j);
				else
					growSucess = WorldGen.GrowPalmTree(i, j);

				// A flag to check if a player is near the sapling
				bool isPlayerNear = WorldGen.PlayerLOS(i, j);

				//If growing the tree was a sucess and the player is near, show growing effects
				if (growSucess && isPlayerNear)
					WorldGen.TreeGrowFXCheck(i, j);
			}
		}

		public override void SetSpriteEffects(int i, int j, ref SpriteEffects effects) {
			if (i % 2 == 1)
				effects = SpriteEffects.FlipHorizontally;
		}
	}
}