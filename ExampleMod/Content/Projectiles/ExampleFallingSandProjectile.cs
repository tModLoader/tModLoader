using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles;

public class ExampleFallingSandProjectile : ModProjectile
{
    public override void SetStaticDefaults()
    {
		ProjectileID.Sets.FallingBlockDoesNotFallThroughPlatforms[Projectile.type] = true;
    }
    public override void SetDefaults()
    {
		Projectile.knockBack = 6f;
		Projectile.width = 10;
		Projectile.height = 10;
		Projectile.friendly = true;
		Projectile.hostile = true;
		Projectile.penetrate = -1;
		Projectile.aiStyle = -1;
    }

    public override void AI()
    {
		if (Main.rand.NextBool(2))
		{
			int i = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Stone, 0f, Projectile.velocity.Y * 0.5f);
			Main.dust[i].velocity.X *= 0.2f;
		}
		Projectile.velocity.Y += 0.41f;
		Projectile.rotation += 0.1f;
		if (Projectile.velocity.Y > 10f)
		{
			Projectile.velocity.Y = 10f;
		}
		base.AI();
	}

	public override void OnKill(int timeLeft)
	{
		Point tileCoords = Projectile.Center.ToTileCoordinates();
		if (tileCoords.X >= 0 && tileCoords.X < Main.maxTilesX && tileCoords.Y >= 0 && tileCoords.Y < Main.maxTilesY)
		{
			Tile tile = Main.tile[tileCoords.X, tileCoords.Y];
			if (tile.IsHalfBlock && Projectile.velocity.Y > 0f && Math.Abs(Projectile.velocity.Y) > Math.Abs(Projectile.velocity.X))
			{
				tile = Main.tile[tileCoords.X, --tileCoords.Y];
			}
			if (Main.tileCut[tile.TileType])
			{
				WorldGen.KillTile(tileCoords.X, tileCoords.Y);
			}
			if (!Main.tileSolid[tile.TileType] || TileID.Sets.IsATreeTrunk[tile.TileType])
			{
				Item.NewItem(WorldGen.GetItemSource_FromTileBreak(tileCoords.X, tileCoords.Y), tileCoords.X * 16, tileCoords.Y * 16, 16, 16, ModContent.ItemType<Items.Placeable.ExampleSandBlock>());
				SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
			}
			if (!tile.HasTile && tile.TileType != TileID.MinecartTrack)
			{
				Tile tileBelow = Main.tile[tileCoords.X, tileCoords.Y + 1];
				if (tileBelow.Slope != SlopeType.Solid)
				{
					tileBelow.Slope = SlopeType.Solid;
				}
				if (tileBelow.IsHalfBlock)
				{
					tileBelow.IsHalfBlock = false;
				}
				WorldGen.PlaceTile(tileCoords.X, tileCoords.Y, ModContent.TileType<Tiles.ExampleSand>(), forced: true);
				WorldGen.SquareTileFrame(tileCoords.X, tileCoords.Y);
			}
		}
	}

	public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
    {
        fallThrough = !ProjectileID.Sets.FallingBlockDoesNotFallThroughPlatforms[Projectile.type];
        return true;
    }
	public override bool? CanDamage()
	{
		return Projectile.localAI[1] != -1f;
	}
}

public class ExampleSandSandgunProjectile : ModProjectile
{
	public override string Texture => ModContent.GetInstance<ExampleFallingSandProjectile>().Texture;
	public override void SetStaticDefaults()
	{
		ProjectileID.Sets.FallingBlockDoesNotFallThroughPlatforms[Projectile.type] = true;
	}
	public override void SetDefaults()
	{
		Projectile.CloneDefaults(ProjectileID.EbonsandBallGun);
		Projectile.aiStyle = -1;
	}

	public override void AI()
	{
		if (Main.rand.NextBool(2))
		{
			int i = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Stone, 0f, Projectile.velocity.Y * 0.5f);
			Main.dust[i].velocity.X *= 0.2f;
		}
		Projectile.ai[1]++;
		if (Projectile.ai[1] >= 60)
		{
			Projectile.ai[1] = 60;
			Projectile.velocity.Y += 0.2f;
		}
		Projectile.rotation += 0.1f;
		if (Projectile.velocity.Y > 10f)
		{
            Projectile.velocity.Y = 10f;
		}
		base.AI();
	}

	public override void OnKill(int timeLeft)
	{
		Point tileCoords = Projectile.Center.ToTileCoordinates();
		if (tileCoords.X >= 0 && tileCoords.X < Main.maxTilesX && tileCoords.Y >= 0 && tileCoords.Y < Main.maxTilesY)
		{
			Tile tile = Main.tile[tileCoords.X, tileCoords.Y];
			if (tile.IsHalfBlock && Projectile.velocity.Y > 0f && Math.Abs(Projectile.velocity.Y) > Math.Abs(Projectile.velocity.X))
			{
				tile = Main.tile[tileCoords.X, --tileCoords.Y];
			}
			if (Main.tileCut[tile.TileType]) {
				WorldGen.KillTile(tileCoords.X, tileCoords.Y);
			}
			// this section allows the projectile to drop its item if it can't be placed as a tile (vanilla doesn't do this for some reason)
			if (!Main.tileSolid[tile.TileType] || TileID.Sets.IsATreeTrunk[tile.TileType])
			{
				Item.NewItem(WorldGen.GetItemSource_FromTileBreak(tileCoords.X, tileCoords.Y), tileCoords.X * 16, tileCoords.Y * 16, 16, 16, ModContent.ItemType<Items.Placeable.ExampleSandBlock>());
				SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
			}
			// end section
			if (!tile.HasTile && tile.TileType != TileID.MinecartTrack)
			{
				Tile tileBelow = Main.tile[tileCoords.X, tileCoords.Y + 1];
				if (tileBelow.Slope != SlopeType.Solid)
				{
					tileBelow.Slope = SlopeType.Solid;
				}
				if (tileBelow.IsHalfBlock)
				{
					tileBelow.IsHalfBlock = false;
				}
				WorldGen.PlaceTile(tileCoords.X, tileCoords.Y, ModContent.TileType<Tiles.ExampleSand>(), forced: true);
				WorldGen.SquareTileFrame(tileCoords.X, tileCoords.Y);
			}
		}
	}

	public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
	{
		fallThrough = !ProjectileID.Sets.FallingBlockDoesNotFallThroughPlatforms[Projectile.type];
		return true;
	}
	public override bool? CanDamage()
	{
		return Projectile.localAI[1] != -1f;
	}
}
