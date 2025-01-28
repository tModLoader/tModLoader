using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.IO;
using HookList = Terraria.ModLoader.Core.GlobalHookList<Terraria.ModLoader.GlobalProjectile>;

namespace Terraria.ModLoader;

//todo: further documentation
/// <summary>
/// This serves as the central class from which projectile-related functions are carried out. It also stores a list of mod projectiles by ID.
/// </summary>
public static class ProjectileLoader
{
	public static int ProjectileCount { get; private set; } = ProjectileID.Count;
	private static readonly IList<ModProjectile> projectiles = new List<ModProjectile>();

	private static readonly List<HookList> hooks = new();
	private static readonly List<HookList> modHooks = new();

	private static HookList AddHook<F>(Expression<Func<GlobalProjectile, F>> func) where F : Delegate
	{
		var hook = HookList.Create(func);
		hooks.Add(hook);
		return hook;
	}

	public static T AddModHook<T>(T hook) where T : HookList
	{
		modHooks.Add(hook);
		return hook;
	}

	internal static int Register(ModProjectile projectile)
	{
		projectiles.Add(projectile);
		return ProjectileCount++;
	}

	/// <summary>
	/// Gets the ModProjectile template instance corresponding to the specified type (not the clone/new instance which gets added to Projectiles as the game is played).
	/// </summary>
	/// <param name="type">The type of the projectile</param>
	/// <returns>The ModProjectile instance in the projectiles array, null if not found.</returns>
	public static ModProjectile GetProjectile(int type)
	{
		return type >= ProjectileID.Count && type < ProjectileCount ? projectiles[type - ProjectileID.Count] : null;
	}

	internal static void ResizeArrays(bool unloading)
	{
		if (!unloading)
			GlobalList<GlobalProjectile>.FinishLoading(ProjectileCount);

		//Textures
		Array.Resize(ref TextureAssets.Projectile, ProjectileCount);

		//Sets
		LoaderUtils.ResetStaticMembers(typeof(ProjectileID));

		//Etc
		Array.Resize(ref Main.projHostile, ProjectileCount);
		Array.Resize(ref Main.projHook, ProjectileCount);
		Array.Resize(ref Main.projFrames, ProjectileCount);
		Array.Resize(ref Main.projPet, ProjectileCount);
		Array.Resize(ref Lang._projectileNameCache, ProjectileCount);

		for (int k = ProjectileID.Count; k < ProjectileCount; k++) {
			Main.projFrames[k] = 1;
			Lang._projectileNameCache[k] = LocalizedText.Empty;
		}

		Array.Resize(ref Projectile.perIDStaticNPCImmunity, ProjectileCount);

		for (int i = 0; i < ProjectileCount; i++) {
			Projectile.perIDStaticNPCImmunity[i] = new uint[200];
		}
	}

	internal static void FinishSetup()
	{
		GlobalLoaderUtils<GlobalProjectile, Projectile>.BuildTypeLookups(new Projectile().SetDefaults);
		UpdateHookLists();
		GlobalTypeLookups<GlobalProjectile>.LogStats();

		foreach (ModProjectile proj in projectiles) {
			Lang._projectileNameCache[proj.Type] = proj.DisplayName;
		}
	}

	private static void UpdateHookLists()
	{
		foreach (var hook in hooks.Union(modHooks)) {
			hook.Update();
		}
	}

	internal static void Unload()
	{
		ProjectileCount = ProjectileID.Count;
		projectiles.Clear();
		GlobalList<GlobalProjectile>.Reset();
		modHooks.Clear();
		UpdateHookLists();
	}

	internal static bool IsModProjectile(Projectile projectile)
	{
		return projectile.type >= ProjectileID.Count;
	}

	internal static void SetDefaults(Projectile projectile, bool createModProjectile = true)
	{
		if (IsModProjectile(projectile) && createModProjectile) {
			projectile.ModProjectile = GetProjectile(projectile.type).NewInstance(projectile);
		}

		GlobalLoaderUtils<GlobalProjectile, Projectile>.SetDefaults(projectile, ref projectile._globals, static e => e.ModProjectile?.SetDefaults());
	}

	private static HookList HookOnSpawn = AddHook<Action<Projectile, IEntitySource>>(g => g.OnSpawn);

	internal static void OnSpawn(Projectile projectile, IEntitySource source)
	{
		projectile.ModProjectile?.OnSpawn(source);

		foreach (var g in HookOnSpawn.Enumerate(projectile)) {
			g.OnSpawn(projectile, source);
		}
	}

