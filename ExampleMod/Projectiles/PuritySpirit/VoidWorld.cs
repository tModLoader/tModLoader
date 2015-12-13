using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Projectiles.PuritySpirit
{
	public class VoidWorld : ModProjectile
	{
		public override void SetDefaults()
		{
			projectile.name = "Void World";
			projectile.width = 80;
			projectile.height = 80;
			projectile.penetrate = -1;
			projectile.magic = true;
			projectile.tileCollide = false;
			projectile.ignoreWater = true;
			Main.projFrames[projectile.type] = 5;
			cooldownSlot = 1;
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(projectile.localAI[0]);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			projectile.localAI[0] = reader.ReadSingle();
		}

		public override void AI()
		{
			projectile.ai[0] += 1f;
			if (projectile.ai[0] < 180f)
			{
				projectile.alpha = (int)((180f - projectile.ai[0]) * 255f / 180f);
			}
			else
			{
				projectile.alpha = 0;
			}
			if (projectile.ai[0] == 180f)
			{
				if (!Main.dedServ && projectile.localAI[0] == 1f)
				{
					ExamplePlayer modPlayer = (ExamplePlayer)Main.player[Main.myPlayer].GetModPlayer(mod, "ExamplePlayer");
					if (modPlayer.heroLives > 0)
					{
						Main.PlaySound(2, -1, -1, 14);
					}
					else
					{
						Main.PlaySound(2, (int)projectile.position.X, (int)projectile.position.Y, 14);
					}
				}
				projectile.hostile = true;
				projectile.frame = 4;
			}
			if (projectile.ai[0] >= 185f)
			{
				projectile.hostile = false;
			}
			if (projectile.ai[0] >= 200f)
			{
				projectile.Kill();
			}
			projectile.rotation += -2f * (float)Math.PI / 60f * projectile.ai[1];
			projectile.spriteDirection = (int)projectile.ai[1];
			if (projectile.frame < 4)
			{
				projectile.frameCounter++;
				if (projectile.frameCounter >= 8)
				{
					projectile.frameCounter = 0;
					projectile.frame++;
					projectile.frame %= 4;
				}
			}
		}

		public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
		{
			if (target.hurtCooldowns[1] <= 0)
			{
				ExamplePlayer modPlayer = (ExamplePlayer)target.GetModPlayer(mod, "ExamplePlayer");
				modPlayer.constantDamage = projectile.damage;
				modPlayer.percentDamage = Main.expertMode ? 1.2f : 1f;
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 ellipseCenter = new Vector2(projHitbox.X, projHitbox.Y) + 0.5f * new Vector2(projHitbox.Width, projHitbox.Height);
			float x = 0f;
			float y = 0f;
			if (targetHitbox.X > ellipseCenter.X)
			{
				x = targetHitbox.X - ellipseCenter.X;
			}
			else if (targetHitbox.X + targetHitbox.Width < ellipseCenter.X)
			{
				x = targetHitbox.X + targetHitbox.Width - ellipseCenter.X;
			}
			if (targetHitbox.Y > ellipseCenter.Y)
			{
				y = targetHitbox.Y - ellipseCenter.Y;
			}
			else if (targetHitbox.Y + targetHitbox.Height < ellipseCenter.Y)
			{
				y = targetHitbox.Y + targetHitbox.Height - ellipseCenter.Y;
			}
			float a = projHitbox.Width / 2f;
			float b = projHitbox.Height / 2f;
			return (x * x) / (a * a) + (y * y) / (b * b) < 1;
		}

		public override Color? GetAlpha(Color lightColor)
		{
			return Color.White * ((255 - projectile.alpha / 2) / 255f);
		}
	}
}