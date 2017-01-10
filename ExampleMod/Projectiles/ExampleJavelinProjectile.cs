using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Projectiles
{
	public class ExampleJavelinProjectile : ModProjectile
	{
		public override void SetDefaults()
		{
			projectile.name = "Javelin";
			projectile.width = 16;
			projectile.height = 16;
			projectile.aiStyle = -1;
			projectile.friendly = true;
			projectile.melee = true;
			projectile.penetrate = 3;
		}

		public override void TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
		{
			width = 10;
			height = 10;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (targetHitbox.Width > 8 && targetHitbox.Height > 8)
			{
				targetHitbox.Inflate(-targetHitbox.Width / 8, -targetHitbox.Height / 8);
			}
			return projHitbox.Intersects(targetHitbox);
		}

		public override void Kill(int timeLeft)
		{
			Main.PlaySound(0, (int)projectile.position.X, (int)projectile.position.Y, 1, 1f, 0f);
			Vector2 vector11 = projectile.position;
			Vector2 value23 = (projectile.rotation - 1.57079637f).ToRotationVector2();
			vector11 += value23 * 16f;
			for (int num356 = 0; num356 < 20; num356++)
			{
				int num357 = Dust.NewDust(vector11, projectile.width, projectile.height, 81, 0f, 0f, 0, default(Color), 1f);
				Main.dust[num357].position = (Main.dust[num357].position + projectile.Center) / 2f;
				Main.dust[num357].velocity += value23 * 2f;
				Main.dust[num357].velocity *= 0.5f;
				Main.dust[num357].noGravity = true;
				vector11 -= value23 * 8f;
			}
			int item = 0;
			if (Main.rand.NextFloat() <= 0.18f)
			{
				item = Item.NewItem((int)projectile.position.X, (int)projectile.position.Y, projectile.width, projectile.height, 3378, 1, false, 0, false, false);
			}
			if (Main.netMode == 1 && item >= 0)
			{
				NetMessage.SendData(21, -1, -1, "", item, 1f, 0f, 0f, 0, 0, 0);
			}
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			projectile.ai[0] = 1f;
			projectile.ai[1] = (float)target.whoAmI;
			projectile.velocity = (target.Center - projectile.Center) * 0.75f;
			projectile.netUpdate = true;
			target.AddBuff(169, 900, false);

			projectile.damage = 0;
			int num27 = 6;
			if (projectile.type == 614)
			{
				num27 = 10;
			}
			if (projectile.type == 636)
			{
				num27 = 8;
			}
			Point[] array2 = new Point[num27];
			int num28 = 0;
			for (int l = 0; l < 1000; l++)
			{
				if (l != projectile.whoAmI && Main.projectile[l].active && Main.projectile[l].owner == Main.myPlayer && Main.projectile[l].type == projectile.type && Main.projectile[l].ai[0] == 1f && Main.projectile[l].ai[1] == (float)target.whoAmI)
				{
					array2[num28++] = new Point(l, Main.projectile[l].timeLeft);
					if (num28 >= array2.Length)
					{
						break;
					}
				}
			}
			if (num28 >= array2.Length)
			{
				int num29 = 0;
				for (int m = 1; m < array2.Length; m++)
				{
					if (array2[m].Y < array2[num29].Y)
					{
						num29 = m;
					}
				}
				Main.projectile[array2[num29].X].Kill();
			}
		}

		public override void AI()
		{
			int num972 = 25;
			if (projectile.alpha > 0)
			{
				projectile.alpha -= num972;
			}
			if (projectile.alpha < 0)
			{
				projectile.alpha = 0;
			}
			if (projectile.ai[0] == 0f)
			{
				projectile.ai[1] += 1f;
				if (projectile.ai[1] >= 45f)
				{
					float num975 = 0.98f;
					float num976 = 0.35f;
					if (projectile.type == 636)
					{
						num975 = 0.995f;
						num976 = 0.15f;
					}
					projectile.ai[1] = 45f;
					projectile.velocity.X = projectile.velocity.X * num975;
					projectile.velocity.Y = projectile.velocity.Y + num976;
				}
				projectile.rotation = projectile.velocity.ToRotation() + 1.57079637f;
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
			if (projectile.ai[0] == 1f)
			{
				projectile.ignoreWater = true;
				projectile.tileCollide = false;
				int num977 = 15;
				bool flag53 = false;
				bool flag54 = false;
				projectile.localAI[0] += 1f;
				if (projectile.localAI[0] % 30f == 0f)
				{
					flag54 = true;
				}
				int num978 = (int)projectile.ai[1];
				if (projectile.localAI[0] >= (float)(60 * num977))
				{
					flag53 = true;
				}
				else if (num978 < 0 || num978 >= 200)
				{
					flag53 = true;
				}
				else if (Main.npc[num978].active && !Main.npc[num978].dontTakeDamage)
				{
					projectile.Center = Main.npc[num978].Center - projectile.velocity * 2f;
					projectile.gfxOffY = Main.npc[num978].gfxOffY;
					if (flag54)
					{
						Main.npc[num978].HitEffect(0, 1.0);
					}
				}
				else
				{
					flag53 = true;
				}
				if (flag53)
				{
					projectile.Kill();
				}
			}
		}
	}
}
