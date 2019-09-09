using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Terraria.ID;
using Terraria.Localization;

namespace Terraria.ModLoader
{
	//todo: further documentation
	/// <summary>
	/// This serves as the central class from which projectile-related functions are carried out. It also stores a list of mod projectiles by ID.
	/// </summary>
	public static class ProjectileLoader
	{
		private static int nextProjectile = ProjectileID.Count;
		internal static readonly IList<ModProjectile> projectiles = new List<ModProjectile>();
		internal static readonly IList<GlobalProjectile> globalProjectiles = new List<GlobalProjectile>();
		internal static GlobalProjectile[] InstancedGlobals = new GlobalProjectile[0];
		internal static readonly IDictionary<string, int> globalIndexes = new Dictionary<string, int>();
		internal static readonly IDictionary<Type, int> globalIndexesByType = new Dictionary<Type, int>();

		private class HookList
		{
			public GlobalProjectile[] arr = new GlobalProjectile[0];
			public readonly MethodInfo method;

			public HookList(MethodInfo method) {
				this.method = method;
			}
		}

		private static List<HookList> hooks = new List<HookList>();

		private static HookList AddHook<F>(Expression<Func<GlobalProjectile, F>> func) {
			var hook = new HookList(ModLoader.Method(func));
			hooks.Add(hook);
			return hook;
		}

