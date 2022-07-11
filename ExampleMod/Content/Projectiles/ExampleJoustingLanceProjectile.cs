using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{
	public class ExampleJoustingLanceProjectile : ModProjectile
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Example Jousting Lance"); // The English name of the projectile
			ProjectileID.Sets.DismountsPlayersOnHit[Type] = true;
		}

		public override void SetDefaults() {
			Projectile.netImportant = true; // Sync this projectile if a player joins mid game.

			// The width and height do not affect the collision of the Jousting Lance because we calculate that separately (see Colliding() below)
			Projectile.width = 25;
			Projectile.height = 25;

			// aiStyle 19 is the AI for Spears. Jousting Lances use the Spear AI. If you set the aiStyle to 19, make sure to set the AIType so it actually behaves like a Jousting Lance.
			// Since we are using custom AI below, we set the aiStyle to -1.
			Projectile.aiStyle = -1;

			Projectile.alpha = 255; // The transparency of the projectile, 255 for completely transparent. Our projectile will fades in (see the AI() below).
			Projectile.friendly = true; // Player shot projectile. Does damage to enemies but not to friendly Town NPCs.
			Projectile.penetrate = -1; // Infinite penetration. The projectile can hit an infinite number of enemies.
			Projectile.tileCollide = false; // Don't kill the projectile if it hits a tile.
			Projectile.scale = 1f; // The scale of the projectile. This only effects the drawing and the width of the collision.
			Projectile.hide = true; // We are drawing the projectile ourself. See PreDraw() below.
			Projectile.ownerHitCheck = true; // Make sure the owner of the projectile has line of sight to the target (aka can't hit things through tile).
			Projectile.DamageType = DamageClass.MeleeNoSpeed; // Set the damage to melee damage.

			// Act like the normal Jousting Lance. Use this if you set the aiStyle to 19.
			// AIType = ProjectileID.JoustingLance; 
		}

		// This is the behavior of the Jousting Lances.
		public override void AI() {
			Player owner = Main.player[Projectile.owner]; // Get the owner of the projectile.
			Vector2 center = owner.RotatedRelativePoint(owner.MountedCenter); // Get the center of the owner.
			Projectile.direction = owner.direction; // Set the direction of the projectile to the same direction as the owner.
			owner.heldProj = Projectile.whoAmI; // Set the owner's held projectile to this projectile.
			owner.itemTime = owner.itemAnimation; // Set the time left on this item to the animation time.
			Projectile.Center = center; // Set the center of the projectile to the center of the owner. Projectile.Center is now actually the tip of the Jousting Lance.

			int itemAnimationMax = owner.itemAnimationMax;
			int itemAnimation = owner.itemAnimation;

			// Changing this denominator will affect how far the projectile can move when it is shot (based on the scaleFactor).
			// It is recommend to keep it between 2 and 12. 13 and above will kill the projectile. 1 will make it not move forward at all. Vanilla uses 3.
			int itemAnimationMaxDiv = itemAnimationMax / 3;

			float smallerItemAnimation = MathHelper.Min(itemAnimation, itemAnimationMaxDiv); // Choose the smaller of the two values.
			float minimumItemAnimation = itemAnimation - smallerItemAnimation;
			float spawnDistance = 28f; // This changes how far out the projectile is spawned. Vanilla uses 28f
			float flyOut = 0.4f; // This changes how far out the projectile flies when it spawns. Vanilla uses 0.4f
			float flyBack = 0.4f; // This changes how far back the projectile flies when it is killed. Vanilla uses 0.4f

			// Fade the projectile in when it first spawns
			Projectile.alpha -= 40;
			if (Projectile.alpha < 0) {
				Projectile.alpha = 0;
			}

			float pushOutMulti = itemAnimationMax - itemAnimationMaxDiv - minimumItemAnimation; // This value will get larger the further out the projectile is from the player until it reaches it's maximum distance.
			float pullInMulti = itemAnimationMaxDiv - smallerItemAnimation; // This value will get larger when the projectile is being pulled back in.
			float scaleFactor = spawnDistance + flyOut * pushOutMulti - flyBack * pullInMulti;
			Projectile.position += Projectile.velocity * scaleFactor; // Move the projectile based on the velocity and scaleFactor

			if (owner.channel && owner.itemAnimation < itemAnimationMaxDiv) {
				owner.SetDummyItemTime(itemAnimationMaxDiv); // This makes it so the projectile never dies while we are holding it (except when we take damage, see ExampleJoustingLancePlayer).
			}

			// If the Jousting Lance is no longer being used, kill the projectile.
			if (owner.ItemAnimationEndingOrEnded) {
				Projectile.Kill();
			}

			// Set the rotation of the projectile.
			// For reference, 0 is the top left, 180 degrees or pi radians is the bottom right.
			Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + (float)Math.PI / 2f + (float)Math.PI / 4f;

			// The Hallowed and Shadow Jousting Lance spawn dusts when the player is moving at a certain speed.
			float minimumPlayerVelocity = 6f;
			float minimumSpeedX = 0.8f;
			float speedX = Vector2.Dot(Projectile.velocity.SafeNormalize(Vector2.UnitX * owner.direction), owner.velocity.SafeNormalize(Vector2.UnitX * owner.direction));
			float playerVelocity = owner.velocity.Length();
			if (playerVelocity > minimumPlayerVelocity && speedX > minimumSpeedX) {

				// The chance for the dust to spawn. The actual chance (see below) is 1/dustChance. We make the chance higher the faster the player is moving by making the denominator smaller.
				int dustChance = 8;
				if (playerVelocity > minimumPlayerVelocity + 1f) {
					dustChance = 5;
				}
				if (playerVelocity > minimumPlayerVelocity + 2f) {
					dustChance = 2;
				}

				// Set your dust types here.
				int dustTypeCommon = ModContent.DustType<Dusts.Sparkle>();
				int dustTypeRare = DustID.WhiteTorch;

				int offset = 4; // This offset will affect how much the dust spreads out.

				// Spawn the dusts based on the dustChance. The dusts are spawned at the tip of the Jousting Lance.
				if (Main.rand.NextBool(dustChance)) {
					int newDust = Dust.NewDust(Projectile.Center - new Vector2(offset, offset), offset * 2, offset * 2, dustTypeCommon, Projectile.velocity.X * 0.2f + (Projectile.direction * 3), Projectile.velocity.Y * 0.2f, 100, default, 1.2f);
					Main.dust[newDust].noGravity = true;
					Main.dust[newDust].velocity *= 0.25f;
					newDust = Dust.NewDust(Projectile.Center - new Vector2(offset, offset), offset * 2, offset * 2, dustTypeCommon, 0f, 0f, 150, default, 1.4f);
					Main.dust[newDust].velocity *= 0.25f;
				}

				if (Main.rand.NextBool(dustChance + 3)) {
					Dust.NewDust(Projectile.Center - new Vector2(offset, offset), offset * 2, offset * 2, dustTypeRare, 0f, 0f, 150, default, 1.4f);
				}
			}
		}

		// This will increase or decrease the knockback of the Jousting Lance depending on how fast the player is moving.
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
			if (damage > 0) {
				knockback *= Main.player[Projectile.owner].velocity.Length() / 7f;
			}
		}

		// This will increase or decrease the damage of the Jousting Lance depending on how fast the player is moving.
		public override void ModifyDamageScaling(ref float damageScale) {
			damageScale *= 0.1f + Main.player[Projectile.owner].velocity.Length() / 7f * 0.9f;
		}

		// This is the custom collision that Jousting Lances uses. 
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			float rotationFactor = Projectile.rotation + (float)Math.PI / 4f; // The rotation of the Jousting Lance.
			float scaleFactor = 95f; // How far back the hit-line will be from the tip of the Jousting Lance. You may need to modify this if you have a bigger or smaller Jousting Lance. Vanilla uses 95f
			float widthMultiplier = 23f; // How thick the hit-line is. Increase or decrease this value if your Jousting Lance is thicker or thinner. Vanilla uses 23f
			float collisionPoint = 0f; // collisionPoint is needed for CheckAABBvLineCollision(), but it isn't used for our collision here. Keep it at 0f.

			// This Rectangle is the width and height of the Jousting Lance's hitbox which is used for the first step of collision.
			// You may need to modify the last two numbers if you have a bigger or smaller Jousting Lance. Vanilla uses (0, 0, 300, 300).
			Rectangle lanceHitboxBounds = new Rectangle(0, 0, 300, 300);

			// Set the position of the large rectangle.
			lanceHitboxBounds.X = (int)Projectile.position.X - lanceHitboxBounds.Width / 2;
			lanceHitboxBounds.Y = (int)Projectile.position.Y - lanceHitboxBounds.Height / 2;

			// First check that our large rectangle intersects with the target hitbox. Then we check to see if a line from the tip of the Jousting Lance to the "end" of the lance intersects with the target hitbox.
			// Projectile.Center is the tip of the Jousting Lance.
			if (lanceHitboxBounds.Intersects(targetHitbox) && Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + rotationFactor.ToRotationVector2() * scaleFactor, widthMultiplier * Projectile.scale, ref collisionPoint)) {
				return true;
			}
			return false;
		}

		// We need to draw the projectile manually. If you don't include this, the Jousting Lance will not be aligned with the player.
		public override bool PreDraw(ref Color lightColor) {

			// SpriteEffects change which direction the sprite is drawn.
			SpriteEffects spriteEffects = SpriteEffects.None;

			// Get texture of projectile
			Texture2D texture = TextureAssets.Projectile[Type].Value;

			// Get the currently selected frame on the texture.
			Rectangle sourceRectangle = texture.Frame(1, Main.projFrames[Type], frameY: Projectile.frame);

			// The origin in this case is (0, 0) of our projectile because Projectile.Center is the tip of our Jousting Lance.
			Vector2 origin = Vector2.Zero;

			// The rotation of the projectile.
			float rotation = Projectile.rotation;

			// If the projectile is facing right, we need to rotate it by -90 degrees, move the origin, and flip the sprite horizontally.
			// This will make it so the bottom of the sprite is correctly facing down when shot to the right.
			if (Projectile.direction > 0) {
				rotation -= (float)Math.PI / 2f;
				origin.X += sourceRectangle.Width;
				spriteEffects = SpriteEffects.FlipHorizontally;
			}

			// The position of the sprite. Not subtracting Main.player[Projectile.owner].gfxOffY will cause the sprite to bounce when walking up blocks.
			Vector2 position = new(Projectile.Center.X, Projectile.Center.Y - Main.player[Projectile.owner].gfxOffY);

			// Apply lighting and draw our projectile
			Color drawColor = Projectile.GetAlpha(lightColor);

			Main.EntitySpriteDraw(texture,
				position - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
				sourceRectangle, drawColor, rotation, origin, Projectile.scale, spriteEffects, 0);

			// It's important to return false, otherwise we also draw the original texture.
			return false;
		}
	}
}