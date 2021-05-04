using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using HookList = Terraria.ModLoader.Core.HookList<Terraria.ModLoader.GlobalProjectile>;

namespace Terraria.ModLoader
{
	//todo: further documentation
	/// <summary>
	/// This serves as the central class from which projectile-related functions are carried out. It also stores a list of mod projectiles by ID.
	/// </summary>
	public static class ProjectileLoader
	{
		internal static readonly IList<ModProjectile> projectiles = new List<ModProjectile>();
		internal static readonly IList<GlobalProjectile> globalProjectiles = new List<GlobalProjectile>();

		private static int nextProjectile = ProjectileID.Count;
		private static readonly List<HookList> hooks = new List<HookList>();
		private static readonly List<HookList> modHooks = new List<HookList>();
		private static Instanced<GlobalProjectile>[] globalProjectilesArray = new Instanced<GlobalProjectile>[0];

		private static HookList AddHook<F>(Expression<Func<GlobalProjectile, F>> func) {
			var hook = new HookList(ModLoader.Method(func));

			hooks.Add(hook);

			return hook;
		}

		public static T AddModHook<T>(T hook) where T : HookList {
			hook.Update(globalProjectiles);

			modHooks.Add(hook);

			return hook;
		}

		internal static int ReserveProjectileID() {
			if (ModNet.AllowVanillaClients)
				throw new Exception("Adding projectiles breaks vanilla client compatibility");

			return nextProjectile++;
		}

		public static int ProjectileCount => nextProjectile;

		/// <summary>
		/// Gets the ModProjectile instance corresponding to the specified type.
		/// </summary>
		/// <param name="type">The type of the projectile</param>
		/// <returns>The ModProjectile instance in the projectiles array, null if not found.</returns>
		public static ModProjectile GetProjectile(int type) {
			return type >= ProjectileID.Count && type < ProjectileCount ? projectiles[type - ProjectileID.Count] : null;
		}
		//change initial size of Terraria.Player.ownedProjectileCounts to ProjectileLoader.ProjectileCount()
		internal static void ResizeArrays() {
			//Textures
			Array.Resize(ref TextureAssets.Projectile, nextProjectile);

			//Sets
			LoaderUtils.ResetStaticMembers(typeof(ProjectileID), true);

			//Etc
			Array.Resize(ref Main.projHostile, nextProjectile);
			Array.Resize(ref Main.projHook, nextProjectile);
			Array.Resize(ref Main.projFrames, nextProjectile);
			Array.Resize(ref Main.projPet, nextProjectile);
			Array.Resize(ref Lang._projectileNameCache, nextProjectile);

			for (int k = ProjectileID.Count; k < nextProjectile; k++) {
				Main.projFrames[k] = 1;
				Lang._projectileNameCache[k] = LocalizedText.Empty;
			}

			Array.Resize(ref Projectile.perIDStaticNPCImmunity, nextProjectile);

			for (int i = 0; i < nextProjectile; i++) {
				Projectile.perIDStaticNPCImmunity[i] = new uint[200];
			}

			globalProjectilesArray = globalProjectiles
				.Select(g => new Instanced<GlobalProjectile>(g.index, g))
				.ToArray();

			foreach (var hook in hooks.Union(modHooks)) {
				hook.Update(globalProjectiles);
			}
		}

		internal static void Unload() {
			projectiles.Clear();
			nextProjectile = ProjectileID.Count;
			globalProjectiles.Clear();
			modHooks.Clear();
		}

		internal static bool IsModProjectile(Projectile projectile) {
			return projectile.type >= ProjectileID.Count;
		}

		private static HookList HookSetDefaults = AddHook<Action<Projectile>>(g => g.SetDefaults);