		internal static int ReserveProjectileID() {
			if (ModNet.AllowVanillaClients) throw new Exception("Adding projectiles breaks vanilla client compatibility");

			int reserveID = nextProjectile;
			nextProjectile++;
			return reserveID;
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
			Array.Resize(ref Main.projectileLoaded, nextProjectile);
			Array.Resize(ref Main.projectileTexture, nextProjectile);
			Array.Resize(ref Main.projHostile, nextProjectile);
			Array.Resize(ref Main.projHook, nextProjectile);
			Array.Resize(ref Main.projFrames, nextProjectile);
			Array.Resize(ref Main.projPet, nextProjectile);
			Array.Resize(ref Lang._projectileNameCache, nextProjectile);
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
			for (int k = ProjectileID.Count; k < nextProjectile; k++) {
				Lang._projectileNameCache[k] = LocalizedText.Empty;
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
			for (int i = 0; i < nextProjectile; i++) {
				Projectile.perIDStaticNPCImmunity[i] = new uint[200];
			}

			InstancedGlobals = globalProjectiles.Where(g => g.InstancePerEntity).ToArray();
			for (int i = 0; i < InstancedGlobals.Length; i++) {
				InstancedGlobals[i].instanceIndex = i;
			}
			foreach (var hook in hooks) {
				hook.arr = ModLoader.BuildGlobalHook(globalProjectiles, hook.method);
			}
		}

		internal static void Unload() {
			projectiles.Clear();
			nextProjectile = ProjectileID.Count;
			globalProjectiles.Clear();
			globalIndexes.Clear();
			globalIndexesByType.Clear();
		}

		internal static bool IsModProjectile(Projectile projectile) {
			return projectile.type >= ProjectileID.Count;
		}

		private static HookList HookSetDefaults = AddHook<Action<Projectile>>(g => g.SetDefaults);

		internal static void SetDefaults(Projectile projectile, bool createModProjectile = true) {
			if (IsModProjectile(projectile) && createModProjectile) {
				projectile.modProjectile = GetProjectile(projectile.type).NewInstance(projectile);
			}
			projectile.globalProjectiles = InstancedGlobals.Select(g => g.NewInstance(projectile)).ToArray();
			projectile.modProjectile?.SetDefaults();
			foreach (GlobalProjectile g in HookSetDefaults.arr) {
				g.Instance(projectile).SetDefaults(projectile);
			}
		}

		internal static GlobalProjectile GetGlobalProjectile(Projectile projectile, Mod mod, string name) {
			int index;
			return globalIndexes.TryGetValue(mod.Name + ':' + name, out index) ? globalProjectiles[index].Instance(projectile) : null;
		}

		internal static GlobalProjectile GetGlobalProjectile(Projectile projectile, Type type) {
			int index;
			return globalIndexesByType.TryGetValue(type, out index) ? (index > -1 ? globalProjectiles[index].Instance(projectile) : null) : null;
		}
		//in Terraria.Projectile rename AI to VanillaAI then make AI call ProjectileLoader.ProjectileAI(this)
		public static void ProjectileAI(Projectile projectile) {
			if (PreAI(projectile)) {
				int type = projectile.type;
				bool useAiType = projectile.modProjectile != null && projectile.modProjectile.aiType > 0;
				if (useAiType) {
					projectile.type = projectile.modProjectile.aiType;
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
			foreach (GlobalProjectile g in HookPreAI.arr) {
				result &= g.Instance(projectile).PreAI(projectile);
			}
			if (result && projectile.modProjectile != null) {
				return projectile.modProjectile.PreAI();
			}
			return result;
		}

		private static HookList HookAI = AddHook<Action<Projectile>>(g => g.AI);

		public static void AI(Projectile projectile) {
			projectile.modProjectile?.AI();

			foreach (GlobalProjectile g in HookAI.arr) {
				g.Instance(projectile).AI(projectile);
			}
		}

		private static HookList HookPostAI = AddHook<Action<Projectile>>(g => g.PostAI);

		public static void PostAI(Projectile projectile) {
			projectile.modProjectile?.PostAI();

			foreach (GlobalProjectile g in HookPostAI.arr) {
				g.Instance(projectile).PostAI(projectile);
			}
		}
		//in Terraria.NetMessage.SendData at end of case 27 call
		//  ProjectileLoader.SendExtraAI(projectile, writer, ref bb14);
		public static byte[] SendExtraAI(Projectile projectile, ref BitsByte flags) {
			if (projectile.modProjectile != null) {
				byte[] data;
				using (MemoryStream stream = new MemoryStream()) {
					using (BinaryWriter modWriter = new BinaryWriter(stream)) {
						projectile.modProjectile.SendExtraAI(modWriter);
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
			if (extraAI.Length > 0 && projectile.modProjectile != null) {
				using (MemoryStream stream = new MemoryStream(extraAI)) {
					using (BinaryReader reader = new BinaryReader(stream)) {
						projectile.modProjectile.ReceiveExtraAI(reader);
					}
				}
			}
		}

		private static HookList HookShouldUpdatePosition = AddHook<Func<Projectile, bool>>(g => g.ShouldUpdatePosition);

		public static bool ShouldUpdatePosition(Projectile projectile) {
			if (IsModProjectile(projectile) && !projectile.modProjectile.ShouldUpdatePosition()) {
				return false;
			}
			foreach (GlobalProjectile g in HookShouldUpdatePosition.arr) {
				if (!g.Instance(projectile).ShouldUpdatePosition(projectile)) {
					return false;
				}
			}
			return true;
		}

		private delegate bool DelegateTileCollideStyle(Projectile projectile, ref int width, ref int height, ref bool fallThrough);
		private static HookList HookTileCollideStyle = AddHook<DelegateTileCollideStyle>(g => g.TileCollideStyle);

		public static bool TileCollideStyle(Projectile projectile, ref int width, ref int height, ref bool fallThrough) {
			if (IsModProjectile(projectile) && !projectile.modProjectile.TileCollideStyle(ref width, ref height, ref fallThrough)) {
				return false;
			}

			foreach (GlobalProjectile g in HookTileCollideStyle.arr) {
				if (!g.Instance(projectile).TileCollideStyle(projectile, ref width, ref height, ref fallThrough)) {
					return false;
				}
			}
			return true;
		}

		private static HookList HookOnTileCollide = AddHook<Func<Projectile, Vector2, bool>>(g => g.OnTileCollide);

		public static bool OnTileCollide(Projectile projectile, Vector2 oldVelocity) {
			bool result = true;
			foreach (GlobalProjectile g in HookOnTileCollide.arr) {
				result &= g.Instance(projectile).OnTileCollide(projectile, oldVelocity);
			}
			if (result && projectile.modProjectile != null) {
				return projectile.modProjectile.OnTileCollide(oldVelocity);
			}
			return result;
		}

		private static HookList HookCanCutTiles = AddHook<Func<Projectile, bool?>>(g => g.CanCutTiles);

		public static bool? CanCutTiles(Projectile projectile) {
			foreach (GlobalProjectile g in HookCanCutTiles.arr) {
				bool? canCutTiles = g.Instance(projectile).CanCutTiles(projectile);
				if (canCutTiles.HasValue) {
					return canCutTiles.Value;
				}
			}
			return projectile.modProjectile?.CanCutTiles();
		}

		private static HookList HookCutTiles = AddHook<Action<Projectile>>(g => g.CutTiles);

		public static void CutTiles(Projectile projectile) {
			foreach (GlobalProjectile g in HookCutTiles.arr) {
				g.Instance(projectile).CutTiles(projectile);
			}
			projectile.modProjectile?.CutTiles();
		}

		private static HookList HookPreKill = AddHook<Func<Projectile, int, bool>>(g => g.PreKill);

		public static bool PreKill(Projectile projectile, int timeLeft) {
			bool result = true;
			foreach (GlobalProjectile g in HookPreKill.arr) {
				result &= g.Instance(projectile).PreKill(projectile, timeLeft);
			}
			if (result && projectile.modProjectile != null) {
				return projectile.modProjectile.PreKill(timeLeft);
			}
			return result;
		}

		private static HookList HookKill = AddHook<Action<Projectile, int>>(g => g.Kill);

		public static void Kill(Projectile projectile, int timeLeft) {
			projectile.modProjectile?.Kill(timeLeft);

			foreach (GlobalProjectile g in HookKill.arr) {
				g.Instance(projectile).Kill(projectile, timeLeft);
			}
		}

		private static HookList HookCanDamage = AddHook<Func<Projectile, bool>>(g => g.CanDamage);

		public static bool CanDamage(Projectile projectile) {
			if (projectile.modProjectile != null && !projectile.modProjectile.CanDamage()) {
				return false;
			}
			foreach (GlobalProjectile g in HookCanDamage.arr) {
				if (!g.Instance(projectile).CanDamage(projectile)) {
					return false;
				}
			}
			return true;
		}

		private static HookList HookMinionContactDamage = AddHook<Func<Projectile, bool>>(g => g.MinionContactDamage);

		public static bool MinionContactDamage(Projectile projectile) {
			if (projectile.modProjectile != null && projectile.modProjectile.MinionContactDamage()) {
				return true;
			}
			foreach (GlobalProjectile g in HookMinionContactDamage.arr) {
				if (g.Instance(projectile).MinionContactDamage(projectile)) {
					return true;
				}
			}
			return false;
		}

		private delegate void DelegateModifyDamageHitbox(Projectile projectile, ref Rectangle hitbox);
		private static HookList HookModifyDamageHitbox = AddHook<DelegateModifyDamageHitbox>(g => g.ModifyDamageHitbox);

		public static void ModifyDamageHitbox(Projectile projectile, ref Rectangle hitbox) {
			projectile.modProjectile?.ModifyDamageHitbox(ref hitbox);
			foreach (GlobalProjectile g in HookModifyDamageHitbox.arr) {
				g.Instance(projectile).ModifyDamageHitbox(projectile, ref hitbox);
			}
		}

		private static HookList HookCanHitNPC = AddHook<Func<Projectile, NPC, bool?>>(g => g.CanHitNPC);

		public static bool? CanHitNPC(Projectile projectile, NPC target) {
			bool? flag = null;
			foreach (GlobalProjectile g in HookCanHitNPC.arr) {
				bool? canHit = g.Instance(projectile).CanHitNPC(projectile, target);
				if (canHit.HasValue && !canHit.Value) {
					return false;
				}
				if (canHit.HasValue) {
					flag = canHit.Value;
				}
			}
			if (projectile.modProjectile != null) {
				bool? canHit = projectile.modProjectile.CanHitNPC(target);
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
			projectile.modProjectile?.ModifyHitNPC(target, ref damage, ref knockback, ref crit, ref hitDirection);

			foreach (GlobalProjectile g in HookModifyHitNPC.arr) {
				g.Instance(projectile).ModifyHitNPC(projectile, target, ref damage, ref knockback, ref crit, ref hitDirection);
			}
		}

		private static HookList HookOnHitNPC = AddHook<Action<Projectile, NPC, int, float, bool>>(g => g.OnHitNPC);

		public static void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit) {
			projectile.modProjectile?.OnHitNPC(target, damage, knockback, crit);

			foreach (GlobalProjectile g in HookOnHitNPC.arr) {
				g.Instance(projectile).OnHitNPC(projectile, target, damage, knockback, crit);
			}
		}

		private static HookList HookCanHitPvp = AddHook<Func<Projectile, Player, bool>>(g => g.CanHitPvp);

		public static bool CanHitPvp(Projectile projectile, Player target) {
			foreach (GlobalProjectile g in HookCanHitPvp.arr) {
				if (!g.Instance(projectile).CanHitPvp(projectile, target)) {
					return false;
				}
			}
			if (projectile.modProjectile != null) {
				return projectile.modProjectile.CanHitPvp(target);
			}
			return true;
		}

		private delegate void DelegateModifyHitPvp(Projectile projectile, Player target, ref int damage, ref bool crit);
		private static HookList HookModifyHitPvp = AddHook<DelegateModifyHitPvp>(g => g.ModifyHitPvp);

		public static void ModifyHitPvp(Projectile projectile, Player target, ref int damage, ref bool crit) {
			projectile.modProjectile?.ModifyHitPvp(target, ref damage, ref crit);

			foreach (GlobalProjectile g in HookModifyHitPvp.arr) {
				g.Instance(projectile).ModifyHitPvp(projectile, target, ref damage, ref crit);
			}
		}

		private static HookList HookOnHitPvp = AddHook<Action<Projectile, Player, int, bool>>(g => g.OnHitPvp);

		public static void OnHitPvp(Projectile projectile, Player target, int damage, bool crit) {
			projectile.modProjectile?.OnHitPvp(target, damage, crit);

			foreach (GlobalProjectile g in HookOnHitPvp.arr) {
				g.Instance(projectile).OnHitPvp(projectile, target, damage, crit);
			}
		}

		private static HookList HookCanHitPlayer = AddHook<Func<Projectile, Player, bool>>(g => g.CanHitPlayer);

		public static bool CanHitPlayer(Projectile projectile, Player target) {
			foreach (GlobalProjectile g in HookCanHitPlayer.arr) {
				if (!g.Instance(projectile).CanHitPlayer(projectile, target)) {
					return false;
				}
			}
			if (projectile.modProjectile != null) {
				return projectile.modProjectile.CanHitPlayer(target);
			}
			return true;
		}

		private delegate void DelegateModifyHitPlayer(Projectile projectile, Player target, ref int damage, ref bool crit);
		private static HookList HookModifyHitPlayer = AddHook<DelegateModifyHitPlayer>(g => g.ModifyHitPlayer);

		public static void ModifyHitPlayer(Projectile projectile, Player target, ref int damage, ref bool crit) {
			projectile.modProjectile?.ModifyHitPlayer(target, ref damage, ref crit);

			foreach (GlobalProjectile g in HookModifyHitPlayer.arr) {
				g.Instance(projectile).ModifyHitPlayer(projectile, target, ref damage, ref crit);
			}
		}

		private static HookList HookOnHitPlayer = AddHook<Action<Projectile, Player, int, bool>>(g => g.OnHitPlayer);

		public static void OnHitPlayer(Projectile projectile, Player target, int damage, bool crit) {
			projectile.modProjectile?.OnHitPlayer(target, damage, crit);

			foreach (GlobalProjectile g in HookOnHitPlayer.arr) {
				g.Instance(projectile).OnHitPlayer(projectile, target, damage, crit);
			}
		}

		private static HookList HookColliding = AddHook<Func<Projectile, Rectangle, Rectangle, bool?>>(g => g.Colliding);

		public static bool? Colliding(Projectile projectile, Rectangle projHitbox, Rectangle targetHitbox) {
			foreach (GlobalProjectile g in HookColliding.arr) {
				bool? colliding = g.Instance(projectile).Colliding(projectile, projHitbox, targetHitbox);
				if (colliding.HasValue) {
					return colliding.Value;
				}
			}
			return projectile.modProjectile?.Colliding(projHitbox, targetHitbox);
		}

		public static void DrawHeldProjInFrontOfHeldItemAndArms(Projectile projectile, ref bool flag) {
			if (projectile.modProjectile != null) {
				flag = projectile.modProjectile.drawHeldProjInFrontOfHeldItemAndArms;
			}
		}

		private static HookList HookGetAlpha = AddHook<Func<Projectile, Color, Color?>>(g => g.GetAlpha);

		public static Color? GetAlpha(Projectile projectile, Color lightColor) {
			foreach (GlobalProjectile g in HookGetAlpha.arr) {
				Color? color = g.Instance(projectile).GetAlpha(projectile, lightColor);
				if (color.HasValue) {
					return color;
				}
			}
			return projectile.modProjectile?.GetAlpha(lightColor);
		}

		public static void DrawOffset(Projectile projectile, ref int offsetX, ref int offsetY, ref float originX) {
			if (projectile.modProjectile != null) {
				offsetX = projectile.modProjectile.drawOffsetX;
				offsetY = -projectile.modProjectile.drawOriginOffsetY;
				originX += projectile.modProjectile.drawOriginOffsetX;
			}
		}

		private static HookList HookPreDrawExtras = AddHook<Func<Projectile, SpriteBatch, bool>>(g => g.PreDrawExtras);

		public static bool PreDrawExtras(Projectile projectile, SpriteBatch spriteBatch) {
			bool result = true;
			foreach (GlobalProjectile g in HookPreDrawExtras.arr) {
				result &= g.Instance(projectile).PreDrawExtras(projectile, spriteBatch);
			}
			if (result && projectile.modProjectile != null) {
				return projectile.modProjectile.PreDrawExtras(spriteBatch);
			}
			return result;
		}

		private static HookList HookPreDraw = AddHook<Func<Projectile, SpriteBatch, Color, bool>>(g => g.PreDraw);

		public static bool PreDraw(Projectile projectile, SpriteBatch spriteBatch, Color lightColor) {
			bool result = true;
			foreach (GlobalProjectile g in HookPreDraw.arr) {
				result &= g.Instance(projectile).PreDraw(projectile, spriteBatch, lightColor);
			}
			if (result && projectile.modProjectile != null) {
				return projectile.modProjectile.PreDraw(spriteBatch, lightColor);
			}
			return result;
		}

		private static HookList HookPostDraw = AddHook<Action<Projectile, SpriteBatch, Color>>(g => g.PostDraw);

		public static void PostDraw(Projectile projectile, SpriteBatch spriteBatch, Color lightColor) {
			projectile.modProjectile?.PostDraw(spriteBatch, lightColor);

			foreach (GlobalProjectile g in HookPostDraw.arr) {
				g.Instance(projectile).PostDraw(projectile, spriteBatch, lightColor);
			}
		}

		private static HookList HookCanUseGrapple = AddHook<Func<int, Player, bool?>>(g => g.CanUseGrapple);

		public static bool? CanUseGrapple(int type, Player player) {
			var flag = GetProjectile(type)?.CanUseGrapple(player);

			foreach (GlobalProjectile g in HookCanUseGrapple.arr) {
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

			foreach (GlobalProjectile g in HookSingleGrappleHook.arr) {
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

			foreach (GlobalProjectile g in HookUseGrapple.arr) {
				g.UseGrapple(player, ref type);
			}
		}

		public static bool GrappleOutOfRange(float distance, Projectile projectile) {
			return distance > projectile.modProjectile?.GrappleRange();
		}

		private delegate void DelegateNumGrappleHooks(Projectile projectile, Player player, ref int numHooks);
		private static HookList HookNumGrappleHooks = AddHook<DelegateNumGrappleHooks>(g => g.NumGrappleHooks);

		public static void NumGrappleHooks(Projectile projectile, Player player, ref int numHooks) {
			projectile.modProjectile?.NumGrappleHooks(player, ref numHooks);

			foreach (GlobalProjectile g in HookNumGrappleHooks.arr) {
				g.Instance(projectile).NumGrappleHooks(projectile, player, ref numHooks);
			}
		}

		private delegate void DelegateGrappleRetreatSpeed(Projectile projectile, Player player, ref float speed);
		private static HookList HookGrappleRetreatSpeed = AddHook<DelegateGrappleRetreatSpeed>(g => g.GrappleRetreatSpeed);

		public static void GrappleRetreatSpeed(Projectile projectile, Player player, ref float speed) {
			projectile.modProjectile?.GrappleRetreatSpeed(player, ref speed);

			foreach (GlobalProjectile g in HookGrappleRetreatSpeed.arr) {
				g.Instance(projectile).GrappleRetreatSpeed(projectile, player, ref speed);
			}
		}

		private delegate void DelegateGrapplePullSpeed(Projectile projectile, Player player, ref float speed);
		private static HookList HookGrapplePullSpeed = AddHook<DelegateGrapplePullSpeed>(g => g.GrapplePullSpeed);

		public static void GrapplePullSpeed(Projectile projectile, Player player, ref float speed) {
			projectile.modProjectile?.GrapplePullSpeed(player, ref speed);

			foreach (GlobalProjectile g in HookGrapplePullSpeed.arr) {
				g.Instance(projectile).GrapplePullSpeed(projectile, player, ref speed);
			}
		}

		private static HookList HookDrawBehind = AddHook<Action<Projectile, int, List<int>, List<int>, List<int>, List<int>>>(g => g.DrawBehind);

		internal static void DrawBehind(Projectile projectile, int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI) {
			projectile.modProjectile?.DrawBehind(index, drawCacheProjsBehindNPCsAndTiles, drawCacheProjsBehindNPCs, drawCacheProjsBehindProjectiles, drawCacheProjsOverWiresUI);

			foreach (GlobalProjectile g in HookDrawBehind.arr) {
				g.Instance(projectile).DrawBehind(projectile, index, drawCacheProjsBehindNPCsAndTiles, drawCacheProjsBehindNPCs, drawCacheProjsBehindProjectiles, drawCacheProjsOverWiresUI);
			}
		}

		private static bool HasMethod(Type t, string method, params Type[] args) {
			return t.GetMethod(method, args).DeclaringType != typeof(GlobalProjectile);
		}

		internal static void VerifyGlobalProjectile(GlobalProjectile projectile) {
			var type = projectile.GetType();

			bool hasInstanceFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				.Any(f => f.DeclaringType != typeof(GlobalProjectile));
			if (hasInstanceFields) {
				if (!projectile.InstancePerEntity) {
					throw new Exception(type + " has instance fields but does not set InstancePerEntity to true. Either use static fields, or per instance globals");
				}
			}
		}
	}
}
