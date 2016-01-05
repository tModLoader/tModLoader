using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;

namespace Terraria.ModLoader
{
	public static class ProjectileLoader
	{
		private static int nextProjectile = ProjectileID.Count;
		internal static readonly IDictionary<int, ModProjectile> projectiles = new Dictionary<int, ModProjectile>();
		internal static readonly IList<GlobalProjectile> globalProjectiles = new List<GlobalProjectile>();
		internal static readonly IList<ProjectileInfo> infoList = new List<ProjectileInfo>();
		internal static readonly IDictionary<string, IDictionary<string, int>> infoIndexes = new Dictionary<string, IDictionary<string, int>>();

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
			infoList.Clear();
			infoIndexes.Clear();
		}

		internal static bool IsModProjectile(Projectile projectile)
		{
			return projectile.type >= ProjectileID.Count;
		}
		//in Terraria.Projectile.SetDefaults get rid of bad type check
		//in Terraria.Projectile.SetDefaults before scaling size call ProjectileLoader.SetupProjectile(this);
		internal static void SetupProjectile(Projectile projectile)
		{
			projectile.projectileInfo.Clear();
			foreach (ProjectileInfo info in infoList)
			{
				projectile.projectileInfo.Add(info.Clone());
			}
			if (IsModProjectile(projectile))
			{
				GetProjectile(projectile.type).SetupProjectile(projectile);
			}
			foreach (GlobalProjectile globalProjectile in globalProjectiles)
			{
				globalProjectile.SetDefaults(projectile);
			}
		}

		internal static ProjectileInfo GetProjectileInfo(Projectile projectile, Mod mod, string name)
		{
			if (!infoIndexes.ContainsKey(mod.Name))
			{
				return null;
			}
			if (!infoIndexes[mod.Name].ContainsKey(name))
			{
				return null;
			}
			return projectile.projectileInfo[infoIndexes[mod.Name][name]];
		}
		//in Terraria.Projectile rename AI to VanillaAI then make AI call ProjectileLoader.ProjectileAI(this)
		public static void ProjectileAI(Projectile projectile)
		{
			if (PreAI(projectile))
			{
				int type = projectile.type;
				bool useAiType = IsModProjectile(projectile) && projectile.modProjectile.aiType > 0;
				if (useAiType)
				{
					projectile.type = projectile.modProjectile.aiType;
				}
				projectile.VanillaAI();
				if (useAiType)
				{
					projectile.type = type;
				}
				AI(projectile);
			}
			PostAI(projectile);
		}

		public static bool PreAI(Projectile projectile)
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

		public static void AI(Projectile projectile)
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

		public static void PostAI(Projectile projectile)
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
		//in Terraria.NetMessage.SendData at end of case 27 call
		//  ProjectileLoader.SendExtraAI(projectile, writer, ref bb14);
		public static void SendExtraAI(Projectile projectile, BinaryWriter writer, ref BitsByte flags)
		{
			if (IsModProjectile(projectile))
			{
				byte[] data;
				using (MemoryStream stream = new MemoryStream())
				{
					using (BinaryWriter modWriter = new BinaryWriter(stream))
					{
						projectile.modProjectile.SendExtraAI(modWriter);
						modWriter.Flush();
						data = stream.ToArray();
					}
				}
				if (data.Length > 0)
				{
					flags[Projectile.maxAI + 1] = true;
					writer.Write((byte)data.Length);
					writer.Write(data);
				}
			}
		}
		//in Terraria.MessageBuffer.GetData for case 27 after reading all data add
		//  byte[] extraAI = ProjectileLoader.ReadExtraAI(reader, bitsByte14);
		public static byte[] ReadExtraAI(BinaryReader reader, BitsByte flags)
		{
			if (flags[Projectile.maxAI + 1])
			{
				return reader.ReadBytes(reader.ReadByte());
			}
			return new byte[0];
		}
		//in Terraria.MessageBuffer.GetData for case 27 before calling ProjectileFixDesperation add
		//  ProjectileLoader.ReceiveExtraAI(projectile, extraAI);
		public static void ReceiveExtraAI(Projectile projectile, byte[] extraAI)
		{
			if (extraAI.Length > 0 && IsModProjectile(projectile))
			{
				using (MemoryStream stream = new MemoryStream(extraAI))
				{
					using (BinaryReader reader = new BinaryReader(stream))
					{
						projectile.modProjectile.ReceiveExtraAI(reader);
					}
				}
			}
		}
		//in Terraria.Projectile.Update before adjusting velocity to tile collisions add
		//  ProjectileLoader.TileCollideStyle(this, ref num25, ref num26, ref flag4);
		public static void TileCollideStyle(Projectile projectile, ref int width, ref int height, ref bool fallThrough)
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
		public static bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
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
		public static bool PreKill(Projectile projectile, int timeLeft)
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
		public static void Kill(Projectile projectile, int timeLeft)
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

