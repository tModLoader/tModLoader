using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Enums;

namespace ExampleMod.Projectiles
{
	public class ExampleLaser : ModProjectile
	{
		private const int maxCharge = 50;
		private Vector2 _targetPos;         //Ending position of the laser beam
		private int _charge;                //The charge level of the weapon
		private float _moveDist = 60f;       //The distance charge particle from the player center

		public override void SetDefaults()
		{
			projectile.name = "Example Laser";
			projectile.width = 10;
			projectile.height = 10;
			projectile.friendly = true;
			projectile.penetrate = -1;
			projectile.tileCollide = false;
			projectile.magic = true;
			projectile.hide = true;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			if (_charge == maxCharge)
			{
				Vector2 unit = _targetPos - Main.player[projectile.owner].Center;
				unit.Normalize();
				DrawLaser(spriteBatch, Main.projectileTexture[projectile.type], Main.player[projectile.owner].Center, unit, 5, projectile.damage, -1.57f, 1f, 1000f, Color.White, (int)60);
			}
			return false;

		}

		/// <summary>
		/// The core function of drawing a laser
		/// </summary>
		public void DrawLaser(SpriteBatch spriteBatch, Texture2D texture, Vector2 start, Vector2 unit, float step, int damage, float rotation = 0f, float scale = 1f, float maxDist = 2000f, Color color = default(Color), int transDist = 50)
		{
			Vector2 origin = start;
			float r = unit.ToRotation() + rotation;

			#region Draw laser body
			for (float i = transDist; i <= _moveDist; i += step)
			{
				Color c = Color.White;
				origin = start + i * unit;
				spriteBatch.Draw(texture, origin - Main.screenPosition,
					new Rectangle(0, 26, 28, 26), i < transDist ? Color.Transparent : c, r,
					new Vector2(28 / 2, 26 / 2), scale, 0, 0);
			}
			#endregion

			#region Draw laser tail
			spriteBatch.Draw(texture, start + unit * (transDist - step) - Main.screenPosition,
				new Rectangle(0, 0, 28, 26), Color.White, r, new Vector2(28 / 2, 26 / 2), scale, 0, 0);
			#endregion

			#region Draw laser head
			spriteBatch.Draw(texture, start + (_moveDist + step) * unit - Main.screenPosition,
				new Rectangle(0, 52, 28, 26), Color.White, r, new Vector2(28 / 2, 26 / 2), scale, 0, 0);
			#endregion
		}

		/// <summary>
		/// Change the way of collision check of the projectile
		/// </summary>
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (_charge == maxCharge)
			{
				Player p = Main.player[projectile.owner];
				Vector2 unit = (Main.player[projectile.owner].Center - _targetPos);
				unit.Normalize();
				float point = 0f;
				if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), p.Center - 95f * unit, p.Center - unit * _moveDist, 22, ref point))
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

		/// <summary>
		/// The AI of the projectile
		/// </summary>
		public override void AI()
		{
			
			Vector2 mousePos = Main.MouseWorld;
			Player player = Main.player[projectile.owner];

			#region Set projectile position
			if (projectile.owner == Main.myPlayer) // Multiplayer support
			{
				Vector2 diff = mousePos - player.Center;
				diff.Normalize();
				projectile.position = player.Center + diff * _moveDist;
				projectile.timeLeft = 2;
				int dir = projectile.position.X > player.position.X ? 1 : -1;
				player.ChangeDir(dir);
				player.heldProj = projectile.whoAmI;
				player.itemTime = 2;
				player.itemAnimation = 2;
				player.itemRotation = (float)Math.Atan2(diff.Y * dir, diff.X * dir);
				#endregion
			}


			#region Charging process
			// Kill the projectile if the player stops channeling
			if (!player.channel)
			{
				projectile.Kill();
			}
			else
			{
				if (Main.time % 10 < 1 && !player.CheckMana(player.inventory[player.selectedItem].mana, true))
				{
					projectile.Kill();
				}
				Vector2 offset = mousePos - player.Center;
				offset.Normalize();
				offset *= _moveDist - 20;
				Vector2 dustPos = player.Center + offset - new Vector2(10, 10);
				if (_charge < maxCharge)
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
			if (_charge < maxCharge) return;
			Vector2 start = player.Center;
			Vector2 unit = (player.Center - mousePos);
			unit.Normalize();
			unit *= -1;
			for (_moveDist = 95f; _moveDist <= 2200; _moveDist += 5)
			{
				start = player.Center + unit * _moveDist;
				if (!Collision.CanHit(player.Center, 1, 1, start, 1, 1))
				{
					_moveDist -= 5f;
					break;
				}
			}
			_targetPos = player.Center + unit *  _moveDist;

			//Imported dust code from source because I'm lazy
			for (int i = 0; i < 2; ++i)
			{
				float num1 = projectile.velocity.ToRotation() + (Main.rand.Next(2) == 1 ? -1.0f : 1.0f) * 1.57f;
				float num2 = (float)(Main.rand.NextDouble() * 0.8f + 1.0f);
				Vector2 dustVel = new Vector2((float)Math.Cos(num1) * num2, (float)Math.Sin(num1) * num2);
				Dust dust = Main.dust[Dust.NewDust(_targetPos, 0, 0, 226, dustVel.X, dustVel.Y, 0, new Color(), 1f)];
				dust.noGravity = true;
				dust.scale = 1.2f;
				// At this part, I was messing with the dusts going across the laser beam very fast, but only really works properly horizontally now
				unit = Main.MouseWorld - Main.player[projectile.owner].Center;
				unit.Normalize();
				unit *= (projectile.width + projectile.height) / 2f;
				dust = Main.dust[Dust.NewDust(Main.player[projectile.owner].Center + unit * (5), 0,0, 226, unit.X, unit.Y, 0, new Color(), 1f)];
				dust.fadeIn = 0f;
				dust.noGravity = true;
				dust.scale = 0.88f;
			}
			if (Main.rand.Next(5) == 0)
			{
				Vector2 offset = projectile.velocity.RotatedBy(1.57f, new Vector2()) * ((float)Main.rand.NextDouble() - 0.5f) * projectile.width;
				Dust dust = Main.dust[Dust.NewDust(_targetPos + offset - Vector2.One * 4f, 8, 8, 31, 0.0f, 0.0f, 100, new Color(), 1.5f)];
				dust.velocity = dust.velocity * 0.5f;
				dust.velocity.Y = -Math.Abs(dust.velocity.Y);

			    unit = _targetPos - Main.player[projectile.owner].Center;
				unit.Normalize();
				dust = Main.dust[Dust.NewDust(Main.player[projectile.owner].Center + 55 * unit, 8, 8, 31, 0.0f, 0.0f, 100, new Color(), 1.5f)];
				dust.velocity = dust.velocity * 0.5f;
				dust.velocity.Y = -Math.Abs(dust.velocity.Y);
			}
			#endregion
		}

		public override bool ShouldUpdatePosition()
		{
			return false;
		}

		public override void CutTiles()
		{
			DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
			Vector2 unit = (Main.player[projectile.owner].Center - _targetPos);
			unit.Normalize();
			Utils.PlotTileLine(projectile.Center, projectile.Center - unit * 5f, (float)(projectile.width + 16) * projectile.scale, new Utils.PerLinePoint(DelegateMethods.CutTiles));
		}
	}
}
