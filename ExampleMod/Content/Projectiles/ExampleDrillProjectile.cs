using ExampleMod.Content.Dusts;
using Microsoft.Xna.Framework;
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
			Projectile.hide = true; //Hides the projectile, so it will draw in the player's hand when
		}

		//This code is adapted from aiStyle 20, to use a different dust. If you want to use aiStyle 20, you do not need to override AI.
		public override void AI() {

			Player player = Main.player[Projectile.owner];

			if (Projectile.soundDelay <= 0) {
				SoundEngine.PlaySound(SoundID.Item22, Projectile.Center);
				Projectile.soundDelay = 30;
			}

			Vector2 myPosition = player.RotatedRelativePoint(player.MountedCenter);
			if (player.channel) {
				float velocity = player.HeldItem.shootSpeed * Projectile.scale;
				Projectile.velocity = Projectile.DirectionTo(Main.MouseWorld) * velocity;
			}
			else
				Projectile.Kill();

			if (Projectile.velocity.X > 0f)
				Projectile.direction = 1;
			if (Projectile.velocity.X < 0f)
				Projectile.direction = -1;

			Projectile.spriteDirection = Projectile.direction;
			Projectile.rotation = (Projectile.velocity).ToRotation() + MathHelper.PiOver2;
			player.ChangeDir(Projectile.direction);
			player.heldProj = Projectile.whoAmI;
			player.SetDummyItemTime(2);
			player.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();

			Projectile.Center = myPosition;
			Projectile.velocity.X *= 1f + Main.rand.Next(-3, 4) * 0.01f;

			//Spawning dust
			if (Main.rand.NextBool(15)) {
				Dust dust = Dust.NewDustDirect(Projectile.position + Projectile.velocity * Main.rand.Next(6, 10) * 0.1f, Projectile.width, Projectile.height, ModContent.DustType<Sparkle>(), 0f, 0f, 80, Color.White, 1.4f);
				dust.position.X -= 4f;
				dust.noGravity = true;
				dust.velocity += Projectile.velocity * 0.03f;
			}
		}
	}
}
