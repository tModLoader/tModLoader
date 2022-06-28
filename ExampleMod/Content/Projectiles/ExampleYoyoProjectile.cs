using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{
	public class ExampleYoyoProjectile : ModProjectile
	{
		public override void SetStaticDefaults() {
			// The following sets are only applicable to yoyo that use aiStyle 99.
			// YoyosLifeTimeMultiplier is how long in seconds the yoyo will stay out before automatically returning to the player. 
			// Vanilla values range from 3f (Wood) to 16f (Chik), and defaults to -1f. Leaving as -1 will make the time infinite.
			ProjectileID.Sets.YoyosLifeTimeMultiplier[Projectile.type] = 3.5f;
			// YoyosMaximumRange is the maximum distance the yoyo sleep away from the player. 
			// Vanilla values range from 130f (Wood) to 400f (Terrarian), and defaults to 200f.
			ProjectileID.Sets.YoyosMaximumRange[Projectile.type] = 300f;
			// YoyosTopSpeed is top speed of the yoyo Projectile.
			// Vanilla values range from 9f (Wood) to 17.5f (Terrarian), and defaults to 10f.
			ProjectileID.Sets.YoyosTopSpeed[Projectile.type] = 13f;
		}

		public override void SetDefaults() {
			Projectile.extraUpdates = 0;
			Projectile.width = 16; // The width of the projectile.
			Projectile.height = 16; // The height of th projectile.
			Projectile.aiStyle = 99; // aiStyle 99 is used for all yoyos, and is Extremely suggested, as yoyos are extremely difficult without them.
			Projectile.friendly = true; // Player shot projectile. Does damage to enemies but not to friendly Town NPCs.
			Projectile.penetrate = -1; // All vanilla yoyos have infinite penetration. The number of enemies the yoyo can hit before being pulled back in is based on YoyosLifeTimeMultiplier.
			Projectile.DamageType = DamageClass.MeleeNoSpeed; // Benefits from melee bonuses. MeleeNoSpeed means the item will not scale with attack speed.
			Projectile.scale = 1f; // The scale of the projectile. Most yoyos are 1f, but a few are larger. The Kraken is the largest at 1.2f
		}

		// notes for aiStyle 99: 
		// localAI[0] is used for timing up to YoyosLifeTimeMultiplier
		// localAI[1] can be used freely by specific types
		// ai[0] and ai[1] usually point towards the x and y world coordinate hover point
		// ai[0] is -1f once YoyosLifeTimeMultiplier is reached, when the player is stoned/frozen, when the yoyo is too far away, or the player is no longer clicking the shoot button.
		// ai[0] being negative makes the yoyo move back towards the player
		// Any AI method can be used for dust, spawning projectiles, etc specific to your yoyo.

		// Let's add some dust for special effect.
		public override void PostAI() {
			if (Main.rand.NextBool()) {
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Dusts.Sparkle>());
				dust.noGravity = true;
				dust.scale = 1.6f;
			}
		}
	}
}
