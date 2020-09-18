using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{
	public class ExampleGolfBallProjectile : ModProjectile
	{
		public override void SetStaticDefaults() {
			ProjectileID.Sets.IsAGolfBall[Type] = true; // Allows the projectile to be placed on the tee.
			ProjectileID.Sets.TrailingMode[Type] = 0; // Creates a trail behind the golf ball.
			ProjectileID.Sets.TrailCacheLength[Type] = 20; // Sets the length of the trail.
		}

		public override void SetDefaults() {
			projectile.netImportant = true;
			projectile.width = 7;
			projectile.height = 7;
			projectile.friendly = true;
			projectile.penetrate = -1;
			projectile.aiStyle = 149; // 149 is the golf ball AI.
			projectile.tileCollide = false;
		}
	}
}
