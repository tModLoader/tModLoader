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
			Projectile.netImportant = true; // Indicates that this projectile will be synced to a joining player (by default, any projectiles active before the player joins (besides pets) are not synced over).
			Projectile.width = 7; // The width of the projectile's hitbox.
			Projectile.height = 7; // The height of the projectile's hitbox.
			Projectile.friendly = true; // Setting this to anything other than true causes an index out of bounds error.
			Projectile.penetrate = -1; // Number of times the projectile can penetrate enemies. -1 sets it to infinite penetration.
			Projectile.aiStyle = ProjAIStyleID.GolfBall; // The aiStyle used by golf balls.
			Projectile.tileCollide = false; // Tile Collision is set to false, as it's handled in the AI.
		}
	}
}