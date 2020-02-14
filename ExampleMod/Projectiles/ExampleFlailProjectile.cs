using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Projectiles
{
	public class ExampleFlailProjectile : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Example Flail Ball");
		}

		public override void SetDefaults()
		{
			projectile.width = 22;
			projectile.height = 22;
			projectile.aiStyle = 15;
			projectile.friendly = true;
			projectile.penetrate = -1;
			projectile.melee = true;
		}

		public override void AI()
		{
			if (Main.player[projectile.owner].dead) {
				projectile.Kill();
				return;
			}

			Main.player[projectile.owner].itemAnimation = 10;
			Main.player[projectile.owner].itemTime = 10;

			if (projectile.position.X + projectile.width / 2 > Main.player[projectile.owner].position.X + Main.player[projectile.owner].width / 2) {
				Main.player[projectile.owner].ChangeDir(1);
				projectile.direction = 1;
			}
			else {
				Main.player[projectile.owner].ChangeDir(-1);
				projectile.direction = -1;
			}

			Vector2 mountedCenter = Main.player[projectile.owner].MountedCenter;
			var position = new Vector2(projectile.position.X + projectile.width * 0.5f, projectile.position.Y + projectile.height * 0.5f);
			float posX = mountedCenter.X - position.X;
			float posY = mountedCenter.Y - position.Y;
			float num = (float)Math.Sqrt(posX * posX + posY * posY);

			if (projectile.ai[0] == 0f) {
				// posibl the length
				float num2 = 360f;
				projectile.tileCollide = true;

				if (num > num2) {
					projectile.ai[0] = 1f;
					projectile.netUpdate = true;
				}
				else if (!Main.player[projectile.owner].channel) {
					if (projectile.velocity.Y < 0f)
						projectile.velocity.Y = projectile.velocity.Y * 0.9f;

					projectile.velocity.Y = projectile.velocity.Y + 1f;
					projectile.velocity.X = projectile.velocity.X * 0.9f;
				}
			}
			else if (projectile.ai[0] == 1f) {
				float num3 = 14f / Main.player[projectile.owner].meleeSpeed;
				float num4 = 0.9f / Main.player[projectile.owner].meleeSpeed;
				float num5 = 300f;
				Math.Abs(posX);
				Math.Abs(posY);
				
				if (projectile.ai[1] == 1f)
					projectile.tileCollide = false;

				if (!Main.player[projectile.owner].channel || num > num5 || !projectile.tileCollide) {
					projectile.ai[1] = 1f;

					if (projectile.tileCollide)
						projectile.netUpdate = true;

					projectile.tileCollide = false;

					if (num < 20f)
						projectile.Kill();
				}

				if (!projectile.tileCollide)
					num4 *= 2f;

				int num6 = 60;

				if (num > num6 || !projectile.tileCollide) {
					num = num3 / num;
					posX *= num;
					posY *= num;
					new Vector2(projectile.velocity.X, projectile.velocity.Y);
					float num7 = posX - projectile.velocity.X;
					float num8 = posY - projectile.velocity.Y;
					float num9 = (float)Math.Sqrt(num7 * num7 + num8 * num8);
					num9 = num4 / num9;
					num7 *= num9;
					num8 *= num9;
					projectile.velocity.X = projectile.velocity.X * 0.98f;
					projectile.velocity.Y = projectile.velocity.Y * 0.98f;
					projectile.velocity.X = projectile.velocity.X + num7;
					projectile.velocity.Y = projectile.velocity.Y + num8;
				}
				else {
					if (Math.Abs(projectile.velocity.X) + Math.Abs(projectile.velocity.Y) < 6f) {
						projectile.velocity.X = projectile.velocity.X * 0.96f;
						projectile.velocity.Y = projectile.velocity.Y + 0.2f;
					}

					if (Main.player[projectile.owner].velocity.X == 0f)
						projectile.velocity.X = projectile.velocity.X * 0.96f;
				}
			}

			projectile.rotation = (float)Math.Atan2(posY, posX) - projectile.velocity.X * 0.1f;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Vector2 mountedCenter = Main.player[projectile.owner].MountedCenter;
			Texture2D chainTexture = ModContent.GetTexture("ExampleMod/Projectiles/ExampleFlailProjectileChain");
			var position = new Vector2(projectile.position.X + projectile.width * 0.5f, projectile.position.Y + projectile.height * 0.5f);
			float posX = mountedCenter.X - position.X;
			float posY = mountedCenter.Y - position.Y;
			float rotation = (float)Math.Atan2(posY, posX) - 1.57f;

			if (projectile.alpha == 0) {
				int num = -1;

				if (projectile.position.X + projectile.width / 2 < mountedCenter.X)
					num = 1;

				if (Main.player[projectile.owner].direction == 1)
					Main.player[projectile.owner].itemRotation = (float)Math.Atan2(posY * num, posX * num);
				else
					Main.player[projectile.owner].itemRotation = (float)Math.Atan2(posY * num, posX * num);
			}

			bool flag = true;

			while (flag) {
				float num2 = (float)Math.Sqrt(posX * posX + posY * posY);

				if (num2 < 25f)
					flag = false;
				else if (float.IsNaN(num2))
					flag = false;
				else {
					num2 = 12f / num2;
					posX *= num2;
					posY *= num2;
					position.X += posX;
					position.Y += posY;
					posX = mountedCenter.X - position.X;
					posY = mountedCenter.Y - position.Y;

					Color color = Lighting.GetColor((int)position.X / 16, (int)(position.Y / 16f));

					Main.spriteBatch.Draw(chainTexture,
						   new Vector2(position.X - Main.screenPosition.X, position.Y - Main.screenPosition.Y),
						   new Rectangle?(new Rectangle(0, 0, chainTexture.Width, chainTexture.Height)),
						   color,
						   rotation,
						   new Vector2(chainTexture.Width * 0.5f, chainTexture.Height * 0.5f),
						   1f,
						   SpriteEffects.None,
						   0f);
				}
			}

			return true;
		}
	}
}