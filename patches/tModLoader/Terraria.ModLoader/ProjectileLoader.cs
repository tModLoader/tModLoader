using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;

namespace Terraria.ModLoader
{
	public static class ProjectileLoader
	{
		private static int nextProjectile = ProjectileID.Count;
		internal static readonly IList<ModProjectile> projectiles = new List<ModProjectile>();
		internal static readonly IList<GlobalProjectile> globalProjectiles = new List<GlobalProjectile>();
		internal static readonly IList<ProjectileInfo> infoList = new List<ProjectileInfo>();
		internal static readonly IDictionary<string, int> infoIndexes = new Dictionary<string, int>();

		private static Action<Projectile>[] HookSetDefaults = new Action<Projectile>[0];
		private static Func<Projectile, bool>[] HookPreAI;
		private static Action<Projectile>[] HookAI;
		private static Action<Projectile>[] HookPostAI;
		private static Func<Projectile, bool>[] HookShouldUpdatePosition;
		private delegate void DelegateTileCollideStyle(Projectile projectile, ref int width, ref int height, ref bool fallThrough);
		private static DelegateTileCollideStyle[] HookTileCollideStyle;
		private static Func<Projectile, Vector2, bool>[] HookOnTileCollide;
		private static Func<Projectile, bool?>[] HookCanCutTiles;
		private static Func<Projectile, int, bool>[] HookPreKill;
		private static Action<Projectile, int>[] HookKill;
		private static Func<Projectile, bool>[] HookCanDamage;
		private static Func<Projectile, bool>[] HookMinionContactDamage;
		private static Func<Projectile, NPC, bool?>[] HookCanHitNPC;
		private delegate void DelegateModifyHitNPC(Projectile projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection);
		private static DelegateModifyHitNPC[] HookModifyHitNPC;
		private static Action<Projectile, NPC, int, float, bool>[] HookOnHitNPC;
		private static Func<Projectile, Player, bool>[] HookCanHitPvp;
		private delegate void DelegateModifyHitPlayer(Projectile projectile, Player target, ref int damage, ref bool crit);
		private static DelegateModifyHitPlayer[] HookModifyHitPvp;
		private static Action<Projectile, Player, int, bool>[] HookOnHitPvp;
		private static Func<Projectile, Player, bool>[] HookCanHitPlayer;
		private static DelegateModifyHitPlayer[] HookModifyHitPlayer;
		private static Action<Projectile, Player, int, bool>[] HookOnHitPlayer;
		private static Func<Projectile, Rectangle, Rectangle, bool?>[] HookColliding;
		private static Func<Projectile, Color, Color?>[] HookGetAlpha;
		private static Func<Projectile, SpriteBatch, bool>[] HookPreDrawExtras;
		private static Func<Projectile, SpriteBatch, Color, bool>[] HookPreDraw;
		private static Action<Projectile, SpriteBatch, Color>[] HookPostDraw;
		private static Func<int, Player, bool?>[] HookCanUseGrapple;
		private static Func<int, Player, bool?>[] HookSingleGrappleHook;
		private delegate void DelegateUseGrapple(Player player, ref int type);
		private static DelegateUseGrapple[] HookUseGrapple;
		private delegate void DelegateNumGrappleHooks(Projectile projectile, Player player, ref int numHooks);
		private static DelegateNumGrappleHooks[] HookNumGrappleHooks;
		private delegate void DelegateGrappleRetreatSpeed(Projectile projectile, Player player, ref float speed);
		private static DelegateGrappleRetreatSpeed[] HookGrappleRetreatSpeed;
		private static Action<Projectile, int, List<int>, List<int>, List<int>, List<int>>[] HookDrawBehind;

		internal static int ReserveProjectileID()
		{
			if (ModNet.AllowVanillaClients) throw new Exception("Adding projectiles breaks vanilla client compatiblity");

			int reserveID = nextProjectile;
			nextProjectile++;
			return reserveID;
		}

		internal static int ProjectileCount => nextProjectile;

