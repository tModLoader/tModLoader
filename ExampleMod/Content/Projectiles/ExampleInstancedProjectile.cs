using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{
	public class ExampleInstancedProjectile : ModProjectile
	{
		private Color trailColor;

		public override void SetDefaults() {
			Projectile.width = 16; // The width of projectile hitbox
			Projectile.height = 16; // The height of projectile hitbox
			Projectile.aiStyle = ProjAIStyleID.Arrow; // The ai style of the projectile, please reference the source code of Terraria
			Projectile.friendly = true; // Can the projectile deal damage to enemies?
			Projectile.hostile = false; // Can the projectile deal damage to the player?
			Projectile.DamageType = DamageClass.Ranged; // Is the projectile shoot by a ranged weapon?
			Projectile.ignoreWater = true; // Does the projectile's speed be influenced by water?
			Projectile.tileCollide = true; // Can the projectile collide with tiles?

			AIType = ProjectileID.Bullet; // Act exactly like default Bullet
		}

		public override bool PreDraw(ref Color lightColor) {
			Dust.NewDustPerfect(Projectile.Center, DustID.TintableDust, newColor: trailColor);

			return false;
		}

		public override void OnSpawn(IEntitySource data) {
			trailColor = Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.7f); // Assign a random color on spawn
		}
	}
}