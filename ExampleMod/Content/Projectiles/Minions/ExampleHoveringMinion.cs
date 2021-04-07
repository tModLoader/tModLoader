using ExampleMod.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Content.Projectiles.Minions
{
	// ExampleHoveringMinion uses inheritace as an example of how it can be useful in modding.
	// ExampleHoveringShooter and Minion classes help abstract common functionality away, which is useful for mods that have many similar behaviors.
	// Inheritance is an advanced topic and could be confusing to new programmers, see ExampleSimpleMinion.cs for a simpler minion example.
	public class ExampleHoveringMinion : ExampleHoveringShooter
	{
		public override void SetStaticDefaults() {
			Main.projFrames[projectile.type] = 3; // Sets the number of frams in ExampleHoveringMinion's animation
			Main.projPet[projectile.type] = true;
			ProjectileID.Sets.MinionSacrificable[projectile.type] = true;
			ProjectileID.Sets.Homing[projectile.type] = true;
			ProjectileID.Sets.MinionTargettingFeature[projectile.type] = true; //This is necessary for right-click targeting
		}

		public override void SetDefaults() {
			projectile.netImportant = true;
			projectile.width = 24; // Sets the width of the projectile
			projectile.height = 32; // Sets the height of the projectile
			projectile.friendly = true;
			projectile.minion = true; // Sets the damage type to minion
			projectile.minionSlots = 1; // ExampleHoveringMinion takes one minion slot
			projectile.penetrate = -1; // Needed so minion doesn't despawn on contact with enemies/tiles
			projectile.timeLeft = 18000;
			projectile.tileCollide = false; // ExampleHoveringMinion can move through blocks
			projectile.ignoreWater = true; // ExampleHoveringMinion's movement ignores water
			inertia = 20f; // Defines how quickly ExampleHoveringMinion accelerates
			shoot = ProjectileType<ExampleMinionProjectile>(); // ExampleHoveringMinion fires ExampleMinionProjectile
			shootSpeed = 12f; // Defines how often the ExampleHoveringMinion can fire
		}

		public override void CheckActive() {
			Player player = Main.player[projectile.owner];
			
			if (player.dead || !player.active) {
				player.ClearBuff(ModContent.BuffType<ExampleHoveringMinion>()); // If player is dead or inactive the ExampleHoveringMinion buff is cleared
			}
			
			// If the player still has the buff, the projectile's timer is refreshed
			if (player.HasBuff(ModContent.BuffType<ExampleHoveringMinion>()) {
				projectile.timeLeft = 2;
			}
		}

		public override void CreateDust() {
			// Creates a dust effect
			if (projectile.ai[0] == 0f) {
				if (Main.rand.NextBool(5)) {
					int dust = Dust.NewDust(projectile.position, projectile.width, projectile.height / 2, DustType<ExampleMinionDust>()); // ExampleHoveringMinion uses the ExampleHoveringMinion dust
					Main.dust[dust].velocity.Y -= 1.2f;
				}
			}
			else {
				if (Main.rand.NextBool(3)) {
					Vector2 dustVel = projectile.velocity;
					if (dustVel != Vector2.Zero) {
						dustVel.Normalize();
					}
					int dust = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustType<ExampleMinionDust>());
					Main.dust[dust].velocity -= 1.2f * dustVel; // Makes the dust effect take into account the movement of ExampleHovering Minion
				}
			}
			// Make ExampleHoveringMinion emit light
			Lighting.AddLight((int)(projectile.Center.X / 16f), (int)(projectile.Center.Y / 16f), 0.6f, 0.9f, 0.3f);
		}

		public override void SelectFrame() {
			// Creates the projectile animation
			projectile.frameCounter++;
			if (projectile.frameCounter >= 8) {
				projectile.frameCounter = 0;
				projectile.frame = (projectile.frame + 1) % 3;
			}
		}
	}
}