		public static ModProjectile GetProjectile(int type)
		{
			return type >= ProjectileID.Count && type < ProjectileCount ? projectiles[type - ProjectileID.Count] : null;
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
			Array.Resize(ref ProjectileID.Sets.YoyosLifeTimeMultiplier, nextProjectile);
			Array.Resize(ref ProjectileID.Sets.YoyosMaximumRange, nextProjectile);
			Array.Resize(ref ProjectileID.Sets.YoyosTopSpeed, nextProjectile);
			Array.Resize(ref ProjectileID.Sets.CanDistortWater, nextProjectile);
			Array.Resize(ref ProjectileID.Sets.MinionShot, nextProjectile);
			Array.Resize(ref ProjectileID.Sets.SentryShot, nextProjectile);
			Array.Resize(ref ProjectileID.Sets.ForcePlateDetection, nextProjectile);
			Array.Resize(ref ProjectileID.Sets.TrailingMode, nextProjectile);
			Array.Resize(ref ProjectileID.Sets.TrailCacheLength, nextProjectile);
			Array.Resize(ref ProjectileID.Sets.LightPet, nextProjectile);
			Array.Resize(ref ProjectileID.Sets.Homing, nextProjectile);
			Array.Resize(ref ProjectileID.Sets.IsADD2Turret, nextProjectile);
			Array.Resize(ref ProjectileID.Sets.TurretFeature, nextProjectile);
			Array.Resize(ref ProjectileID.Sets.MinionTargettingFeature, nextProjectile);
			Array.Resize(ref ProjectileID.Sets.MinionSacrificable, nextProjectile);
			Array.Resize(ref ProjectileID.Sets.DontAttachHideToAlpha, nextProjectile);
			Array.Resize(ref ProjectileID.Sets.NeedsUUID, nextProjectile);
			Array.Resize(ref ProjectileID.Sets.StardustDragon, nextProjectile);
			Array.Resize(ref ProjectileID.Sets.NoLiquidDistortion, nextProjectile);
			for (int k = ProjectileID.Count; k < nextProjectile; k++)
			{
				ProjectileID.Sets.YoyosLifeTimeMultiplier[k] = -1;
				ProjectileID.Sets.YoyosMaximumRange[k] = 200f;
				ProjectileID.Sets.YoyosTopSpeed[k] = 10f;
				ProjectileID.Sets.CanDistortWater[k] = true;
				Main.projectileLoaded[k] = true;
				Main.projFrames[k] = 1;
				ProjectileID.Sets.TrailingMode[k] = -1;
				ProjectileID.Sets.TrailCacheLength[k] = 10;
			}
			Array.Resize(ref Projectile.perIDStaticNPCImmunity, nextProjectile);
			for (int i = 0; i < nextProjectile; i++)
			{
				Projectile.perIDStaticNPCImmunity[i] = new int[200];
			}

			ModLoader.BuildGlobalHook(ref HookSetDefaults, globalProjectiles, g => g.SetDefaults);
			ModLoader.BuildGlobalHook(ref HookPreAI, globalProjectiles, g => g.PreAI);
			ModLoader.BuildGlobalHook(ref HookAI, globalProjectiles, g => g.AI);
			ModLoader.BuildGlobalHook(ref HookPostAI, globalProjectiles, g => g.PostAI);
			ModLoader.BuildGlobalHook(ref HookShouldUpdatePosition, globalProjectiles, g => g.ShouldUpdatePosition);
			ModLoader.BuildGlobalHook(ref HookTileCollideStyle, globalProjectiles, g => g.TileCollideStyle);
			ModLoader.BuildGlobalHook(ref HookOnTileCollide, globalProjectiles, g => g.OnTileCollide);
			ModLoader.BuildGlobalHook(ref HookCanCutTiles, globalProjectiles, g => g.CanCutTiles);
			ModLoader.BuildGlobalHook(ref HookPreKill, globalProjectiles, g => g.PreKill);
			ModLoader.BuildGlobalHook(ref HookKill, globalProjectiles, g => g.Kill);
			ModLoader.BuildGlobalHook(ref HookCanDamage, globalProjectiles, g => g.CanDamage);
			ModLoader.BuildGlobalHook(ref HookMinionContactDamage, globalProjectiles, g => g.MinionContactDamage);
			ModLoader.BuildGlobalHook(ref HookCanHitNPC, globalProjectiles, g => g.CanHitNPC);
			ModLoader.BuildGlobalHook(ref HookModifyHitNPC, globalProjectiles, g => g.ModifyHitNPC);
			ModLoader.BuildGlobalHook(ref HookOnHitNPC, globalProjectiles, g => g.OnHitNPC);
			ModLoader.BuildGlobalHook(ref HookCanHitPvp, globalProjectiles, g => g.CanHitPvp);
			ModLoader.BuildGlobalHook(ref HookModifyHitPvp, globalProjectiles, g => g.ModifyHitPvp);
			ModLoader.BuildGlobalHook(ref HookOnHitPvp, globalProjectiles, g => g.OnHitPvp);
			ModLoader.BuildGlobalHook(ref HookCanHitPlayer, globalProjectiles, g => g.CanHitPlayer);
			ModLoader.BuildGlobalHook(ref HookModifyHitPlayer, globalProjectiles, g => g.ModifyHitPlayer);
			ModLoader.BuildGlobalHook(ref HookOnHitPlayer, globalProjectiles, g => g.OnHitPlayer);
			ModLoader.BuildGlobalHook(ref HookColliding, globalProjectiles, g => g.Colliding);
			ModLoader.BuildGlobalHook(ref HookGetAlpha, globalProjectiles, g => g.GetAlpha);
			ModLoader.BuildGlobalHook(ref HookPreDrawExtras, globalProjectiles, g => g.PreDrawExtras);
			ModLoader.BuildGlobalHook(ref HookPreDraw, globalProjectiles, g => g.PreDraw);
			ModLoader.BuildGlobalHook(ref HookPostDraw, globalProjectiles, g => g.PostDraw);
			ModLoader.BuildGlobalHook(ref HookCanUseGrapple, globalProjectiles, g => g.CanUseGrapple);
			ModLoader.BuildGlobalHook(ref HookSingleGrappleHook, globalProjectiles, g => g.SingleGrappleHook);
			ModLoader.BuildGlobalHook(ref HookUseGrapple, globalProjectiles, g => g.UseGrapple);
			ModLoader.BuildGlobalHook(ref HookNumGrappleHooks, globalProjectiles, g => g.NumGrappleHooks);
			ModLoader.BuildGlobalHook(ref HookGrappleRetreatSpeed, globalProjectiles, g => g.GrappleRetreatSpeed);
			ModLoader.BuildGlobalHook(ref HookDrawBehind, globalProjectiles, g => g.DrawBehind);
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
			SetupProjectileInfo(projectile);
			if (IsModProjectile(projectile))
			{
				GetProjectile(projectile.type).SetupProjectile(projectile);
			}
			foreach (var hook in HookSetDefaults)
			{
				hook(projectile);
			}
		}

