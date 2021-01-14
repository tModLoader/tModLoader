using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{
	public class MinionBossEye : ModProjectile
	{
		public bool FadedIn {
			get => projectile.localAI[0] == 1f;
			set => projectile.localAI[0] = value ? 1f : 0f;
		}

		public bool PlayedSpawnSound {
			get => projectile.localAI[1] == 1f;
			set => projectile.localAI[1] = value ? 1f : 0f;
		}

		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 2;
		}

		public override void SetDefaults() {
			projectile.width = 20;
			projectile.height = 20;
			projectile.alpha = 255;
			projectile.timeLeft = 90;
			projectile.penetrate = -1;
			projectile.friendly = false;
			projectile.hostile = true;
			projectile.tileCollide = false;
			projectile.ignoreWater = true;
			projectile.netImportant = true;
			projectile.aiStyle = -1;
		}

		public override Color? GetAlpha(Color lightColor) {
			return Color.White * (1f - projectile.alpha / 255f);
		}

		private void FadeInAndOut() {
			//Fade in (we have projectile.alpha = 255 in SetDefaults which means it spawns transparent)
			int fadeSpeed = 10;
			if (!FadedIn && projectile.alpha > 0) {
				projectile.alpha -= fadeSpeed;
				if (projectile.alpha < 0) {
					FadedIn = true;
					projectile.alpha = 0;
				}
			}
			else if (FadedIn && projectile.timeLeft < 255f / fadeSpeed) {
				//Fade out so it aligns with the projectile despawning
				projectile.alpha += fadeSpeed;
				if (projectile.alpha > 255) {
					projectile.alpha = 255;
				}
			}
		}

		public override void AI() {
			FadeInAndOut();

			if (!PlayedSpawnSound) {
				PlayedSpawnSound = true;

				//Common practice regarding spawn sounds for projectiles is to put them into AI, playing sounds in the same place where they are spawned
				//is not multiplayer compatible (either no one will hear it, or only you and not others)
				SoundEngine.PlaySound(SoundID.Item8, projectile.position);
			}

			//Accelerate
			projectile.velocity *= 1.01f;

			projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
		}
	}
}
