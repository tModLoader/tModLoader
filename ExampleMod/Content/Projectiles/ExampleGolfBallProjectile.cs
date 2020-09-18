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
			projectile.netImportant = true; // Indicates that this projectile will be synced to a joining player (by default, any projectiles active before the player joins (besides pets) are not synced over)
			projectile.width = 7; // the width of the projectile's hitbox
			projectile.height = 7; // the height of the projectile's hitbox
			projectile.friendly = true; // Can the projectile damage enemies?
			projectile.penetrate = -1; // Can the projectile penetrate enemies? -1 sets it to infinite penetration.
			projectile.aiStyle = 149; // 149 is the golf ball AI.
			projectile.tileCollide = false; // Can the projectile collide with tiles?
		}
	}
}