		internal static void SetupProjectileInfo(Projectile projectile)
		{
			projectile.projectileInfo = infoList.Select(info => info.Clone()).ToArray();
		}

		internal static ProjectileInfo GetProjectileInfo(Projectile projectile, Mod mod, string name)
		{
			int index;
			return infoIndexes.TryGetValue(mod.Name + ':' + name, out index) ? projectile.projectileInfo[index] : null;
		}
		//in Terraria.Projectile rename AI to VanillaAI then make AI call ProjectileLoader.ProjectileAI(this)
		public static void ProjectileAI(Projectile projectile)
		{
			if (PreAI(projectile))
			{
				int type = projectile.type;
				bool useAiType = projectile.modProjectile != null && projectile.modProjectile.aiType > 0;
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
			foreach (var hook in HookPreAI)
			{
				if (!hook(projectile))
				{
					return false;
				}
			}
			if (projectile.modProjectile != null)
			{
				return projectile.modProjectile.PreAI();
			}
			return true;
		}

		public static void AI(Projectile projectile)
		{
			projectile.modProjectile?.AI();

			foreach (var hook in HookAI)
			{
				hook(projectile);
			}
		}

		public static void PostAI(Projectile projectile)
		{
			projectile.modProjectile?.PostAI();

			foreach (var hook in HookPostAI)
			{
				hook(projectile);
			}
		}
		//in Terraria.NetMessage.SendData at end of case 27 call
		//  ProjectileLoader.SendExtraAI(projectile, writer, ref bb14);
		public static byte[] SendExtraAI(Projectile projectile, ref BitsByte flags)
		{
			if (projectile.modProjectile != null)
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
				}
				return data;
			}
			return new byte[0];
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
			if (extraAI.Length > 0 && projectile.modProjectile != null)
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

