using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{

	public class ExampleArrowProjectile : ModProjectile
	{
		public override void SetDefaults() {
			Projectile.width = 10; // The width of projectile hitbox
			Projectile.height = 10; // The height of projectile hitbox

			Projectile.aiStyle = 1; // The ai style of the projectile, please reference the source code of Terraria
			Projectile.friendly = true; // Does the projectile damage enemies?
			Projectile.hostile = false; // Does the projectile damage the player? (town npcs will be affected by this aswell.)
			Projectile.DamageType = DamageClass.Ranged; // Is the projectile shoot by a ranged weapon?
			Projectile.timeLeft = 600; // The live time for the projectile (60 = 1 second, so 600 is 10 seconds)
			Projectile.alpha = 0; // The transparency of the projectile, 255 for completely transparent. (aiStyle 1 quickly fades the projectile in) Remember to delete this if you aren't using an aiStyle that fades in.
			Projectile.ignoreWater = true; // Does the projectiles speed/velocity change whilst underwater?
			Projectile.tileCollide = true; // Can the projectile collide with tiles?
			Projectile.extraUpdates = 1; // Set to above 0 if you want the projectile to update multiple time in a frame

			AIType = ProjectileID.WoodenArrowFriendly; // Makes the projectile behave like the standard arrow
		}


		public override void OnKill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.Dig, Projectile.position); //Plays the basic sound most projectiles make when hitting blocks.
			for (int index1 = 0; index1 < 5; ++index1)//Creates a splash of dust around the position the projectile dies.
			{
				int index2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Silver);
				Main.dust[index2].noGravity = true;
				Main.dust[index2].velocity *= 1.5f;
				Main.dust[index2].scale *= 0.9f;
			}
		}
	}
}