		internal static void SetDefaults(Projectile projectile, bool createModProjectile = true) {
			if (IsModProjectile(projectile) && createModProjectile) {
				projectile.ModProjectile = GetProjectile(projectile.type).NewInstance(projectile);
			}

			GlobalProjectile Instantiate(GlobalProjectile g)
				=> g.InstancePerEntity ? g.NewInstance(projectile) : g;

			LoaderUtils.InstantiateGlobals(projectile, globalProjectiles, ref projectile.globalProjectiles, Instantiate, () => {
				projectile.ModProjectile?.SetDefaults();
			});

			foreach (GlobalProjectile g in HookSetDefaults.Enumerate(projectile.globalProjectiles)) {
				g.SetDefaults(projectile);
			}
		}

		//in Terraria.Projectile rename AI to VanillaAI then make AI call ProjectileLoader.ProjectileAI(this)
		public static void ProjectileAI(Projectile projectile) {
			if (PreAI(projectile)) {
				int type = projectile.type;
				bool useAiType = projectile.ModProjectile != null && projectile.ModProjectile.AIType > 0;
				if (useAiType) {
					projectile.type = projectile.ModProjectile.AIType;
				}
				projectile.VanillaAI();
				if (useAiType) {
					projectile.type = type;
				}
				AI(projectile);
			}
			PostAI(projectile);
		}

		private static HookList HookPreAI = AddHook<Func<Projectile, bool>>(g => g.PreAI);

		public static bool PreAI(Projectile projectile) {
			bool result = true;

			foreach (GlobalProjectile g in HookPreAI.Enumerate(projectile.globalProjectiles)) {
				result &= g.PreAI(projectile);
			}

			if (result && projectile.ModProjectile != null) {
				return projectile.ModProjectile.PreAI();
			}

			return result;
		}

		private static HookList HookAI = AddHook<Action<Projectile>>(g => g.AI);

		public static void AI(Projectile projectile) {
			projectile.ModProjectile?.AI();

			foreach (GlobalProjectile g in HookAI.Enumerate(projectile.globalProjectiles)) {
				g.AI(projectile);
			}
		}

		private static HookList HookPostAI = AddHook<Action<Projectile>>(g => g.PostAI);

		public static void PostAI(Projectile projectile) {
			projectile.ModProjectile?.PostAI();

			foreach (GlobalProjectile g in HookPostAI.Enumerate(projectile.globalProjectiles)) {
				g.PostAI(projectile);
			}
		}
		//in Terraria.NetMessage.SendData at end of case 27 call
		//  ProjectileLoader.SendExtraAI(projectile, writer, ref bb14);
		public static byte[] SendExtraAI(Projectile projectile, ref BitsByte flags) {
			if (projectile.ModProjectile != null) {
				byte[] data;
				using (MemoryStream stream = new MemoryStream()) {
					using (BinaryWriter modWriter = new BinaryWriter(stream)) {
						projectile.ModProjectile.SendExtraAI(modWriter);
						modWriter.Flush();
						data = stream.ToArray();
					}
				}
				if (data.Length > 0) {
					flags[Projectile.maxAI + 1] = true;
				}
				return data;
			}
			return new byte[0];
		}
		//in Terraria.MessageBuffer.GetData for case 27 after reading all data add
		//  byte[] extraAI = ProjectileLoader.ReadExtraAI(reader, bitsByte14);
		public static byte[] ReadExtraAI(BinaryReader reader, BitsByte flags) {
			if (flags[Projectile.maxAI + 1]) {
				return reader.ReadBytes(reader.ReadByte());
			}
			return new byte[0];
		}
		//in Terraria.MessageBuffer.GetData for case 27 before calling ProjectileFixDesperation add
		//  ProjectileLoader.ReceiveExtraAI(projectile, extraAI);
		public static void ReceiveExtraAI(Projectile projectile, byte[] extraAI) {
			if (extraAI.Length > 0 && projectile.ModProjectile != null) {
				using (MemoryStream stream = new MemoryStream(extraAI)) {
					using (BinaryReader reader = new BinaryReader(stream)) {
						projectile.ModProjectile.ReceiveExtraAI(reader);
					}
				}
			}
		}

		private static HookList HookShouldUpdatePosition = AddHook<Func<Projectile, bool>>(g => g.ShouldUpdatePosition);

