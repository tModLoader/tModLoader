using ExampleMod.Content.Dusts;
using ExampleMod.Content.Items.Placeable;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Tiles.Plants
{
	// This is a plant that grows naturally on ExampleGrass. The natural spawning logic for this tile is in ExampleGrass.
	public class ExamplePlants : ModTile
	{
		public override void SetStaticDefaults() {
			TileID.Sets.TileCutIgnore.Regrowth[Type] = true;
			TileID.Sets.ReplaceTileBreakUp[Type] = true;
			TileID.Sets.SlowlyDiesInWater[Type] = true;
			TileID.Sets.SwaysInWindBasic[Type] = true;
			TileID.Sets.IgnoredByGrowingSaplings[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileCut[Type] = true;
			Main.tileNoFail[Type] = true;
			Main.tileLavaDeath[Type] = true;
			Main.tileLighted[Type] = true;

			HitSound = SoundID.Grass;
			DustType = ModContent.DustType<Sparkle>();

			AddMapEntry(new Color(135, 150, 174));
		}

		public override IEnumerable<Item> GetItemDrops(int i, int j) {
			// Drop the seeds very rarely
			if (Main.rand.NextBool(100)) {
				yield return new Item(ModContent.ItemType<ExampleGrassSeeds>());
			}

			/*
			// Drop a Seed (50% chance) if the Blowpipe or Blowgun (ItemID.Sets.DropSeedsIfInInventory) is in the player's inventory.
			if (WorldGen.KillTile_ShouldDropSeeds(i, j)) {
				yield return new Item(ItemID.Seed);
			}
			*/
		}

		public override void SickleDropHay(int i, int j, ref int hayStack) {
			hayStack = Main.rand.Next(1, 3); // This plant drops 1-2 hay when cut with a sickle, like other 1 tile tall plants.
		}

		public override void DropCritterChance(int i, int j, ref int wormChance, ref int grassHopperChance, ref int jungleGrubChance) {
			// This critter drop chance logic matches normal grass.
			wormChance = 400;
			grassHopperChance = 100;

			// If the player is wearing Flower Boots, drop rates are decreased drastically for "flower" styles, likely to prevent players from farming critters with Flower Boots.
			int playerIndex = Player.FindClosest(new Vector2(i * 16, j * 16), 16, 16);
			if (Main.player[playerIndex].flowerBoots && Main.tile[i, j].TileFrameX >= 6 * 18) {
				wormChance *= 10000;
				grassHopperChance *= 10000;
			}
		}

		// When the tile below ExamplePlant changes to another tile, we might need to adjust it by using TileFrame.
		// If the tile below is mined or changed to a non-grass tile, we kill this plant.
		// If the tile below changes to a grass tile, we will convert to the corresponding vanilla plant tile.
		// ExampleSolutionConversion.ConvertGrass handles the opposite conversion, converting vanilla plants to ExamplePlants.
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			Tile tile = Main.tile[i, j];
			Tile below = Main.tile[i, j + 1];

			// Verify that the tile below is a valid placement for this tile
			int belowTileType = -1;
			if (below.HasUnactuatedTile && !below.IsHalfBlock && below.Slope == SlopeType.Solid)
				belowTileType = below.TileType;

			int[] validGrassTypes = [ModContent.TileType<ExampleGrass_Dirt>(), ModContent.TileType<ExampleGrass_ExampleBlock>()];
			if (validGrassTypes.Contains(belowTileType))
				return true;

			// If the tile below is not valid, attempt to convert to another vanilla plant tile corresponding to the grass tile
			// TileFrameX adjustments are used to control the final style varieties for the converted plants since not all plants have matching variety counts.
			int plantTileType = -1;
			switch (belowTileType) {
				case TileID.CorruptGrass:
				case TileID.CorruptJungleGrass:
					plantTileType = TileID.CorruptPlants;
					if (tile.TileFrameX >= 162)
						tile.TileFrameX = 126; 
					break;
				case TileID.Grass:
				case TileID.GolfGrass:
					plantTileType = TileID.Plants;
					break;
				case TileID.HallowedGrass:
				case TileID.GolfGrassHallowed:
					plantTileType = TileID.HallowedPlants;
					break;
				case TileID.CrimsonGrass:
				case TileID.CrimsonJungleGrass:
					plantTileType = TileID.CrimsonPlants;
					break;
				case TileID.JungleGrass:
					plantTileType = TileID.JunglePlants;
					while (tile.TileFrameX > 126) {
						tile.TileFrameX -= 126;
					}
					break;
				case TileID.MushroomGrass:
					plantTileType = TileID.MushroomPlants;
					while (tile.TileFrameX > 72) {
						tile.TileFrameX -= 72;
					}
					break;
			}

			if (plantTileType != -1) {
				tile.TileType = (ushort)plantTileType;
			}
			else {
				// The tile is killed if a suitable conversion is not found
				WorldGen.KillTile(i, j);
			}

			return true;
		}

		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			height = 20; // This tile is taller than normal to draw into the grass
		}

		public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
			if (i % 2 == 0) {
				spriteEffects = SpriteEffects.FlipHorizontally;
			}
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			// These values give it a slight glow similar to AshGrass. Remove if that is not desired.
			r = 0.325f;
			g = 0.15f;
			b = 0.05f;
		}
	}
}
