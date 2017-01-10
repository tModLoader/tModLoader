using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Projectiles
{
	public class ExampleSpearProjectile : ModProjectile
	{
		public override void SetDefaults()
		{
			projectile.name = "Spear";
			projectile.width = 18;
			projectile.height = 18;
			projectile.aiStyle = 19;
			projectile.friendly = true;
			projectile.penetrate = -1;
			projectile.tileCollide = false;
			projectile.scale = 1.3f;
			projectile.hide = true;
			projectile.ownerHitCheck = true;
			projectile.melee = true;
			projectile.alpha = 0;
		}

		public override void AI()
		{
			Vector2 vector21 = Main.player[projectile.owner].RotatedRelativePoint(Main.player[projectile.owner].MountedCenter, true);
			projectile.direction = Main.player[projectile.owner].direction;
			Main.player[projectile.owner].heldProj = projectile.whoAmI;
			Main.player[projectile.owner].itemTime = Main.player[projectile.owner].itemAnimation;
			projectile.position.X = vector21.X - (float)(projectile.width / 2);
			projectile.position.Y = vector21.Y - (float)(projectile.height / 2);
			if (!Main.player[projectile.owner].frozen)
			{
				if (projectile.ai[0] == 0f)
				{
					projectile.ai[0] = 3f;
					projectile.netUpdate = true;
				}
				if (Main.player[projectile.owner].itemAnimation < Main.player[projectile.owner].itemAnimationMax / 3)
				{
					projectile.ai[0] -= 2.4f;
				}
				else
				{
					projectile.ai[0] += 2.1f;
				}
			}
			projectile.position += projectile.velocity * projectile.ai[0];
			if (Main.player[projectile.owner].itemAnimation == 0)
			{
				projectile.Kill();
			}
			projectile.rotation = (float)Math.Atan2((double)projectile.velocity.Y, (double)projectile.velocity.X) + 2.355f;
			if (projectile.spriteDirection == -1)
			{
				projectile.rotation -= 1.57f;
			}
			if (Main.rand.Next(3) == 0)
			{
				int num260 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, mod.DustType<Dusts.Sparkle>(), projectile.velocity.X * 0.2f, projectile.velocity.Y * 0.2f, 200, default(Color), 1.2f);
				Main.dust[num260].velocity += projectile.velocity * 0.3f;
				Main.dust[num260].velocity *= 0.2f;
			}
			if (Main.rand.Next(4) == 0)
			{
				int num261 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, mod.DustType<Dusts.Smoke>(), 0f, 0f, 254, default(Color), 0.3f);
				Main.dust[num261].velocity += projectile.velocity * 0.5f;
				Main.dust[num261].velocity *= 0.5f;
				return;
			}
		}
	}
}
