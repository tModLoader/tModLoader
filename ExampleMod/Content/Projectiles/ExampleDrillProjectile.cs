using ExampleMod.Content.Dusts;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{
	public class ExampleDrillProjectile : ModProjectile
	{
		public override void SetDefaults() {
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.ownerHitCheck = true;
			Projectile.aiStyle = -1; //Replace with 20 if you do not want custom code
			Projectile.hide = true; //Hides the projectile, so it will draw in the player's hand when we set the player's heldProj to this one.
		}

		//This code is adapted from aiStyle 20 to use a different dust and more noises. If you want to use aiStyle 20, you do not need to do any of this.
		//It should be noted that this projectile has no effect on mining and is mostly visual.
		public override void AI() {

			Player player = Main.player[Projectile.owner];

			Projectile.timeLeft = 60;

			//Plays a sound every 20 ticks. In aiStyle 20, soundDelay is set to 30 ticks.
			if (Projectile.soundDelay <= 0) {
				SoundEngine.PlaySound(SoundID.Item22, Projectile.Center);
				Projectile.soundDelay = 20;
			}

			Vector2 myPosition = player.RotatedRelativePoint(player.MountedCenter);
			if (player.channel) {
				float num178 = player.inventory[player.selectedItem].shootSpeed * Projectile.scale;
				float newVelocityX = Main.mouseX + Main.screenPosition.X - myPosition.X;
				float newVelocityY = Main.mouseY + Main.screenPosition.Y - myPosition.Y;
				if (player.gravDir == -1f)
					newVelocityY = Main.screenHeight - Main.mouseY + Main.screenPosition.Y - myPosition.Y;

				float num181 = (float)Math.Sqrt(newVelocityX * newVelocityX + newVelocityY * newVelocityY);
				num181 = num178 / num181;
				newVelocityX *= num181;
				newVelocityY *= num181;
				if (newVelocityX != Projectile.velocity.X || newVelocityY != Projectile.velocity.Y)
					Projectile.netUpdate = true;

				Projectile.velocity.X = newVelocityX;
				Projectile.velocity.Y = newVelocityY;
			}
			else
				Projectile.Kill();

			if (Projectile.velocity.X > 0f)
				player.ChangeDir(1);
			else if (Projectile.velocity.X < 0f)
				player.ChangeDir(-1);

			Projectile.spriteDirection = Projectile.direction;
			player.ChangeDir(Projectile.direction); //Change the player's direction based on the projectile's own
			player.heldProj = Projectile.whoAmI; //We tell the player that the drill is the held projectile, so it will draw in their hand
			player.SetDummyItemTime(2); //Make sure the player's item time does not change while the projectile is out
			Projectile.Center = myPosition;
			Projectile.rotation = (Projectile.velocity).ToRotation() + MathHelper.PiOver2;
			player.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();

			Projectile.velocity.X *= 1f + Main.rand.Next(-3, 4) * 0.01f;

			//Spawning dust
			if (Main.rand.NextBool(10)) {
				Dust dust = Dust.NewDustDirect(Projectile.position + Projectile.velocity * Main.rand.Next(6, 10) * 0.15f, Projectile.width, Projectile.height, ModContent.DustType<Sparkle>(), 0f, 0f, 80, Color.White, 1f);
				dust.position.X -= 4f;
				dust.noGravity = true;
				dust.velocity.X *= 0.5f;
				dust.velocity.Y = -Main.rand.Next(3, 8) * 0.1f;
			}
		}
	}
}
