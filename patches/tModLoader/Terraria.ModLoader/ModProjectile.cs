using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
		public int cooldownSlot = -1;
		public int drawOffsetX = 0;
		public int drawOriginOffsetY = 0;
		public float drawOriginOffsetX = 0f;
		public bool drawHeldProjInFrontOfHeldItemAndArms = false;

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

		public virtual void SendExtraAI(BinaryWriter writer)
		{
		}

		public virtual void ReceiveExtraAI(BinaryReader reader)
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

		public virtual bool MinionContactDamage()
		{
			return false;
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

		public virtual bool CanHitPlayer(Player target)
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

		public virtual Color? GetAlpha(Color lightColor)
		{
			return null;
		}

		public virtual bool PreDrawExtras(SpriteBatch spriteBatch)
		{
			return true;
		}

		public virtual bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			return true;
		}

		public virtual void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
		}

		public virtual bool? CanUseGrapple(Player player)
		{
			return null;
		}

		public virtual bool? SingleGrappleHook(Player player)
		{
			return null;
		}

		public virtual void UseGrapple(Player player, ref int type)
		{
		}

		public virtual float GrappleRange()
		{
			return 300f;
		}

		public virtual void NumGrappleHooks(Player player, ref int numHooks)
		{
		}

		public virtual void GrappleRetreatSpeed(Player player, ref float speed)
		{
		}

		public virtual void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles)
		{
		}
	}
}