		public static bool ShouldUpdatePosition(Projectile projectile) {
			if (IsModProjectile(projectile) && !projectile.ModProjectile.ShouldUpdatePosition()) {
				return false;
			}

			foreach (GlobalProjectile g in HookShouldUpdatePosition.Enumerate(projectile.globalProjectiles)) {
				if (!g.ShouldUpdatePosition(projectile)) {
					return false;
				}
			}

			return true;
		}

		private delegate bool DelegateTileCollideStyle(Projectile projectile, ref int width, ref int height, ref bool fallThrough);
		private static HookList HookTileCollideStyle = AddHook<DelegateTileCollideStyle>(g => g.TileCollideStyle);

		public static bool TileCollideStyle(Projectile projectile, ref int width, ref int height, ref bool fallThrough) {
			if (IsModProjectile(projectile) && !projectile.ModProjectile.TileCollideStyle(ref width, ref height, ref fallThrough)) {
				return false;
			}

			foreach (GlobalProjectile g in HookTileCollideStyle.Enumerate(projectile.globalProjectiles)) {
				if (!g.TileCollideStyle(projectile, ref width, ref height, ref fallThrough)) {
					return false;
				}
			}

			return true;
		}

		private static HookList HookOnTileCollide = AddHook<Func<Projectile, Vector2, bool>>(g => g.OnTileCollide);

		public static bool OnTileCollide(Projectile projectile, Vector2 oldVelocity) {
			bool result = true;

			foreach (GlobalProjectile g in HookOnTileCollide.Enumerate(projectile.globalProjectiles)) {
				result &= g.OnTileCollide(projectile, oldVelocity);
			}

			if (result && projectile.ModProjectile != null) {
				return projectile.ModProjectile.OnTileCollide(oldVelocity);
			}

			return result;
		}

		private static HookList HookCanCutTiles = AddHook<Func<Projectile, bool?>>(g => g.CanCutTiles);

		public static bool? CanCutTiles(Projectile projectile) {
			foreach (GlobalProjectile g in HookCanCutTiles.Enumerate(projectile.globalProjectiles)) {
				bool? canCutTiles = g.CanCutTiles(projectile);

				if (canCutTiles.HasValue) {
					return canCutTiles.Value;
				}
			}

			return projectile.ModProjectile?.CanCutTiles();
		}

		private static HookList HookCutTiles = AddHook<Action<Projectile>>(g => g.CutTiles);

		public static void CutTiles(Projectile projectile) {

			foreach (GlobalProjectile g in HookCutTiles.Enumerate(projectile.globalProjectiles)) {
				g.CutTiles(projectile);
			}

			projectile.ModProjectile?.CutTiles();
		}

		private static HookList HookPreKill = AddHook<Func<Projectile, int, bool>>(g => g.PreKill);

		public static bool PreKill(Projectile projectile, int timeLeft) {
			bool result = true;

			foreach (GlobalProjectile g in HookPreKill.Enumerate(projectile.globalProjectiles)) {
				result &= g.PreKill(projectile, timeLeft);
			}

			if (result && projectile.ModProjectile != null) {
				return projectile.ModProjectile.PreKill(timeLeft);
			}

			return result;
		}

		private static HookList HookKill = AddHook<Action<Projectile, int>>(g => g.Kill);

		public static void Kill(Projectile projectile, int timeLeft) {
			projectile.ModProjectile?.Kill(timeLeft);

			foreach (GlobalProjectile g in HookKill.Enumerate(projectile.globalProjectiles)) {
				g.Kill(projectile, timeLeft);
			}
		}

		private static HookList HookCanDamage = AddHook<Func<Projectile, bool?>>(g => g.CanDamage);

		public static bool? CanDamage(Projectile projectile) {
			bool? result = null;

			foreach (GlobalProjectile g in HookCanDamage.Enumerate(projectile.globalProjectiles)) {
				bool? canDamage = g.CanDamage(projectile);

				if (canDamage.HasValue) {
					if (!canDamage.Value) {
						return false;
					}

					result = true;
				}
			}

			return result ?? projectile.ModProjectile?.CanDamage();
		}

