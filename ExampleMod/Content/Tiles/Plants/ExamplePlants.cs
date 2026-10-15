using ExampleMod.Content.Dusts;
using ExampleMod.Content.Items.Placeable;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
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
