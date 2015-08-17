using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace Terraria.ModLoader
{
	public class ModProjectile
	{
		//add modProjectile property to Terraria.Projectile (internal set)
		//set modProjectile to null at beginning of Terraria.Projectile.SetDefaults
		public Projectile projectile
		{
			get;
			internal set;
		}

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

		internal string texture;
		public int aiType = 0;

		public ModProjectile()
		{
			projectile = new Projectile();
			projectile.modProjectile = this;
		}

		public virtual bool Autoload(ref string name, ref string texture)
		{
			return mod.Properties.Autoload;
		}

		internal void SetupProjectile(Projectile projectile)
		{
			ModProjectile newProjectile = (ModProjectile)Activator.CreateInstance(GetType());
			newProjectile.projectile = projectile;
			projectile.modProjectile = newProjectile;
			newProjectile.mod = mod;
			newProjectile.SetDefaults();
		}

		public virtual void SetDefaults()
		{
		}

		public virtual bool PreAI()
		{
			return true;
		}

		public virtual void AI()
		{
		}

		public virtual void PostAI()
		{
		}

		public virtual void TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
		{
		}

		public virtual bool OnTileCollide(Vector2 oldVelocity)
		{
			return true;
		}

		public virtual bool PreKill(int timeLeft)
		{
			return true;
		}

		public virtual void Kill(int timeLeft)
		{
		}

		public virtual bool? CanHitNPC(NPC target)
		{
			return null;
		}

		public virtual void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit)
		{
		}

		public virtual void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
		}

		public virtual bool CanHitPvp(Player target)
		{
			return true;
		}

		public virtual void ModifyHitPvp(Player target, ref int damage, ref bool crit)
		{
		}

		public virtual void OnHitPvp(Player target, int damage, bool crit)
		{
		}

		public virtual bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			return true;
		}

		public virtual void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
		{
		}

		public virtual void OnHitPlayer(Player target, int damage, bool crit)
		{
		}

		public virtual bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return null;
		}
	}
}
