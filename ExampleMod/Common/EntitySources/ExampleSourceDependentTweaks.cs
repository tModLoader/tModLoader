using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Common.EntitySources
{
	// The following classes showcases pattern matching of IEntitySource instances to make things happen only in specific contexts.
	
	public sealed class ExampleSourceDependentProjectileTweaks : GlobalProjectile
	{
		public override void OnSpawn(Projectile projectile, IEntitySource source) {
			// If the projectile was shot by Torch God, and is aimed at a player - triple the amount by shooting 2 slower and spread ones.
			if (source is EntitySource_TorchGod { TargetedEntity: Player } && projectile.type == ProjectileID.TorchGod) {
				var newSource = projectile.GetSource_FromThis(); // Use a separate source for the newly created projectiles, to not cause a stack overflow.
				
				for (int i = -1; i < 2; i += 2) {
					var velocity = projectile.velocity.RotatedBy(MathHelper.ToRadians(15f * i)) * 0.5f;

					Projectile.NewProjectile(newSource, projectile.position, velocity, projectile.type, projectile.damage, projectile.knockBack, projectile.owner);
				}
			}

			// For every projectile created in falling stars' context.
			if (source is EntitySource_Misc { Context: "FallingStar" }) {
				float closestPlayerSqrDistance = -1f;
				Player closestPlayer = null;

				for (int i = 0; i < Main.maxPlayers; i++) {
					var player = Main.player[i];

					if (player?.active != true || player.DeadOrGhost) {
						continue;
					}

					// Squared distance is satisfactory here, so we use it since it's way quicker to get.
					float sqrDistance = player.Center.LengthSquared();

					if (closestPlayer == null || sqrDistance < closestPlayerSqrDistance) {
						closestPlayer = player;
						closestPlayerSqrDistance = sqrDistance;
					}
				}

				// If the closest player is found
				if (closestPlayer != null) {
					// Aim the falling star towards the closest player -- don't worry, they don't seem to deal damage to players.
					var directionTowardsPlayer = (closestPlayer.Center - projectile.Center).SafeNormalize(default);

					if (directionTowardsPlayer != default) {
						projectile.velocity = directionTowardsPlayer * (projectile.velocity.Length() + 10f); // With a 'small' boost.

						//NOTE: OnSpawn is called before the projectile is synchronized, so in this case we don't have to sync the velocity for multiplayer.
					}
				}
			}
		}
	}
	
	public sealed class ExampleSourceDependentItemTweaks : GlobalItem
	{
		public override void OnSpawn(Item item, IEntitySource source) {
			// Accompany all loot from trees with a slime.
			if (source is EntitySource_ShakeTree) {
				var newSource = item.GetSource_FromThis(); // Use a separate source for the newly created projectiles, to not cause a stack overflow.

				NPC.NewNPC(newSource, (int)item.position.X, (int)item.position.Y, NPCID.BlueSlime);
			}
		}
	}
}
