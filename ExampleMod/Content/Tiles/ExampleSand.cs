using ExampleMod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Tiles
{
	// ExampleSand is a sand tile. Sand tiles are unique in how they cascade down.
	// When a sand tile determines that no tile is below it, it destroys itself and spawns a falling projectile (ExampleSandBallFallingProjectile) in its place.
	// When that projectile hits another tile, it creates the sand tile at that location.
	public class ExampleSand : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBrick[Type] = true;
			Main.tileMergeDirt[Type] = true;
			Main.tileBlockLight[Type] = true;

			// Sand specific properties
			Main.tileSand[Type] = true;
			TileID.Sets.Conversion.Sand[Type] = true; // Allows Clentaminator solutions to convert this tile to their respective Sand tiles.
			TileID.Sets.ForAdvancedCollision.ForSandshark[Type] = true; // Allows Sandshark enemies to "swim" in this sand.
			TileID.Sets.CanBeDugByShovel[Type] = true;
			TileID.Sets.Falling[Type] = true;
			TileID.Sets.Suffocate[Type] = true;
			TileID.Sets.FallingBlockProjectile[Type] = new TileID.Sets.FallingBlockProjectileInfo(ModContent.ProjectileType<ExampleSandBallFallingProjectile>(), 10); // Tells which falling projectile to spawn when the tile should fall.

			TileID.Sets.CanBeClearedDuringOreRunner[Type] = true;
			TileID.Sets.GeneralPlacementTiles[Type] = false;
			TileID.Sets.ChecksForMerge[Type] = true;

			MineResist = 0.5f; // Sand tile typically require half as many hits to mine.
			DustType = DustID.Stone;
			AddMapEntry(new Color(150, 150, 150));
		}

		public override bool HasWalkDust() {
			return Main.rand.NextBool(3);
		}

		public override void WalkDust(ref int dustType, ref bool makeDust, ref Color color) {
			dustType = DustID.Sand;
		}
		// This code is called when the game attempts to convert our example tile into a new biome
		public override void Convert(int i, int j, int conversionType) {
			switch (conversionType) {
				case BiomeConversionID.Sand: // Yellow (desert) solution also converts tiles back into purity, so don't forget that check!
					WorldGen.ConvertTile(i, j, TileID.Sand);
					return;
			}
		}
	}
}