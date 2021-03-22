using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Projectiles
{
	public class FlamethrowerProj : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Flamethrower Projectile");     //The English name of the projectile
		}

		public override void SetDefaults()
		{
			projectile.width = 8; //The width of projectile hitbox
			projectile.height = 8; //The height of projectile hitbox
			projectile.alpha = 255; //This makes the projectile invisible, only showing the dust.
			projectile.friendly = true; //Can the projectile deal damage to enemies?
			projectile.hostile = false; //Can the projectile deal damage to the player?
			projectile.penetrate = 3; //How many monsters the projectile can penetrate. Change this to make the flamethrower pierce more mobs.
			projectile.timeLeft = 30; //A short life time for this projectile to get the flamethrower effect
			projectile.ignoreWater = false;
			projectile.tileCollide = true;
		}

		public override void AI()
		{
			/*if (projectile.wet)
			{
				projectile.Kill(); //This kills the projectile when touching water. However, since our projectile is a cursed flame, we will comment this so that it won't run it. If you want to test this, feel free to uncomment this.
			}*/
			for (int k = 0; k < 10; k++) 
			{
				if (Main.rand.Next(5) == 0) 
				{
					Vector2 position = projectile.Center;
					Dust dust = Main.dust[Terraria.Dust.NewDust(position, 16, 16, 75, 0f, 0f, 140, new Color(61, 252, 3), Main.rand.NextFloat(2.6f, 4.4f))];
					dust.noGravity = true;
					//creates lots of dust for the fire effect
				}
			}
		}

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
			target.AddBuff(BuffID.CursedInferno, 240); //Gives cursed flames to target for 4 seconds. (60 = 1 second, 240 = 4 seconds)
        }
    }
}
