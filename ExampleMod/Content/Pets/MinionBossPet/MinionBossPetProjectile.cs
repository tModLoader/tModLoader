using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Pets.MinionBossPet
{
	// You can find a simple pet example in ExampleMod\Content\Pets\ExamplePet
	// This pet uses custom AI and drawing to make it more special (It's a Master Mode boss pet after all)
	// It behaves similarly to the Creeper Egg or Suspicious Grinning Eye pets, but takes some visual properties from ExampleMod's Minion Boss
	public class MinionBossPetProjectile : ModProjectile
	{
		// This is a ref property, lets us write Projectile.ai[0] as whatever name we want
		public ref float AlphaForVisuals => ref Projectile.ai[0];

		// This projectile uses an additional texture for drawing
		public static Asset<Texture2D> EyeAsset;

		public override void Load() {
			// load/cache the additional texture
			EyeAsset = ModContent.Request<Texture2D>(Texture + "_Eye");
		}

		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 6;
			Main.projPet[Type] = true;

			// Basics of CharacterPreviewAnimations explained in ExamplePetProjectile
			// Notice we define our own method to use in .WithCode() below. This technically allows us to animate the projectile manually using frameCounter and frame as well
			ProjectileID.Sets.CharacterPreviewAnimations[Type] = ProjectileID.Sets.SimpleLoop(0, Main.projFrames[Type], 5)
				.WithOffset(-2, -22f)
				.WithCode(CharacterPreviewCustomization);
		}

		public static void CharacterPreviewCustomization(Projectile proj, bool walking) {
			// Modified floating from DelegateMethods.CharacterPreview.Float, this is technically not representative of how the pet actually looks and moves ingame, but the Suspicious Grinning Eye has that too

			// If you don't need to modify it, just call DelegateMethods.CharacterPreview.Float(proj, walking) directly here instead and change properties of your pet after it.
			// You do not need this otherwise and can use the preset directly as showcased in ExamplePetProjectile
			float half = 0.5f;
			float timer = (float)Main.timeForVisualEffects % 60f / 60f;
			float speed = 1f; // This is normally 2
			proj.position.Y += 0f - half + (float)(Math.Cos(timer * MathHelper.TwoPi * speed) * half * 2f);

			// We are only using this method for one specific projectile, so it's fine to cast the ModProjectile directly like this
			MinionBossPetProjectile minion = (MinionBossPetProjectile)proj.ModProjectile;

			// Need to set the alpha to 1f to hide the eyes that would normally draw and show the actual pet
			minion.AlphaForVisuals = 1f;

			// You can use Projectile.isAPreviewDummy in the draw code instead, it depends if you prefer changing the conditions leading up to the drawing, or the drawing itself
		}

		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.EyeOfCthulhuPet); // Copy the stats of the Suspicious Grinning Eye projectile

			Projectile.aiStyle = -1; // Use custom AI
		}

		public override Color? GetAlpha(Color lightColor) {
			return Color.White * AlphaForVisuals * Projectile.Opacity;
		}

		public override void PostDraw(Color lightColor) {
			// Draw surrounding eyes to mimic the boss
			Texture2D eyeTexture = EyeAsset.Value;

			Vector2 offset = new Vector2(0, Projectile.gfxOffY); // Vertical offset when the projectile is changing elevation on tiles (does not apply to this particular projectile because it is always airborne)
			Vector2 orbitingCenter = Projectile.Center + offset;

			// Don't need to draw the eyes if the pet is fully faded in
			if (AlphaForVisuals >= 1) {
				return;
			}

			int eyeCount = 10;
			for (int i = 0; i < eyeCount; i++) {
				Vector2 origin = Vector2.Zero; // Using origin as zero because the draw position is the center
				Vector2 rotatedPos = (Vector2.UnitY * 24).RotatedBy(i / (float)eyeCount * MathHelper.TwoPi); // Create a vector of length 24 with a specific rotation based on loop index
				Vector2 drawPos = orbitingCenter - Main.screenPosition + origin + rotatedPos; // Always important to substract Main.screenPosition to translate it into screen coordinates
				Color color = Color.White * (1f - AlphaForVisuals) * Projectile.Opacity; // Draw it in reversed alpha to the projectile

				// Use this instead of Main.spriteBatch.Draw so that dyes apply to it
				Main.EntitySpriteDraw(eyeTexture, drawPos, eyeTexture.Bounds, color, 0f, origin, 1f, SpriteEffects.None, 0);
			}
		}

		public override void AI() {
			Player player = Main.player[Projectile.owner];

			// For organization, the AI is split into several methods defined below
			// They are NOT part of the ModProjectile class!
			CheckActive(player);

			bool movesFast = Movement(player);

			Animate(movesFast);

			AlphaForVisuals = GetAlphaForVisuals(player);
		}

		private void CheckActive(Player player) {
			// Keep the projectile from disappearing as long as the player isn't dead and has the pet buff
			if (!player.dead && player.HasBuff(ModContent.BuffType<MinionBossPetBuff>())) {
				Projectile.timeLeft = 2;
			}
		}

		private bool Movement(Player player) {
			// Handles movement, returns true if moving fast (used for animation)
			float velDistanceChange = 2f;

			// Calculates the desired resting position, as well as some vectors used in velocity/rotation calculations
			int dir = player.direction;
			Projectile.direction = Projectile.spriteDirection = dir;

			Vector2 desiredCenterRelative = new Vector2(dir * 30, -30f);

			// Add some sine motion
			desiredCenterRelative.Y += (float)Math.Sin(Main.GameUpdateCount / 120f * MathHelper.TwoPi) * 5;

			Vector2 desiredCenter = player.MountedCenter + desiredCenterRelative;
			Vector2 betweenDirection = desiredCenter - Projectile.Center;
			float betweenSQ = betweenDirection.LengthSquared(); // It is recommended to operate on squares of distances, to save computing time on square-rooting

			if (betweenSQ > 1000f * 1000f || betweenSQ < velDistanceChange * velDistanceChange) {
				// Set position directly if too far away from the player, or when near the desired location
				Projectile.Center = desiredCenter;
				Projectile.velocity = Vector2.Zero;
			}

			if (betweenDirection != Vector2.Zero) {
				Projectile.velocity = betweenDirection * 0.1f * 2;
			}

			bool movesFast = Projectile.velocity.LengthSquared() > 6f * 6f;

			if (movesFast) {
				// If moving very fast, rotate the projectile towards it smoothly
				float rotationVel = Projectile.velocity.X * 0.08f + Projectile.velocity.Y * Projectile.spriteDirection * 0.02f;
				if (Math.Abs(Projectile.rotation - rotationVel) >= MathHelper.Pi) {
					if (rotationVel < Projectile.rotation) {
						Projectile.rotation -= MathHelper.TwoPi;
					}
					else {
						Projectile.rotation += MathHelper.TwoPi;
					}
				}

				float rotationInertia = 12f;
				Projectile.rotation = (Projectile.rotation * (rotationInertia - 1f) + rotationVel) / rotationInertia;
			}
			else {
				// If moving at regular speeds, rotate the projectile towards its default rotation (0) smoothly if necessary
				if (Projectile.rotation > MathHelper.Pi) {
					Projectile.rotation -= MathHelper.TwoPi;
				}

				if (Projectile.rotation > -0.005f && Projectile.rotation < 0.005f) {
					Projectile.rotation = 0f;
				}
				else {
					Projectile.rotation *= 0.96f;
				}
			}

			return movesFast;
		}

		private void Animate(bool movesFast) {
			int animationSpeed = 7;

			if (movesFast) {
				// Increase animation speed if projectile moves fast (less is faster)
				animationSpeed = 4;
			}

			// Animate all frames from top to bottom, going back to the first
			Projectile.frameCounter++;
			if (Projectile.frameCounter > animationSpeed) {
				Projectile.frameCounter = 0;
				Projectile.frame++;

				if (Projectile.frame >= Main.projFrames[Type]) {
					Projectile.frame = 0;
				}
			}
		}

		private static float GetAlphaForVisuals(Player player) {
			// 0f on full life, 1f for below half life
			float lifeRatio = player.statLife / (float)player.statLifeMax2;
			return Utils.Clamp(2 * (1f - lifeRatio), 0f, 1f);
		}
	}
}