		public static bool MinionContactDamage(Projectile projectile)
		{
			if (IsModProjectile(projectile) && projectile.modProjectile.MinionContactDamage())
			{
				return true;
			}
			foreach (GlobalProjectile globalProjectile in globalProjectiles)
			{
				if (globalProjectile.MinionContactDamage(projectile))
				{
					return true;
				}
			}
			return false;
		}
		//in Terraria.Projectile.Damage for damaging NPCs before flag2 is checked... just check the patch files
		public static bool? CanHitNPC(Projectile projectile, NPC target)
		{
			bool? flag = null;
			foreach (GlobalProjectile globalProjectile in globalProjectiles)
			{
				bool? canHit = globalProjectile.CanHitNPC(projectile, target);
				if (canHit.HasValue && !canHit.Value)
				{
					return false;
				}
				if (canHit.HasValue)
				{
					flag = canHit.Value;
				}
			}
			if (IsModProjectile(projectile))
			{
				bool? canHit = projectile.modProjectile.CanHitNPC(target);
				if (canHit.HasValue && !canHit.Value)
				{
					return false;
				}
				if (canHit.HasValue)
				{
					flag = canHit.Value;
				}
			}
			return flag;
		}
		//in Terraria.Projectile.Damage before calling StatusNPC call this and add local knockback variable
		public static void ModifyHitNPC(Projectile projectile, NPC target, ref int damage, ref float knockback, ref bool crit)
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
		public static void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit)
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
		public static bool CanHitPvp(Projectile projectile, Player target)
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
		public static void ModifyHitPvp(Projectile projectile, Player target, ref int damage, ref bool crit)
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
		public static void OnHitPvp(Projectile projectile, Player target, int damage, bool crit)
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
		public static bool CanHitPlayer(Projectile projectile, Player target)
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
		public static void ModifyHitPlayer(Projectile projectile, Player target, ref int damage, ref bool crit)
		{
			if (IsModProjectile(projectile))
			{
				projectile.modProjectile.ModifyHitPlayer(target, ref damage, ref crit);
			}
			foreach (GlobalProjectile globalProjectile in globalProjectiles)
			{
				globalProjectile.ModifyHitPlayer(projectile, target, ref damage, ref crit);
			}
		}
		//in Terraria.Projectile.Damage for damaging my player before decreasing projectile penetration call this
		//  and assign return value from Player.Hurt to local variable to pass as a parameter
		public static void OnHitPlayer(Projectile projectile, Player target, int damage, bool crit)
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
		//in Terraria.Projectile.Colliding after modifying myRect add
		//  bool? modColliding = ProjectileLoader.Colliding(this, myRect, targetRect);
		//  if(modColliding.HasValue) { return modColliding.Value; }
		public static bool? Colliding(Projectile projectile, Rectangle projHitbox, Rectangle targetHitbox)
		{
			foreach (GlobalProjectile globalProjectile in globalProjectiles)
			{
				bool? colliding = globalProjectile.Colliding(projectile, projHitbox, targetHitbox);
				if (colliding.HasValue)
				{
					return colliding.Value;
				}
			}
			if (IsModProjectile(projectile))
			{
				return projectile.modProjectile.Colliding(projHitbox, targetHitbox);
			}
			return null;
		}
		//at beginning of Terraria.Projectile.GetAlpha add
		//  Color? modColor = ProjectileLoader.GetAlpha(this, newColor);
		//  if(modColor.HasValue) { return modColor.Value; }
		public static Color? GetAlpha(Projectile projectile, Color lightColor)
		{
			foreach (GlobalProjectile globalProjectile in globalProjectiles)
			{
				Color? color = globalProjectile.GetAlpha(projectile, lightColor);
				if (color.HasValue)
				{
					return color;
				}
			}
			if (IsModProjectile(projectile))
			{
				return projectile.modProjectile.GetAlpha(lightColor);
			}
			return null;
		}
		//in Terraria.Main.DrawProj after setting offsets call
		//  ProjectileLoader.DrawOffset(projectile, ref num148, ref num149);
		public static void DrawOffset(Projectile projectile, ref int offsetX, ref int offsetY, ref float originX)
		{
			if (IsModProjectile(projectile))
			{
				offsetX = projectile.modProjectile.drawOffsetX;
				offsetY = -projectile.modProjectile.drawOriginOffsetY;
				originX += projectile.modProjectile.drawOriginOffsetX;
			}
		}

		public static bool PreDrawExtras(Projectile projectile, SpriteBatch spriteBatch)
		{
			foreach (GlobalProjectile globalProjectile in globalProjectiles)
			{
				if (!globalProjectile.PreDrawExtras(projectile, spriteBatch))
				{
					return false;
				}
			}
			if (IsModProjectile(projectile))
			{
				return projectile.modProjectile.PreDrawExtras(spriteBatch);
			}
			return true;
		}
		//in Terraria.Main.DrawProj after modifying light color add
		//  if(!ProjectileLoader.PreDraw(projectile, Main.spriteBatch, color25))
		//  { ProjectileLoader.PostDraw(projectile, Main.spriteBatch, color25); return; }
		public static bool PreDraw(Projectile projectile, SpriteBatch spriteBatch, Color lightColor)
		{
			foreach (GlobalProjectile globalProjectile in globalProjectiles)
			{
				if (!globalProjectile.PreDraw(projectile, spriteBatch, lightColor))
				{
					return false;
				}
			}
			if (IsModProjectile(projectile))
			{
				return projectile.modProjectile.PreDraw(spriteBatch, lightColor);
			}
			return true;
		}
		//at end of Terraria.Main.DrawProj call ProjectileLoader.PostDraw(projectile, Main.spriteBatch, color25);
		public static void PostDraw(Projectile projectile, SpriteBatch spriteBatch, Color lightColor)
		{
			if (IsModProjectile(projectile))
			{
				projectile.modProjectile.PostDraw(spriteBatch, lightColor);
			}
			foreach (GlobalProjectile globalProjectile in globalProjectiles)
			{
				globalProjectile.PostDraw(projectile, spriteBatch, lightColor);
			}
		}

		public static bool? CanUseGrapple(int type, Player player)
		{
			bool? flag = null;
			ModProjectile modProjectile = GetProjectile(type);
			if (modProjectile != null)
			{
				flag = modProjectile.CanUseGrapple(player);
			}
			foreach (GlobalProjectile globalProjectile in globalProjectiles)
			{
				bool? canGrapple = globalProjectile.CanUseGrapple(type, player);
				if (canGrapple.HasValue)
				{
					flag = canGrapple;
				}
			}
			return flag;
		}

		public static bool? SingleGrappleHook(int type, Player player)
		{
			bool? flag = null;
			ModProjectile modProjectile = GetProjectile(type);
			if (modProjectile != null)
			{
				flag = modProjectile.SingleGrappleHook(player);
			}
			foreach (GlobalProjectile globalProjectile in globalProjectiles)
			{
				bool? singleHook = globalProjectile.SingleGrappleHook(type, player);
				if (singleHook.HasValue)
				{
					flag = singleHook;
				}
			}
			return flag;
		}

		public static void UseGrapple(Player player, ref int type)
		{
			ModProjectile modProjectile = GetProjectile(type);
			if (modProjectile != null)
			{
				modProjectile.UseGrapple(player, ref type);
			}
			foreach (GlobalProjectile globalProjectile in globalProjectiles)
			{
				globalProjectile.UseGrapple(player, ref type);
			}
		}

		public static bool GrappleOutOfRange(float distance, Projectile projectile)
		{
			if (IsModProjectile(projectile))
			{
				return distance > projectile.modProjectile.GrappleRange();
			}
			return false;
		}

		public static void NumGrappleHooks(Projectile projectile, Player player, ref int numHooks)
		{
			if (IsModProjectile(projectile))
			{
				projectile.modProjectile.NumGrappleHooks(player, ref numHooks);
			}
			foreach (GlobalProjectile globalProjectile in globalProjectiles)
			{
				globalProjectile.NumGrappleHooks(projectile, player, ref numHooks);
			}
		}

		public static void GrappleRetreatSpeed(Projectile projectile, Player player, ref float speed)
		{
			if (IsModProjectile(projectile))
			{
				projectile.modProjectile.GrappleRetreatSpeed(player, ref speed);
			}
			foreach (GlobalProjectile globalProjectile in globalProjectiles)
			{
				globalProjectile.GrappleRetreatSpeed(projectile, player, ref speed);
			}
		}
	}
}
