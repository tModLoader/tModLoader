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
	}
}
