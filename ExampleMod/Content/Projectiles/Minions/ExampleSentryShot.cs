using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles.Minions
{
	// The projectile shot by ExampleSentry.
	// Note that this class inherits from Sparkling ball and changes a few properties, but otherwise behaves very similarly
	public class ExampleSentryShot : SparklingBall
	{
		public override string Texture => "ExampleMod/Content/Projectiles/SparklingBall";

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();

			// It is important that projectiles shot by sentries are in this set to properly work with effects that are triggered by sentry attacks.
			ProjectileID.Sets.SentryShot[Type] = true;
		}

		public override void SetDefaults() {
			// Since we are inheriting from SparklingBall, we need to use base.SetDefaults(); 
			base.SetDefaults();

			Projectile.DamageType = DamageClass.Summon;
		}
	}
}
