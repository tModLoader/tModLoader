using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace Terraria.ModLoader
{
	public class GlobalProjectile
	{
		public Mod mod
		{
			get;
			internal set;
		}

		public string Name
		{
			get;
			internal set;
		}

		public virtual bool Autoload(ref string name)
		{
			return mod.Properties.Autoload;
		}

		public virtual void SetDefaults(Projectile projectile)
		{
		}

		public virtual bool PreAI(Projectile projectile)
		{
			return true;
		}

		public virtual void AI(Projectile projectile)
		{
		}

		public virtual void PostAI(Projectile projectile)
		{
		}

		public virtual void TileCollideStyle(Projectile projectile, ref int width, ref int height, ref bool fallThrough)
		{
		}

		public virtual bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
		{
			return true;
		}

		public virtual bool PreKill(Projectile projectile, int timeLeft)
		{
			return true;
		}

		public virtual void Kill(Projectile projectile, int timeLeft)
		{
		}

		public virtual bool? CanHitNPC(Projectile projectile, NPC target)
		{
			return null;
		}

		public virtual void ModifyHitNPC(Projectile projectile, NPC target, ref int damage, ref float knockback, ref bool crit)
		{
		}

		public virtual void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit)
		{
		}

		public virtual bool CanHitPvp(Projectile projectile, Player target)
		{
			return true;
		}

		public virtual void ModifyHitPvp(Projectile projectile, Player target, ref int damage, ref bool crit)
		{
		}

		public virtual void OnHitPvp(Projectile projectile, Player target, int damage, bool crit)
		{
		}

		public virtual bool CanHitPlayer(Projectile projectile, Player target)
		{
			return true;
		}

		public virtual void ModifyHitPlayer(Projectile projectile, Player target, ref int damage, ref bool crit)
		{
		}

		public virtual void OnHitPlayer(Projectile projectile, Player target, int damage, bool crit)
		{
		}

		public virtual bool? Colliding(Projectile projectile, Rectangle projHitbox, Rectangle targetHitbox)
		{
			return null;
		}

		public virtual Color? GetAlpha(Projectile projectile, Color lightColor)
		{
			return null;
		}

		public virtual bool PreDraw(Projectile projectile, SpriteBatch spriteBatch, Color lightColor)
		{
			return true;
		}

		public virtual void PostDraw(Projectile projectile, SpriteBatch spriteBatch, Color lightColor)
		{
		}
	}
}