	//in Terraria.Projectile rename AI to VanillaAI then make AI call ProjectileLoader.ProjectileAI(this)
	public static void ProjectileAI(Projectile projectile)
	{
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

	public static bool PreAI(Projectile projectile)
	{
		bool result = true;

		foreach (var g in HookPreAI.Enumerate(projectile)) {
			result &= g.PreAI(projectile);
		}

		if (result && projectile.ModProjectile != null) {
			return projectile.ModProjectile.PreAI();
		}

		return result;
	}

	private static HookList HookAI = AddHook<Action<Projectile>>(g => g.AI);

	public static void AI(Projectile projectile)
	{
		projectile.ModProjectile?.AI();

		foreach (var g in HookAI.Enumerate(projectile)) {
			g.AI(projectile);
		}
	}

	private static HookList HookPostAI = AddHook<Action<Projectile>>(g => g.PostAI);

	public static void PostAI(Projectile projectile)
	{
		projectile.ModProjectile?.PostAI();

		foreach (var g in HookPostAI.Enumerate(projectile)) {
			g.PostAI(projectile);
		}
	}

	public static void SendExtraAI(BinaryWriter writer, byte[] extraAI)
	{
		writer.Write7BitEncodedInt(extraAI.Length);

		if (extraAI.Length > 0) {
			writer.Write(extraAI);
		}
	}

	private static HookList HookSendExtraAI = AddHook<Action<Projectile, BitWriter, BinaryWriter>>(g => g.SendExtraAI);

	public static byte[] WriteExtraAI(Projectile projectile)
	{
		// Data is ordered as follows: GlobalProjectile BitWriter, ModProjectile Bytes, GlobalProjectile Bytes
		using var stream = new MemoryStream();
		using var modWriter = new BinaryWriter(stream);

		using var bufferedStream = new MemoryStream();
		using var binaryWriter = new BinaryWriter(bufferedStream);

		BitWriter bitWriter = new BitWriter();

		projectile.ModProjectile?.SendExtraAI(binaryWriter);

		foreach (var g in HookSendExtraAI.Enumerate(projectile)) {
			g.SendExtraAI(projectile, bitWriter, binaryWriter);
		}

		bitWriter.Flush(modWriter);
		modWriter.Write(bufferedStream.ToArray());

		byte[] bytes = stream.ToArray();
		// If the only byte is the bitWriter.Flush length byte, no extra data.
		if (bytes.Length == 1) {
			Debug.Assert(bytes[0] == 0);
			return null;
		}
		return bytes;
	}

	public static byte[] ReadExtraAI(BinaryReader reader)
	{
		return reader.ReadBytes(reader.Read7BitEncodedInt());
	}

	private static HookList HookReceiveExtraAI = AddHook<Action<Projectile, BitReader, BinaryReader>>(g => g.ReceiveExtraAI);

	public static void ReceiveExtraAI(Projectile projectile, byte[] extraAI)
	{
		using var stream = extraAI.ToMemoryStream();
		using var modReader = new BinaryReader(stream);

		GlobalProjectile lastGlobalProjectile = null;
		try {
			BitReader bitReader = new BitReader(modReader);

			var bitReaderEnd = stream.Position;
			projectile.ModProjectile?.ReceiveExtraAI(modReader);

			foreach (var g in HookReceiveExtraAI.Enumerate(projectile)) {
				lastGlobalProjectile = g;
				g.ReceiveExtraAI(projectile, bitReader, modReader);
			}

			if (bitReader.BitsRead < bitReader.MaxBits) {
				throw new IOException($"Read underflow {bitReader.MaxBits - bitReader.BitsRead} of {bitReader.MaxBits} compressed bits in ReceiveExtraAI, more info below");
			}

			if (stream.Position < stream.Length) {
				throw new IOException($"Read underflow {stream.Length - stream.Position} of {stream.Length - bitReaderEnd} bytes in ReceiveExtraAI, more info below");
			}
		}
		catch (Exception e) {
			string message = $"Error in ReceiveExtraAI for Projectile {projectile.ModProjectile?.FullName ?? projectile.Name}";
			if (lastGlobalProjectile != null) {
				message += ", may be caused by one of these GlobalProjectiles:";
				foreach (var g in HookReceiveExtraAI.Enumerate(projectile)) {
					message += $"\n\t{g.FullName}";
					if (lastGlobalProjectile == g)
						break;
				}
			}

			Logging.tML.Error(message);
		}
	}

	private static HookList HookShouldUpdatePosition = AddHook<Func<Projectile, bool>>(g => g.ShouldUpdatePosition);

	public static bool ShouldUpdatePosition(Projectile projectile)
	{
		if (IsModProjectile(projectile) && !projectile.ModProjectile.ShouldUpdatePosition()) {
			return false;
		}

		foreach (var g in HookShouldUpdatePosition.Enumerate(projectile)) {
			if (!g.ShouldUpdatePosition(projectile)) {
				return false;
			}
		}

		return true;
	}

	private delegate bool DelegateTileCollideStyle(Projectile projectile, ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac);
	private static HookList HookTileCollideStyle = AddHook<DelegateTileCollideStyle>(g => g.TileCollideStyle);

	public static bool TileCollideStyle(Projectile projectile, ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
	{
		if (IsModProjectile(projectile) && !projectile.ModProjectile.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac)) {
			return false;
		}

		foreach (var g in HookTileCollideStyle.Enumerate(projectile)) {
			if (!g.TileCollideStyle(projectile, ref width, ref height, ref fallThrough, ref hitboxCenterFrac)) {
				return false;
			}
		}

		return true;
	}

	private static HookList HookOnTileCollide = AddHook<Func<Projectile, Vector2, bool>>(g => g.OnTileCollide);

	public static bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
	{
		bool result = true;

		foreach (var g in HookOnTileCollide.Enumerate(projectile)) {
			result &= g.OnTileCollide(projectile, oldVelocity);
		}

		if (result && projectile.ModProjectile != null) {
			return projectile.ModProjectile.OnTileCollide(oldVelocity);
		}

		return result;
	}

	private static HookList HookCanCutTiles = AddHook<Func<Projectile, bool?>>(g => g.CanCutTiles);

	public static bool? CanCutTiles(Projectile projectile)
	{
		foreach (var g in HookCanCutTiles.Enumerate(projectile)) {
			bool? canCutTiles = g.CanCutTiles(projectile);

			if (canCutTiles.HasValue) {
				return canCutTiles.Value;
			}
		}

		return projectile.ModProjectile?.CanCutTiles();
	}

	private static HookList HookCutTiles = AddHook<Action<Projectile>>(g => g.CutTiles);

	public static void CutTiles(Projectile projectile)
	{

		foreach (var g in HookCutTiles.Enumerate(projectile)) {
			g.CutTiles(projectile);
		}

		projectile.ModProjectile?.CutTiles();
	}

	private static HookList HookPreKill = AddHook<Func<Projectile, int, bool>>(g => g.PreKill);

	public static bool PreKill(Projectile projectile, int timeLeft)
	{
		bool result = true;

		foreach (var g in HookPreKill.Enumerate(projectile)) {
			result &= g.PreKill(projectile, timeLeft);
		}

		if (result && projectile.ModProjectile != null) {
			return projectile.ModProjectile.PreKill(timeLeft);
		}

		return result;
	}

	[Obsolete]
	private static HookList HookKill = AddHook<Action<Projectile, int>>(g => g.Kill);

	[Obsolete("Renamed to OnKill")]
	public static void Kill_Obsolete(Projectile projectile, int timeLeft)
	{
		projectile.ModProjectile?.Kill(timeLeft);

		foreach (var g in HookKill.Enumerate(projectile)) {
			g.Kill(projectile, timeLeft);
		}
	}

	private static HookList HookOnKill = AddHook<Action<Projectile, int>>(g => g.OnKill);

	public static void OnKill(Projectile projectile, int timeLeft)
	{
		projectile.ModProjectile?.OnKill(timeLeft);
		Kill_Obsolete(projectile, timeLeft); // Placed here so both ModProjectile methods are called and then the GlobalProjectile methods

		foreach (var g in HookOnKill.Enumerate(projectile)) {
			g.OnKill(projectile, timeLeft);
		}
	}

	private static HookList HookCanDamage = AddHook<Func<Projectile, bool?>>(g => g.CanDamage);

	public static bool? CanDamage(Projectile projectile)
	{
		bool? result = null;

		foreach (var g in HookCanDamage.Enumerate(projectile)) {
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

	public static bool MinionContactDamage(Projectile projectile)
	{
		if (projectile.ModProjectile != null && projectile.ModProjectile.MinionContactDamage()) {
			return true;
		}

		foreach (var g in HookMinionContactDamage.Enumerate(projectile)) {
			if (g.MinionContactDamage(projectile)) {
				return true;
			}
		}

		return false;
	}

	private delegate void DelegateModifyDamageHitbox(Projectile projectile, ref Rectangle hitbox);
	private static HookList HookModifyDamageHitbox = AddHook<DelegateModifyDamageHitbox>(g => g.ModifyDamageHitbox);

	public static void ModifyDamageHitbox(Projectile projectile, ref Rectangle hitbox)
	{
		projectile.ModProjectile?.ModifyDamageHitbox(ref hitbox);

		foreach (var g in HookModifyDamageHitbox.Enumerate(projectile)) {
			g.ModifyDamageHitbox(projectile, ref hitbox);
		}
	}

	private static HookList HookCanHitNPC = AddHook<Func<Projectile, NPC, bool?>>(g => g.CanHitNPC);

	public static bool? CanHitNPC(Projectile projectile, NPC target)
	{
		bool? flag = null;

		foreach (var g in HookCanHitNPC.Enumerate(projectile)) {
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

	private delegate void DelegateModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers);
	private static HookList HookModifyHitNPC = AddHook<DelegateModifyHitNPC>(g => g.ModifyHitNPC);

	public static void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
	{
		projectile.ModProjectile?.ModifyHitNPC(target, ref modifiers);

		foreach (var g in HookModifyHitNPC.Enumerate(projectile)) {
			g.ModifyHitNPC(projectile, target, ref modifiers);
		}
	}

	private static HookList HookOnHitNPC = AddHook<Action<Projectile, NPC, NPC.HitInfo, int>>(g => g.OnHitNPC);

	public static void OnHitNPC(Projectile projectile, NPC target, in NPC.HitInfo hit, int damageDone)
	{
		projectile.ModProjectile?.OnHitNPC(target, hit, damageDone);

		foreach (var g in HookOnHitNPC.Enumerate(projectile)) {
			g.OnHitNPC(projectile, target, hit, damageDone);
		}
	}

	private static HookList HookCanHitPvp = AddHook<Func<Projectile, Player, bool>>(g => g.CanHitPvp);

	public static bool CanHitPvp(Projectile projectile, Player target)
	{
		foreach (var g in HookCanHitPvp.Enumerate(projectile)) {
			if (!g.CanHitPvp(projectile, target)) {
				return false;
			}
		}

		if (projectile.ModProjectile != null) {
			return projectile.ModProjectile.CanHitPvp(target);
		}

		return true;
	}

	private static HookList HookCanHitPlayer = AddHook<Func<Projectile, Player, bool>>(g => g.CanHitPlayer);

	public static bool CanHitPlayer(Projectile projectile, Player target)
	{
		foreach (var g in HookCanHitPlayer.Enumerate(projectile)) {
			if (!g.CanHitPlayer(projectile, target)) {
				return false;
			}
		}

		if (projectile.ModProjectile != null) {
			return projectile.ModProjectile.CanHitPlayer(target);
		}

		return true;
	}

	private delegate void DelegateModifyHitPlayer(Projectile projectile, Player target, ref Player.HurtModifiers modifiers);
	private static HookList HookModifyHitPlayer = AddHook<DelegateModifyHitPlayer>(g => g.ModifyHitPlayer);

	public static void ModifyHitPlayer(Projectile projectile, Player target, ref Player.HurtModifiers modifiers)
	{
		projectile.ModProjectile?.ModifyHitPlayer(target, ref modifiers);

		foreach (var g in HookModifyHitPlayer.Enumerate(projectile)) {
			g.ModifyHitPlayer(projectile, target, ref modifiers);
		}
	}

	private static HookList HookOnHitPlayer = AddHook<Action<Projectile, Player, Player.HurtInfo>>(g => g.OnHitPlayer);

	public static void OnHitPlayer(Projectile projectile, Player target, in Player.HurtInfo hurtInfo)
	{
		projectile.ModProjectile?.OnHitPlayer(target, hurtInfo);

		foreach (var g in HookOnHitPlayer.Enumerate(projectile)) {
			g.OnHitPlayer(projectile, target, hurtInfo);
		}
	}

	private static HookList HookColliding = AddHook<Func<Projectile, Rectangle, Rectangle, bool?>>(g => g.Colliding);

	public static bool? Colliding(Projectile projectile, Rectangle projHitbox, Rectangle targetHitbox)
	{
		foreach (var g in HookColliding.Enumerate(projectile)) {
			bool? colliding = g.Colliding(projectile, projHitbox, targetHitbox);

			if (colliding.HasValue) {
				return colliding.Value;
			}
		}

		return projectile.ModProjectile?.Colliding(projHitbox, targetHitbox);
	}

	public static void DrawHeldProjInFrontOfHeldItemAndArms(Projectile projectile, ref bool flag)
	{
		if (projectile.ModProjectile != null) {
			flag = projectile.ModProjectile.DrawHeldProjInFrontOfHeldItemAndArms;
		}
	}

	[Obsolete($"Moved to ItemLoader. Fishing line position and color are now set by the pole used.")]
	public static void ModifyFishingLine(Projectile projectile, ref float polePosX, ref float polePosY, ref Color lineColor)
	{
		if (projectile.ModProjectile == null)
			return;

		Vector2 lineOriginOffset = Vector2.Zero;
		Player player = Main.player[projectile.owner];

		projectile.ModProjectile?.ModifyFishingLine(ref lineOriginOffset, ref lineColor);

		polePosX += lineOriginOffset.X * player.direction;
		polePosY += lineOriginOffset.Y * player.gravDir;
	}

	private static HookList HookGetAlpha = AddHook<Func<Projectile, Color, Color?>>(g => g.GetAlpha);

	public static Color? GetAlpha(Projectile projectile, Color lightColor)
	{
		foreach (var g in HookGetAlpha.Enumerate(projectile)) {
			Color? color = g.GetAlpha(projectile, lightColor);

			if (color.HasValue) {
				return color;
			}
		}

		return projectile.ModProjectile?.GetAlpha(lightColor);
	}

	public static void DrawOffset(Projectile projectile, ref int offsetX, ref int offsetY, ref float originX)
	{
		if (projectile.ModProjectile != null) {
			offsetX = projectile.ModProjectile.DrawOffsetX;
			offsetY = -projectile.ModProjectile.DrawOriginOffsetY;
			originX += projectile.ModProjectile.DrawOriginOffsetX;
		}
	}

	private static HookList HookPreDrawExtras = AddHook<Func<Projectile, bool>>(g => g.PreDrawExtras);

	public static bool PreDrawExtras(Projectile projectile)
	{
		bool result = true;

		foreach (var g in HookPreDrawExtras.Enumerate(projectile)) {
			result &= g.PreDrawExtras(projectile);
		}

		if (result && projectile.ModProjectile != null) {
			return projectile.ModProjectile.PreDrawExtras();
		}

		return result;
	}

	private delegate bool DelegatePreDraw(Projectile projectile, ref Color lightColor);
	private static HookList HookPreDraw = AddHook<DelegatePreDraw>(g => g.PreDraw);

	public static bool PreDraw(Projectile projectile, ref Color lightColor)
	{
		bool result = true;

		foreach (var g in HookPreDraw.Enumerate(projectile)) {
			result &= g.PreDraw(projectile, ref lightColor);
		}

		if (result && projectile.ModProjectile != null) {
			return projectile.ModProjectile.PreDraw(ref lightColor);
		}

		return result;
	}

	private static HookList HookPostDraw = AddHook<Action<Projectile, Color>>(g => g.PostDraw);

	public static void PostDraw(Projectile projectile, Color lightColor)
	{
		projectile.ModProjectile?.PostDraw(lightColor);

		foreach (var g in HookPostDraw.Enumerate(projectile)) {
			g.PostDraw(projectile, lightColor);
		}
	}

	private static HookList HookCanUseGrapple = AddHook<Func<int, Player, bool?>>(g => g.CanUseGrapple);

	public static bool? CanUseGrapple(int type, Player player)
	{
		bool? flag = GetProjectile(type)?.CanUseGrapple(player);

		foreach (var g in HookCanUseGrapple.Enumerate(type)) {
			bool? canGrapple = g.CanUseGrapple(type, player);

			if (canGrapple.HasValue) {
				flag = canGrapple;
			}
		}

		return flag;
	}

	private delegate void DelegateUseGrapple(Player player, ref int type);
	private static HookList HookUseGrapple = AddHook<DelegateUseGrapple>(g => g.UseGrapple);

	public static void UseGrapple(Player player, ref int type)
	{
		GetProjectile(type)?.UseGrapple(player, ref type);

		foreach (var g in HookUseGrapple.Enumerate()) {
			g.UseGrapple(player, ref type);
		}
	}

	public static bool GrappleOutOfRange(float distance, Projectile projectile)
	{
		return distance > projectile.ModProjectile?.GrappleRange();
	}

	private delegate void DelegateNumGrappleHooks(Projectile projectile, Player player, ref int numHooks);
	private static HookList HookNumGrappleHooks = AddHook<DelegateNumGrappleHooks>(g => g.NumGrappleHooks);

	public static void NumGrappleHooks(Projectile projectile, Player player, ref int numHooks)
	{
		projectile.ModProjectile?.NumGrappleHooks(player, ref numHooks);

		foreach (var g in HookNumGrappleHooks.Enumerate(projectile)) {
			g.NumGrappleHooks(projectile, player, ref numHooks);
		}
	}

	private delegate void DelegateGrappleRetreatSpeed(Projectile projectile, Player player, ref float speed);
	private static HookList HookGrappleRetreatSpeed = AddHook<DelegateGrappleRetreatSpeed>(g => g.GrappleRetreatSpeed);

	public static void GrappleRetreatSpeed(Projectile projectile, Player player, ref float speed)
	{
		projectile.ModProjectile?.GrappleRetreatSpeed(player, ref speed);

		foreach (var g in HookGrappleRetreatSpeed.Enumerate(projectile)) {
			g.GrappleRetreatSpeed(projectile, player, ref speed);
		}
	}

	private delegate void DelegateGrapplePullSpeed(Projectile projectile, Player player, ref float speed);
	private static HookList HookGrapplePullSpeed = AddHook<DelegateGrapplePullSpeed>(g => g.GrapplePullSpeed);

	public static void GrapplePullSpeed(Projectile projectile, Player player, ref float speed)
	{
		projectile.ModProjectile?.GrapplePullSpeed(player, ref speed);

		foreach (var g in HookGrapplePullSpeed.Enumerate(projectile)) {
			g.GrapplePullSpeed(projectile, player, ref speed);
		}
	}

	private delegate void DelegateGrappleTargetPoint(Projectile projectile, Player player, ref float grappleX, ref float grappleY);
	private static HookList HookGrappleTargetPoint = AddHook<DelegateGrappleTargetPoint>(g => g.GrappleTargetPoint);

	public static void GrappleTargetPoint(Projectile projectile, Player player, ref float grappleX, ref float grappleY)
	{
		projectile.ModProjectile?.GrappleTargetPoint(player, ref grappleX, ref grappleY);

		foreach (var g in HookGrappleTargetPoint.Enumerate(projectile)) {
			g.GrappleTargetPoint(projectile, player, ref grappleX, ref grappleY);
		}
	}

	private static HookList HookGrappleCanLatchOnTo = AddHook<Func<Projectile, Player, int, int, bool?>>(g => g.GrappleCanLatchOnTo);

	public static bool? GrappleCanLatchOnTo(Projectile projectile, Player player, int x, int y)
	{
		bool? flag = projectile.ModProjectile?.GrappleCanLatchOnTo(player, x, y);

		foreach (var g in HookGrappleCanLatchOnTo.Enumerate(projectile)) {
			bool? globalFlag = g.GrappleCanLatchOnTo(projectile, player, x, y);

			if (globalFlag.HasValue) {
				if (!globalFlag.Value)
					return false;

				flag = globalFlag;
			}
		}

		return flag;
	}

	private static HookList HookDrawBehind = AddHook<Action<Projectile, int, List<int>, List<int>, List<int>, List<int>, List<int>>>(g => g.DrawBehind);

	internal static void DrawBehind(Projectile projectile, int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
	{
		projectile.ModProjectile?.DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);

		foreach (var g in HookDrawBehind.Enumerate(projectile)) {
			g.DrawBehind(projectile, index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
		}
	}

	private static HookList HookPrepareBombToBlow = AddHook<Action<Projectile>>(g => g.PrepareBombToBlow);

	internal static void PrepareBombToBlow(Projectile projectile)
	{
		projectile.ModProjectile?.PrepareBombToBlow();

		foreach (var g in HookPrepareBombToBlow.Enumerate(projectile)) {
			g.PrepareBombToBlow(projectile);
		}
	}

	private static HookList HookEmitEnchantmentVisualsAt = AddHook<Action<Projectile, Vector2, int, int>>(g => g.EmitEnchantmentVisualsAt);

	internal static void EmitEnchantmentVisualsAt(Projectile projectile, Vector2 boxPosition, int boxWidth, int boxHeight) {
		projectile.ModProjectile?.EmitEnchantmentVisualsAt(boxPosition, boxWidth, boxHeight);

		foreach (var g in HookEmitEnchantmentVisualsAt.Enumerate(projectile)) {
			g.EmitEnchantmentVisualsAt(projectile, boxPosition, boxWidth, boxHeight);
		}
	}
}
