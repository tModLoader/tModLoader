using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{
	// Example Kite is a adaptation of a basic vanilla kite. The code has been cleaned up slightly to make it easier to follow.
	public class ExampleKiteProjectile : ModProjectile
	{
		public override void SetStaticDefaults() {
			// Total count animation frames
			Main.projFrames[Type] = 4;

			ProjectileID.Sets.BreaksFromToyBreaker[Type] = true;
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 60;
		}

		public override void SetDefaults() {
			Projectile.DefaultToKite();
		}

		public override bool PreDraw(Player player, ref Color lightColor) {
			// Draw a string connecting from the players hand to the kite
			DrawString();

			SpriteEffects spriteEffects = SpriteEffects.None;
			if (Projectile.spriteDirection == -1)
				spriteEffects = SpriteEffects.FlipHorizontally;

			Texture2D texture = TextureAssets.Projectile[Type].Value;

			// Our kites frames are horizontal so we divide width instead of height
			int frameWidth = texture.Width / Main.projFrames[Type];
			Rectangle sourceRectangle = new Rectangle(frameWidth * Projectile.frame, 0, frameWidth, texture.Height);

			Vector2 origin = sourceRectangle.Size() / 2f;
			origin.X += -2 * Projectile.spriteDirection;

			Main.EntitySpriteDraw(texture,
				Projectile.Center - Main.screenPosition,
				sourceRectangle, Projectile.GetAlpha(lightColor), Projectile.rotation,
				origin, Projectile.scale, spriteEffects, 0);

			return false;
		}

		private void DrawString() {
			Texture2D texture = TextureAssets.FishingLine.Value;
			Rectangle frame = texture.Frame();
			Vector2 origin = new Vector2(frame.Width / 2, 2f);

			Vector2 armPosition = Main.GetPlayerArmPosition(Projectile, Main.player[Projectile.owner]);
			Vector2 kiteCenter = Projectile.Center;

			// Total string length used to fade the string near the player
			float totalDistance = Vector2.Distance(kiteCenter, armPosition);
			if (totalDistance == 0f)
				totalDistance = 1f;

			// The string sags less when the kite moves fast, swapping axes ensures this works in all directions
			Vector2 velocity = Projectile.velocity;
			if (Math.Abs(velocity.X) > Math.Abs(velocity.Y))
				Utils.Swap(ref velocity.X, ref velocity.Y);

			// Draw the string segment by segment starting from the arm
			Vector2 segmentDirection = kiteCenter - armPosition;
			float totalLength = segmentDirection.Length();
			bool drawing = totalLength != 0f;

			if (drawing) {
				segmentDirection *= 12f / totalLength;
				armPosition -= segmentDirection;
				segmentDirection = kiteCenter - armPosition;
			}

			while (drawing) {
				float segmentLength = segmentDirection.Length();
				float previousLength = segmentLength;

				if (float.IsNaN(segmentLength) || segmentLength == 0f) {
					drawing = false;
					continue;
				}

				// Shrinks the last segment to fit remaining distance
				if (segmentLength < 20f) {
					drawing = false;
					armPosition += segmentDirection.SafeNormalize(Vector2.Zero) * 12f;
					segmentDirection = kiteCenter - armPosition;
					frame.Height = (int)(segmentDirection.Length() + 4f);
				}
				else {
					segmentDirection *= 12f / segmentLength;
					armPosition += segmentDirection;
					segmentDirection = kiteCenter - armPosition;

					// Sag the string based on kite velocity and distance
					if (previousLength > 12f) {
						float sag = 0.3f;
						float speed = Math.Min(Math.Abs(velocity.X) + Math.Abs(velocity.Y), 16f);
						sag *= 1f - speed / 16f;
						sag *= Math.Min(previousLength / 80f, 1f);
						sag = Math.Max(sag, 0f);

						if (segmentDirection.Y > 0f) {
							segmentDirection.Y *= 1f + sag;
							segmentDirection.X *= 1f - sag;
						}
						else {
							// Sag sideways when the string curves upward
							float sideSag = MathHelper.Clamp(Math.Abs(velocity.X) / 3f, 0f, 1f) - 0.5f;
							sag *= sideSag;
							if (sag > 0f)
								sag *= 2f;
							segmentDirection.Y *= 1f + sag;
							segmentDirection.X *= 1f - sag;
						}
					}
				}

				// Fade the string out near the player
				float fromValue = 1f - Vector2.Distance(kiteCenter, armPosition) / totalDistance;
				Color color = Lighting.GetColor(armPosition.ToTileCoordinates());
				color *= 0.42745098f;
				color *= Utils.Remap(fromValue, 0f, 1f, 0.5f, 1f);

				float rotation = segmentDirection.ToRotation() - MathHelper.PiOver2;

				// Draw the string segment
				Main.EntitySpriteDraw(texture, armPosition - Main.screenPosition, frame, color, rotation, origin, new Vector2(0.8f, 1f), SpriteEffects.None);
			}
		}
	}
}