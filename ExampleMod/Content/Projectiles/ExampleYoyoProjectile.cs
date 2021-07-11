using ExampleMod.Content.Dusts;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{
	public class ExampleYoyoProjectile : ModProjectile
	{
		public override void SetStaticDefaults() {
			// The following sets are only applicable to yoyos that use aiStyle 99.
			ProjectileID.Sets.YoyosLifeTimeMultiplier[Projectile.type] = 18f; //The amount of time that the yoyo can be held out, in seconds.
		    //Vanilla values range from 3f(Wood) to 16f(Chik), and defaults to -1f. Leaving it as -1 will make the time infinite.
																			  
			ProjectileID.Sets.YoyosMaximumRange[Projectile.type] = 300f; //The maximum distance the yoyo sleep away from the player.
			// Vanilla values range from 130f(Wood) to 400f(Terrarian), and defaults to 200f.
																		
			ProjectileID.Sets.YoyosTopSpeed[Projectile.type] = 17f; //The top speed of the yoyo.
			// Vanilla values range from 9f(Wood) to 17.5f(Terrarian), and defaults to 10f.
		}

		public override void SetDefaults() {
			Projectile.width = 16; //The width of the projectile's hitbox.
			Projectile.height = 16; //The height of the projectile's hitbox.

			Projectile.aiStyle = 99; //The projectile's ai style. Yoyos use aiStyle 99.

			Projectile.friendly = true; //Makes the projectile deal damage to enemies.
			Projectile.DamageType = DamageClass.Melee; //Sets the item's damage type to melee.
			Projectile.penetrate = -1; //The amount of enemies the projectile can hit before disappearing. Setting it to -1 makes it infinite.
		}

		// notes for aiStyle 99: 
		// localAI[0] is used for timing up to YoyosLifeTimeMultiplier
		// localAI[1] can be used freely by specific types
		// ai[0] and ai[1] usually point towards the x and y world coordinate hover point
		// ai[0] is -1f once YoyosLifeTimeMultiplier is reached, when the player is stoned/frozen, when the yoyo is too far away, or the player is no longer clicking the shoot button.
		// ai[0] being negative makes the yoyo move back towards the player
		// Any AI method can be used for dust, spawning projectiles, etc specific to your yoyo.

		public override void PostAI() {
			if (Main.rand.NextBool(5)) {
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Sparkle>()); //Makes the projectile emit dust.
			}
		}
	}
}
