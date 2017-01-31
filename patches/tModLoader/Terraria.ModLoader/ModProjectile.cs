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
			ModProjectile newProjectile = (ModProjectile)(CloneNewInstances ? MemberwiseClone()
				: Activator.CreateInstance(GetType()));
			newProjectile.projectile = projectile;
			projectile.modProjectile = newProjectile;
			newProjectile.mod = mod;
			newProjectile.SetDefaults();
		}

		public virtual bool CloneNewInstances => false;

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

		public virtual bool ShouldUpdatePosition()
		{
			return true;
		}

		public virtual void TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
		{
		}

		public virtual bool OnTileCollide(Vector2 oldVelocity)
		{
			return true;
		}

		public virtual bool? CanCutTiles()
		{
			return null;
		}

		public virtual void CutTiles()
		{
		}

		public virtual bool PreKill(int timeLeft)
		{
			return true;
		}

		public virtual void Kill(int timeLeft)
		{
		}

		public virtual bool CanDamage()
		{
			return true;
		}

		public virtual bool MinionContactDamage()
		{
			return false;
		}

		public virtual bool? CanHitNPC(NPC target)
		{
			return null;
		}

		public virtual void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
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

		/// <summary>
		/// Whether or not a grappling hook can only have one hook per player in the world at a time. Return null to use the vanilla code. Returns null by default.
		/// </summary>
		public virtual void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
		}

		/// <summary>
		/// This code is called whenever the player uses a grappling hook that shoots this type of projectile. Use it to change what kind of hook is fired (for example, the Dual Hook does this), to kill old hook projectiles, etc.
		/// </summary>
		public virtual bool? CanUseGrapple(Player player)
		{
			return null;
		}

		/// <summary>
		/// Whether or not a grappling hook can only have one hook per player in the world at a time. Return null to use the vanilla code. Returns null by default.
		/// </summary>
		public virtual bool? SingleGrappleHook(Player player)
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
		/// How far away this grappling hook can travel away from its player before it retracts.
		/// </summary>
		public virtual float GrappleRange()
		{
			return 300f;
		}

		/// <summary>
		/// How many of this type of grappling hook the given player can latch onto blocks before the hooks start disappearing. Change the numHooks parameter to determine this; by default it will be 3.
		/// </summary>
		public virtual void NumGrappleHooks(Player player, ref int numHooks)
		{
		}

		/// <summary>
		/// The speed at which the grapple retreats back to the player after not hitting anything. Defaults to 11, but vanilla hooks go up to 24.
		/// </summary>
		public virtual void GrappleRetreatSpeed(Player player, ref float speed)
		{
		}

		/// <summary>
		/// The speed at which the grapple pulls the player after hitting something. Defaults to 11, but the Bat Hook uses 16.
		/// </summary>
		public virtual void GrapplePullSpeed(Player player, ref float speed)
		{
		}

		public virtual void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
		{
		}
	}
}
