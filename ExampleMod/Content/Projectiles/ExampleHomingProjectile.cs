﻿using ExampleMod.Content.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{
	// This Example show how to implement simple homing projectile
	// Can be tested with ExampleCustomAmmoGun
	public class ExampleHomingProjectile : ModProjectile
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Example Homing Projectile"); // Name of the projectile. It can be appear in chat
			ProjectileID.Sets.CountsAsHoming[Projectile.type] = true; // Tell the game that it is a homing projectile
		}

		// Setting the default parameters of the projectile
		// You can check most of Fields and Properties here https://github.com/tModLoader/tModLoader/wiki/Projectile-Class-Documentation
		public override void SetDefaults() {
			Projectile.width = 8; // The width of projectile hitbox
			Projectile.height = 8; // The height of projectile hitbox

			Projectile.aiStyle = 0; // The ai style of the projectile (0 means custom AI). For more please reference the source code of Terraria
			Projectile.DamageType = DamageClass.Ranged; // What type of damage does this projectile affect?
			Projectile.friendly = true; // Can the projectile deal damage to enemies?
			Projectile.hostile = false; // Can the projectile deal damage to the player?
			Projectile.ignoreWater = true; // Does the projectile's speed be influenced by water?
			Projectile.light = 1f; // How much light emit around the projectile
			Projectile.tileCollide = false; // Can the projectile collide with tiles?
			Projectile.timeLeft = 600; //The live time for the projectile (60 = 1 second, so 600 is 10 seconds)
		}

		// Custom AI
		public override void AI() {
			float maxDetectRadius = 400f; // The maximum radius at which a projectile can detect a target
			float projSpeed = 5f; // The speed at which the projectile moves towards the target

			// Trying to find NPC closest to the projectile 
			NPC closestNPC = FindClosestNPC(maxDetectRadius);
			if (closestNPC == null)
				return;

			// If found, change the velocity of the projectile and turn it in the direction of the target
			Projectile.velocity = Vector2.Normalize(closestNPC.Center - Projectile.Center) * projSpeed;
			Projectile.rotation = Projectile.velocity.ToRotation();
		}

		// Finding the closest NPC to attack within maxDetectDistance range
		// If not found then returns null
		public NPC FindClosestNPC(float maxDetectDistance) {
			NPC closestNPC = null;
			// Loop through all NPCs(max always 200)
			for (int k = 0; k < Main.maxNPCs; k++) {
				NPC target = Main.npc[k];
				// Check if NPC able to be targeted. It means that NPC is
				// 1. active (alive)
				// 2. chaseable (e.g. not a cultist archer)
				// 3. max life bigger than 5 (e.g. not a critter)
				// 4. can take damage (e.g. moonlord core after all it's parts are downed)
				// 5. hostile (!friendly)
				// 6. not immortal (e.g. not a target dummy)
				if (target.CanBeChasedBy()) {
					float distanceToTarget = Vector2.Distance(target.Center, Projectile.Center);
					// Check if it is within the radius
					if (distanceToTarget < maxDetectDistance) {
						maxDetectDistance = distanceToTarget;
						closestNPC = target;
					}
				}
			}
			return closestNPC;
		}
	}
}
