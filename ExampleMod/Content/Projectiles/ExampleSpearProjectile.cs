using ExampleMod.Content.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Content.Projectiles
{
	public class ExampleSpearProjectile : ModProjectile
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Spear");
		}

		public override void SetDefaults() {
			Projectile.width = 18; // The width of the projectile's hitbox in pixels.
			Projectile.height = 18; // The height of the projectile's hitbox in pixels.
			Projectile.aiStyle = 19; // Selects which vanilla code to use for the AI method. Modders can use vanilla aiStyle to mimic AI code already in the game. aistyle 19 is the Polearm effect. Projectile extends a small distance from the player. Used for: Dark Lance, Spear, Trident, Cobalt Naginata, Mythril Halberd, Adamantite Glaive, Gungnir.
			Projectile.penetrate = -1; // The Peneration of the projectile. -1 is infinite peneration.
			Projectile.scale = 1.3f; // The size multiplier of the projectile's sprite while the item is being used.

			Projectile.hide = true; // Projectile will go behind tiles instead of always being in front of them.
			Projectile.ownerHitCheck = true; // //so you can't hit enemies through walls
			Projectile.DamageType = DamageClass.Melee; // Determines which crit chance will influence the damage of this projectile. 
			Projectile.tileCollide = false; // The projectile will collide with tiles.
			Projectile.friendly = true; // If True, this projectile will hurt enemies.
		}

		// In here the AI uses this example, to make the code more organized and readable
		public float velocityMultiplier // Change this value to alter how fast the spear moves
		{
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		// It appears that for this AI, only the ai0 field is used!
		public override void AI() {
			// Since we access the owner player instance so much, it's useful to create a helper local variable for this
			// Sadly, Projectile/ModProjectile does not have its own
			Player projOwner = Main.player[Projectile.owner];
			// Here we set some of the projectile's owner properties, such as held item and itemtime, along with projectile direction and position based on the player
			Vector2 ownerMountedCenter = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);
			Projectile.direction = projOwner.direction; // Projectile's Direction is the Player's Direction.
			projOwner.heldProj = Projectile.whoAmI; // Update player's held projectile
			projOwner.itemTime = projOwner.itemAnimation;  // Sets item time to the frames of the item animation while used.
			Projectile.position.X = ownerMountedCenter.X - (float)(Projectile.width / 2);
			Projectile.position.Y = ownerMountedCenter.Y - (float)(Projectile.height / 2);
			// As long as the player isn't frozen, the spear can move
			if (!projOwner.frozen) {
				if (velocityMultiplier == 0f) // When initially thrown out, the ai0 will be 0f
				{
					velocityMultiplier = 3f; // Make sure the spear accelerates forward when initially thrown out
					Projectile.netUpdate = true; // Make sure to netUpdate this spear
				}
				if (projOwner.itemAnimation < projOwner.itemAnimationMax / 3) // Somewhere along the item animation, make sure the spear decelerates back
				{
					velocityMultiplier -= 2.4f;
				}
				else // Otherwise, increase the movement factor
				{
					velocityMultiplier += 2.1f;
				}
			}
			// Change the spear position based off of the velocity and the movementFactor
			Projectile.position += Projectile.velocity * velocityMultiplier;
			// When we reach the end of the animation, we can kill the spear projectile
			if (projOwner.itemAnimation == 0) {
				Projectile.Kill();
			}
			// Apply proper rotation, with an offset of 135 degrees due to the sprite's rotation, notice the usage of MathHelper, use this class!
			// MathHelper.ToRadians(xx degrees here)
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(135f);
			// Offset by 90 degrees here
			// -1 to check if sprite directions is left
			if (Projectile.spriteDirection == -1) {
				Projectile.rotation -= MathHelper.ToRadians(90f);

			}

			// These dusts are added later, for the 'ExampleMod' effect
			if (Main.rand.NextBool(3)) {
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.height, Projectile.width, DustType<Sparkle>(),
					Projectile.velocity.X * 2f, Projectile.velocity.Y * 2f, 200, Scale: 1.2f);

			}
			if (Main.rand.NextBool(4)) {
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.height, Projectile.width, DustType<Sparkle>(),
					Projectile.velocity.X * 2f, Projectile.velocity.Y * 2f, 254, Scale: 0.3f);

			}
		}
	}
}