		private static HookList HookMinionContactDamage = AddHook<Func<Projectile, bool>>(g => g.MinionContactDamage);

		public static bool MinionContactDamage(Projectile projectile) {
			if (projectile.ModProjectile != null && !projectile.ModProjectile.MinionContactDamage()) {
				return false;
			}

			foreach (GlobalProjectile g in HookMinionContactDamage.Enumerate(projectile.globalProjectiles)) {
				if (!g.MinionContactDamage(projectile)) {
					return false;
				}
			}

			return true;
		}

		private delegate void DelegateModifyDamageHitbox(Projectile projectile, ref Rectangle hitbox);
		private static HookList HookModifyDamageHitbox = AddHook<DelegateModifyDamageHitbox>(g => g.ModifyDamageHitbox);

		public static void ModifyDamageHitbox(Projectile projectile, ref Rectangle hitbox) {
			projectile.ModProjectile?.ModifyDamageHitbox(ref hitbox);

			foreach (GlobalProjectile g in HookModifyDamageHitbox.Enumerate(projectile.globalProjectiles)) {
				g.ModifyDamageHitbox(projectile, ref hitbox);
			}
		}

		private static HookList HookCanHitNPC = AddHook<Func<Projectile, NPC, bool?>>(g => g.CanHitNPC);

		public static bool? CanHitNPC(Projectile projectile, NPC target) {
			bool? flag = null;

			foreach (GlobalProjectile g in HookCanHitNPC.Enumerate(projectile.globalProjectiles)) {
				bool? canHit = g.CanHitNPC(projectile, target);

				if (canHit.HasValue && !canHit.Value) {
					return false;
				}

				if (canHit.HasValue) {
					flag = canHit.Value;
				}
			}

			if (projectile.ModProjectile != null) {
				bool? canHit = projectile.ModProjectile.CanHitNPC(target);

				if (canHit.HasValue && !canHit.Value) {
					return false;
				}
				if (canHit.HasValue) {
					flag = canHit.Value;
				}
			}
			return flag;
		}

		private delegate void DelegateModifyHitNPC(Projectile projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection);
		private static HookList HookModifyHitNPC = AddHook<DelegateModifyHitNPC>(g => g.ModifyHitNPC);

		public static void ModifyHitNPC(Projectile projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
			projectile.ModProjectile?.ModifyHitNPC(target, ref damage, ref knockback, ref crit, ref hitDirection);

			foreach (GlobalProjectile g in HookModifyHitNPC.Enumerate(projectile.globalProjectiles)) {
				g.ModifyHitNPC(projectile, target, ref damage, ref knockback, ref crit, ref hitDirection);
			}
		}

		private static HookList HookOnHitNPC = AddHook<Action<Projectile, NPC, int, float, bool>>(g => g.OnHitNPC);

		public static void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit) {
			projectile.ModProjectile?.OnHitNPC(target, damage, knockback, crit);

			foreach (GlobalProjectile g in HookOnHitNPC.Enumerate(projectile.globalProjectiles)) {
				g.OnHitNPC(projectile, target, damage, knockback, crit);
			}
		}

		private static HookList HookCanHitPvp = AddHook<Func<Projectile, Player, bool>>(g => g.CanHitPvp);

		public static bool CanHitPvp(Projectile projectile, Player target) {
			foreach (GlobalProjectile g in HookCanHitPvp.Enumerate(projectile.globalProjectiles)) {
				if (!g.CanHitPvp(projectile, target)) {
					return false;
				}
			}

			if (projectile.ModProjectile != null) {
				return projectile.ModProjectile.CanHitPvp(target);
			}

			return true;
		}

		private delegate void DelegateModifyHitPvp(Projectile projectile, Player target, ref int damage, ref bool crit);
		private static HookList HookModifyHitPvp = AddHook<DelegateModifyHitPvp>(g => g.ModifyHitPvp);

