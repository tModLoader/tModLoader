using ExampleMod.Content.Buffs;
using ExampleMod.Content.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles.Minions
{
	// ExampleHoveringMinion uses inheritace as an example of how it can be useful in modding.
	// ExampleHoveringShooter and Minion classes help abstract common functionality away, which is useful for mods that have many similar behaviors.
	// Inheritance is an advanced topic and could be confusing to new programmers, see ExampleSimpleMinion.cs for a simpler minion example.
	public class ExampleHoveringMinion : ExampleHoveringShooterMinion
	{
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 3; // Sets the number of frams in ExampleHoveringMinion's animation
			Main.projPet[Projectile.type] = true;
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true; //This is necessary for right-click targeting
		}

		public override void SetDefaults() {
			Projectile.netImportant = true;
			Projectile.width = 24; // Sets the width of the projectile
			Projectile.height = 32; // Sets the height of the projectile
			Projectile.friendly = true;
			Projectile.minion = true; // Sets the damage type to minion
			Projectile.minionSlots = 1; // ExampleHoveringMinion takes one minion slot
			Projectile.penetrate = -1; // Needed so minion doesn't despawn on contact with enemies/tiles
			Projectile.timeLeft = 18000;
			Projectile.tileCollide = false; // ExampleHoveringMinion can move through blocks
			Projectile.ignoreWater = true; // ExampleHoveringMinion's movement ignores water
			
			inertia = 20f; // Defines how quickly ExampleHoveringMinion accelerates
			shoot = ModContent.ProjectileType<ExampleMinionProjectile>(); // ExampleHoveringMinion fires ExampleMinionProjectile
			shootSpeed = 12f; // Defines how often the ExampleHoveringMinion can fire
		}

		public override void CheckActive() {
			Player player = Main.player[Projectile.owner];
			
			if (player.dead || !player.active) {
				player.ClearBuff(ModContent.BuffType<ExampleHoveringMinionBuff>()); // If player is dead or inactive the ExampleHoveringMinion buff is cleared
			}
			
			// If the player still has the buff, the projectile's timer is refreshed
			if (player.HasBuff(ModContent.BuffType<ExampleHoveringMinionBuff>())) {
				Projectile.timeLeft = 2;
			}
		}

		public override void CreateDust() {
			// Creates a dust effect
			if (Projectile.ai[0] == 0f) {
				if (Main.rand.NextBool(5)) {
					int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height / 2, ModContent.DustType<ExampleHoveringMinionDust>()); // ExampleHoveringMinion uses the ExampleHoveringMinion dust
					Main.dust[dust].velocity.Y -= 1.2f;
				}
			}
			else {
				if (Main.rand.NextBool(3)) {
					Vector2 dustVel = Projectile.velocity;
					
					if (dustVel != Vector2.Zero) {
						dustVel.Normalize();
					}
					
					var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<ExampleHoveringMinionDust>());
					
					dust.velocity -= 1.2f * dustVel; // Makes the dust effect take into account the movement of ExampleHovering Minion
				}
			}
			
			// Make ExampleHoveringMinion emit light
			Lighting.AddLight((int)(Projectile.Center.X / 16f), (int)(Projectile.Center.Y / 16f), 0.6f, 0.9f, 0.3f);
		}

		public override void SelectFrame() {
			// Creates the projectile animation
			Projectile.frameCounter++;
			
			if (Projectile.frameCounter >= 8) {
				Projectile.frameCounter = 0;
				Projectile.frame = (Projectile.frame + 1) % 3;
			}
		}
	}
}
