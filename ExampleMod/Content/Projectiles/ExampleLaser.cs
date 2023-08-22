using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Enums;
using Microsoft.CodeAnalysis;
using System;
using Microsoft.Xna.Framework.Graphics;

namespace ExampleMod.Content.Projectiles
{
	public class ExampleLaser : ModProjectile
	{
		// Constants for readability.
		// The length of time it takes to charge up the laser.
		public const float MaxCharge = 50f;

		// The distance away from the player that the laser is held at.
		public const float HoldOffset = 60f;

		// The max distance the laser will travel.
		public const float MaxDistance = 2200f;

		// Wrap ai[0] around the LaserDistance property, allowing for modifying it by using LaserDistance instead.
		// This makes using it much more readable.
		public ref float LaserDistance => ref Projectile.ai[0];

		// Do the same with the LaserCharge property, but for localAI[0]
		public ref float LaserCharge => ref Projectile.localAI[0];

		// Helper property to easily get the projectile's owner.
		public Player Owner => Main.player[Projectile.owner];

		// Helper property to easily get the current end position of the laser.
		public Vector2 LaserEndPosition => Projectile.Center + Projectile.velocity * (LaserDistance - HoldOffset);

		// Helper property to check if the laser is fully charged and should fire.
		public bool IsAtMaxCharge => LaserCharge >= MaxCharge;

		public override void SetDefaults() {
			Projectile.width = Projectile.height = 22;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.DamageType = DamageClass.Magic;
			// As this is being set as the player's held projectile, this is required to ensure it draws properly.
			Projectile.hide = true;
			Projectile.timeLeft = 2;
		}

		public override void AI() {
			// Set the center of the projectile to be offset from the player.
			Projectile.Center = Owner.Center + Projectile.velocity * HoldOffset;

			// Die if the player can no longer fire the laser.
			bool doesNothaveEnoughMana = Main.time % 10 < 1 && !Owner.CheckMana(Owner.inventory[Owner.selectedItem].mana, true);
			if (doesNothaveEnoughMana || !Owner.channel || Owner.dead || Owner.CCed || !Owner.active) {
				Projectile.Kill();
				return;
			}

			// Ensure that the laser does not die by running out of time.
			Projectile.timeLeft = 2;

			// Update various things about the player, based on the laser.
			UpdatePlayer();
			// Handle charging the laser, and spawning charging dust.
			ChargeLaser();

			// Only continue if the laser if fully charged.
			if (LaserCharge < MaxCharge) {
				return;
			}

			SetLaserPosition();
			CastLight();
			SpawnImpactDust();
		}

		public override void CutTiles() {
			// Cut thing like grass along the laser's length.
			DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
			Utils.PlotTileLine(Projectile.Center, LaserEndPosition, Projectile.height, DelegateMethods.CutTiles);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			// Only hit things if at max charge, and the laser is fired.
			if (!IsAtMaxCharge) {
				return false;
			}

			float point = 0f;
			// Return an AABB vs line check. It will look for collisions with the target along the given line.
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Owner.Center, LaserEndPosition, Projectile.width, ref point);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			// Set a custom immune time of 5 frames upon hitting something.
			target.immune[Owner.whoAmI] = 5;
		}

		private void UpdatePlayer() {
			// Only change the projectiles variables if the current player is the owner of the projectile.
			if (Owner.whoAmI == Main.myPlayer) {
				// Get the unit vector direction to the players mouse, and set the projectile's velocity to it.
				Vector2 aimDirection = Owner.Center.DirectionTo(Main.MouseWorld);
				Projectile.velocity = aimDirection;
				Projectile.direction = Main.MouseWorld.X > Owner.Center.X ? 1 : -1;
			}

			// Make the player's direction match the projectile's.
			Owner.ChangeDir(Projectile.direction);
			// Set this projectile as the player's held projectile.
			Owner.heldProj = Projectile.whoAmI;
			// Set the item time and animation length to 2 frames while the projectile is in use.
			Owner.itemTime = Owner.itemAnimation = 2;
			// Update the item rotation to the aim direction, accounting for flipping the rotation if facing to the left.
			Owner.itemRotation = Projectile.velocity.ToRotation() + (Projectile.direction < 0 ? MathHelper.Pi : 0f);
		}

		private void ChargeLaser() {
			// Increment the charge amount.
			if (LaserCharge < MaxCharge) {
				LaserCharge++;
			}

			// Get the position to spawn the charging dust at.
			Vector2 offset = Projectile.velocity * (HoldOffset - 20f);
			Vector2 baseDustPosition = Owner.Center + offset - Vector2.One * 10f;

			// Get the amount of dust to spawn per frame, this increases with charge.
			int amountOfDust = (int)(LaserCharge / 30f);

			for (int i = 0; i < amountOfDust; i++) {
				// Offset the spawn position of the dust randomly, getting closer to the original point over the charge length.
				Vector2 actualDustPosition = baseDustPosition + Main.rand.NextVector2Unit() * (12f - amountOfDust * 2f);
				// Get the velocity for the dust based on the offset position, getting slower over the charge length.
				Vector2 dustVelocity = baseDustPosition.DirectionTo(actualDustPosition) * 1.5f * (10f - amountOfDust * 2f) / 10f;
				float dustScale = Main.rand.NextFloat(0.5f, 1f);

				Dust dust = Dust.NewDustDirect(actualDustPosition, 20, 20, DustID.Electric, dustVelocity.X, dustVelocity.Y, Scale: dustScale);
				// Disable gravity for the dust.
				dust.noGravity = true;
			}
		}

