using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{
	public class ExampleBobber : ModProjectile
	{
		public override void SetDefaults() {
			// These are copied through the CloneDefaults method
			// Projectile.width = 14;
			// Projectile.height = 14;
			// Projectile.aiStyle = 61;
			// Projectile.bobber = true;
			// Projectile.penetrate = -1;
			// Projectile.netImportant = true;
			Projectile.CloneDefaults(ProjectileID.BobberWooden);

			DrawOriginOffsetY = -8; // Adjusts the draw position
		}

		public override void AI() {
			// Always ensure that graphics-related code doesn't run on dedicated servers via this check.
			if (!Main.dedServ) {
				// Create some light.
				Lighting.AddLight(Projectile.Center, Main.DiscoColor.ToVector3());
			}
		}
	}
}