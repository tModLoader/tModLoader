using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Projectiles
{
	public class ExampleSolarEruptionExplosion : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Example Solar Eruption Explosion");
			Main.projFrames[projectile.type] = 5;
		}

		public override void SetDefaults()
		{
			projectile.width = 52;
			projectile.height = 52;
			projectile.friendly = true;
			projectile.alpha = 255;
			projectile.ignoreWater = true;
			projectile.timeLeft = 60;
			projectile.tileCollide = false;
			projectile.penetrate = -1;
			projectile.usesLocalNPCImmunity = true;
		}

		public override void AI()
		{
			Player player = Main.player[projectile.owner];
			// Add light in place of the explosion. Color.White can be any color you want, and 1f at the end is a radius multiplier.
			//Lighting.AddLight(projectile.Center, Color.White.ToVector3() * 1f);
			
			// Make the explosion larger overtime. By the time of death (60 ticks) it grows by 0.6f.
			projectile.ai[1] += 0.01f;
			projectile.scale = projectile.ai[1];

			// When it spawns, play an explosion sound and 
			if (projectile.ai[0] == 0)
            {
				Main.PlaySound(SoundID.Item14, projectile.Center);
			}

			projectile.ai[0]++;
			// The larger amount of frames the explosion has, the longer it takes to die (still up to 60 ticks.)
			if (projectile.ai[0] >= 3 * Main.projFrames[projectile.type])
			{
				projectile.Kill();
				return;
			}

			// Animates the explosion.
			if (++projectile.frameCounter >= 3)
			{
				projectile.frameCounter = 0;
				if (++projectile.frame >= Main.projFrames[projectile.type])
				{
					projectile.hide = true;
				}
			}

			// Fades in the explosion.
			projectile.alpha -= 63;
			if (projectile.alpha < 0)
			{
				projectile.alpha = 0;
			}

			projectile.Damage();

			// Basic explosion dust
			int dusts = 5;
			for (int i = 0; i < dusts; i++)
			{
				if (Main.rand.NextBool(3))
				{
					float speed = 6f;
					Dust dust1 = Dust.NewDustPerfect(projectile.Center, DustID.Smoke, new Vector2(0f, -speed * Main.rand.NextFloat(0.5f, 1.2f)).RotatedBy(MathHelper.ToRadians(360f / i * dusts + Main.rand.NextFloat(-50f, 50f))), 150, Color.White, 1.5f);
					dust1.noGravity = true;
					dust1.shader = GameShaders.Armor.GetSecondaryShader(player.cPet, player);
				}
			}
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			int cooldown = 4;
			projectile.localNPCImmunity[target.whoAmI] = 6;
			target.immune[projectile.owner] = cooldown;
		}

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
			// Redraw the projectile with its origin on the center of the hitbox, to compensate for hitbox inflation for accurate explosions.
			Texture2D texture = Main.projectileTexture[projectile.type];
			Rectangle rectangle = texture.Frame(1, Main.projFrames[projectile.type], 0, projectile.frame);
			Color color = projectile.GetAlpha(lightColor);

			if (!projectile.hide)
            {
				spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, rectangle, color, projectile.rotation, rectangle.Size() * 0.5f, projectile.scale, SpriteEffects.None, 0f);
            }
			return false;
        }
    }
}