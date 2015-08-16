using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace Terraria.ModLoader
{
	public static class ProjectileLoader
	{
		private static int nextProjectile = ProjectileID.Count;
		internal static readonly IDictionary<int, ModProjectile> projectiles = new Dictionary<int, ModProjectile>();
		internal static readonly IList<GlobalProjectile> globalProjectiles = new List<GlobalProjectile>();

		internal static int ReserveProjectileID()
		{
			int reserveID = nextProjectile;
			nextProjectile++;
			return reserveID;
		}

		internal static int ProjectileCount()
		{
			return nextProjectile;
		}

		public static ModProjectile GetProjectile(int type)
		{
			if (projectiles.ContainsKey(type))
			{
				return projectiles[type];
			}
			else
			{
				return null;
			}
		}
		//change initial size of Terraria.Player.ownedProjectileCounts to ProjectileLoader.ProjectileCount()
		internal static void ResizeArrays()
		{
			Array.Resize(ref Main.projectileLoaded, nextProjectile);
			Array.Resize(ref Main.projectileTexture, nextProjectile);
			Array.Resize(ref Main.projHostile, nextProjectile);
			Array.Resize(ref Main.projHook, nextProjectile);
			Array.Resize(ref Main.projFrames, nextProjectile);
			Array.Resize(ref Main.projPet, nextProjectile);
			Array.Resize(ref ProjectileID.Sets.TrailingMode, nextProjectile);
			Array.Resize(ref ProjectileID.Sets.TrailCacheLength, nextProjectile);
			Array.Resize(ref ProjectileID.Sets.LightPet, nextProjectile);
			Array.Resize(ref ProjectileID.Sets.Homing, nextProjectile);
			Array.Resize(ref ProjectileID.Sets.MinionSacrificable, nextProjectile);
			Array.Resize(ref ProjectileID.Sets.DontAttachHideToAlpha, nextProjectile);
			Array.Resize(ref ProjectileID.Sets.NeedsUUID, nextProjectile);
			Array.Resize(ref ProjectileID.Sets.StardustDragon, nextProjectile);
			for (int k = ProjectileID.Count; k < nextProjectile; k++)
			{
				Main.projectileLoaded[k] = true;
				Main.projFrames[k] = 1;
				ProjectileID.Sets.TrailingMode[k] = -1;
				ProjectileID.Sets.TrailCacheLength[k] = 10;
			}
		}

		internal static void Unload()
		{
			projectiles.Clear();
			nextProjectile = ProjectileID.Count;
			globalProjectiles.Clear();
		}

		internal static bool IsModProjectile(Projectile projectile)
		{
			return projectile.type >= ProjectileID.Count;
		}
		//in Terraria.Projectile.SetDefaults get rid of bad type check
		//in Terraria.Projectile.SetDefaults before scaling size call ProjectileLoader.SetupProjectile(this);
		internal static void SetupProjectile(Projectile projectile)
		{
			if (IsModProjectile(projectile))
			{
				GetProjectile(projectile.type).SetupProjectile(projectile);
			}
			foreach (GlobalProjectile globalProjectile in globalProjectiles)
			{
				globalProjectile.SetDefaults(projectile);
			}
		}
		//in Terraria.Projectile rename AI to VanillaAI then make AI call ProjectileLoader.ProjectileAI(this)
		internal static void ProjectileAI(Projectile projectile)
		{
			if (PreAI(projectile))
			{
				int type = projectile.type;
				if (IsModProjectile(projectile) && projectile.modProjectile.aiType > 0)
				{
					projectile.type = projectile.modProjectile.aiType;
				}
				projectile.VanillaAI();
				projectile.type = type;
				AI(projectile);
			}
			PostAI(projectile);
		}

		internal static bool PreAI(Projectile projectile)
		{
			foreach (GlobalProjectile globalProjectile in globalProjectiles)
			{
				if (!globalProjectile.PreAI(projectile))
				{
					return false;
				}
			}
			if (IsModProjectile(projectile))
			{
				return projectile.modProjectile.PreAI();
			}
			return true;
		}

		internal static void AI(Projectile projectile)
		{
			if (IsModProjectile(projectile))
			{
				projectile.modProjectile.AI();
			}
			foreach (GlobalProjectile globalProjectile in globalProjectiles)
			{
				globalProjectile.AI(projectile);
			}
		}

		internal static void PostAI(Projectile projectile)
		{
			if (IsModProjectile(projectile))
			{
				projectile.modProjectile.PostAI();
			}
			foreach (GlobalProjectile globalProjectile in globalProjectiles)
			{
				globalProjectile.PostAI(projectile);
			}
		}
		//in Terraria.Projectile.Update before adjusting velocity to tile collisions add
		//  ProjectileLoader.TileCollideStyle(this, ref num25, ref num26, ref flag4);
		internal static void TileCollideStyle(Projectile projectile, ref int width, ref int height, ref bool fallThrough)
		{
			if (IsModProjectile(projectile))
			{
				projectile.modProjectile.TileCollideStyle(ref width, ref height, ref fallThrough);
			}
			foreach (GlobalProjectile globalProjectile in globalProjectiles)
			{
				globalProjectile.TileCollideStyle(projectile, ref width, ref height, ref fallThrough);
			}
		}
		//in Terraria.Projectile.Update before if/else chain for tile collide behavior add
		//  if(!ProjectileLoader.OnTileCollide(this, velocity)) { } else
		internal static bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
		{
			foreach (GlobalProjectile globalProjectile in globalProjectiles)
			{
				if (!globalProjectile.OnTileCollide(projectile, oldVelocity))
				{
					return false;
				}
			}
			if (IsModProjectile(projectile))
			{
				return projectile.modProjectile.OnTileCollide(oldVelocity);
			}
			return true;
		}
		//in Terraria.Projectile.Kill before if statements determining kill behavior add
		//  if(!ProjectileLoader.PreKill(this, num)) { this.active = false; return; }
		internal static bool PreKill(Projectile projectile, int timeLeft)
		{
			foreach (GlobalProjectile globalProjectile in globalProjectiles)
			{
				if (!globalProjectile.PreKill(projectile, timeLeft))
				{
					return false;
				}
			}
			if (IsModProjectile(projectile))
			{
				return projectile.modProjectile.PreKill(timeLeft);
			}
			return true;
		}
		//at end of Terraria.Projectile.Kill before setting active to false add
		//  ProjectileLoader.Kill(this, num);
		internal static void Kill(Projectile projectile, int timeLeft)
		{
			if (IsModProjectile(projectile))
			{
				projectile.modProjectile.Kill(timeLeft);
			}
			foreach (GlobalProjectile globalProjectile in globalProjectiles)
			{
				globalProjectile.Kill(projectile, timeLeft);
			}
		}
		//in Terraria.Projectile.Damage for damaging NPCs before flag2 is checked... just check the patch files
		internal static bool? CanHitNPC(Projectile projectile, NPC target)
		{
			foreach (GlobalProjectile globalProjectile in globalProjectiles)
			{
				bool? canHit = globalProjectile.CanHitNPC(projectile, target);
				if (canHit.HasValue && !canHit.Value)
				{
					return false;
				}
			}
			if (IsModProjectile(projectile))
			{
				return projectile.modProjectile.CanHitNPC(target);
			}
			return null;
		}
		//in Terraria.Projectile.Damage before calling StatusNPC call this and add local knockback variable
		internal static void ModifyHitNPC(Projectile projectile, NPC target, ref int damage, ref float knockback, ref bool crit)
		{
			if (IsModProjectile(projectile))
			{
				projectile.modProjectile.ModifyHitNPC(target, ref damage, ref knockback, ref crit);
			}
			foreach (GlobalProjectile globalProjectile in globalProjectiles)
			{
				globalProjectile.ModifyHitNPC(projectile, target, ref damage, ref knockback, ref crit);
			}
		}
		//in Terraria.Projectile.Damage before penetration check for NPCs call this
		internal static void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit)
		{
			if (IsModProjectile(projectile))
			{
				projectile.modProjectile.OnHitNPC(target, damage, knockback, crit);
			}
			foreach (GlobalProjectile globalProjectile in globalProjectiles)
			{
				globalProjectile.OnHitNPC(projectile, target, damage, knockback, crit);
			}
		}
		//in Terraria.Projectile.Damage add this before collision check for pvp damage
		internal static bool CanHitPvp(Projectile projectile, Player target)
		{
			foreach (GlobalProjectile globalProjectile in globalProjectiles)
			{
				if (!globalProjectile.CanHitPvp(projectile, target))
				{
					return false;
				}
			}
			if (IsModProjectile(projectile))
			{
				return projectile.modProjectile.CanHitPvp(target);
			}
			return true;
		}
		//in Terraria.Projectile.Damage for pvp damage call this after damage var
		internal static void ModifyHitPvp(Projectile projectile, Player target, ref int damage, ref bool crit)
		{
			if (IsModProjectile(projectile))
			{
				projectile.modProjectile.ModifyHitPvp(target, ref damage, ref crit);
			}
			foreach (GlobalProjectile globalProjectile in globalProjectiles)
			{
				globalProjectile.ModifyHitPvp(projectile, target, ref damage, ref crit);
			}
		}
		//in Terraria.Projectile.Damage for pvp damage call this before net message stuff
		internal static void OnHitPvp(Projectile projectile, Player target, int damage, bool crit)
		{
			if (IsModProjectile(projectile))
			{
				projectile.modProjectile.OnHitPvp(target, damage, crit);
			}
			foreach (GlobalProjectile globalProjectile in globalProjectiles)
			{
				globalProjectile.OnHitPvp(projectile, target, damage, crit);
			}
		}
		//in Terraria.Projectile.Damage for damaging my player, add this before collision check
		internal static bool CanHitPlayer(Projectile projectile, Player target)
		{
			foreach (GlobalProjectile globalProjectile in globalProjectiles)
			{
				if (!globalProjectile.CanHitPlayer(projectile, target))
				{
					return false;
				}
			}
			if (IsModProjectile(projectile))
			{
				return projectile.modProjectile.CanHitPlayer(target);
			}
			return true;
		}
		//in Terraria.Projectile.Damage for damaging my player, call this after damage variation and add local crit variable
		internal static void ModifyHitPlayer(Projectile projectile, Player target, ref int damage, ref bool crit, ref int cooldownCounter)
		{
			if (IsModProjectile(projectile))
			{
				projectile.modProjectile.ModifyHitPlayer(target, ref damage, ref crit, ref cooldownCounter);
			}
			foreach (GlobalProjectile globalProjectile in globalProjectiles)
			{
				globalProjectile.ModifyHitPlayer(projectile, target, ref damage, ref crit, ref cooldownCounter);
			}
		}
		//in Terraria.Projectile.Damage for damaging my player before decreasing projectile penetration call this
		//  and assign return value from Player.Hurt to local variable to pass as a parameter
		internal static void OnHitPlayer(Projectile projectile, Player target, int damage, bool crit)
		{
			if (IsModProjectile(projectile))
			{
				projectile.modProjectile.OnHitPlayer(target, damage, crit);
			}
			foreach (GlobalProjectile globalProjectile in globalProjectiles)
			{
				globalProjectile.OnHitPlayer(projectile, target, damage, crit);
			}
		}
	}
}
