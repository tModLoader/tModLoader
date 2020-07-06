using ExampleMod.Buffs;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Projectiles
{
	public class HealProj : GlobalProjectile
	{
		public override bool PreAI(Projectile projectile) {
			if (projectile.aiStyle == 52) {
				Player player = Main.player[(int)projectile.ai[0]];
				Vector2 center = new Vector2(projectile.position.X + projectile.width * 0.5f, projectile.position.Y + projectile.height * 0.5f);
				float offsetX = player.Center.X - center.X;
				float offsetY = player.Center.Y - center.Y;
				float distance = (float)Math.Sqrt(offsetX * offsetX + offsetY * offsetY);
				if (distance < 50f && projectile.position.X < player.position.X + player.width && projectile.position.X + projectile.width > player.position.X && projectile.position.Y < player.position.Y + player.height && projectile.position.Y + projectile.height > player.position.Y) {
					if (projectile.owner == Main.myPlayer && !Main.LocalPlayer.moonLeech) {
						int heal = (int)projectile.ai[1];
						int damage = player.statLifeMax2 - player.statLife;
						if (heal > damage) {
							heal = damage;
						}
						if (heal > 0) {
							player.AddBuff(BuffType<Undead2>(), 2 * heal, false);
						}
					}
				}
			}
			return base.PreAI(projectile);
		}
	}
}