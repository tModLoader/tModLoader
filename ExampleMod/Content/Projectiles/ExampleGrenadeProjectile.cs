using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{
	// This grenade is for the grenades shot by the Grenade Launcher, not the grenades that you can throw.
	public class ExampleGrenadeProjectile : ModProjectile
	{
		public override void SetStaticDefaults() {
			ProjectileID.Sets.PlayerHurtDamageIgnoresDifficultyScaling[Type] = true; // Damage dealt to players does not scale with difficulty in vanilla.
		}
		public override void SetDefaults() {
			Projectile.width = 14;
			Projectile.height = 14;
			// Grenades use explosive AI, ProjAIStyleID.Explosive (16). You could use that instead here with the correct AIType.
			// But, using our own AI allows us to customize things like the dusts that the grenade creates.
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.penetrate = -1; // Infinite penetration so that the blast can hit all enemies within its radius.
			Projectile.DamageType = DamageClass.Ranged;
			// usesLocalNPCImmunity and localNPCHitCooldown of -1 mean the projectile can only hit the same target once.
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;

			Projectile.timeLeft = 180;

			// AIType = ProjectileID.GrenadeI;
		}
		public override void AI() {
			// If timeLeft is <= 3, then explode the rocket.
			if (Projectile.owner == Main.myPlayer && Projectile.timeLeft <= 3) {
				PrepareBombToBlow();
			}
			else {
				// Spawn a smoke dust.
				Dust smokeDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100);
				smokeDust.scale *= 1f + (float)Main.rand.Next(10) * 0.1f;
				smokeDust.velocity *= 0.2f;
				smokeDust.noGravity = true;
			}

			Projectile.ai[0] += 1f;
			// Wait 15 ticks until applying friction and gravity.
			if (Projectile.ai[0] > 15f) {
				// Slow down if on the ground.
				if (Projectile.velocity.Y == 0f) {
					Projectile.velocity.X *= 0.95f;
				}

				// Fall down. Remember, positive Y is down.
				Projectile.velocity.Y += 0.2f;
			}

			// Rotate the grenade in the direction it is moving.
			Projectile.rotation += Projectile.velocity.X * 0.1f;

			// Explosives behave differently when touching Shimmer.
			if (Projectile.shimmerWet) {
				int projX = (int)(Projectile.Center.X / 16f);
				int projY = (int)(Projectile.position.Y / 16f);
				// If the projectile is inside of Shimmer:
				if (WorldGen.InWorld(projX, projY) && Main.tile[projX, projY] != null &&
						Main.tile[projX, projY].LiquidAmount == byte.MaxValue &&
						Main.tile[projX, projY].LiquidType == LiquidID.Shimmer &&
						WorldGen.InWorld(projX, projY - 1) && Main.tile[projX, projY - 1] != null &&
						Main.tile[projX, projY - 1].LiquidAmount > 0 &&
						Main.tile[projX, projY - 1].LiquidType == LiquidID.Shimmer) {
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
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			// Bounce off of tiles.
			if (Projectile.velocity.X != oldVelocity.X) {
				Projectile.velocity.X = oldVelocity.X * -0.4f;
			}

			if (Projectile.velocity.Y != oldVelocity.Y && oldVelocity.Y > 0.7f) {
				Projectile.velocity.Y = oldVelocity.Y * -0.4f;
			}

			// Return false so the projectile doesn't get killed. If you do want your projectile to explode on contact with tiles, do not return true here.
			// If you return true, the projectile will die without being resized (no blast radius).
			// Instead, set `Projectile.timeLeft = 3;` like the Example Rocket Projectile.
			return false;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (Projectile.timeLeft > 3) {
				Projectile.timeLeft = 3; // Set the timeLeft to 3 so it can get ready to explode.
			}

			// Set the direction of the projectile so the knockback is always in the correct direction.
			Projectile.direction = (target.Center.X > Projectile.Center.X).ToDirectionInt();
		}

		// This is only to make it so the grenade explodes when hitting a player in PVP. Otherwise the grenade will continue through the enemy player.
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			if (modifiers.PvP && Projectile.timeLeft > 3) {
				Projectile.timeLeft = 3; // Set the timeLeft to 3 so it can get ready to explode.
			}
			// Set the direction of the projectile so the knockback is always in the correct direction.
			Projectile.direction = (target.Center.X > Projectile.Center.X).ToDirectionInt();
		}

		/// <summary>
		/// This function will manually damage players if applicable.
		/// </summary>
		/// <param name="projRectangle">Position and hitbox of the projectile.</param>
		/// <param name="playerIndex">The index of the player in Main.player[]</param>
		private void BombsHurtPlayers(Rectangle projRectangle, int playerIndex) {
			Player targetPlayer = Main.player[playerIndex];
			// Check that the grenade should damage the player in the first place. If not, return.
			if (Projectile.timeLeft > 1 || !targetPlayer.active || targetPlayer.dead || targetPlayer.immune) {
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
			// If in For the Worthy or Get Fixed Boi worlds, the blast damage can damage other players.
			if (Main.getGoodWorld && Projectile.owner != Main.myPlayer) {
				PrepareBombToBlow();
			}

			Rectangle blastRectangle = new Rectangle((int)Projectile.position.X, (int)Projectile.position.Y, Projectile.width, Projectile.height);
			// If in For the Worthy or Get Fixed Boi worlds, the blast damage can damage other players.
			if (Projectile.friendly && Main.getGoodWorld && Main.netMode == NetmodeID.MultiplayerClient && Projectile.owner != Main.myPlayer && !Projectile.npcProj) {
				BombsHurtPlayers(blastRectangle, Main.myPlayer);
			}
			// Damage the player who fired the rocket.
			else if (Projectile.friendly && Projectile.owner == Main.myPlayer && !Projectile.npcProj) {
				BombsHurtPlayers(blastRectangle, Projectile.owner);
				CutTiles(); // Destroy tall grass and flowers around the explosion.
			}

			// Play an exploding sound.
			SoundEngine.PlaySound(SoundID.Item62, Projectile.position);

			// Resize the projectile again so the explosion dust and gore spawn from the middle.
			// Rocket I: 22, Rocket III: 80, Mini Nuke Rocket: 50
			Projectile.Resize(22, 22);

			// Spawn a bunch of smoke dusts.
			for (int i = 0; i < 30; i++) {
				Dust smoke = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 1.5f);
				smoke.velocity *= 1.4f;
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

			// To make the explosion destroy tiles, take a look at the commented out code in Example Rocket Projectile.
		}
	}
}