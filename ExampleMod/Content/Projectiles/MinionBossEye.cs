using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{
	public class MinionBossEye : ModProjectile
	{
		public bool FadedIn {
			get => Projectile.localAI[0] == 1f;
			set => Projectile.localAI[0] = value ? 1f : 0f;
		}

		public bool PlayedSpawnSound {
			get => Projectile.localAI[1] == 1f;
			set => Projectile.localAI[1] = value ? 1f : 0f;
		}

		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 2;
		}

		public override void SetDefaults() {
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.alpha = 255;
			Projectile.timeLeft = 90;
			Projectile.penetrate = -1;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.netImportant = true;
			Projectile.aiStyle = -1;
			CooldownSlot = ImmunityCooldownID.Bosses; // use the boss immunity cooldown counter, to prevent ignoring boss attacks by taking damage from other sources
		}

		public override Color? GetAlpha(Color lightColor) {
			// When overriding GetAlpha, you usually want to take the projectiles alpha into account. As it is a value between 0 and 255,
			// it's annoying to convert it into a float to multiply. Luckily the Opacity property handles that for us (0f transparent, 1f opaque)
			return Color.White * Projectile.Opacity;
		}

		private void FadeInAndOut() {
			// Fade in (we have Projectile.alpha = 255 in SetDefaults which means it spawns transparent)
			int fadeSpeed = 10;
			if (!FadedIn && Projectile.alpha > 0) {
				Projectile.alpha -= fadeSpeed;
				if (Projectile.alpha < 0) {
					FadedIn = true;
					Projectile.alpha = 0;
				}
			}
			else if (FadedIn && Projectile.timeLeft < 255f / fadeSpeed) {
				// Fade out so it aligns with the projectile despawning
				Projectile.alpha += fadeSpeed;
				if (Projectile.alpha > 255) {
					Projectile.alpha = 255;
				}
			}
		}

		public override void AI() {
			FadeInAndOut();

			if (!PlayedSpawnSound) {
				PlayedSpawnSound = true;

				// Common practice regarding spawn sounds for projectiles is to put them into AI, playing sounds in the same place where they are spawned
				// is not multiplayer compatible (either no one will hear it, or only you and not others)
				SoundEngine.PlaySound(SoundID.Item8, Projectile.position);
			}

			// Accelerate
			Projectile.velocity *= 1.01f;

			// If the sprite points upwards, this will make it point towards the move direction (for other sprite orientations, change MathHelper.PiOver2)
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
		}
	}
}
