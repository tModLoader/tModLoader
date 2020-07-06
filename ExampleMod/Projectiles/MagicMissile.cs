using System;
using ExampleMod.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Projectiles
{
	// Code adapted from the vanilla's magic missile.
	public class MagicMissile : ModProjectile
	{
		public override void SetDefaults() {
			projectile.width = 10;
			projectile.height = 10;
			// projectile.aiStyle = 9; // Vanilla magic missile uses this aiStyle, but using it wouldn't let us fine tune the projectile speed or dust
			projectile.friendly = true;
			projectile.light = 0.8f;
			projectile.magic = true;
			drawOriginOffsetY = -6;
		}

		public override Color? GetAlpha(Color lightColor) => new Color(255, 255, 255, 0);

		public override void AI() {
			// This part makes the projectile do a shime sound every 10 ticks as long as it is moving.
			if (projectile.soundDelay == 0 && Math.Abs(projectile.velocity.X) + Math.Abs(projectile.velocity.Y) > 2f) {
				projectile.soundDelay = 10;
				SoundEngine.PlaySound(SoundID.Item9, projectile.position);
			}

			Vector2 dustPosition = projectile.Center + new Vector2(Main.rand.Next(-4, 5), Main.rand.Next(-4, 5));
			Dust dust = Dust.NewDustPerfect(dustPosition, DustType<Sparkle>(), null, 100, Color.Lime, 0.8f);
			dust.velocity *= 0.3f;
			dust.noGravity = true;

			// In Multi Player (MP) This code only runs on the client of the projectile's owner, this is because it relies on mouse position, which isn't the same across all clients.
			if (Main.myPlayer == projectile.owner && projectile.ai[0] == 0f) {

				Player player = Main.player[projectile.owner];
				// If the player channels the weapon, do something. This check only works if item.channel is true for the weapon.
				if (player.channel) {
					float maxDistance = 18f; // This also sets the maximun speed the projectile can reach while following the cursor.
					Vector2 vectorToCursor = Main.MouseWorld - projectile.Center;
					float distanceToCursor = vectorToCursor.Length();

					// Here we can see that the speed of the projectile depends on the distance to the cursor.
					if (distanceToCursor > maxDistance) {
						distanceToCursor = maxDistance / distanceToCursor;
						vectorToCursor *= distanceToCursor;
					}

					int velocityXBy1000 = (int)(vectorToCursor.X * 1000f);
					int oldVelocityXBy1000 = (int)(projectile.velocity.X * 1000f);
					int velocityYBy1000 = (int)(vectorToCursor.Y * 1000f);
					int oldVelocityYBy1000 = (int)(projectile.velocity.Y * 1000f);

					// This code checks if the precious velocity of the projectile is different enough from its new velocity, and if it is, syncs it with the server and the other clients in MP.
					// We previously multiplied the speed by 1000, then casted it to int, this is to reduce its precision and prevent the speed from being synced too much.
					if (velocityXBy1000 != oldVelocityXBy1000 || velocityYBy1000 != oldVelocityYBy1000) {
						projectile.netUpdate = true;
					}

					projectile.velocity = vectorToCursor;

				}
				// If the player stops channeling, do something else.
				else if (projectile.ai[0] == 0f) {

					// This code block is very similar to the previous one, but only runs once after the player stops channeling their weapon.
					projectile.netUpdate = true;

					float maxDistance = 14f; // This also sets the maximun speed the projectile can reach after it stops following the cursor.
					Vector2 vectorToCursor = Main.MouseWorld - projectile.Center;
					float distanceToCursor = vectorToCursor.Length();

					//If the projectile was at the cursor's position, set it to move in the oposite direction from the player.
					if (distanceToCursor == 0f) {
						vectorToCursor = projectile.Center - player.Center;
						distanceToCursor = vectorToCursor.Length();
					}

					distanceToCursor = maxDistance / distanceToCursor;
					vectorToCursor *= distanceToCursor;

					projectile.velocity = vectorToCursor;

					if (projectile.velocity == Vector2.Zero) {
						projectile.Kill();
					}

					projectile.ai[0] = 1f;
				}
			}

			// Set the rotation so the projectile points towards where it's going.
			if (projectile.velocity != Vector2.Zero) {
				projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver4;
			}
		}

		public override void Kill(int timeLeft) {
			// If the projectile dies without hitting an enemy, crate a small explosion that hits all enemies in the area.
			if (projectile.penetrate == 1) {
				// Makes the projectile hit all enemies as it circunvents the penetrate limit.
				projectile.maxPenetrate = -1;
				projectile.penetrate = -1;

				int explosionArea = 60;
				Vector2 oldSize = projectile.Size;
				// Resize the projectile hitbox to be bigger.
				projectile.position = projectile.Center;
				projectile.Size += new Vector2(explosionArea);
				projectile.Center = projectile.position;

				projectile.tileCollide = false;
				projectile.velocity *= 0.01f;
				// Damage enemies inside the hitbox area
				projectile.Damage();
				projectile.scale = 0.01f;

				//Resize the hitbox to its original size
				projectile.position = projectile.Center;
				projectile.Size = new Vector2(10);
				projectile.Center = projectile.position;
			}

			SoundEngine.PlaySound(SoundID.Item10, projectile.position);
			for (int i = 0; i < 10; i++) {
				Dust dust = Dust.NewDustDirect(projectile.position - projectile.velocity, projectile.width, projectile.height, DustType<Sparkle>(), 0, 0, 100, Color.Lime, 0.8f);
				dust.noGravity = true;
				dust.velocity *= 2f;
				dust = Dust.NewDustDirect(projectile.position - projectile.velocity, projectile.width, projectile.height, DustType<Sparkle>(), 0f, 0f, 100, Color.Lime, 0.5f);
			}
		}
	}
}