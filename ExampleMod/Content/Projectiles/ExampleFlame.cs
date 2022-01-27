 Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace ExampleMod.Content.Projectile
{
    public class ExampleFlame : ModProjectile
    {
		public override string Texture => "Terraria/Projectile_" + ProjectileID.Flames;
		public override void SetDefaults()
		{
			Projectile.width = 6;
			Projectile.height = 6;
			Projectile.alpha = 255;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 60;
		    Projectile.ignoreWater = false;
			Projectile.tileCollide = true;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.extraUpdates = 2;
		}
		public override void AI()
		{
			/*if (projectile.wet)
				{
					projectile.Kill(); //This kills the projectile when touching water. However, since our projectile is a cursed flame, we will comment this so that it won't run it. If you want to test this, feel free to uncomment this.
				}*/
			// Using a timer, we scale the earliest spawned dust smaller than the rest.
			float dustScale = 0.75f;
			if (Projectile.ai[0] == 0f)
			{
				dustScale = 0.3f;
			}
			else if (Projectile.ai[0] == 1f)
			{
				dustScale = 0.5f;
			}
			else if (Projectile.ai[0] == 2f)
			{
				dustScale = 0.5f;
			}
			// Some dust will be large, the others small and with gravity, to give visual variety.
			if (Main.rand.Next(2) == 0)
			{
				int dusts = 2;
				for (int i = 0; i < dusts; i++)
				{
					Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.CursedTorch, Projectile.velocity.X * 0.3f, Projectile.velocity.Y * 0.3f, 100, default(Color), 1f);
					if (Utils.NextBool(Main.rand, 3))
					{
						dust.noGravity = true;
						dust.scale *= 3f;
						Dust dust2 = dust;
						dust2.velocity.X = dust2.velocity.X * 2f;
						Dust dust3 = dust;
						dust3.velocity.Y = dust3.velocity.Y * 2f;
					}
					dust.scale *= 2.5f;
					dust.velocity *= 1.5f;
					dust.scale *= dustScale;
				}
			}
			Projectile.ai[0] += 1f;
		}
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
			target.AddBuff(BuffID.CursedInferno, 300);
        }
        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
			target.AddBuff(BuffID.CursedInferno, 300);
        }
    }
}