		public static bool ShouldUpdatePosition(Projectile projectile)
		{
			if (IsModProjectile(projectile) && !projectile.modProjectile.ShouldUpdatePosition())
			{
				return false;
			}
			foreach (var hook in HookShouldUpdatePosition)
			{
				if (!hook(projectile))
				{
					return false;
				}
			}
			return true;
		}
		//in Terraria.Projectile.Update before adjusting velocity to tile collisions add
		//  ProjectileLoader.TileCollideStyle(this, ref num25, ref num26, ref flag4);
		public static void TileCollideStyle(Projectile projectile, ref int width, ref int height, ref bool fallThrough)
		{
			projectile.modProjectile?.TileCollideStyle(ref width, ref height, ref fallThrough);

			foreach (var hook in HookTileCollideStyle)
			{
				hook(projectile, ref width, ref height, ref fallThrough);
			}
		}
		//in Terraria.Projectile.Update before if/else chain for tile collide behavior add
		//  if(!ProjectileLoader.OnTileCollide(this, velocity)) { } else
		public static bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
		{
			foreach (var hook in HookOnTileCollide)
			{
				if (!hook(projectile, oldVelocity))
				{
					return false;
				}
			}
			if (projectile.modProjectile != null)
			{
				return projectile.modProjectile.OnTileCollide(oldVelocity);
			}
			return true;
		}

		//in Terraria.Projectile.CanCutTiles, change to
		//    if (!ProjectileLoader.CanCutTiles(this).HasValue)
		//    {
		//        return (this.aiStyle != 45 && this.aiStyle != 92 && this.aiStyle != 105 && this.aiStyle != 106 && this.type != 463 && this.type != 69 && this.type != 70 && this.type != 621 && this.type != 10 && this.type != 11 && this.type != 379 && this.type != 407 && this.type != 476 && this.type != 623 && (this.type< 625 || this.type> 628));
		//    }
		//    else return (ProjectileLoader.CanCutTiles(this).Value);
		//when it returns null, it does the vanilla check
		public static bool? CanCutTiles(Projectile projectile)
		{
			foreach (var hook in HookCanCutTiles)
			{
				return (hook(projectile));
			}
			if (projectile.modProjectile != null)
			{
				return projectile.modProjectile.CanCutTiles();
			}
			return null;
		}

		//in Terraria.Projectile.Kill before if statements determining kill behavior add
		//  if(!ProjectileLoader.PreKill(this, num)) { this.active = false; return; }
		public static bool PreKill(Projectile projectile, int timeLeft)
		{
			foreach (var hook in HookPreKill)
			{
				if (!hook(projectile, timeLeft))
				{
					return false;
				}
			}
			if (projectile.modProjectile != null)
			{
				return projectile.modProjectile.PreKill(timeLeft);
			}
			return true;
		}
		//at end of Terraria.Projectile.Kill before setting active to false add
		//  ProjectileLoader.Kill(this, num);
		public static void Kill(Projectile projectile, int timeLeft)
		{
			projectile.modProjectile?.Kill(timeLeft);

			foreach (var hook in HookKill)
			{
				hook(projectile, timeLeft);
			}
		}

		public static bool CanDamage(Projectile projectile)
		{
			if (projectile.modProjectile != null && !projectile.modProjectile.CanDamage())
			{
				return false;
			}
			foreach (var hook in HookCanDamage)
			{
				if (!hook(projectile))
				{
					return false;
				}
			}
			return true;
		}

