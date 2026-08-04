using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Enums;
using Terraria.GameContent.Tile_Entities;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{
	// Shortsword projectiles are handled in a special way with how they draw and damage things
	// The "hitbox" itself is closer to the player, the sprite is centered on it
	// However the interactions with the world will occur offset from this hitbox, closer to the sword's tip (CutTiles, Colliding)
	// Values chosen mostly correspond to Iron Shortsword
	public class ExampleShortswordProjectile : ModProjectile
	{
		public const int FadeInDuration = 7;
		public const int FadeOutDuration = 4;

		public const int TotalDuration = 16;

		// The "width" of the blade
		public float CollisionWidth => 10f * Projectile.scale;

		public int Timer {
			get => (int)Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		public override void SetDefaults() {
			Projectile.Size = new Vector2(18); // This sets width and height to the same value (important when projectiles can rotate)
			Projectile.aiStyle = -1; // Use our own AI to customize how it behaves, if you don't want that, keep this at ProjAIStyleID.ShortSword. You would still need to use the code in SetVisualOffsets() though
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.scale = 1f;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.ownerHitCheck = true; // Prevents hits through tiles. Most melee weapons that use projectiles have this
			Projectile.extraUpdates = 1; // Update 1+extraUpdates times per tick
			Projectile.timeLeft = 360; // This value does not matter since we manually kill it earlier, it just has to be higher than the duration we use in AI
			Projectile.usesOwnerLight = true;
			Projectile.drawLayer = ProjectileDrawLayerID.HeldProj; // Draws over the player's body and under the player's hands
		}

		public override void AI() {
			Player player = Main.player[Projectile.owner];

			Timer += 1;
			if (Timer >= TotalDuration) {
				// Kill the projectile if it reaches it's intended lifetime
				Projectile.Kill();
				return;
			}
			else {
				// Important so that the sprite draws "in" the player's hand and not fully in front or behind the player
				player.heldProj = Projectile.whoAmI;
			}

			// Fade in and out
			// GetLerpValue returns a value between 0f and 1f - if clamped is true - representing how far Timer got along the "distance" defined by the first two parameters
			// The first call handles the fade in, the second one the fade out.
			// Notice the second call's parameters are swapped, this means the result will be reverted
			Projectile.Opacity = Utils.GetLerpValue(0f, FadeInDuration, Timer, clamped: true) * Utils.GetLerpValue(TotalDuration, TotalDuration - FadeOutDuration, Timer, clamped: true);

			// Keep locked onto the player, but extend further based on the given velocity (Requires ShouldUpdatePosition returning false to work)
			Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter, reverseRotation: false);
			Projectile.Center = playerCenter + Projectile.velocity * (Timer - 1f);

			// Set spriteDirection based on moving left or right. Left -1, right 1
			Projectile.spriteDirection = (Vector2.Dot(Projectile.velocity, Vector2.UnitX) >= 0f).ToDirectionInt();

			// Point towards where it is moving, applied offset for top right of the sprite respecting spriteDirection
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2 - MathHelper.PiOver4 * Projectile.spriteDirection;

			// The code in this method is important to align the sprite with the hitbox how we want it to
			SetVisualOffsets();
		}

		private void SetVisualOffsets() {
			// 32 is the sprite size (here both width and height equal)
			const int HalfSpriteWidth = 32 / 2;
			const int HalfSpriteHeight = 32 / 2;

			int HalfProjWidth = Projectile.width / 2;
			int HalfProjHeight = Projectile.height / 2;

			// Vanilla configuration for "hitbox in middle of sprite"
			DrawOriginOffsetX = 0;
			DrawOffsetX = -(HalfSpriteWidth - HalfProjWidth);
			DrawOriginOffsetY = -(HalfSpriteHeight - HalfProjHeight);

			// Vanilla configuration for "hitbox towards the end"
			//if (Projectile.spriteDirection == 1) {
			//	DrawOriginOffsetX = -(HalfProjWidth - HalfSpriteWidth);
			//	DrawOffsetX = (int)-DrawOriginOffsetX * 2;
			//	DrawOriginOffsetY = 0;
			//}
			//else {
			//	DrawOriginOffsetX = (HalfProjWidth - HalfSpriteWidth);
			//	DrawOffsetX = 0;
			//	DrawOriginOffsetY = 0;
			//}
		}

		public override bool ShouldUpdatePosition() {
			// Update Projectile.Center manually
			return false;
		}

		public override void CutTiles() {
			// "cutting tiles" refers to breaking pots, grass, queen bee larva, etc.
			DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
			Vector2 start = Projectile.Center;
			Vector2 end = start + Projectile.velocity.SafeNormalize(-Vector2.UnitY) * 10f;
			Utils.PlotTileLine(start, end, CollisionWidth, DelegateMethods.CutTiles);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			// "Hit anything between the player and the tip of the sword"
			// shootSpeed is 2.1f for reference, so this is basically plotting 12 pixels ahead from the center
			Vector2 start = Projectile.Center;
			Vector2 end = start + Projectile.velocity * 6f;
			float collisionPoint = 0f; // Don't need that variable, but required as parameter
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, CollisionWidth, ref collisionPoint);
		}

		// This hook lets us change how the held projectile looks while a mannequin is holding it.
		// The following code is adapted from vanilla's Projectile.AI_DisplayDoll for aiStyle 161 (Shortsword)
		// Due to our custom AI code, using ProjAIStyleID.Shortsword for this one wouldn't display correctly.
		public override bool DisplayDollSettings(Player doll, TEDisplayDoll.DisplayDollPose pose, ref int aiStyle, ref int aiType) {

			// The code in this method is important to align the sprite with the hitbox how we want it to
			SetVisualOffsets();

			Projectile.spriteDirection = Projectile.direction; // Set the direction of the sprite to the direction the projectile is facing.
			Vector2 projectileDirection = Vector2.UnitX * 12f; // This will move the projectile forward in the mannequin's hand because the hitbox of the projectile is the center of the sprite.
			float armRotation = 0f;
			if (pose.ItemAimRadians.HasValue)
				armRotation = pose.ItemAimRadians.Value; // The rotation of the mannequin's hand.

			projectileDirection = projectileDirection.RotatedBy(armRotation); // Rotate the projectile based on the rotation of the mannequin's hand.
			if (Projectile.direction == -1)
				projectileDirection.X *= -1f;

			Projectile.velocity = projectileDirection;
			Projectile.position += projectileDirection + (projectileDirection * 1.5f); // Move the projectile's location while being held by the mannequin.

			Projectile.rotation = projectileDirection.ToRotation() + MathHelper.PiOver2; // Set the projectile's rotation based on the mannequin's hand.
			Projectile.rotation -= MathHelper.PiOver4 * Projectile.spriteDirection; // Correct the rotation of the shortsword since we set this in the AI.

			return false;
		}
	}
}
