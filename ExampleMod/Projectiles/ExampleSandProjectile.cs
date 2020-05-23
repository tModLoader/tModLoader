using ExampleMod.Dusts;
using ExampleMod.Tiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Projectiles
{
	public class ExampleSandProjectile : ModProjectile
	{
		protected bool falling = true;
		protected int tileType;
		protected int dustType;

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Example Sand Ball");
			ProjectileID.Sets.ForcePlateDetection[projectile.type] = true;
		}

		public override void SetDefaults() {
			projectile.knockBack = 6f;
			projectile.width = 10;
			projectile.height = 10;
			projectile.friendly = true;
			projectile.hostile = true;
			projectile.penetrate = -1;
			//Set the tile type to ExampleSand
			tileType = ModContent.TileType<ExampleSand>();
			dustType = ModContent.DustType<Sparkle>();
		}

		public override void AI() {
			//Change the 5 to determine how much dust will spawn. lower for more, higher for less
			if (Main.rand.Next(5) == 0) {
				int dust = Dust.NewDust(projectile.position, projectile.width, projectile.height, dustType);
				Main.dust[dust].velocity.X *= 0.4f;
			}

			projectile.tileCollide = true;
			projectile.localAI[1] = 0f;

			if (projectile.ai[0] == 1f) {
				if (!falling) {
					projectile.ai[1] += 1f;

					if (projectile.ai[1] >= 60f) {
						projectile.ai[1] = 60f;
						projectile.velocity.Y += 0.2f;
					}
				}
				else
					projectile.velocity.Y += 0.41f;
			}
			else if (projectile.ai[0] == 2f) {
				projectile.velocity.Y += 0.2f;

				if (projectile.velocity.X < -0.04f)
					projectile.velocity.X += 0.04f;
				else if (projectile.velocity.X > 0.04f)
					projectile.velocity.X -= 0.04f;
				else
					projectile.velocity.X = 0f;
			}

			projectile.rotation += 0.1f;

			if (projectile.velocity.Y > 10f)
				projectile.velocity.Y = 10f;
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough) {
			if (falling)
				projectile.velocity = Collision.AnyCollision(projectile.position, projectile.velocity, projectile.width, projectile.height, true);
			else
				projectile.velocity = Collision.TileCollision(projectile.position, projectile.velocity, projectile.width, projectile.height, fallThrough, fallThrough, 1);

			return false;
		}

		public override void Kill(int timeLeft) {
			if (projectile.owner == Main.myPlayer && !projectile.noDropItem) {
				int tileX = (int)(projectile.position.X + projectile.width / 2) / 16;
				int tileY = (int)(projectile.position.Y + projectile.width / 2) / 16;

				Tile tile = Main.tile[tileX, tileY];
				Tile tileBelow = Main.tile[tileX, tileY + 1];

				if (tile.halfBrick() && projectile.velocity.Y > 0f && System.Math.Abs(projectile.velocity.Y) > System.Math.Abs(projectile.velocity.X))
					tileY--;

				if (!tile.active()) {
					bool onMinecartTrack = tileY < Main.maxTilesY - 2 && tileBelow != null && tileBelow.active() && tileBelow.type == TileID.MinecartTrack;

					if (!onMinecartTrack)
						WorldGen.PlaceTile(tileX, tileY, tileType, false, true);

					if (!onMinecartTrack && tile.active() && tile.type == tileType) {
						if (tileBelow.halfBrick() || tileBelow.slope() != 0) {
							WorldGen.SlopeTile(tileX, tileY + 1, 0);

							if (Main.netMode == NetmodeID.Server)
								NetMessage.SendData(MessageID.TileChange, -1, -1, null, 14, tileX, tileY + 1);
						}

						if (Main.netMode != NetmodeID.SinglePlayer)
							NetMessage.SendData(MessageID.TileChange, -1, -1, null, 1, tileX, tileY, tileType);
					}
				}
			}
		}

		public override bool CanDamage() => projectile.localAI[1] != -1f;
	}
}