using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{
	// This file showcases a projectile moving in a sine wave
	// Can be tested with ExampleMagicRod
	public class ExampleSineWaveMotionProjectile : ModProjectile
	{
		// This field is used in this projectie's custom AI
		public Vector2 initialCenter;

		// This field is used as a counter for the wave motion
		public int sineTimer;

		public int waveDirection = 1;

		public Color drawColor = Color.Red;

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Example Sine Wave Motion Projectile"); // Name of the projectile. It can be appear in chat

			Main.projFrames[Type] = 3; // This projectile has 3 frames in its spritesheet
		}

		// Setting the default parameters of the projectile
		// You can check most of Fields and Properties here https://github.com/tModLoader/tModLoader/wiki/Projectile-Class-Documentation
		public override void SetDefaults() {
			Projectile.width = 8; // The width of projectile hitbox
			Projectile.height = 8; // The height of projectile hitbox

			Projectile.aiStyle = 0; // The ai style of the projectile (0 means custom AI). For more please reference the source code of Terraria
			Projectile.DamageType = DamageClass.Magic; // What type of damage does this projectile affect?
			Projectile.friendly = true; // Can the projectile deal damage to enemies?
			Projectile.hostile = false; // Can the projectile deal damage to the player?
			Projectile.ignoreWater = true; // Does the projectile's speed be influenced by water?
			Projectile.light = 1f; // How much light emit around the projectile
			Projectile.tileCollide = true; // Can the projectile collide with tiles?
			Projectile.timeLeft = 600; // The live time for the projectile (60 = 1 second, so 600 is 10 seconds)
		}

		// This projectile updates its position manually
		public override bool ShouldUpdatePosition() {
			return false;
		}

		public override void OnSpawn(IEntitySource source) {
			initialCenter = Projectile.Center;
		}

		public override Color? GetAlpha(Color lightColor) {
			return drawColor * 0.64f;
		}

		public override void AI() {
			// Ensure that the "waveDirection = 1" projectile starts above the "waveDirection = -1" projectile not matter which direction they were fired from
			int trueDirection = Projectile.direction * waveDirection;

			// How many oscillations happen per second
			// Higher value = more oscillations
			float wavesPerSecond = 2;

			// Math.Sin expects a radians angle instead of degrees
			float radians = MathHelper.ToRadians(sineTimer * 6f * wavesPerSecond);
			float sine = (float)Math.Sin(radians) * trueDirection;

			// Using the calculated sine value, generate an offset used to position the projectile on the wave
			// The offset should be perpendicular to the velocity direction, hence the RotatedBy call
			Vector2 offset = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2 * -1);

			// How wide the wave should be, times two
			// An amplitude of 24 pixels is 1.5 tiles, meaning the total wave width is 48 pixels, or 3 tiles
			float waveAmplitude = 24;

			// Get the offset used to adjust the projectile's position
			offset *= sine * waveAmplitude;

			// Update the position manually since ShouldUpdatePosition returns false
			initialCenter += Projectile.velocity;
			Projectile.Center = initialCenter + offset;

			// Update the rotation used to draw the projectile
			// This projectile should act as if it were moving along the sine wave
			float cosine = (float)Math.Cos(radians) * trueDirection;
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4 * cosine * -1;

			// Update the frame used to draw the projectile
			const float sqrtTwo = 0.707106781f;
			float cosineAbsolute = Math.Abs(cosine);
			if (sine > 0) {
				// Offset is counter-clockwise to the direction
				if (cosineAbsolute > sqrtTwo) {
					Projectile.frame = Projectile.direction == 1 ? 0 : 2;
				} else {
					Projectile.frame = 1;
				}
			} else if (sine < 0) {
				// Offset is clockwise to the direction
				if (cosineAbsolute > sqrtTwo) {
					Projectile.frame = Projectile.direction == 1 ? 2 : 0;
				} else {
					Projectile.frame = 1;
				}
			} else {
				// Offset is zero
				Projectile.frame = 1;
			}

			// Spawn dusts
			Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.ExampleSolution>(), Velocity: Vector2.Zero, newColor: drawColor, Scale: 0.5f);

			Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();

			sineTimer++;
		}

		public override bool PreDraw(ref Color lightColor) {
			// In order to make the projectile draw correctly, that will need to be done manually
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Rectangle frame = texture.Frame(1, 3, 0, Projectile.frame);
			Vector2 rotationOrigin;
			float rotation;
			SpriteEffects effects;

			if (Projectile.direction == -1) {
				rotationOrigin = new Vector2(5, 5);
				rotation = Projectile.rotation + MathHelper.Pi;
				effects = SpriteEffects.FlipHorizontally;
			} else {
				rotationOrigin = new Vector2(13, 5);
				rotation = Projectile.rotation;
				effects = SpriteEffects.None;
			}

			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(lightColor), rotation, rotationOrigin, Projectile.scale, effects, 0);

			return false;
		}
	}
}
