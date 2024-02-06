using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Tiles;

public class ExampleSand : ModTile
{
	public override void SetStaticDefaults()
	{
		AddMapEntry(new Color(150, 150, 150));
		Main.tileSolid[Type] = true;
		Main.tileBrick[Type] = true;
		Main.tileMergeDirt[Type] = true;
		Main.tileBlockLight[Type] = true;
		Main.tileSand[Type] = true;
		TileID.Sets.Conversion.Sand[Type] = true;
		TileID.Sets.ForAdvancedCollision.ForSandshark[Type] = true;
		TileID.Sets.CanBeDugByShovel[Type] = true;
		TileID.Sets.GeneralPlacementTiles[Type] = false;
		TileID.Sets.ChecksForMerge[Type] = true;
		TileID.Sets.Falling[Type] = true;
		TileID.Sets.CanBeClearedDuringOreRunner[Type] = true;
		Main.tileMerge[Type][TileID.Stone] = true;
		Main.tileMerge[TileID.Stone][Type] = true;
		// Set the FallingBlockProjectileID to the projectile's ID
		TileID.Sets.FallingBlockProjectileID[Type] = ModContent.ProjectileType<Projectiles.ExampleFallingSandProjectile>();
		// Set the FallingBlockProjectileDamage to 10
		TileID.Sets.FallingBlockProjectileDamage[Type] = 10;
		DustType = DustID.Stone;
	}

	public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
	{
		Tile tile = Main.tile[i, j];
		Tile tileAbove = Main.tile[i, j - 1];
		Tile tileBelow = Main.tile[i, j + 1];
		if (j < Main.maxTilesY && !Main.tile[i, j + 1].HasTile)
		{
			WorldGen.SpawnFallingBlockProjectile(i, j, tile, tileAbove, tileBelow, Type);
		}
		return true;
	}
	public override bool HasWalkDust()
	{
		return Main.rand.NextBool(3);
	}
}