		public static void ModifyHitPvp(Projectile projectile, Player target, ref int damage, ref bool crit) {
			projectile.ModProjectile?.ModifyHitPvp(target, ref damage, ref crit);

			foreach (GlobalProjectile g in HookModifyHitPvp.Enumerate(projectile.globalProjectiles)) {
				g.ModifyHitPvp(projectile, target, ref damage, ref crit);
			}
		}

		private static HookList HookOnHitPvp = AddHook<Action<Projectile, Player, int, bool>>(g => g.OnHitPvp);

		public static void OnHitPvp(Projectile projectile, Player target, int damage, bool crit) {
			projectile.ModProjectile?.OnHitPvp(target, damage, crit);

			foreach (GlobalProjectile g in HookOnHitPvp.Enumerate(projectile.globalProjectiles)) {
				g.OnHitPvp(projectile, target, damage, crit);
			}
		}

		private static HookList HookCanHitPlayer = AddHook<Func<Projectile, Player, bool>>(g => g.CanHitPlayer);

		public static bool CanHitPlayer(Projectile projectile, Player target) {
			foreach (GlobalProjectile g in HookCanHitPlayer.Enumerate(projectile.globalProjectiles)) {
				if (!g.CanHitPlayer(projectile, target)) {
					return false;
				}
			}

			if (projectile.ModProjectile != null) {
				return projectile.ModProjectile.CanHitPlayer(target);
			}

			return true;
		}

		private delegate void DelegateModifyHitPlayer(Projectile projectile, Player target, ref int damage, ref bool crit);
		private static HookList HookModifyHitPlayer = AddHook<DelegateModifyHitPlayer>(g => g.ModifyHitPlayer);

		public static void ModifyHitPlayer(Projectile projectile, Player target, ref int damage, ref bool crit) {
			projectile.ModProjectile?.ModifyHitPlayer(target, ref damage, ref crit);

			foreach (GlobalProjectile g in HookModifyHitPlayer.Enumerate(projectile.globalProjectiles)) {
				g.ModifyHitPlayer(projectile, target, ref damage, ref crit);
			}
		}

		private static HookList HookOnHitPlayer = AddHook<Action<Projectile, Player, int, bool>>(g => g.OnHitPlayer);

		public static void OnHitPlayer(Projectile projectile, Player target, int damage, bool crit) {
			projectile.ModProjectile?.OnHitPlayer(target, damage, crit);

			foreach (GlobalProjectile g in HookOnHitPlayer.Enumerate(projectile.globalProjectiles)) {
				g.OnHitPlayer(projectile, target, damage, crit);
			}
		}

		private static HookList HookColliding = AddHook<Func<Projectile, Rectangle, Rectangle, bool?>>(g => g.Colliding);

		public static bool? Colliding(Projectile projectile, Rectangle projHitbox, Rectangle targetHitbox) {
			foreach (GlobalProjectile g in HookColliding.Enumerate(projectile.globalProjectiles)) {
				bool? colliding = g.Colliding(projectile, projHitbox, targetHitbox);

				if (colliding.HasValue) {
					return colliding.Value;
				}
			}

			return projectile.ModProjectile?.Colliding(projHitbox, targetHitbox);
		}

		public static void DrawHeldProjInFrontOfHeldItemAndArms(Projectile projectile, ref bool flag) {
			if (projectile.ModProjectile != null) {
				flag = projectile.ModProjectile.DrawHeldProjInFrontOfHeldItemAndArms;
			}
		}

		public static void ModifyFishingLine(Projectile projectile, ref float polePosX, ref float polePosY, ref Color lineColor) {
			if (projectile.ModProjectile == null)
				return;

			Vector2 lineOriginOffset = Vector2.Zero;
			Player player = Main.player[projectile.owner];

			projectile.ModProjectile?.ModifyFishingLine(ref lineOriginOffset, ref lineColor);

			polePosX += lineOriginOffset.X * player.direction;
			if (player.direction < 0)
				polePosX -= 13f;
			polePosY += lineOriginOffset.Y * player.gravDir;
		}

