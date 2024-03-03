using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{
	public class ExampleRocketProjectile : ModProjectile
	{
		public override void SetStaticDefaults() {
			ProjectileID.Sets.IsARocketThatDealsDoubleDamageToPrimaryEnemy[Type] = true; // Deals double damage on direct hits.
			ProjectileID.Sets.PlayerHurtDamageIgnoresDifficultyScaling[Type] = true; // Damage dealt to players does not scale with difficulty in vanilla.
			ProjectileID.Sets.Explosive[Type] = true;
			// ProjectileID.Sets.RocketsSkipDamageForPlayers[Type] = true; // This set makes it so the rocket doesn't deal damage to players. Be sure to remove the BombsHurtPlayers() part, too.
		}
		public override void SetDefaults() {
			Projectile.width = 14;
			Projectile.height = 14;
			// Rockets use explosive AI, ProjAIStyleID.Explosive (16). You could use that instead here with the correct AIType.
			// But, using our own AI allows us to customize things like the dusts that the rocket creates.
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.penetrate = -1; // Infinite penetration so that the blast can hit all enemies within its radius.
			Projectile.DamageType = DamageClass.Ranged;

			// AIType = ProjectileID.RocketI;
		}
		public override void AI() {
			// If timeLeft is <= 3, then explode the rocket.
			if (Projectile.owner == Main.myPlayer && Projectile.timeLeft <= 3) {
				PrepareBombToBlow();
			}
			else {
				// Spawn dusts if the rocket is moving at or greater than half of its max speed.
				if (Math.Abs(Projectile.velocity.X) >= 8f || Math.Abs(Projectile.velocity.Y) >= 8f) {
					for (int i = 0; i < 2; i++) {
						float posOffsetX = 0f;
						float posOffsetY = 0f;
						if (i == 1) {
							posOffsetX = Projectile.velocity.X * 0.5f;
							posOffsetY = Projectile.velocity.Y * 0.5f;
						}

						// Spawn fire dusts at the back of the rocket.
						Dust fireDust = Dust.NewDustDirect(new Vector2(Projectile.position.X + 3f + posOffsetX, Projectile.position.Y + 3f + posOffsetY) - Projectile.velocity * 0.5f,
							Projectile.width - 8, Projectile.height - 8, DustID.Torch, 0f, 0f, 100);
						fireDust.scale *= 2f + Main.rand.Next(10) * 0.1f;
						fireDust.velocity *= 0.2f;
						fireDust.noGravity = true;

						// Used by the liquid rockets which leave trails of their liquid instead of fire.
						// if (fireDust.type == Dust.dustWater()) {
						//	fireDust.scale *= 0.65f;
						//	fireDust.velocity += Projectile.velocity * 0.1f;
						// }

						// Spawn smoke dusts at the back of the rocket.
						Dust smokeDust = Dust.NewDustDirect(new Vector2(Projectile.position.X + 3f + posOffsetX, Projectile.position.Y + 3f + posOffsetY) - Projectile.velocity * 0.5f, Projectile.width - 8, Projectile.height - 8, DustID.Smoke, 0f, 0f, 100, default, 0.5f);
						smokeDust.fadeIn = 1f + Main.rand.Next(5) * 0.1f;
						smokeDust.velocity *= 0.05f;
					}
				}

				// Increase the speed of the rocket if it is moving less than 1 block per second.
				// It is not recommended to increase the number past 16f to increase the speed of the rocket. It could start no clipping through blocks.
				// Instead, increase extraUpdates in SetDefaults() to make the rocket move faster.
				if (Math.Abs(Projectile.velocity.X) <= 15f && Math.Abs(Projectile.velocity.Y) <= 15f) {
					Projectile.velocity *= 1.1f;
				}
			}

			// Rotate the rocket in the direction that it is moving.
			if (Projectile.velocity != Vector2.Zero) {
				Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + MathHelper.PiOver2;
			}

			/*
			// Explosives behave differently when touching Shimmer.
			if (Projectile.shimmerWet) {
				int projTileX = (int)(Projectile.Center.X / 16f);
				int projTileY = (int)(Projectile.position.Y / 16f);
				// If the projectile is inside of Shimmer:
				if (WorldGen.InWorld(projTileX, projTileY) && Main.tile[projTileX, projTileY] != null &&
						Main.tile[projTileX, projTileY].LiquidAmount == byte.MaxValue &&
						Main.tile[projTileX, projTileY].LiquidType == LiquidID.Shimmer &&
						WorldGen.InWorld(projTileX, projTileY - 1) && Main.tile[projTileX, projTileY - 1] != null &&
						Main.tile[projTileX, projTileY - 1].LiquidAmount > 0 &&
						Main.tile[projTileX, projTileY - 1].LiquidType == LiquidID.Shimmer) {
					Projectile.Kill(); // Kill the projectile with no (player or enemy damaging) blast radius.
				}
				// Otherwise, bounce off of the top of the Shimmer if traveling downwards.
				else if (Projectile.velocity.Y > 0f) {
					Projectile.velocity.Y *= -1f; // Reverse the Y velocity.
					Projectile.netUpdate = true; // Sync the change in multiplayer.
					if (Projectile.timeLeft > 600) {
						Projectile.timeLeft = 600; // Set the max time to 10 seconds (instead of the default 1 minute).
					}
					
					Projectile.timeLeft -= 60; // Subtract 1 second from the time left.
					Projectile.shimmerWet = false;
					Projectile.wet = false;
				}
			}
			*/
		}

		// When the rocket hits a tile, NPC, or player, get ready to explode.
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.velocity *= 0f; // Stop moving so the explosion is where the rocket was.
			Projectile.timeLeft = 3; // Set the timeLeft to 3 so it can get ready to explode.
			return false; // Returning false is important here. Otherwise the projectile will die without being resized (no blast radius).
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			/*
			if (Projectile.timeLeft > 3) {
				Projectile.timeLeft = 3; // Set the timeLeft to 3 so it can get ready to explode.
			}

			// Set the direction of the projectile so the knockback is always in the correct direction.
			Projectile.direction = (target.Center.X > Projectile.Center.X).ToDirectionInt();
			*/
		}

		// This is only to make it so the rocket explodes when hitting a player in PVP. Otherwise the rocket will continue through the enemy player.
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			/*
			if (modifiers.PvP && Projectile.timeLeft > 3) {
				Projectile.timeLeft = 3; // Set the timeLeft to 3 so it can get ready to explode.
			}
			// Set the direction of the projectile so the knockback is always in the correct direction.
			Projectile.direction = (target.Center.X > Projectile.Center.X).ToDirectionInt();
			*/
		}

		/// <summary>
		/// This function will manually damage players if applicable.
		/// </summary>
		/// <param name="projRectangle">Position and hitbox of the projectile.</param>
		/// <param name="playerIndex">The index of the player in Main.player[]</param>
		private void BombsHurtPlayers(Rectangle projRectangle, int playerIndex) {
			Player targetPlayer = Main.player[playerIndex];
			// Check that the rocket should damage the player in the first place. If not, return.
			if (Projectile.timeLeft > 1  || !targetPlayer.active || targetPlayer.dead || targetPlayer.immune) {
				return;
			}

			// Check that the blast radius intersects the player's hitbox. If not, return.
			Rectangle playerHitbox = new Rectangle((int)targetPlayer.position.X, (int)targetPlayer.position.Y, targetPlayer.width, targetPlayer.height);
			if (!projRectangle.Intersects(playerHitbox)) {
				return;
			}

			// Set the direction of the projectile so the knockback is always in the correct direction.
			Projectile.direction = (targetPlayer.Center.X > Projectile.Center.X).ToDirectionInt();

			int damageVariation = Main.DamageVar(Projectile.damage, 0f - targetPlayer.luck); // Get the damage variation (affected by luck).
			PlayerDeathReason damageSource = PlayerDeathReason.ByProjectile(Projectile.owner, Projectile.whoAmI); // Get the death message.

			// Apply damage to the player.
			targetPlayer.Hurt(damageSource, damageVariation, Projectile.direction, pvp: true, quiet: false, -1, Projectile.IsDamageDodgable(), armorPenetration: Projectile.ArmorPenetration);
		}

		/// <summary> Resizes the projectile for the explosion blast radius. </summary>
		private void PrepareBombToBlow() {
			Projectile.tileCollide = false; // This is important or the explosion will be in the wrong place if the rocket explodes on slopes.
			Projectile.alpha = 255; // Make the rocket invisible.

			// Resize the hitbox of the projectile for the blast "radius".
			// Rocket I: 128, Rocket III: 200, Mini Nuke Rocket: 250
			// Measurements are in pixels, so 128 / 16 = 8 tiles.
			Projectile.Resize(128, 128);
			// Set the knockback of the blast.
			// Rocket I: 8f, Rocket III: 10f, Mini Nuke Rocket: 12f
			Projectile.knockBack = 8f;
		}

		public override void OnKill(int timeLeft) {
			/*
			// If in For the Worthy or Get Fixed Boi worlds, the blast damage can damage other players.
			if (Main.getGoodWorld && Projectile.owner != Main.myPlayer) {
				PrepareBombToBlow();
			}
			*/

			Rectangle blastRectangle = new Rectangle((int)Projectile.position.X, (int)Projectile.position.Y, Projectile.width, Projectile.height);
			// If in For the Worthy or Get Fixed Boi worlds, the blast damage can damage other players.

			/*if (Projectile.friendly && Main.getGoodWorld && Main.netMode == NetmodeID.MultiplayerClient && Projectile.owner != Main.myPlayer && !Projectile.npcProj) {
				BombsHurtPlayers(blastRectangle, Main.myPlayer);
			}
			*/
			// Damage the player who fired the rocket.
			/*else*/ if (Projectile.friendly && Projectile.owner == Main.myPlayer && !Projectile.npcProj) {
				BombsHurtPlayers(blastRectangle, Projectile.owner);
				CutTiles(); // Destroy tall grass and flowers around the explosion.
			}

			// Play an exploding sound.
			SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

			// Resize the projectile again so the explosion dust and gore spawn from the middle.
			// Rocket I: 22, Rocket III: 80, Mini Nuke Rocket: 50
			Projectile.Resize(22, 22);

			// Spawn a bunch of smoke dusts.
			for (int i = 0; i < 30; i++) {
				Dust smokeDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 1.5f);
				smokeDust.velocity *= 1.4f;
			}

			// Spawn a bunch of fire dusts.
			for (int j = 0; j < 20; j++) {
				Dust fireDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 3.5f);
				fireDust.noGravity = true;
				fireDust.velocity *= 7f;
				fireDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 1.5f);
				fireDust.velocity *= 3f;
			}

			// Spawn a bunch of smoke gores.
			for (int k = 0; k < 2; k++) {
				float speedMulti = 0.4f;
				if (k == 1) {
					speedMulti = 0.8f;
				}

				Gore smokeGore = Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.position, default, Main.rand.Next(GoreID.Smoke1, GoreID.Smoke3 + 1));
				smokeGore.velocity *= speedMulti;
				smokeGore.velocity += Vector2.One;
				smokeGore = Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.position, default, Main.rand.Next(GoreID.Smoke1, GoreID.Smoke3 + 1));
				smokeGore.velocity *= speedMulti;
				smokeGore.velocity.X -= 1f;
				smokeGore.velocity.Y += 1f;
				smokeGore = Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.position, default, Main.rand.Next(GoreID.Smoke1, GoreID.Smoke3 + 1));
				smokeGore.velocity *= speedMulti;
				smokeGore.velocity.X += 1f;
				smokeGore.velocity.Y -= 1f;
				smokeGore = Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.position, default, Main.rand.Next(GoreID.Smoke1, GoreID.Smoke3 + 1));
				smokeGore.velocity *= speedMulti;
				smokeGore.velocity -= Vector2.One;
			}

			// Rocket II explosion that damages tiles.
			//if (Projectile.owner == Main.myPlayer) {
			//	int blastRadius = 3; // Rocket IV: 5, Mini Nuke Rocket II: 7

			//	int minTileX = (int)(Projectile.position.X / 16f - blastRadius);
			//	int maxTileX = (int)(Projectile.position.X / 16f + blastRadius);
			//	int minTileY = (int)(Projectile.position.Y / 16f - blastRadius);
			//	int maxTileY = (int)(Projectile.position.Y / 16f + blastRadius);

				// Make sure the tiles are inside the world.
			// Utils.ClampWithinWorld(ref minTileX, ref maxTileX, ref minTileY, ref maxTileY);

			// Check to see if the walls should be destroyed, too.
			//	bool wallSplode = Projectile.ShouldWallExplode(Projectile.position, blastRadius, minTileX, maxTileX, minTileY, maxTileY);
			// Do the damage.
			//	Projectile.ExplodeTiles(Projectile.position, blastRadius, minTileX, maxTileX, minTileY, maxTileY, wallSplode);
			//}
		}
	}
}