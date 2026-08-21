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
