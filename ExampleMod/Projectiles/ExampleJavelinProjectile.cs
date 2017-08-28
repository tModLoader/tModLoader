using ExampleMod.Items.Weapons;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Projectiles
{
	public class ExampleJavelinProjectile : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Javelin");
		}

		public override void SetDefaults()
		{
			projectile.width = 16;
			projectile.height = 16;
			projectile.aiStyle = -1;
			projectile.friendly = true;
			projectile.melee = true;
			projectile.penetrate = 3;
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
		{
			// For going through platforms and such, javelins use a tad smaller size
			width = height = 10; // notice we set the width to the height, the height to 10. so both are 10
			return true;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			// Inflate some target hitboxes if they are beyond 8,8 size
			if (targetHitbox.Width > 8 && targetHitbox.Height > 8)
			{
				targetHitbox.Inflate(-targetHitbox.Width / 8, -targetHitbox.Height / 8);
			}
			// Return if the hitboxes intersects, which means the javelin collides or not
			return projHitbox.Intersects(targetHitbox);
		}

		public override void Kill(int timeLeft)
		{
			Main.PlaySound(0, (int)projectile.position.X, (int)projectile.position.Y); // Play a death sound
			Vector2 usePos = projectile.position; // Position to use for dusts
			// Please note the usage of MathHelper, please use this! We subtract 90 degrees as radians to the rotation vector to offset the sprite as its default rotation in the sprite isn't aligned properly.
			Vector2 rotVector =
				(projectile.rotation - MathHelper.ToRadians(90f)).ToRotationVector2(); // rotation vector to use for dust velocity
			usePos += rotVector * 16f;

			// Spawn some dusts upon javelin death
			for (int i = 0; i < 20; i++)
			{
				// Create a new dust
				Dust dust = Dust.NewDustDirect(usePos, projectile.width, projectile.height, 81);
				dust.position = (dust.position + projectile.Center) / 2f;
				dust.velocity += rotVector * 2f;
				dust.velocity *= 0.5f;
				dust.noGravity = true;
				usePos -= rotVector * 8f;
			}

			// Drop a javelin item, 18% chance
			int item =
				Main.rand.Next(18) == 0
					? Item.NewItem((int)projectile.position.X, (int)projectile.position.Y, projectile.width, projectile.height, mod.ItemType<ExampleJavelin>())
					: 0;

			// Sync the drop for multiplayer
			// Note the usage of Terraria.ID.MessageID, please use this!
			if (Main.netMode == 1 && item >= 0)
			{
				NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f);
			}
		}

		// Here's an example on how you could make your AI even more readable, by giving AI fields more descriptive names
		// These are not used in AI, but it is good practice to apply some form like this to keep things organized

		// Are we sticking to a target?
		public bool isStickingToTarget
		{
			get { return projectile.ai[0] == 1f; }
			set { projectile.ai[0] = value ? 1f : 0f; }
		}

		// WhoAmI of the current target
		public float targetWhoAmI
		{
			get { return projectile.ai[1]; }
			set { projectile.ai[1] = value; }
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit,
			ref int hitDirection)
		{
			// If you'd use the example above, you'd do: isStickingToTarget = 1f;
			// and: targetWhoAmI = (float)target.whoAmI;
			isStickingToTarget = true; // we are sticking to a target
			targetWhoAmI = (float)target.whoAmI; // Set the target whoAmI
			projectile.velocity =
				(target.Center - projectile.Center) *
				0.75f; // Change velocity based on delta center of targets (difference between entity centers)
			projectile.netUpdate = true; // netUpdate this javelin
			target.AddBuff(169, 900); // Adds the Penetrated debuff, a very small DoT

			projectile.damage = 0; // Makes sure the sticking javelins do not deal damage anymore

			// The following code handles the javelin sticking to the enemy hit.
			int maxStickingJavelins = 6; // This is the max. amount of javelins being able to attach
			Point[] stickingJavelins = new Point[maxStickingJavelins]; // The point array holding for sticking javelins
			int javelinIndex = 0; // The javelin index
			for (int i = 0; i < Main.maxProjectiles; i++) // Loop all projectiles
			{
				Projectile currentProjectile = Main.projectile[i];
				if (i != projectile.whoAmI // Make sure the looped projectile is not the current javelin
				    && currentProjectile.active // Make sure the projectile is active
				    && currentProjectile.owner == Main.myPlayer // Make sure the projectile's owner is the client's player
				    && currentProjectile.type == projectile.type // Make sure the projectile is of the same type as this javelin
				    && currentProjectile.ai[0] == 1f // Make sure ai0 state is set to 1f (set earlier in ModifyHitNPC)
				    && currentProjectile.ai[1] == (float)target.whoAmI
				) // Make sure ai1 is set to the target whoAmI (set earlier in ModifyHitNPC)
				{
					stickingJavelins[javelinIndex++] =
						new Point(i, currentProjectile.timeLeft); // Add the current projectile's index and timeleft to the point array
					if (javelinIndex >= stickingJavelins.Length
					) // If the javelin's index is bigger than or equal to the point array's length, break
					{
						break;
					}
				}
			}
			// Here we loop the other javelins if new javelin needs to take an older javelin's place.
			if (javelinIndex >= stickingJavelins.Length)
			{
				int oldJavelinIndex = 0;
				// Loop our point array
				for (int i = 1; i < stickingJavelins.Length; i++)
				{
					// Remove the already existing javelin if it's timeLeft value (which is the Y value in our point array) is smaller than the new javelin's timeLeft
					if (stickingJavelins[i].Y < stickingJavelins[oldJavelinIndex].Y)
					{
						oldJavelinIndex = i; // Remember the index of the removed javelin
					}
				}
				// Remember that the X value in our point array was equal to the index of that javelin, so it's used here to kill it.
				Main.projectile[stickingJavelins[oldJavelinIndex].X].Kill();
			}
		}

		// Added these 2 constant to showcase how you could make AI code cleaner by doing this
		// Change this number if you want to alter how long the javelin can travel at a constant speed
		private const float maxTicks = 45f;

		// Change this number if you want to alter how the alpha changes
		private const int alphaReduction = 25;

		public override void AI()
		{
			// Slowly remove alpha as it is present
			if (projectile.alpha > 0)
			{
				projectile.alpha -= alphaReduction;
			}
			// If alpha gets lower than 0, set it to 0
			if (projectile.alpha < 0)
			{
				projectile.alpha = 0;
			}
			// If ai0 is 0f, run this code. This is the 'movement' code for the javelin as long as it isn't sticking to a target
			if (!isStickingToTarget)
			{
				targetWhoAmI += 1f;
				// For a little while, the javelin will travel with the same speed, but after this, the javelin drops velocity very quickly.
				if (targetWhoAmI >= maxTicks)
				{
					// Change these multiplication factors to alter the javelin's movement change after reaching maxTicks
					float velXmult = 0.98f; // x velocity factor, every AI update the x velocity will be 98% of the original speed
					float
						velYmult = 0.35f; // y velocity factor, every AI update the y velocity will be be 0.35f bigger of the original speed, causing the javelin to drop to the ground
					targetWhoAmI = maxTicks; // set ai1 to maxTicks continuously
					projectile.velocity.X = projectile.velocity.X * velXmult;
					projectile.velocity.Y = projectile.velocity.Y + velYmult;
				}
				// Make sure to set the rotation accordingly to the velocity, and add some to work around the sprite's rotation
				projectile.rotation =
					projectile.velocity.ToRotation() +
					MathHelper.ToRadians(
						90f); // Please notice the MathHelper usage, offset the rotation by 90 degrees (to radians because rotation uses radians) because the sprite's rotation is not aligned!

				// Spawn some random dusts as the javelin travels
				if (Main.rand.Next(3) == 0)
				{
					Dust dust = Dust.NewDustDirect(projectile.position, projectile.height, projectile.width, mod.DustType<Dusts.Sparkle>(),
						projectile.velocity.X * .2f, projectile.velocity.Y * .2f, 200, Scale: 1.2f);
					dust.velocity += projectile.velocity * 0.3f;
					dust.velocity *= 0.2f;
				}
				if (Main.rand.Next(4) == 0)
				{
					Dust dust = Dust.NewDustDirect(projectile.position, projectile.height, projectile.width, mod.DustType<Dusts.Sparkle>(),
						0, 0, 254, Scale: 0.3f);
					dust.velocity += projectile.velocity * 0.5f;
					dust.velocity *= 0.5f;
				}
			}
			// This code is ran when the javelin is sticking to a target
			if (isStickingToTarget)
			{
				// These 2 could probably be moved to the ModifyNPCHit hook, but in vanilla they are present in the AI
				projectile.ignoreWater = true; // Make sure the projectile ignores water
				projectile.tileCollide = false; // Make sure the projectile doesn't collide with tiles anymore
				int aiFactor = 15; // Change this factor to change the 'lifetime' of this sticking javelin
				bool killProj = false; // if true, kill projectile at the end
				bool hitEffect = false; // if true, perform a hit effect
				projectile.localAI[0] += 1f;
				// Every 30 ticks, the javelin will perform a hit effect
				hitEffect = projectile.localAI[0] % 30f == 0f;
				int projTargetIndex = (int)targetWhoAmI;
				if (projectile.localAI[0] >= (float)(60 * aiFactor)// If it's time for this javelin to die, kill it
				    || (projTargetIndex < 0 || projTargetIndex >= 200)) // If the index is past its limits, kill it
				{
					killProj = true;
				}
				else if (Main.npc[projTargetIndex].active && !Main.npc[projTargetIndex].dontTakeDamage) // If the target is active and can take damage
				{
					// Set the projectile's position relative to the target's center
					projectile.Center = Main.npc[projTargetIndex].Center - projectile.velocity * 2f;
					projectile.gfxOffY = Main.npc[projTargetIndex].gfxOffY;
					if (hitEffect) // Perform a hit effect here
					{
						Main.npc[projTargetIndex].HitEffect(0, 1.0);
					}
				}
				else // Otherwise, kill the projectile
				{
					killProj = true;
				}

				if (killProj) // Kill the projectile
				{
					projectile.Kill();
				}
			}
		}
	}
}
