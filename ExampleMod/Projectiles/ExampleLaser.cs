using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace ExampleMod.Projectiles
{
	class ExampleLaser : ModProjectile
	{
		Vector2 _targetPos;         //Ending position of the laser beam
		int _charge;                //The charge level of the weapon
		float _moveDis = 95f;       //The distance charge particle from the player center

		public override void SetDefaults()
		{
			projectile.name = "Example Laser";
			projectile.width = 10;
			projectile.height = 10;
			projectile.friendly = true;
			projectile.penetrate = -1;
			projectile.tileCollide = false;
			projectile.magic = true;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			if (_charge == 100)
			{
				Vector2 unit = _targetPos - Main.player[projectile.owner].Center;
				unit.Normalize();
				DrawLaser(spriteBatch, Main.projectileTexture[projectile.type], Main.player[projectile.owner].Center, unit, 5, projectile.damage, -1.57f, 1, 1000, Color.White, 95);
			}
			return false;

		}

		/// <summary>
		/// The core function of drawing a laser
		/// </summary>
		public void DrawLaser(SpriteBatch sb, Texture2D tex, Vector2 start, Vector2 unit, float step, int damage, float rotation = 0f, float scale = 1f, float maxDis = 2000f, Color color = default(Color), int transdis = 50)
		{
			Vector2 orig = start;
			float r = unit.ToRotation() + rotation;
			for (float i = transdis; i <= _moveDis; i += step)
			{
				Color c = Color.White;
				orig = start + i * unit;
				sb.Draw(tex, orig - Main.screenPosition,
					new Rectangle(0, 26, 28, 26), i < transdis ? Color.Transparent : c, r,
					new Vector2(28 / 2, 26 / 2), scale, 0, 0);
			}
			//Draw laser head
			sb.Draw(tex, start + unit * (transdis - step) - Main.screenPosition,
				new Rectangle(0, 0, 28, 26), Color.White, r, new Vector2(28 / 2, 26 / 2), scale, 0, 0);
			//Draw laser tail
			sb.Draw(tex, start + (_moveDis + step) * unit - Main.screenPosition,
				new Rectangle(0, 52, 28, 26), Color.White, r, new Vector2(28 / 2, 26 / 2), scale, 0, 0);
		}

		/// <summary>
		/// Change the way of collision check of the projectile
		/// </summary>
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (_charge == 100)
			{
				Player p = Main.player[projectile.owner];
				Vector2 unit = (Main.player[projectile.owner].Center - _targetPos);
				unit.Normalize();
				float point = 0f;
				if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), p.Center - 95f * unit, p.Center - unit * _moveDis, 22, ref point))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Change the behavior after hit a NPC
		/// </summary>
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.immune[projectile.owner] = 5;
		}

		public override void AI()
		{
			Vector2 mousePos = Main.screenPosition + new Vector2(Main.mouseX, Main.mouseY);
			Player p = Main.player[projectile.owner];
			#region Set projectile position
			Vector2 diff = mousePos - p.Center;
			diff.Normalize();
			projectile.position = p.Center + diff * _moveDis;
			projectile.timeLeft = 2;
			int dir = projectile.position.X > p.position.X ? 1 : -1;
			p.ChangeDir(dir);
			p.heldProj = projectile.whoAmI;
			p.itemTime = 2;
			p.itemAnimation = 2;
			p.itemRotation = (float)Math.Atan2(diff.Y * dir, diff.X * dir);
			#endregion


			#region Charging process
			if (!p.channel)             //If player is not firing the weapon
			{
				projectile.Kill();
			}
			else
			{
				if (Main.time % 10 < 1 && !p.CheckMana(p.inventory[p.selectedItem].mana, true))
				{
					projectile.Kill();
				}
				Vector2 offset = mousePos - p.Center;
				offset.Normalize();
				offset *= _moveDis - 20;
				Vector2 dustPos = p.Center + offset - new Vector2(10, 10);
				if (_charge < 100)
				{
					_charge++;
				}
				int chargeFact = _charge / 20;
				Vector2 dustVelocity = Vector2.UnitX * 18f;
				dustVelocity = dustVelocity.RotatedBy(projectile.rotation - 1.57f, default(Vector2));
				Vector2 spawnPos = projectile.Center + dustVelocity;
				for (int k = 0; k < chargeFact + 1; k++)
				{
					Vector2 spawn = spawnPos + ((float)Main.rand.NextDouble() * 6.28f).ToRotationVector2() * (12f - (chargeFact * 2));
					Dust dust = Main.dust[Dust.NewDust(dustPos, 20, 20, 226, projectile.velocity.X / 2f,
						projectile.velocity.Y / 2f, 0, default(Color), 1f)];
					dust.velocity = Vector2.Normalize(spawnPos - spawn) * 1.5f * (10f - chargeFact * 2f) / 10f;
					dust.noGravity = true;
					dust.scale = Main.rand.Next(10, 20) * 0.05f;
				}
			}
			#endregion


			#region Set laser tail position and dusts
			if (_charge < 100) return;
			Vector2 start = p.Center;
			Vector2 unit = (p.Center - mousePos);
			unit.Normalize();
			unit *= -1;
			for (_moveDis = 95f; _moveDis <= 2200; _moveDis += 5)
			{
				start = p.Center + unit * _moveDis;
				if (!Collision.CanHit(p.Center, 1, 1, start, 1, 1))
				{
					_moveDis -= 5f;
					break;
				}
			}
			_targetPos = p.Center + unit * _moveDis;

			//Imported dust code from source because I'm lazy
			for (int i = 0; i < 2; ++i)
			{
				float num1 = projectile.velocity.ToRotation() + (Main.rand.Next(2) == 1 ? -1.0f : 1.0f) * 1.57f;
				float num2 = (float)(Main.rand.NextDouble() * 0.8f + 1.0f);
				Vector2 dustVel = new Vector2((float)Math.Cos(num1) * num2, (float)Math.Sin(num1) * num2);
				Dust dust = Main.dust[Dust.NewDust(_targetPos, 0, 0, 226, dustVel.X, dustVel.Y, 0, new Color(), 1f)];
				dust.noGravity = true;
				dust.scale = 1.2f;
			}
			if (Main.rand.Next(5) == 0)
			{
				Vector2 offset = projectile.velocity.RotatedBy(1.57f, new Vector2()) * ((float)Main.rand.NextDouble() - 0.5f) * projectile.width;
				Dust dust = Main.dust[Dust.NewDust(_targetPos + offset - Vector2.One * 4f, 8, 8, 31, 0.0f, 0.0f, 100, new Color(), 1.5f)];
				dust.velocity = dust.velocity * 0.5f;
				dust.velocity.Y = -Math.Abs(dust.velocity.Y);
			}
			#endregion
		}
	}
}
