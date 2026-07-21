using ExampleMod.Content.Tiles.Plants;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Tiles
{
	// This file is an example of making a grass tile. Grass tiles have a few unique behaviors:
	// 1. Grass tiles are tiles that place over an existing "dirt" tile, unlike other tiles which can only be placed in empty spaces.
	// 2. When mined, grass tiles revert to their "dirt" tile instead of being destroyed.
	// 3. Grass tiles spread to nearby "dirt" tiles and also usually spawn other plants as well.
	//
	// In Terraria, Dirt, Mud, and Ash are all "dirt" tile options, but modded "dirt" tiles are also supported. Some grasses are placeable on multiple "dirt" tiles.
	// This example includes ExampleGrass_Dirt and ExampleGrass_ExampleBlock. ExampleGrass_Dirt is placed on Dirt and ExampleGrass_ExampleBlock is placed on ExampleBlock.
	// The ExampleGrass abstract class contains the shared logic.
	//
	// ExampleGrassSeeds places both of these grass tiles, similar to how Corrupt Grass and Corrupt Jungle Grass are both placed by Corrupt Seeds.
	public abstract class ExampleGrass : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileBrick[Type] = true;
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;

			TileID.Sets.Conversion.Grass[Type] = true; // Indicates that this tile converts with other grass tiles. Also affects tile frameing and merging.
			TileID.Sets.CanBeDugByShovel[Type] = true; // Allows the shovel item to mine this tile.
			TileID.Sets.ResetsHalfBrickPlacementAttempt[Type] = false; // Prevents the existing tile from losing its half-brick status when this grass is placed over it.
			TileID.Sets.DoesntPlaceWithTileReplacement[Type] = true;
			TileID.Sets.ChecksForMerge[Type] = true;
			//TileID.Sets.GrassNotKilledWhenSurrounded[Type] = true; // Would allow the tile to stay even when surrounded by solid tiles.
			TileID.Sets.Infectable[Type] = true;

			AddMapEntry(new Color(152, 171, 198));
		}

		public override void OverridePlacementTile(int i, int j, Item item, ref int tileToCreate, ref int previewPlaceStyle, ref int? forcedRandom, ref bool? overrideCanPlace) {
			// When this tile is about to be placed, this method allows us to change which tile is actually placed depending on the existing "dirt" tile at the target location. This is how grass tiles can support multiple "dirt" tiles. This method can be removed for grass tiles that do not have alternate "dirt" tiles.
			Tile tile = Main.tile[i, j];
			if (tile.HasTile) {
				if (tile.TileType == TileID.Dirt) {
					tileToCreate = ModContent.TileType<ExampleGrass_Dirt>();
				}
				if (tile.TileType == ModContent.TileType<ExampleBlock>()) {
					tileToCreate = ModContent.TileType<ExampleGrass_ExampleBlock>();
				}
			}
			return;
		}

		// We use RandomUpdate to spread this grass and also grow ExamplePlants on top.
		public override void RandomUpdate(int i, int j) {
			Tile tile = Main.tile[i, j];
			int grassType = tile.TileType;

			Tile above = Main.tile[i, j - 1];
			if (!above.HasTile && WorldGen.genRand.NextBool(10)) {
				WorldGen.PlaceTile(i, j - 1, ModContent.TileType<ExamplePlants>(), mute: true);
				if (above.HasTile) {
					above.CopyPaintAndCoating(tile); // Plants that grow on grass inherit the paint of the grass.

					// Manually assign a random placement style. Since this tile doesn't use a TileObjectData, we need to do this here.
					above.TileFrameX = (short)(WorldGen.genRand.Next(11) * 18);
				}

				if (Main.netMode == NetmodeID.Server && above.HasTile) {
					NetMessage.SendTileSquare(-1, i, j - 1);
				}
			}

			// Attempt to spread grass to neighboring "dirt" tiles.
			TileColorCache tileColor = tile.BlockColorAndCoating();
			bool grassHasSpread = false;
			for (int i2 = i - 1; i2 <= i + 1; i2++) {
				for (int j2 = j - 1; j2 <= j + 1; j2++) {
					Tile neighbor = Main.tile[i2, j2];
					if (i == i2 && j == j2 || !neighbor.HasTile) {
						continue;
					}
					if (neighbor.TileType == TileID.Dirt) {
						WorldGen.SpreadGrass(i2, j2, TileID.Dirt, grassType, repeat: false, tileColor);
						if (neighbor.TileType == grassType) {
							WorldGen.SquareTileFrame(i2, j2);
							grassHasSpread = true;
						}
					}
					else if (neighbor.TileType == ModContent.TileType<ExampleBlock>()) {
						WorldGen.SpreadGrass(i2, j2, ModContent.TileType<ExampleBlock>(), grassType, repeat: false, tileColor);
						if (neighbor.TileType == grassType) {
							WorldGen.SquareTileFrame(i2, j2);
							grassHasSpread = true;
						}
					}
				}
			}

			if (Main.netMode == NetmodeID.Server && grassHasSpread) {
				NetMessage.SendTileSquare(-1, i, j, 3);
			}
		}
	}

	// These classes inherit from the abstract ExampleGrass class to share logic.
	public class ExampleGrass_Dirt : ExampleGrass
	{
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();

			// Indicates that this tile is a grass tile that uses TileID.Dirt as its "dirt" tile.
			TileID.Sets.Grass[Type] = true;

			TileID.Sets.Conversion.MergesWithDirtInASpecialWay[Type] = true; // Facilitates tile framing and merging with dirt.
		}
	}

	public class ExampleGrass_ExampleBlock : ExampleGrass
	{
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();

			// Indicates that this tile is a grass tile that uses ExampleBlock as its "dirt" tile.
			TileID.Sets.NeedsGrassFraming[Type] = true;
			TileID.Sets.NeedsGrassFramingDirt[Type] = ModContent.TileType<ExampleBlock>();
		}
	}
}