		private static HookList HookGetAlpha = AddHook<Func<Projectile, Color, Color?>>(g => g.GetAlpha);

		public static Color? GetAlpha(Projectile projectile, Color lightColor) {
			foreach (GlobalProjectile g in HookGetAlpha.Enumerate(projectile.globalProjectiles)) {
				Color? color = g.GetAlpha(projectile, lightColor);

				if (color.HasValue) {
					return color;
				}
			}

			return projectile.ModProjectile?.GetAlpha(lightColor);
		}

		public static void DrawOffset(Projectile projectile, ref int offsetX, ref int offsetY, ref float originX) {
			if (projectile.ModProjectile != null) {
				offsetX = projectile.ModProjectile.DrawOffsetX;
				offsetY = -projectile.ModProjectile.DrawOriginOffsetY;
				originX += projectile.ModProjectile.DrawOriginOffsetX;
			}
		}

		private static HookList HookPreDrawExtras = AddHook<Func<Projectile, bool>>(g => g.PreDrawExtras);

		public static bool PreDrawExtras(Projectile projectile) {
			bool result = true;

			foreach (GlobalProjectile g in HookPreDrawExtras.Enumerate(projectile.globalProjectiles)) {
				result &= g.PreDrawExtras(projectile);
			}

			if (result && projectile.ModProjectile != null) {
				return projectile.ModProjectile.PreDrawExtras();
			}

			return result;
		}

		private delegate bool DelegatePreDraw(Projectile projectile, ref Color lightColor);
		private static HookList HookPreDraw = AddHook<DelegatePreDraw>(g => g.PreDraw);

		public static bool PreDraw(Projectile projectile, ref Color lightColor) {
			bool result = true;

			foreach (GlobalProjectile g in HookPreDraw.Enumerate(projectile.globalProjectiles)) {
				result &= g.PreDraw(projectile, ref lightColor);
			}

			if (result && projectile.ModProjectile != null) {
				return projectile.ModProjectile.PreDraw(ref lightColor);
			}

			return result;
		}

		private static HookList HookPostDraw = AddHook<Action<Projectile, Color>>(g => g.PostDraw);

		public static void PostDraw(Projectile projectile, Color lightColor) {
			projectile.ModProjectile?.PostDraw(lightColor);

			foreach (GlobalProjectile g in HookPostDraw.Enumerate(projectile.globalProjectiles)) {
				g.PostDraw(projectile, lightColor);
			}
		}

		private static HookList HookCanUseGrapple = AddHook<Func<int, Player, bool?>>(g => g.CanUseGrapple);

		public static bool? CanUseGrapple(int type, Player player) {
			bool? flag = GetProjectile(type)?.CanUseGrapple(player);

			foreach (GlobalProjectile g in HookCanUseGrapple.Enumerate(globalProjectilesArray)) {
				bool? canGrapple = g.CanUseGrapple(type, player);

				if (canGrapple.HasValue) {
					flag = canGrapple;
				}
			}

			return flag;
		}

		private static HookList HookSingleGrappleHook = AddHook<Func<int, Player, bool?>>(g => g.SingleGrappleHook);

		public static bool? SingleGrappleHook(int type, Player player) {
			bool? flag = GetProjectile(type)?.SingleGrappleHook(player);

			foreach (GlobalProjectile g in HookSingleGrappleHook.Enumerate(globalProjectilesArray)) {
				bool? singleHook = g.SingleGrappleHook(type, player);

				if (singleHook.HasValue) {
					flag = singleHook;
				}
			}

			return flag;
		}

		private delegate void DelegateUseGrapple(Player player, ref int type);
		private static HookList HookUseGrapple = AddHook<DelegateUseGrapple>(g => g.UseGrapple);