		private void SetLaserPosition() {
			float chargeIncrement = 5f;
			// Loop along the laser's distance to determine the maximum distance it can hit.
			for (LaserDistance = HoldOffset; LaserDistance <= MaxDistance; LaserDistance += chargeIncrement) {
				Vector2 checkPosition = Owner.Center + Projectile.velocity * LaserDistance;

				// If the current checkPosition is blocked via collision, decrease the distance to the last good value and break out of the loop.
				if (!Collision.CanHit(Owner.Center, Projectile.width, Projectile.height, checkPosition, Projectile.width, Projectile.height)) {
					LaserDistance -= chargeIncrement;
					break;
				}
			}
		}

		private void CastLight() {
			// Cast light along the laser. The values in the Vector3 are the rgb values of the desired color, divided by 255.
			DelegateMethods.v3_1 = new Vector3(0.8f, 0.8f, 1f);
			Utils.PlotTileLine(Projectile.Center, LaserEndPosition, Projectile.width, DelegateMethods.CastLight);
		}

		private void SpawnImpactDust() {
			Vector2 dustDirection = Projectile.velocity * -1;
			Vector2 baseDustPosition = LaserEndPosition;

			for (int i = 0; i < 1; i++) {
				// Add a bit of randomness to the velocity of the dust, this causes it to roughly form a cone shape.
				Vector2 dustVelocity = dustDirection.RotatedBy(Main.rand.NextFloat(-MathHelper.PiOver2, MathHelper.PiOver2));
				Dust dust = Dust.NewDustDirect(baseDustPosition, 0, 0, DustID.Electric, dustVelocity.X, dustVelocity.Y, Scale: 1.2f);
				dust.noGravity = true;

				// Spawn some smoke dust at the laser's start, moving down the laser.
				dust = Dust.NewDustDirect(Owner.Center, 0, 0, DustID.Smoke, Projectile.velocity.X * HoldOffset, Projectile.velocity.Y * HoldOffset, newColor: Color.Cyan, Scale: 0.88f);
				dust.noGravity = true;
			}

			// Have a 1/5 chance to spawn additional dust.
			if (Main.rand.NextBool(5)) {
				Vector2 dustOffset = Projectile.velocity.RotatedBy(1.57f) * (Main.rand.NextFloat() - 0.5f) * Projectile.width;
				Dust dust = Dust.NewDustDirect(baseDustPosition + dustOffset - Vector2.One * 4f, 8, 8, DustID.Smoke, Alpha: 100, Scale: 1.5f);
				// Dust creates its own velocity, slow that down.
				dust.velocity *= 0.5f;
				// Math.Abs() always returns positive. By making its return value negative, the dust will always move upwards.
				dust.velocity.Y = -Math.Abs(dust.velocity.Y);

				// Create additional dust.
				Vector2 directionToDust = Owner.Center.DirectionTo(baseDustPosition);
				dust = Dust.NewDustDirect(Owner.Center + directionToDust * 55f, 8, 8, DustID.Smoke, Alpha: 100, Scale: 1.5f);
				dust.velocity *= 0.5f;
				dust.velocity.Y = -Math.Abs(dust.velocity.Y);
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			if (!IsAtMaxCharge)
				return false;

			// Get the head and tail texture.
			Texture2D laserTexture = ModContent.Request<Texture2D>(Texture).Value;
			// Get the body texture. This is seperated for tiling reasons.
			Texture2D bodyTexture = ModContent.Request<Texture2D>(Texture + "Body").Value;

			// The frames for the head and tail.
			Rectangle tailFrame = new(0, 0, 26, 22);
			Rectangle headFrame = new(0, 24, 26, 22);

			// Get the draw rotation. As the texture has them facing downwards, add an extra rotation of -90 degrees.
			float rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

			// Get the start and end positions for the laser body, along with the distance between them.
			Vector2 bodyStart = Owner.Center + Projectile.velocity * 40f - Main.screenPosition;
			Vector2 bodyEnd = LaserEndPosition - Main.screenPosition;
			float bodyDistance = Vector2.Distance(bodyStart, bodyEnd);

			// Get the draw origin for the body. The scaling will use this the center point.
			// Here it is the top middle of the body texture.
			Vector2 bodyOrigin = new(bodyTexture.Width / 2f, 0f);

			// One way to draw the body of the laser would be to loop through the distance at even segments and draw the body texture at each one. However, that would result in a significant
			// amount of draw calls. A more efficient way is to restart the spritebatch, changing the sampler state to "PointWrap". This causes the texture to repeat itself instead of stretching
			// along the source rectangle if it is larger that the texture dimensions. This allows for the entire body to be drawn in a single draw call.

			// Stretch the sourceRectangle to the distance the body needs to draw over. As the texture is facing downwards, the distance affects the height.
			Rectangle sourceRectangle = new(0, 0, Projectile.width, (int)bodyDistance);

			// Restart the spritebatch, changing the samplerState to "PointWrap".
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

			// Draw the body of the laser, using the streched sourceRectangle and the bodyOrigin.
			Main.spriteBatch.Draw(bodyTexture, bodyStart, sourceRectangle, Color.White, rotation, bodyOrigin, Projectile.scale, SpriteEffects.None, 0f);

			// Restart the spritebatch back to what it was before we modified it, to ensure anything that draws after this does so correctly.
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

			// Draw the tail of the laser.
			Main.spriteBatch.Draw(laserTexture, bodyStart, tailFrame, Color.White, rotation, tailFrame.Size() / 2f,
				Projectile.scale, SpriteEffects.None, 0f);

			// Draw the head of the laser.
			Main.spriteBatch.Draw(laserTexture, bodyEnd, headFrame, Color.White, rotation, headFrame.Size() / 2f,
				Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}
	}
}
