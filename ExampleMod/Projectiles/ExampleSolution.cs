using ExampleMod.Tiles;
using ExampleMod.Walls;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Projectiles
{
	public class ExampleSolution : ModProjectile
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Example Spray");
		}

		public override void SetDefaults() {
			projectile.width = 6;
			projectile.height = 6;
			projectile.friendly = true;
			projectile.alpha = 255;
			projectile.penetrate = -1;
			projectile.extraUpdates = 2;
			projectile.tileCollide = false;
			projectile.ignoreWater = true;
		}

		public override void AI() {
			int dustType = DustType<Dusts.ExampleSolution>();
			if (projectile.owner == Main.myPlayer) {
				Convert((int)(projectile.position.X + (float)(projectile.width / 2)) / 16, (int)(projectile.position.Y + (float)(projectile.height / 2)) / 16, 2);
			}
			if (projectile.timeLeft > 133) {
				projectile.timeLeft = 133;
			}
			if (projectile.ai[0] > 7f) {
				float dustScale = 1f;
				if (projectile.ai[0] == 8f) {
					dustScale = 0.2f;
				}
				else if (projectile.ai[0] == 9f) {
					dustScale = 0.4f;
				}
				else if (projectile.ai[0] == 10f) {
					dustScale = 0.6f;
				}
				else if (projectile.ai[0] == 11f) {
					dustScale = 0.8f;
				}
				projectile.ai[0] += 1f;
				for (int i = 0; i < 1; i++) {
					int dustIndex = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, dustType, projectile.velocity.X * 0.2f, projectile.velocity.Y * 0.2f, 100, default(Color), 1f);
					Dust dust = Main.dust[dustIndex];
					dust.noGravity = true;
					dust.scale *= 1.75f;
					dust.velocity.X *= 2f;
					dust.velocity.Y *= 2f;
					dust.scale *= dustScale;
				}
			}
			else {
				projectile.ai[0] += 1f;
			}
			projectile.rotation += 0.3f * (float)projectile.direction;
		}

		public void Convert(int i, int j, int size = 4) {
			for (int k = i - size; k <= i + size; k++) {
				for (int l = j - size; l <= j + size; l++) {
					if (WorldGen.InWorld(k, l, 1) && Math.Abs(k - i) + Math.Abs(l - j) < Math.Sqrt(size * size + size * size)) {
						int type = (int)Main.tile[k, l].type;
						int wall = (int)Main.tile[k, l].wall;
						if (wall != 0) {
							Main.tile[k, l].wall = (ushort)WallType<ExampleWall>();
							WorldGen.SquareWallFrame(k, l, true);
							NetMessage.SendTileSquare(-1, k, l, 1);
						}
						if (TileID.Sets.Conversion.Stone[type]) {
							Main.tile[k, l].type = (ushort)TileType<ExampleBlock>();
							WorldGen.SquareTileFrame(k, l, true);
							NetMessage.SendTileSquare(-1, k, l, 1);
						}
						else if (type == TileID.Chairs && Main.tile[k, l - 1].type == TileID.Chairs) {
							Main.tile[k, l].type = (ushort)TileType<ExampleChair>();
							Main.tile[k, l - 1].type = (ushort)TileType<ExampleChair>();
							WorldGen.SquareTileFrame(k, l, true);
							NetMessage.SendTileSquare(-1, k, l, 1);
						}
						else if (type == TileID.WorkBenches && Main.tile[k - 1, l].type == TileID.WorkBenches) {
							Main.tile[k, l].type = (ushort)TileType<ExampleWorkbench>();
							Main.tile[k - 1, l].type = (ushort)TileType<ExampleWorkbench>();
							WorldGen.SquareTileFrame(k, l, true);
							NetMessage.SendTileSquare(-1, k, l, 1);
						}
					}
				}
			}
		}
	}
}