		public static void UseGrapple(Player player, ref int type) {
			GetProjectile(type)?.UseGrapple(player, ref type);

			foreach (GlobalProjectile g in HookUseGrapple.Enumerate(globalProjectilesArray)) {
				g.UseGrapple(player, ref type);
			}
		}

		public static bool GrappleOutOfRange(float distance, Projectile projectile) {
			return distance > projectile.ModProjectile?.GrappleRange();
		}

		private delegate void DelegateNumGrappleHooks(Projectile projectile, Player player, ref int numHooks);
		private static HookList HookNumGrappleHooks = AddHook<DelegateNumGrappleHooks>(g => g.NumGrappleHooks);

		public static void NumGrappleHooks(Projectile projectile, Player player, ref int numHooks) {
			projectile.ModProjectile?.NumGrappleHooks(player, ref numHooks);

			foreach (GlobalProjectile g in HookNumGrappleHooks.Enumerate(projectile.globalProjectiles)) {
				g.NumGrappleHooks(projectile, player, ref numHooks);
			}
		}

		private delegate void DelegateGrappleRetreatSpeed(Projectile projectile, Player player, ref float speed);
		private static HookList HookGrappleRetreatSpeed = AddHook<DelegateGrappleRetreatSpeed>(g => g.GrappleRetreatSpeed);

		public static void GrappleRetreatSpeed(Projectile projectile, Player player, ref float speed) {
			projectile.ModProjectile?.GrappleRetreatSpeed(player, ref speed);

			foreach (GlobalProjectile g in HookGrappleRetreatSpeed.Enumerate(projectile.globalProjectiles)) {
				g.GrappleRetreatSpeed(projectile, player, ref speed);
			}
		}

		private delegate void DelegateGrapplePullSpeed(Projectile projectile, Player player, ref float speed);
		private static HookList HookGrapplePullSpeed = AddHook<DelegateGrapplePullSpeed>(g => g.GrapplePullSpeed);

		public static void GrapplePullSpeed(Projectile projectile, Player player, ref float speed) {
			projectile.ModProjectile?.GrapplePullSpeed(player, ref speed);

			foreach (GlobalProjectile g in HookGrapplePullSpeed.Enumerate(projectile.globalProjectiles)) {
				g.GrapplePullSpeed(projectile, player, ref speed);
			}
		}

		private delegate void DelegateGrappleTargetPoint(Projectile projectile, Player player, ref float grappleX, ref float grappleY);
		private static HookList HookGrappleTargetPoint = AddHook<DelegateGrappleTargetPoint>(g => g.GrappleTargetPoint);

		public static void GrappleTargetPoint(Projectile projectile, Player player, ref float grappleX, ref float grappleY) {
			projectile.ModProjectile?.GrappleTargetPoint(player, ref grappleX, ref grappleY);

			foreach (GlobalProjectile g in HookGrappleTargetPoint.Enumerate(projectile.globalProjectiles)) {
				g.GrappleTargetPoint(projectile, player, ref grappleX, ref grappleY);
			}
		}

		private static HookList HookDrawBehind = AddHook<Action<Projectile, int, List<int>, List<int>, List<int>, List<int>, List<int>>>(g => g.DrawBehind);

		internal static void DrawBehind(Projectile projectile, int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			projectile.ModProjectile?.DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);

			foreach (GlobalProjectile g in HookDrawBehind.Enumerate(projectile.globalProjectiles)) {
				g.DrawBehind(projectile, index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
			}
		}

		private static bool HasMethod(Type t, string method, params Type[] args) {
			return t.GetMethod(method, args).DeclaringType != typeof(GlobalProjectile);
		}

		internal static void VerifyGlobalProjectile(GlobalProjectile projectile) {
			var type = projectile.GetType();

			bool hasInstanceFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				.Any(f => f.DeclaringType.IsSubclassOf(typeof(GlobalProjectile)));

			if (hasInstanceFields) {
				if (!projectile.InstancePerEntity) {
					throw new Exception(type + " has instance fields but does not set InstancePerEntity to true. Either use static fields, or per instance globals");
				}
			}
		}
	}
}
