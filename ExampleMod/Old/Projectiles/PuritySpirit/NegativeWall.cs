using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Projectiles.PuritySpirit
{
	public class NegativeWall : ModProjectile
	{
		public const float speed = 2f;
		private static readonly Color color1 = new Color(100, 0, 100);
		private static readonly Color color2 = new Color(100, 0, 0);

		public override void SetDefaults() {
			projectile.width = 32;
			projectile.height = 32;
			projectile.alpha = 127;
			projectile.penetrate = -1;
			projectile.magic = true;
			projectile.tileCollide = false;
			projectile.ignoreWater = true;
		}

		public override void AI() {
			projectile.localAI[0] += 1f;
			projectile.localAI[0] %= 120f;
			if (projectile.ai[1] > 0f && projectile.height != (int)projectile.ai[1]) {
				Vector2 center = projectile.Center;
				projectile.height = (int)projectile.ai[1];
				projectile.Center = center;
			}
			if (projectile.ai[1] < 0f && projectile.width != (int)-projectile.ai[1]) {
				Vector2 center = projectile.Center;
				projectile.width = (int)-projectile.ai[1];
				projectile.Center = center;
			}
			NPC npc = Main.npc[(int)projectile.ai[0]];
			int arenaWidth = NPCs.PuritySpirit.PuritySpirit.arenaWidth;
			int arenaHeight = NPCs.PuritySpirit.PuritySpirit.arenaHeight;
			if (projectile.Center.X >= npc.Center.X + arenaWidth / 2) {
				projectile.velocity.X = -speed;
			}
			else if (projectile.Center.X <= npc.Center.X - arenaWidth / 2) {
				projectile.velocity.X = speed;
			}
			if (projectile.Center.Y >= npc.Center.Y + arenaHeight / 2) {
				projectile.velocity.Y = -speed;
			}
			else if (projectile.Center.Y <= npc.Center.Y - arenaHeight / 2) {
				projectile.velocity.Y = speed;
			}
			for (int k = 0; k < 255; k++) {
				Player player = Main.player[k];
				if (player.active && !player.dead && player.Hitbox.Intersects(projectile.Hitbox)) {
					ExamplePlayer modPlayer = player.GetModPlayer<ExamplePlayer>();
					if (modPlayer.purityDebuffCooldown <= 0) {
						modPlayer.PuritySpiritDebuff();
						modPlayer.purityDebuffCooldown = Main.expertMode ? 60 : 90;
					}
				}
			}
			projectile.timeLeft = 2;
			if (!npc.active || npc.type != NPCType<NPCs.PuritySpirit.PuritySpirit>()) {
				projectile.Kill();
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
			Color color;
			if (projectile.localAI[0] < 60f) {
				color = Color.Lerp(color1, color2, projectile.localAI[0] / 60f);
			}
			else {
				color = Color.Lerp(color2, color1, (projectile.localAI[0] - 60f) / 30f);
			}
			color *= 0.85f;
			Vector2 drawPos = projectile.position - Main.screenPosition;
			spriteBatch.Draw(Main.projectileTexture[projectile.type], drawPos, null, color, 0f, Vector2.Zero, projectile.Size, SpriteEffects.None, 0f);
			return false;
		}
	}
}