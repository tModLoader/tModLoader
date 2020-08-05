using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Pets.ExampleLightPet
{
	public class ExampleLightPetProjectile : ModProjectile
	{
		private const int DashCooldown = 1000; //How frequently this pet will dash at enemies.
		private const float DashSpeed = 20f; //The speed with which this pet will dash at enemies.
		private const int FadeInTicks = 30;
		private const int FullBrightTicks = 200;
		private const int FadeOutTicks = 30;
		private const float Range = 500f;

		private static readonly float RangeHypoteneus = (float)(Math.Sqrt(2.0) * Range); // This comes from the formula for calculating the diagonal of a square (a * √2)
		private static readonly float RangeHypoteneusSquared = RangeHypoteneus * RangeHypoteneus;

		//The following 2 lines of code are ref properties (learn about them in google) to the projectile.ai array entries, which will help us make our code way more readable.
		//We're using the ai array because it's automatically synchronized by the base game in multiplayer, which saves us from writing a lot of boilerplate code.
		//Note that the projectile.ai array is only 2 entries big. If you need more than 2 synchronized variables - you'll have to use fields and sync them manually.
		public ref float AIFadeProgress => ref projectile.ai[0];
		public ref float AIDashCharge => ref projectile.ai[1];

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Annoying Light");

			Main.projFrames[projectile.type] = 1;
			Main.projPet[projectile.type] = true;
			ProjectileID.Sets.TrailingMode[projectile.type] = 2;
			ProjectileID.Sets.LightPet[projectile.type] = true;
		}

		public override void SetDefaults() {
			projectile.width = 30;
			projectile.height = 30;
			projectile.penetrate = -1;
			projectile.netImportant = true;
			projectile.timeLeft *= 5;
			projectile.friendly = true;
			projectile.ignoreWater = true;
			projectile.scale = 0.8f;
			projectile.tileCollide = false;
		}

		public override void AI() {
			Player player = Main.player[projectile.owner];

			//If the player is no longer active (online) - deactivate (remove) the projectile.
			if (!player.active) {
				projectile.active = false;
				return;
			}

			//Keep the projectile disappearing as long as the player isn't dead and has the pet buff.
			if (!player.dead && player.HasBuff(ModContent.BuffType<ExampleLightPetBuff>())) {
				projectile.timeLeft = 2;
			}

			UpdateDash(player);
			UpdateFading(player);
			UpdateExtraMovement();

			//Rotates the pet when it moves horizontally.
			projectile.rotation += projectile.velocity.X / 20f;

			//Lights up area around it.
			if (!Main.dedServ) {
				Lighting.AddLight(projectile.Center, (255 - projectile.alpha) * 0.9f / 255f, (255 - projectile.alpha) * 0.1f / 255f, (255 - projectile.alpha) * 0.3f / 255f);
			}
		}

		private void UpdateDash(Player player) {
			//The following code makes our pet dash at enemies when certain conditions are met

			AIDashCharge++;

			if (AIDashCharge <= DashCooldown || (int)AIFadeProgress % 100 != 0) {
				return;
			}

			//Enumerate 
			for (int i = 0; i < Main.maxNPCs; i++) {
				var npc = Main.npc[i];

				//Ignore this npc if it's not active, or if it's friendly.
				if (!npc.active || npc.friendly) {
					continue;
				}

				//Ignore this npc if it's too far away. Note that we're using squared values for our checks, to avoid square root calculations as a small, but effective optimization.
				if (player.DistanceSQ(npc.Center) >= RangeHypoteneusSquared) {
					continue;
				}

				projectile.velocity += Vector2.Normalize(npc.Center - projectile.Center) * DashSpeed; //Fling the projectile towards the npc.
				AIDashCharge = 0f; //Reset the charge.

				//Play a sound.
				if (!Main.dedServ) {
					SoundEngine.PlaySound(SoundID.Item42, projectile.Center);
				}

				break;
			}
		}

		private void UpdateFading(Player player) {
			//TODO: Comment and clean this up more.

			var playerCenter = player.Center; //Cache the player's center vector to avoid recalculations.

			AIFadeProgress++;

			if (AIFadeProgress < FadeInTicks) {
				projectile.alpha = (int)(255 - 255 * AIFadeProgress / FadeInTicks);
			}
			else if (AIFadeProgress < FadeInTicks + FullBrightTicks) {
				projectile.alpha = 0;

				if (Main.rand.NextBool(6)) {
					var dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 73, 0f, 0f, 200, default, 0.8f);

					dust.velocity *= 0.3f;
				}
			}
			else if (AIFadeProgress < FadeInTicks + FullBrightTicks + FadeOutTicks) {
				projectile.alpha = (int)(255 * (AIFadeProgress - FadeInTicks - FullBrightTicks) / FadeOutTicks);
			}
			else {
				projectile.Center = playerCenter + Main.rand.NextVector2Circular(Range, Range);
				AIFadeProgress = 0f;

				projectile.velocity = 2f * Vector2.Normalize(playerCenter - projectile.Center);
			}

			if (Vector2.Distance(playerCenter, projectile.Center) > RangeHypoteneus) {
				projectile.Center = playerCenter + Main.rand.NextVector2Circular(Range, Range);
				AIFadeProgress = 0f;

				projectile.velocity += 2f * Vector2.Normalize(playerCenter - projectile.Center);
			}

			if ((int)AIFadeProgress % 100 == 0) {
				projectile.velocity = projectile.velocity.RotatedByRandom(MathHelper.ToRadians(90));
			}
		}

		private void UpdateExtraMovement() {
			//Adds some friction to the pet's movement as long as its speed is above 1
			if (projectile.velocity.Length() > 1f) {
				projectile.velocity *= 0.98f;
			}

			//If the pet stops - launch it into a random direction at a low speed.
			if (projectile.velocity == Vector2.Zero) {
				projectile.velocity = Vector2.UnitX.RotatedBy(Main.rand.NextFloat() * MathHelper.TwoPi) * 2f;
			}
		}
	}
}