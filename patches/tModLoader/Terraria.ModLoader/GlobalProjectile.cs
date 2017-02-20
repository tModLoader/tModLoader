using System;
using System.Collections.Generic;
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

		public virtual bool ShouldUpdatePosition(Projectile projectile)
		{
			return true;
		}

		public virtual bool TileCollideStyle(Projectile projectile, ref int width, ref int height, ref bool fallThrough)
		{
			return true;
		}

		public virtual bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
		{
			return true;
		}

		public virtual bool? CanCutTiles(Projectile projectile)
		{
			return null;
		}

		public virtual void CutTiles(Projectile projectile)
		{
		}

		public virtual bool PreKill(Projectile projectile, int timeLeft)
		{
			return true;
		}

		public virtual void Kill(Projectile projectile, int timeLeft)
		{
		}

		public virtual bool CanDamage(Projectile projectile)
		{
			return true;
		}

		public virtual bool MinionContactDamage(Projectile projectile)
		{
			return false;
		}

		public virtual bool? CanHitNPC(Projectile projectile, NPC target)
		{
			return null;
		}

		public virtual void ModifyHitNPC(Projectile projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
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

		public virtual bool PreDrawExtras(Projectile projectile, SpriteBatch spriteBatch)
		{
			return true;
		}

		public virtual bool PreDraw(Projectile projectile, SpriteBatch spriteBatch, Color lightColor)
		{
			return true;
		}

		public virtual void PostDraw(Projectile projectile, SpriteBatch spriteBatch, Color lightColor)
		{
		}

		/// <summary>
		/// Whether or not a grappling hook that shoots this type of projectile can be used by the given player. Return null to use the default code (whether or not the player is in the middle of firing the grappling hook). Returns null by default.
		/// </summary>
		public virtual bool? CanUseGrapple(int type, Player player)
		{
			return null;
		}

		/// <summary>
		/// Whether or not a grappling hook can only have one hook per player in the world at a time. Return null to use the vanilla code. Returns null by default.
		/// </summary>
		public virtual bool? SingleGrappleHook(int type, Player player)
		{
			return null;
		}

		/// <summary>
		/// This code is called whenever the player uses a grappling hook that shoots this type of projectile. Use it to change what kind of hook is fired (for example, the Dual Hook does this), to kill old hook projectiles, etc.
		/// </summary>
		public virtual void UseGrapple(Player player, ref int type)
		{
		}

		/// <summary>
		/// How many of this type of grappling hook the given player can latch onto blocks before the hooks start disappearing. Change the numHooks parameter to determine this; by default it will be 3.
		/// </summary>
		public virtual void NumGrappleHooks(Projectile projectile, Player player, ref int numHooks)
		{
		}

		/// <summary>
		/// The speed at which the grapple retreats back to the player after not hitting anything. Defaults to 11, but vanilla hooks go up to 24.
		/// </summary>
		public virtual void GrappleRetreatSpeed(Projectile projectile, Player player, ref float speed)
		{
		}

		/// <summary>
		/// The speed at which the grapple pulls the player after hitting something. Defaults to 11, but the Bat Hook uses 16.
		/// </summary>
		public virtual void GrapplePullSpeed(Projectile projectile, Player player, ref float speed)
		{
		}

		public virtual void DrawBehind(Projectile projectile, int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
		{
		}
	}
}
