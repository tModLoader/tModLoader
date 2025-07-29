using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{
	public class ExamplePaperAirplaneProjectile : ModProjectile
	{
		public override void SetDefaults() {
			Projectile.width = 10; // The width of the projectile
			Projectile.height = 10; // The height of the projectile

			Projectile.aiStyle = -1; // We are setting the aiStyle to -1 to use the custom AI below. If just want the vanilla behavior, you can set the aiStyle to 159.

			Projectile.friendly = true; // Can the projectile deal damage to enemies?
			Projectile.DamageType = DamageClass.Ranged; // Set the damage type to ranged damage.

			// Setting this to true will stop the projectile from automatically flipping its sprite when changing directions.
			// The vanilla paper airplanes have this set to true.
			// If this is true the projectile won't flip its sprite vertically while doing a loop, but the paper airplane can be upside down if it is shot one direction and then turns around on its own.
			// Set to false if you want the projectile to always be right side up.
			Projectile.manualDirectionChange = true;

			// If you are using Projectile.aiStyle = 159, setting the AIType isn't necessary here because the two types of vanilla paper airplanes aiStyles have the same AI.
			// AIType = ProjectileID.PaperAirplaneA;
		}

		// This is the behavior of the paper airplane.
		// If you just want the same vanilla behavior, you can instead set Projectile.aiStyle = 159 in SetDefaults and remove this AI() section.
		public override void AI() {
			// All projectiles have timers that help to delay certain events
			// Projectile.ai[0], Projectile.ai[1] — timers that are automatically synchronized on the client and server

			// This will run only once as soon as the projectile spawns.
			if (Projectile.ai[1] == 0f) {
				Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt(); // If it is moving right, then set Projectile.direction to 1. If it is moving left, then set Projectile.direction to -1.
				Projectile.rotation = Projectile.velocity.ToRotation(); // Set the rotation based on the velocity.
				Projectile.ai[1] = 1f; // Set Projectile.ai[1] to 1. This is only used to make this section of code run only once.
				Projectile.ai[0] = -Main.rand.Next(30, 80); // Set Projectile.ai[0] to a random number from -30 to -79.
				Projectile.netUpdate = true; // Sync the projectile in a multiplayer game.
			}

			// Kill the projectile if it touches a liquid. (It will automatically get killed by touching a tile. You can change that by returning false in OnTileCollide())
			if (Projectile.wet && Projectile.owner == Main.myPlayer) {
				Projectile.Kill();
			}

			Projectile.ai[0] += 1f; // Increase Projectile.ai[0] by 1 every tick. Remember, there are 60 ticks per second.

			Vector2 rotationVector = Projectile.rotation.ToRotationVector2() * 8f; // Get the rotation of the projectile.

			float ySinModifier = (float)Math.Sin((float)Math.PI * 2f * (float)(Main.timeForVisualEffects % 90.0 / 90.0)) * Projectile.direction * Main.WindForVisuals; // This will make the projectile fly in a sine wave fashion.

			Vector2 newVelocity = rotationVector + new Vector2(Main.WindForVisuals, ySinModifier); // Create a new velocity using the rotation and wind.

			bool directionSameAsWind = Projectile.direction == Math.Sign(Main.WindForVisuals) && Projectile.velocity.Length() > 3f; // True if the projectile is moving the same direction as the wind and is not moving slowly.
			bool readyForFlip = Projectile.ai[0] >= 20f && Projectile.ai[0] <= 69f; // True if Projectile.ai[0] is between 20 and 69

			// Once Projectile.ai[0] reaches 70...
			if (Projectile.ai[0] == 70f) {
				Projectile.ai[0] = -Main.rand.Next(120, 600); // Set it back to a random number from -120 to -599.
			}

			// Do a flip! This will cause the projectile to fly in a loop if directionSameAsWind and readyForFlip are true.
			if (readyForFlip && directionSameAsWind) {
				float lerpValue = Utils.GetLerpValue(0f, 30f, Projectile.ai[0], clamped: true);
				newVelocity = rotationVector.RotatedBy((-Projectile.direction) * ((float)Math.PI * 2f) * 0.02f * lerpValue);
			}

			Projectile.velocity = newVelocity.SafeNormalize(Vector2.UnitY) * Projectile.velocity.Length(); // Set the velocity to the value we calculated above.

			// If it is flying normally. i.e. not flying a loop.
			if (!(readyForFlip && directionSameAsWind)) {
				float yModifier = MathHelper.Lerp(0.15f, 0.05f, Math.Abs(Main.WindForVisuals));

				// Half of time, decrease the y velocity a little.
				if (Projectile.timeLeft % 40 < 20) {
					Projectile.velocity.Y -= yModifier;
				}
				// The other half of time, increase the y velocity a little.
				else {
					Projectile.velocity.Y += yModifier;
				}

				// Cap the y velocity so the projectile falls slowly and doesn't rise too quickly.
				// MathHelper.Clamp() allows you to set a minimum and maximum value. In this case, the result will always be between -2f and 2f (inclusive).
				Projectile.velocity.Y = MathHelper.Clamp(Projectile.velocity.Y, -2f, 2f);

				// Set the x velocity.
				// MathHelper.Clamp() allows you to set a minimum and maximum value. In this case, the result will always be between -6f and 6f (inclusive).
				Projectile.velocity.X = MathHelper.Clamp(Projectile.velocity.X + Main.WindForVisuals * 0.006f, -6f, 6f);

				// Switch direction when the current velocity and the oldVelocity have different signs.
				if (Projectile.velocity.X * Projectile.oldVelocity.X < 0f) {
					Projectile.direction *= -1; // Reverse the direction
					Projectile.ai[0] = -Main.rand.Next(120, 300); // Set Projectile.ai[0] to a random number from -120 to -599.
					Projectile.netUpdate = true; // Sync the projectile in a multiplayer game.
				}
			}

			// Set the rotation and spriteDirection
			Projectile.rotation = Projectile.velocity.ToRotation();
			Projectile.spriteDirection = Projectile.direction;

			// Let's add some dust for special effect. In this case, it runs every other tick (30 ticks per second).
			if (Projectile.timeLeft % 2 == 0) {
				Dust.NewDustPerfect(new Vector2(Projectile.Center.X - (Projectile.width * Projectile.direction), Projectile.Center.Y), ModContent.DustType<Dusts.Sparkle>(), null, 0, default, 0.5f); // Here we spawn the dust at the back of the projectile with half scale.
			}
		}

		// We need to draw the projectile manually. If you don't include this, the projectile will be facing the wrong direction when flying left.
		public override bool PreDraw(ref Color lightColor) {
			// This is where we specify which way to flip the sprite. If the projectile is moving to the left, then flip it vertically.
			SpriteEffects spriteEffects = ((Projectile.spriteDirection <= 0) ? SpriteEffects.FlipVertically : SpriteEffects.None);

			// Getting texture of projectile
			Texture2D texture = TextureAssets.Projectile[Type].Value;

			// Get the currently selected frame on the texture.
			Rectangle sourceRectangle = texture.Frame(1, Main.projFrames[Type], frameY: Projectile.frame);

			Vector2 origin = sourceRectangle.Size() / 2f;

			// Applying lighting and draw our projectile
			Color drawColor = Projectile.GetAlpha(lightColor);
			Main.EntitySpriteDraw(texture,
				Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
				sourceRectangle, drawColor, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);

			// It's important to return false, otherwise we also draw the original texture.
			return false;
		}

		public override void OnKill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position); // Play a sound when the projectile dies. In this case, that is when it hits a block or a liquid.

			if (Projectile.owner == Main.myPlayer && !Projectile.noDropItem) {
				int dropItemType = ModContent.ItemType<Items.ExamplePaperAirplane>(); // This the item we want the paper airplane to drop.
				int newItem = Item.NewItem(Projectile.GetSource_DropAsItem(), Projectile.Hitbox, dropItemType); // Create a new item in the world.
				Main.item[newItem].noGrabDelay = 0; // Set the new item to be able to be picked up instantly

				// Here we need to make sure the item is synced in multiplayer games.
				if (Main.netMode == NetmodeID.MultiplayerClient && newItem >= 0) {
					NetMessage.SendData(MessageID.SyncItem, -1, -1, null, newItem, 1f);
				}
			}

			// Let's add some dust for special effect.
			for (int i = 0; i < 10; i++) {
				Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, ModContent.DustType<Dusts.Sparkle>());
			}
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			if (target.type == NPCID.GraniteGolem) {
				// Paper beats Rock!
				// Use FinalDamage since the projectile isn't conceptually stronger, the target is weaker to this weapon.
				modifiers.FinalDamage *= 20f; // 20x damage...isn't much since defense is high and damage is low.
			}
		}
	}
}