		public static bool MinionContactDamage(Projectile projectile)
		{
			if (projectile.modProjectile != null && projectile.modProjectile.MinionContactDamage())
			{
				return true;
			}
			foreach (var hook in HookMinionContactDamage)
			{
				if (hook(projectile))
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
			foreach (var hook in HookCanHitNPC)
			{
				bool? canHit = hook(projectile, target);
				if (canHit.HasValue && !canHit.Value)
				{
					return false;
				}
				if (canHit.HasValue)
				{
					flag = canHit.Value;
				}
			}
			if (projectile.modProjectile != null)
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
		public static void ModifyHitNPC(Projectile projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			projectile.modProjectile?.ModifyHitNPC(target, ref damage, ref knockback, ref crit, ref hitDirection);

			foreach (var hook in HookModifyHitNPC)
			{
				hook(projectile, target, ref damage, ref knockback, ref crit, ref hitDirection);
			}
		}
		//in Terraria.Projectile.Damage before penetration check for NPCs call this
		public static void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit)
		{
			projectile.modProjectile?.OnHitNPC(target, damage, knockback, crit);

			foreach (var hook in HookOnHitNPC)
			{
				hook(projectile, target, damage, knockback, crit);
			}
		}
		//in Terraria.Projectile.Damage add this before collision check for pvp damage
		public static bool CanHitPvp(Projectile projectile, Player target)
		{
			foreach (var hook in HookCanHitPvp)
			{
				if (!hook(projectile, target))
				{
					return false;
				}
			}
			if (projectile.modProjectile != null)
			{
				return projectile.modProjectile.CanHitPvp(target);
			}
			return true;
		}
		//in Terraria.Projectile.Damage for pvp damage call this after damage var
		public static void ModifyHitPvp(Projectile projectile, Player target, ref int damage, ref bool crit)
		{
			projectile.modProjectile?.ModifyHitPvp(target, ref damage, ref crit);

			foreach (var hook in HookModifyHitPvp)
			{
				hook(projectile, target, ref damage, ref crit);
			}
		}
		//in Terraria.Projectile.Damage for pvp damage call this before net message stuff
		public static void OnHitPvp(Projectile projectile, Player target, int damage, bool crit)
		{
			projectile.modProjectile?.OnHitPvp(target, damage, crit);

			foreach (var hook in HookOnHitPvp)
			{
				hook(projectile, target, damage, crit);
			}
		}
		//in Terraria.Projectile.Damage for damaging my player, add this before collision check
		public static bool CanHitPlayer(Projectile projectile, Player target)
		{
			foreach (var hook in HookCanHitPlayer)
			{
				if (!hook(projectile, target))
				{
					return false;
				}
			}
			if (projectile.modProjectile != null)
			{
				return projectile.modProjectile.CanHitPlayer(target);
			}
			return true;
		}
		//in Terraria.Projectile.Damage for damaging my player, call this after damage variation and add local crit variable
		public static void ModifyHitPlayer(Projectile projectile, Player target, ref int damage, ref bool crit)
		{
			projectile.modProjectile?.ModifyHitPlayer(target, ref damage, ref crit);

			foreach (var hook in HookModifyHitPlayer)
			{
				hook(projectile, target, ref damage, ref crit);
			}
		}
		//in Terraria.Projectile.Damage for damaging my player before decreasing projectile penetration call this
		//  and assign return value from Player.Hurt to local variable to pass as a parameter
		public static void OnHitPlayer(Projectile projectile, Player target, int damage, bool crit)
		{
			projectile.modProjectile?.OnHitPlayer(target, damage, crit);

			foreach (var hook in HookOnHitPlayer)
			{
				hook(projectile, target, damage, crit);
			}
		}
		//in Terraria.Projectile.Colliding after modifying myRect add
		//  bool? modColliding = ProjectileLoader.Colliding(this, myRect, targetRect);
		//  if(modColliding.HasValue) { return modColliding.Value; }
		public static bool? Colliding(Projectile projectile, Rectangle projHitbox, Rectangle targetHitbox)
		{
			foreach (var hook in HookColliding)
			{
				bool? colliding = hook(projectile, projHitbox, targetHitbox);
				if (colliding.HasValue)
				{
					return colliding.Value;
				}
			}
			return projectile.modProjectile?.Colliding(projHitbox, targetHitbox);
		}

		public static void DrawHeldProjInFrontOfHeldItemAndArms(Projectile projectile, ref bool flag)
		{
			if (projectile.modProjectile != null)
			{
				flag = projectile.modProjectile.drawHeldProjInFrontOfHeldItemAndArms;
			}
		}
		//at beginning of Terraria.Projectile.GetAlpha add
		//  Color? modColor = ProjectileLoader.GetAlpha(this, newColor);
		//  if(modColor.HasValue) { return modColor.Value; }
		public static Color? GetAlpha(Projectile projectile, Color lightColor)
		{
			foreach (var hook in HookGetAlpha)
			{
				Color? color = hook(projectile, lightColor);
				if (color.HasValue)
				{
					return color;
				}
			}
			return projectile.modProjectile?.GetAlpha(lightColor);
		}
		//in Terraria.Main.DrawProj after setting offsets call
		//  ProjectileLoader.DrawOffset(projectile, ref num148, ref num149);
		public static void DrawOffset(Projectile projectile, ref int offsetX, ref int offsetY, ref float originX)
		{
			if (projectile.modProjectile != null)
			{
				offsetX = projectile.modProjectile.drawOffsetX;
				offsetY = -projectile.modProjectile.drawOriginOffsetY;
				originX += projectile.modProjectile.drawOriginOffsetX;
			}
		}

		public static bool PreDrawExtras(Projectile projectile, SpriteBatch spriteBatch)
		{
			foreach (var hook in HookPreDrawExtras)
			{
				if (!hook(projectile, spriteBatch))
				{
					return false;
				}
			}
			if (projectile.modProjectile != null)
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
			foreach (var hook in HookPreDraw)
			{
				if (!hook(projectile, spriteBatch, lightColor))
				{
					return false;
				}
			}
			if (projectile.modProjectile != null)
			{
				return projectile.modProjectile.PreDraw(spriteBatch, lightColor);
			}
			return true;
		}
		//at end of Terraria.Main.DrawProj call ProjectileLoader.PostDraw(projectile, Main.spriteBatch, color25);
		public static void PostDraw(Projectile projectile, SpriteBatch spriteBatch, Color lightColor)
		{
			projectile.modProjectile?.PostDraw(spriteBatch, lightColor);

			foreach (var hook in HookPostDraw)
			{
				hook(projectile, spriteBatch, lightColor);
			}
		}

		public static bool? CanUseGrapple(int type, Player player)
		{
			var flag = GetProjectile(type)?.CanUseGrapple(player);

			foreach (var hook in HookCanUseGrapple)
			{
				bool? canGrapple = hook(type, player);
				if (canGrapple.HasValue)
				{
					flag = canGrapple;
				}
			}
			return flag;
		}

		public static bool? SingleGrappleHook(int type, Player player)
		{
			bool? flag = GetProjectile(type)?.SingleGrappleHook(player);

			foreach (var hook in HookSingleGrappleHook)
			{
				bool? singleHook = hook(type, player);
				if (singleHook.HasValue)
				{
					flag = singleHook;
				}
			}
			return flag;
		}

		public static void UseGrapple(Player player, ref int type)
		{
			GetProjectile(type)?.UseGrapple(player, ref type);

			foreach (var hook in HookUseGrapple)
			{
				hook(player, ref type);
			}
		}

		public static bool GrappleOutOfRange(float distance, Projectile projectile)
		{
			return distance > projectile.modProjectile?.GrappleRange();
		}

		public static void NumGrappleHooks(Projectile projectile, Player player, ref int numHooks)
		{
			projectile.modProjectile?.NumGrappleHooks(player, ref numHooks);

			foreach (var hook in HookNumGrappleHooks)
			{
				hook(projectile, player, ref numHooks);
			}
		}

		public static void GrappleRetreatSpeed(Projectile projectile, Player player, ref float speed)
		{
			projectile.modProjectile?.GrappleRetreatSpeed(player, ref speed);

			foreach (var hook in HookGrappleRetreatSpeed)
			{
				hook(projectile, player, ref speed);
			}
		}

		internal static void DrawBehind(Projectile projectile, int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
		{
			projectile.modProjectile?.DrawBehind(index, drawCacheProjsBehindNPCsAndTiles, drawCacheProjsBehindNPCs, drawCacheProjsBehindProjectiles, drawCacheProjsOverWiresUI);

			foreach (var hook in HookDrawBehind)
			{
				hook(projectile, index, drawCacheProjsBehindNPCsAndTiles, drawCacheProjsBehindNPCs, drawCacheProjsBehindProjectiles, drawCacheProjsOverWiresUI);
			}
		}
	}
}
