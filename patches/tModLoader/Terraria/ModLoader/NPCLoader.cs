using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.Utilities;
using HookList = Terraria.ModLoader.Core.HookList<Terraria.ModLoader.GlobalNPC>;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader;

//todo: further documentation
/// <summary>
/// This serves as the central class from which NPC-related functions are carried out. It also stores a list of mod NPCs by ID.
/// </summary>
public static class NPCLoader
{
	internal static bool loaded = false;
	private static int nextNPC = NPCID.Count;
	internal static readonly IList<ModNPC> npcs = new List<ModNPC>();
	internal static readonly List<GlobalNPC> globalNPCs = new();
	internal static readonly IDictionary<int, int> bannerToItem = new Dictionary<int, int>();
	internal static readonly int[] shopToNPC = new int[Main.MaxShopIDs - 1];
	/// <summary>
	/// Allows you to stop an NPC from dropping loot by adding item IDs to this list. This list will be cleared whenever NPCLoot ends. Useful for either removing an item or change the drop rate of an item in the NPC's loot table. To change the drop rate of an item, use the PreNPCLoot hook, spawn the item yourself, then add the item's ID to this list.
	/// </summary>
	public static readonly IList<int> blockLoot = new List<int>();

	private static readonly List<HookList> hooks = new List<HookList>();
	private static readonly List<HookList> modHooks = new List<HookList>();

	private static HookList AddHook<F>(Expression<Func<GlobalNPC, F>> func) where F : Delegate
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

	internal static int ReserveNPCID()
	{
		if (ModNet.AllowVanillaClients) throw new Exception("Adding npcs breaks vanilla client compatibility");

		int reserveID = nextNPC;
		nextNPC++;
		return reserveID;
	}

	public static int NPCCount => nextNPC;

	/// <summary>
	/// Gets the ModNPC template instance corresponding to the specified type (not the clone/new instance which gets added to NPCs as the game is played).
	/// </summary>
	/// <param name="type">The type of the npc</param>
	/// <returns>The ModNPC instance in the npcs array, null if not found.</returns>
	public static ModNPC GetNPC(int type)
	{
		return type >= NPCID.Count && type < NPCCount ? npcs[type - NPCID.Count] : null;
	}

	internal static void ResizeArrays(bool unloading)
	{
		// Textures
		Array.Resize(ref TextureAssets.Npc, nextNPC);

		// Sets
		LoaderUtils.ResetStaticMembers(typeof(NPCID), true);
		Main.ShopHelper.ReinitializePersonalityDatabase();
		NPCHappiness.RegisterVanillaNpcRelationships();

		// Etc
		Array.Resize(ref Main.townNPCCanSpawn, nextNPC);
		Array.Resize(ref Main.slimeRainNPC, nextNPC);
		Array.Resize(ref Main.npcCatchable, nextNPC);
		Array.Resize(ref Main.npcFrameCount, nextNPC);
		Array.Resize(ref Main.SceneMetrics.NPCBannerBuff, nextNPC);
		Array.Resize(ref NPC.killCount, nextNPC);
		Array.Resize(ref NPC.ShimmeredTownNPCs, nextNPC);
		Array.Resize(ref NPC.npcsFoundForCheckActive, nextNPC);
		Array.Resize(ref Lang._npcNameCache, nextNPC);
		Array.Resize(ref EmoteBubble.CountNPCs, nextNPC);
		Array.Resize(ref WorldGen.TownManager._hasRoom, nextNPC);

		foreach (var player in Main.player) {
			Array.Resize(ref player.npcTypeNoAggro, nextNPC);
		}

		for (int k = NPCID.Count; k < nextNPC; k++) {
			Main.npcFrameCount[k] = 1;
			Lang._npcNameCache[k] = LocalizedText.Empty;
		}

		foreach (var hook in hooks.Union(modHooks)) {
			hook.Update(globalNPCs);
		}

		if (!unloading) {
			loaded = true;
		}
	}

	internal static void FinishSetup()
	{
		foreach (ModNPC npc in npcs) {
			Lang._npcNameCache[npc.Type] = npc.DisplayName;
		}
	}

	internal static void Unload()
	{
		loaded = false;
		npcs.Clear();
		nextNPC = NPCID.Count;
		globalNPCs.Clear();
		bannerToItem.Clear();
		modHooks.Clear();
	}

	internal static bool IsModNPC(NPC npc)
	{
		return npc.type >= NPCID.Count;
	}

	private static HookList HookSetDefaults = AddHook<Action<NPC>>(g => g.SetDefaults);

	internal static void SetDefaults(NPC npc, bool createModNPC = true)
	{
		if (IsModNPC(npc)) {
			if (createModNPC) {
				npc.ModNPC = GetNPC(npc.type).NewInstance(npc);
			}
			else //the default NPCs created and bound to ModNPCs are initialized before ResizeArrays. They come here during SetupContent.
			{
				Array.Resize(ref npc.buffImmune, BuffLoader.BuffCount);
			}
		}

		LoaderUtils.InstantiateGlobals(npc, globalNPCs, ref npc.globalNPCs, () => {
			npc.ModNPC?.SetDefaults();
		});

		foreach (GlobalNPC g in HookSetDefaults.Enumerate(npc.globalNPCs)) {
			g.SetDefaults(npc);
		}
	}

	private static HookList HookOnSpawn = AddHook<Action<NPC, IEntitySource>>(g => g.OnSpawn);

	internal static void OnSpawn(NPC npc, IEntitySource source)
	{
		npc.ModNPC?.OnSpawn(source);

		foreach (GlobalNPC g in HookOnSpawn.Enumerate(npc.globalNPCs)) {
			g.OnSpawn(npc, source);
		}
	}

	private static HookList HookApplyDifficultyAndPlayerScaling = AddHook<Action<NPC, int, float, float>>(g => g.ApplyDifficultyAndPlayerScaling);

	public static void ApplyDifficultyAndPlayerScaling(NPC npc, int numPlayers, float balance, float bossAdjustment)
	{
		npc.ModNPC?.ApplyDifficultyAndPlayerScaling(numPlayers, balance, bossAdjustment);

		foreach (GlobalNPC g in HookApplyDifficultyAndPlayerScaling.Enumerate(npc.globalNPCs)) {
			g.ApplyDifficultyAndPlayerScaling(npc, numPlayers, balance, bossAdjustment);
		}
	}

	private delegate void DelegateSetBestiary(NPC npc, BestiaryDatabase database, BestiaryEntry bestiaryEntry);
	private static HookList HookSetBestiary = AddHook<DelegateSetBestiary>(g => g.SetBestiary);
	public static void SetBestiary(NPC npc, BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		if(IsModNPC(npc)) {
			bestiaryEntry.Info.Add(npc.ModNPC.Mod.ModSourceBestiaryInfoElement);
			foreach (var type in npc.ModNPC.SpawnModBiomes) {
				bestiaryEntry.Info.Add(LoaderManager.Get<BiomeLoader>().Get(type).ModBiomeBestiaryInfoElement);
			}
		}

		npc.ModNPC?.SetBestiary(database, bestiaryEntry);

		foreach (GlobalNPC g in HookSetBestiary.Enumerate(npc.globalNPCs)) {
			g.SetBestiary(npc, database, bestiaryEntry);
		}
	}

	private delegate ITownNPCProfile DelegateModifyTownNPCProfile(NPC npc);
	private static HookList HookModifyTownNPCProfile = AddHook<DelegateModifyTownNPCProfile>(g => g.ModifyTownNPCProfile);
	public static void ModifyTownNPCProfile(NPC npc, ref ITownNPCProfile profile)
	{
		profile = npc.ModNPC?.TownNPCProfile() ?? profile;

		foreach (GlobalNPC g in HookModifyTownNPCProfile.Enumerate(npc.globalNPCs)) {
			profile = g.ModifyTownNPCProfile(npc) ?? profile;
		}
	}

	private static HookList HookResetEffects = AddHook<Action<NPC>>(g => g.ResetEffects);

	public static void ResetEffects(NPC npc)
	{
		npc.ModNPC?.ResetEffects();

		foreach (GlobalNPC g in HookResetEffects.Enumerate(npc.globalNPCs)) {
			g.ResetEffects(npc);
		}
	}

	public static void NPCAI(NPC npc)
	{
		if (PreAI(npc)) {
			int type = npc.type;
			bool useAiType = npc.ModNPC != null && npc.ModNPC.AIType > 0;
			if (useAiType) {
				npc.type = npc.ModNPC.AIType;
			}
			npc.VanillaAI();
			if (useAiType) {
				npc.type = type;
			}
			AI(npc);
		}
		PostAI(npc);
	}

	private static HookList HookPreAI = AddHook<Func<NPC, bool>>(g => g.PreAI);

	public static bool PreAI(NPC npc)
	{
		bool result = true;
		foreach (GlobalNPC g in HookPreAI.Enumerate(npc.globalNPCs)) {
			result &= g.PreAI(npc);
		}
		if (result && npc.ModNPC != null) {
			return npc.ModNPC.PreAI();
		}
		return result;
	}

	private static HookList HookAI = AddHook<Action<NPC>>(g => g.AI);

	public static void AI(NPC npc)
	{
		npc.ModNPC?.AI();

		foreach (GlobalNPC g in HookAI.Enumerate(npc.globalNPCs)) {
			g.AI(npc);
		}
	}

	private static HookList HookPostAI = AddHook<Action<NPC>>(g => g.PostAI);

	public static void PostAI(NPC npc)
	{
		npc.ModNPC?.PostAI();

		foreach (GlobalNPC g in HookPostAI.Enumerate(npc.globalNPCs)) {
			g.PostAI(npc);
		}
	}

	public static void SendExtraAI(BinaryWriter writer, byte[] extraAI)
	{
		writer.Write7BitEncodedInt(extraAI.Length);

		if (extraAI.Length > 0) {
			writer.Write(extraAI);
		}
	}

	private static HookList HookWriteExtraAI = AddHook<Action<NPC, BitWriter, BinaryWriter>>(g => g.SendExtraAI);

	public static byte[] WriteExtraAI(NPC npc)
	{
		using var stream = new MemoryStream();
		using var modWriter = new BinaryWriter(stream);

		npc.ModNPC?.SendExtraAI(modWriter);

		using var bufferedStream = new MemoryStream();
		using var globalWriter = new BinaryWriter(bufferedStream);

		BitWriter bitWriter = new BitWriter();

		foreach (GlobalNPC g in HookWriteExtraAI.Enumerate(npc.globalNPCs)) {
			g.SendExtraAI(npc, bitWriter, globalWriter);
		}

		bitWriter.Flush(modWriter);

		modWriter.Write(bufferedStream.ToArray());

		globalWriter.Flush();

		modWriter.Flush();

		return stream.ToArray();
	}

	public static byte[] ReadExtraAI(BinaryReader reader)
	{
		return reader.ReadBytes(reader.Read7BitEncodedInt());
	}

	private static HookList HookReceiveExtraAI = AddHook<Action<NPC, BitReader, BinaryReader>>(g => g.ReceiveExtraAI);

	public static void ReceiveExtraAI(NPC npc, byte[] extraAI)
	{
		using var stream = new MemoryStream(extraAI);
		using var modReader = new BinaryReader(stream);

		npc.ModNPC?.ReceiveExtraAI(modReader);

		BitReader bitReader = new BitReader(modReader);

		try {
			foreach (GlobalNPC g in HookReceiveExtraAI.Enumerate(npc.globalNPCs)) {
				g.ReceiveExtraAI(npc, bitReader, modReader);
			}

			if (bitReader.BitsRead < bitReader.MaxBits) {
				throw new IOException($"Read underflow {bitReader.MaxBits - bitReader.BitsRead} of {bitReader.MaxBits} compressed bits in ReceiveExtraAI, more info below");
			}

			if (stream.Position < stream.Length) {
				throw new IOException($"Read underflow {stream.Length - stream.Position} of {stream.Length} bytes in ReceiveExtraAI, more info below");
			}
		}
		catch (IOException e) {
			Logging.tML.Error(e.ToString());

			string message = $"Above IOException error in NPC {(npc.ModNPC == null ? npc.TypeName : npc.ModNPC.FullName)} occured";

			var culprits = new List<GlobalNPC>();
			foreach (GlobalNPC g in HookReceiveExtraAI.Enumerate(npc.globalNPCs)) {
				culprits.Add(g);
			}

			if (culprits.Count > 0) {
				message += ", may be caused by one of these:";
				foreach (GlobalNPC g in culprits) {
					message += $"\n\t{g.FullName}";
				}
			}

			Logging.tML.Error(message);
		}
	}

	private static HookList HookFindFrame = AddHook<Action<NPC, int>>(g => g.FindFrame);

	public static void FindFrame(NPC npc, int frameHeight)
	{
		npc.VanillaFindFrame(frameHeight, npc.isLikeATownNPC, npc.ModNPC?.AnimationType is > 0 ? npc.ModNPC.AnimationType : npc.type);
		npc.ModNPC?.FindFrame(frameHeight);

		foreach (GlobalNPC g in HookFindFrame.Enumerate(npc.globalNPCs)) {
			g.FindFrame(npc, frameHeight);
		}
	}

	private static HookList HookHitEffect = AddHook<Action<NPC, NPC.HitInfo>>(g => g.HitEffect);

	public static void HitEffect(NPC npc, in NPC.HitInfo hit)
	{
		npc.ModNPC?.HitEffect(hit);

		foreach (GlobalNPC g in HookHitEffect.Enumerate(npc.globalNPCs)) {
			g.HitEffect(npc, hit);
		}
	}

	private delegate void DelegateUpdateLifeRegen(NPC npc, ref int damage);
	private static HookList HookUpdateLifeRegen = AddHook<DelegateUpdateLifeRegen>(g => g.UpdateLifeRegen);

	public static void UpdateLifeRegen(NPC npc, ref int damage)
	{
		npc.ModNPC?.UpdateLifeRegen(ref damage);

		foreach (GlobalNPC g in HookUpdateLifeRegen.Enumerate(npc.globalNPCs)) {
			g.UpdateLifeRegen(npc, ref damage);
		}
	}

	private static HookList HookCheckActive = AddHook<Func<NPC, bool>>(g => g.CheckActive);

	public static bool CheckActive(NPC npc)
	{
		if (npc.ModNPC != null && !npc.ModNPC.CheckActive()) {
			return false;
		}
		foreach (GlobalNPC g in HookCheckActive.Enumerate(npc.globalNPCs)) {
			if (!g.CheckActive(npc)) {
				return false;
			}
		}
		return true;
	}

	private static HookList HookCheckDead = AddHook<Func<NPC, bool>>(g => g.CheckDead);

	public static bool CheckDead(NPC npc)
	{
		bool result = true;

		if (npc.ModNPC != null) {
			result = npc.ModNPC.CheckDead();
		}

		foreach (GlobalNPC g in HookCheckDead.Enumerate(npc.globalNPCs)) {
			result &= g.CheckDead(npc);
		}

		return result;
	}

	private static HookList HookSpecialOnKill = AddHook<Func<NPC, bool>>(g => g.SpecialOnKill);

	public static bool SpecialOnKill(NPC npc)
	{
		foreach (GlobalNPC g in HookSpecialOnKill.Enumerate(npc.globalNPCs)) {
			if (g.SpecialOnKill(npc)) {
				return true;
			}
		}
		if (npc.ModNPC != null) {
			return npc.ModNPC.SpecialOnKill();
		}
		return false;
	}

	private static HookList HookPreKill = AddHook<Func<NPC, bool>>(g => g.PreKill);

	public static bool PreKill(NPC npc)
	{
		bool result = true;
		foreach (GlobalNPC g in HookPreKill.Enumerate(npc.globalNPCs)) {
			result &= g.PreKill(npc);
		}

		if (result && npc.ModNPC != null) {
			result = npc.ModNPC.PreKill();
		}

		if (!result) {
			blockLoot.Clear();
			return false;
		}

		return true;
	}

	private static HookList HookOnKill = AddHook<Action<NPC>>(g => g.OnKill);

	public static void OnKill(NPC npc)
	{
		npc.ModNPC?.OnKill();

		foreach (GlobalNPC g in HookOnKill.Enumerate(npc.globalNPCs)) {
			g.OnKill(npc);
		}

		blockLoot.Clear();
	}

	private static HookList HookModifyNPCLoot = AddHook<Action<NPC, NPCLoot>>(g => g.ModifyNPCLoot);
	public static void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
	{
		npc.ModNPC?.ModifyNPCLoot(npcLoot);

		foreach (GlobalNPC g in HookModifyNPCLoot.Enumerate(npc.globalNPCs)) {
			g.ModifyNPCLoot(npc, npcLoot);
		}
	}

	private static HookList HookModifyGlobalLoot = AddHook<Action<GlobalLoot>>(g => g.ModifyGlobalLoot);
	public static void ModifyGlobalLoot(GlobalLoot globalLoot)
	{
		foreach (GlobalNPC g in HookModifyGlobalLoot.Enumerate(globalNPCs)) {
			g.ModifyGlobalLoot(globalLoot);
		}
	}

	public static void BossLoot(NPC npc, ref string name, ref int potionType)
	{
		npc.ModNPC?.BossLoot(ref name, ref potionType);
	}

	private static HookList HookCanFallThroughPlatforms = AddHook<Func<NPC, bool?>>(g => g.CanFallThroughPlatforms);

	public static bool? CanFallThroughPlatforms(NPC npc)
	{
		bool? ret = npc.ModNPC?.CanFallThroughPlatforms() ?? null;
		if (ret.HasValue) {
			if (!ret.Value) {
				return false;
			}
			ret = true;
		}

		foreach (GlobalNPC g in HookCanFallThroughPlatforms.Enumerate(npc.globalNPCs)) {
			bool? globalRet = g.CanFallThroughPlatforms(npc);
			if (globalRet.HasValue) {
				if (!globalRet.Value) {
					return false;
				}
				ret = true;
			}
		}

		return ret;
	}

	private static HookList HookCanBeCaughtBy = AddHook<Func<NPC, Item, Player, bool?>>(g => g.CanBeCaughtBy);

	public static bool? CanBeCaughtBy(NPC npc, Item item, Player player)
	{
		bool? canBeCaughtOverall = null;
		foreach (GlobalNPC g in HookCanBeCaughtBy.Enumerate(npc.globalNPCs)) {
			bool? canBeCaughtFromGlobalNPC = g.CanBeCaughtBy(npc, item, player);
			if (canBeCaughtFromGlobalNPC.HasValue) {
				if (!canBeCaughtFromGlobalNPC.Value)
					return false;

				canBeCaughtOverall = true;
			}
		}
		if (npc.ModNPC != null) {
			bool? canBeCaughtAsModNPC = npc.ModNPC.CanBeCaughtBy(item, player);
			if (canBeCaughtAsModNPC.HasValue) {
				if (!canBeCaughtAsModNPC.Value)
					return false;

				canBeCaughtOverall = true;
			}
		}
		return canBeCaughtOverall;
	}

	private static HookList HookOnCaughtBy = AddHook<Action<NPC, Player, Item, bool>>(g => g.OnCaughtBy);

	public static void OnCaughtBy(NPC npc, Player player, Item item, bool failed)
	{
		npc.ModNPC?.OnCaughtBy(player, item, failed);

		foreach (GlobalNPC g in HookOnCaughtBy.Enumerate(npc.globalNPCs)) {
			g.OnCaughtBy(npc, player, item, failed);
		}
	}

	private delegate bool DelegateCanHitPlayer(NPC npc, Player target, ref int cooldownSlot);
	private static HookList HookCanHitPlayer = AddHook<DelegateCanHitPlayer>(g => g.CanHitPlayer);

	public static bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
	{
		foreach (GlobalNPC g in HookCanHitPlayer.Enumerate(npc.globalNPCs)) {
			if (!g.CanHitPlayer(npc, target, ref cooldownSlot)) {
				return false;
			}
		}
		if (npc.ModNPC != null) {
			return npc.ModNPC.CanHitPlayer(target, ref cooldownSlot);
		}
		return true;
	}

	private delegate void DelegateModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers);
	private static HookList HookModifyHitPlayer = AddHook<DelegateModifyHitPlayer>(g => g.ModifyHitPlayer);

	public static void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
	{
		npc.ModNPC?.ModifyHitPlayer(target, ref modifiers);

		foreach (GlobalNPC g in HookModifyHitPlayer.Enumerate(npc.globalNPCs)) {
			g.ModifyHitPlayer(npc, target, ref modifiers);
		}
	}

	private static HookList HookOnHitPlayer = AddHook<Action<NPC, Player, Player.HurtInfo>>(g => g.OnHitPlayer);

	public static void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
	{
		npc.ModNPC?.OnHitPlayer(target, hurtInfo);

		foreach (GlobalNPC g in HookOnHitPlayer.Enumerate(npc.globalNPCs)) {
			g.OnHitPlayer(npc, target, hurtInfo);
		}
	}

	private static HookList HookCanHitNPC = AddHook<Func<NPC, NPC, bool>>(g => g.CanHitNPC);

	public static bool CanHitNPC(NPC npc, NPC target)
	{
		foreach (GlobalNPC g in HookCanHitNPC.Enumerate(npc.globalNPCs)) {
			if (!g.CanHitNPC(npc, target))
				return false;
		}

		return npc.ModNPC?.CanHitNPC(target) ?? true;
	}

	private delegate void DelegateModifyHitNPC(NPC npc, NPC target, ref NPC.HitModifiers modifiers);
	private static HookList HookModifyHitNPC = AddHook<DelegateModifyHitNPC>(g => g.ModifyHitNPC);

	public static void ModifyHitNPC(NPC npc, NPC target, ref NPC.HitModifiers modifiers)
	{
		npc.ModNPC?.ModifyHitNPC(target, ref modifiers);

		foreach (GlobalNPC g in HookModifyHitNPC.Enumerate(npc.globalNPCs)) {
			g.ModifyHitNPC(npc, target, ref modifiers);
		}
	}

	private static HookList HookOnHitNPC = AddHook<Action<NPC, NPC, NPC.HitInfo>>(g => g.OnHitNPC);

	public static void OnHitNPC(NPC npc, NPC target, in NPC.HitInfo hit)
	{
		npc.ModNPC?.OnHitNPC(target, hit);

		foreach (GlobalNPC g in HookOnHitNPC.Enumerate(npc.globalNPCs)) {
			g.OnHitNPC(npc, target, hit);
		}
	}

	private static HookList HookCanBeHitByItem = AddHook<Func<NPC, Player, Item, bool?>>(g => g.CanBeHitByItem);

	public static bool? CanBeHitByItem(NPC npc, Player player, Item item)
	{
		bool? flag = null;

		foreach (GlobalNPC g in HookCanBeHitByItem.Enumerate(npc.globalNPCs)) {
			bool? canHit = g.CanBeHitByItem(npc, player, item);

			if (canHit.HasValue) {
				if (!canHit.Value) {
					return false;
				}

				flag = true;
			}
		}

		if (npc.ModNPC != null) {
			bool? canHit = npc.ModNPC.CanBeHitByItem(player, item);

			if (canHit.HasValue) {
				if (!canHit.Value) {
					return false;
				}

				flag = true;
			}
		}

		return flag;
	}

	private static HookList HookCanCollideWithPlayerMeleeAttack = AddHook<Func<NPC, Player, Item, Rectangle, bool?>>(g => g.CanCollideWithPlayerMeleeAttack);
	public static bool? CanCollideWithPlayerMeleeAttack(NPC npc, Player player, Item item, Rectangle meleeAttackHitbox)
	{
		bool? flag = null;
		foreach (GlobalNPC g in HookCanCollideWithPlayerMeleeAttack.Enumerate(npc.globalNPCs)) {
			bool? canCollide = g.CanCollideWithPlayerMeleeAttack(npc, player, item, meleeAttackHitbox);
			if (canCollide.HasValue) {
				if (!canCollide.Value) {
					return false;
				}

				flag = true;
			}
		}

		if (npc.ModNPC != null) {
			bool? canHit = npc.ModNPC.CanCollideWithPlayerMeleeAttack(player, item, meleeAttackHitbox);

			if (canHit.HasValue) {
				if (!canHit.Value) {
					return false;
				}

				flag = true;
			}
		}

		return flag;
	}

	private delegate void DelegateModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers);
	private static HookList HookModifyHitByItem = AddHook<DelegateModifyHitByItem>(g => g.ModifyHitByItem);

	public static void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
	{
		npc.ModNPC?.ModifyHitByItem(player, item, ref modifiers);

		foreach (GlobalNPC g in HookModifyHitByItem.Enumerate(npc.globalNPCs)) {
			g.ModifyHitByItem(npc, player, item, ref modifiers);
		}
	}

	private static HookList HookOnHitByItem = AddHook<Action<NPC, Player, Item, NPC.HitInfo, int>>(g => g.OnHitByItem);

	public static void OnHitByItem(NPC npc, Player player, Item item, in NPC.HitInfo hit, int damageDone)
	{
		npc.ModNPC?.OnHitByItem(player, item, hit, damageDone);

		foreach (GlobalNPC g in HookOnHitByItem.Enumerate(npc.globalNPCs)) {
			g.OnHitByItem(npc, player, item, hit, damageDone);
		}
	}

	private static HookList HookCanBeHitByProjectile = AddHook<Func<NPC, Projectile, bool?>>(g => g.CanBeHitByProjectile);

	public static bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
	{
		bool? flag = null;
		foreach (GlobalNPC g in HookCanBeHitByProjectile.Enumerate(npc.globalNPCs)) {
			bool? canHit = g.CanBeHitByProjectile(npc, projectile);
			if (canHit.HasValue && !canHit.Value) {
				return false;
			}
			if (canHit.HasValue) {
				flag = canHit.Value;
			}
		}
		if (npc.ModNPC != null) {
			bool? canHit = npc.ModNPC.CanBeHitByProjectile(projectile);
			if (canHit.HasValue && !canHit.Value) {
				return false;
			}
			if (canHit.HasValue) {
				flag = canHit.Value;
			}
		}
		return flag;
	}

	private delegate void DelegateModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers);
	private static HookList HookModifyHitByProjectile = AddHook<DelegateModifyHitByProjectile>(g => g.ModifyHitByProjectile);

	public static void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
	{
		npc.ModNPC?.ModifyHitByProjectile(projectile, ref modifiers);

		foreach (GlobalNPC g in HookModifyHitByProjectile.Enumerate(npc.globalNPCs)) {
			g.ModifyHitByProjectile(npc, projectile, ref modifiers);
		}
	}

	private static HookList HookOnHitByProjectile = AddHook<Action<NPC, Projectile, NPC.HitInfo, int>>(g => g.OnHitByProjectile);

	public static void OnHitByProjectile(NPC npc, Projectile projectile, in NPC.HitInfo hit, int damageDone)
	{
		npc.ModNPC?.OnHitByProjectile(projectile, hit, damageDone);

		foreach (GlobalNPC g in HookOnHitByProjectile.Enumerate(npc.globalNPCs)) {
			g.OnHitByProjectile(npc, projectile, hit, damageDone);
		}
	}

	private delegate void DelegateAddModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers);
	private static HookList HookAddModifyIncomingHit = AddHook<DelegateAddModifyIncomingHit>(g => g.ModifyIncomingHit);

	public static void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
	{
		npc.ModNPC?.ModifyIncomingHit(ref modifiers);
		foreach (GlobalNPC g in HookAddModifyIncomingHit.Enumerate(npc.globalNPCs)) {
			g.ModifyIncomingHit(npc, ref modifiers);
		}
	}

	private delegate void DelegateBossHeadSlot(NPC npc, ref int index);
	private static HookList HookBossHeadSlot = AddHook<DelegateBossHeadSlot>(g => g.BossHeadSlot);

	public static void BossHeadSlot(NPC npc, ref int index)
	{
		npc.ModNPC?.BossHeadSlot(ref index);

		foreach (GlobalNPC g in HookBossHeadSlot.Enumerate(npc.globalNPCs)) {
			g.BossHeadSlot(npc, ref index);
		}
	}

	private delegate void DelegateBossHeadRotation(NPC npc, ref float rotation);
	private static HookList HookBossHeadRotation = AddHook<DelegateBossHeadRotation>(g => g.BossHeadRotation);

	public static void BossHeadRotation(NPC npc, ref float rotation)
	{
		npc.ModNPC?.BossHeadRotation(ref rotation);

		foreach (GlobalNPC g in HookBossHeadRotation.Enumerate(npc.globalNPCs)) {
			g.BossHeadRotation(npc, ref rotation);
		}
	}

	private delegate void DelegateBossHeadSpriteEffects(NPC npc, ref SpriteEffects spriteEffects);
	private static HookList HookBossHeadSpriteEffects = AddHook<DelegateBossHeadSpriteEffects>(g => g.BossHeadSpriteEffects);

	public static void BossHeadSpriteEffects(NPC npc, ref SpriteEffects spriteEffects)
	{
		npc.ModNPC?.BossHeadSpriteEffects(ref spriteEffects);

		foreach (GlobalNPC g in HookBossHeadSpriteEffects.Enumerate(npc.globalNPCs)) {
			g.BossHeadSpriteEffects(npc, ref spriteEffects);
		}
	}

	private static HookList HookGetAlpha = AddHook<Func<NPC, Color, Color?>>(g => g.GetAlpha);

	public static Color? GetAlpha(NPC npc, Color lightColor)
	{
		foreach (GlobalNPC g in HookGetAlpha.Enumerate(npc.globalNPCs)) {
			Color? color = g.GetAlpha(npc, lightColor);
			if (color.HasValue) {
				return color.Value;
			}
		}
		return npc.ModNPC?.GetAlpha(lightColor);
	}

	private delegate void DelegateDrawEffects(NPC npc, ref Color drawColor);
	private static HookList HookDrawEffects = AddHook<DelegateDrawEffects>(g => g.DrawEffects);

	public static void DrawEffects(NPC npc, ref Color drawColor)
	{
		npc.ModNPC?.DrawEffects(ref drawColor);

		foreach (GlobalNPC g in HookDrawEffects.Enumerate(npc.globalNPCs)) {
			g.DrawEffects(npc, ref drawColor);
		}
	}

	private delegate bool DelegatePreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor);
	private static HookList HookPreDraw = AddHook<DelegatePreDraw>(g => g.PreDraw);

	public static bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		bool result = true;
		foreach (GlobalNPC g in HookPreDraw.Enumerate(npc.globalNPCs)) {
			result &= g.PreDraw(npc, spriteBatch, screenPos, drawColor);
		}
		if (result && npc.ModNPC != null) {
			return npc.ModNPC.PreDraw(spriteBatch, screenPos, drawColor);
		}
		return result;
	}

	private delegate void DelegatePostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor);
	private static HookList HookPostDraw = AddHook<DelegatePostDraw>(g => g.PostDraw);

	public static void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		npc.ModNPC?.PostDraw(spriteBatch, screenPos, drawColor);

		foreach (GlobalNPC g in HookPostDraw.Enumerate(npc.globalNPCs)) {
			g.PostDraw(npc, spriteBatch, screenPos, drawColor);
		}
	}

	private static HookList HookDrawBehind = AddHook<Action<NPC, int>>(g => g.DrawBehind);

	internal static void DrawBehind(NPC npc, int index)
	{
		npc.ModNPC?.DrawBehind(index);

		foreach (GlobalNPC g in HookDrawBehind.Enumerate(npc.globalNPCs)) {
			g.DrawBehind(npc, index);
		}
	}

	private delegate bool? DelegateDrawHealthBar(NPC npc, byte bhPosition, ref float scale, ref Vector2 position);
	private static HookList HookDrawHealthBar = AddHook<DelegateDrawHealthBar>(g => g.DrawHealthBar);

	public static bool DrawHealthBar(NPC npc, ref float scale)
	{
		Vector2 position = new Vector2(npc.position.X + npc.width / 2, npc.position.Y + npc.gfxOffY);
		if (Main.HealthBarDrawSettings == 1) {
			position.Y += npc.height + 10f + Main.NPCAddHeight(npc);
		}
		else if (Main.HealthBarDrawSettings == 2) {
			position.Y -= 24f + Main.NPCAddHeight(npc) / 2f;
		}
		foreach (GlobalNPC g in HookDrawHealthBar.Enumerate(npc.globalNPCs)) {
			bool? result = g.DrawHealthBar(npc, Main.HealthBarDrawSettings, ref scale, ref position);
			if (result.HasValue) {
				if (result.Value) {
					DrawHealthBar(npc, position, scale);
				}
				return false;
			}
		}
		if (NPCLoader.IsModNPC(npc)) {
			bool? result = npc.ModNPC.DrawHealthBar(Main.HealthBarDrawSettings, ref scale, ref position);
			if (result.HasValue) {
				if (result.Value) {
					DrawHealthBar(npc, position, scale);
				}
				return false;
			}
		}
		return true;
	}

	private static void DrawHealthBar(NPC npc, Vector2 position, float scale)
	{
		float alpha = Lighting.Brightness((int)(npc.Center.X / 16f), (int)(npc.Center.Y / 16f));
		Main.instance.DrawHealthBar(position.X, position.Y, npc.life, npc.lifeMax, alpha, scale);
	}

	private delegate void DelegateEditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns);
	private static HookList HookEditSpawnRate = AddHook<DelegateEditSpawnRate>(g => g.EditSpawnRate);

	public static void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
	{
		foreach (GlobalNPC g in HookEditSpawnRate.Enumerate(globalNPCs)) {
			g.EditSpawnRate(player, ref spawnRate, ref maxSpawns);
		}
	}

	private delegate void DelegateEditSpawnRange(Player player, ref int spawnRangeX, ref int spawnRangeY,
		ref int safeRangeX, ref int safeRangeY);
	private static HookList HookEditSpawnRange = AddHook<DelegateEditSpawnRange>(g => g.EditSpawnRange);

	public static void EditSpawnRange(Player player, ref int spawnRangeX, ref int spawnRangeY,
		ref int safeRangeX, ref int safeRangeY)
	{
		foreach (GlobalNPC g in HookEditSpawnRange.Enumerate(globalNPCs)) {
			g.EditSpawnRange(player, ref spawnRangeX, ref spawnRangeY, ref safeRangeX, ref safeRangeY);
		}
	}

	private static HookList HookEditSpawnPool = AddHook<Action<Dictionary<int, float>, NPCSpawnInfo>>(g => g.EditSpawnPool);

	public static int? ChooseSpawn(NPCSpawnInfo spawnInfo)
	{
		NPCSpawnHelper.Reset();
		NPCSpawnHelper.DoChecks(spawnInfo);

		IDictionary<int, float> pool = new Dictionary<int, float>();
		pool[0] = 1f;
		foreach (ModNPC npc in npcs) {
			float weight = npc.SpawnChance(spawnInfo);
			if (weight > 0f) {
				pool[npc.NPC.type] = weight;
			}
		}
		foreach (GlobalNPC g in HookEditSpawnPool.Enumerate(globalNPCs)) {
			g.EditSpawnPool(pool, spawnInfo);
		}
		float totalWeight = 0f;
		foreach (int type in pool.Keys) {
			if (pool[type] < 0f) {
				pool[type] = 0f;
			}
			totalWeight += pool[type];
		}
		float choice = (float)Main.rand.NextDouble() * totalWeight;
		foreach (int type in pool.Keys) {
			float weight = pool[type];
			if (choice < weight) {
				return type;
			}
			choice -= weight;
		}
		return null;
	}

	private static HookList HookSpawnNPC = AddHook<Action<int, int, int>>(g => g.SpawnNPC);

	public static int SpawnNPC(int type, int tileX, int tileY)
	{
		var npc = type >= NPCID.Count
			? GetNPC(type).SpawnNPC(tileX, tileY)
			: NPC.NewNPC(NPC.GetSpawnSourceForNaturalSpawn(), tileX * 16 + 8, tileY * 16, type);

		foreach (GlobalNPC g in HookSpawnNPC.Enumerate(Main.npc[npc].globalNPCs)) {
			g.SpawnNPC(npc, tileX, tileY);
		}

		return npc;
	}

	public static void CanTownNPCSpawn(int numTownNPCs)
	{
		foreach (ModNPC modNPC in npcs) {
			var npc = modNPC.NPC;

			if (npc.townNPC && NPC.TypeToDefaultHeadIndex(npc.type) >= 0 && !NPC.AnyNPCs(npc.type) && modNPC.CanTownNPCSpawn(numTownNPCs)) {

				Main.townNPCCanSpawn[npc.type] = true;

				if (WorldGen.prioritizedTownNPCType == 0) {
					WorldGen.prioritizedTownNPCType = npc.type;
				}
			}
		}
	}

	/* Disabled until #2083 is addressed. Originally introduced in #1323, but was refactored and now would be for additional features outside PR scope.
	private static HookList HookModifyNPCHappiness = AddHook(g => g.ModifyNPCHappiness);

	public static void ModifyNPCHappiness(NPC npc, int primaryPlayerBiome, ShopHelper shopHelperInstance, bool[] nearbyNPCsByType) {
		npc.ModNPC?.ModifyNPCHappiness(primaryPlayerBiome, shopHelperInstance, nearbyNPCsByType);

		foreach (GlobalNPC g in HookModifyNPCHappiness.Enumerate(globalNPCs)) {
			g.Instance(npc).ModifyNPCHappiness(npc, primaryPlayerBiome, shopHelperInstance, nearbyNPCsByType);
		}
	}
	*/

	public static bool CheckConditions(int type)
	{
		return GetNPC(type)?.CheckConditions(WorldGen.roomX1, WorldGen.roomX2, WorldGen.roomY1, WorldGen.roomY2) ?? true;
	}

	private delegate void DelegateModifyTypeName(NPC npc, ref string typeName);
	private static HookList HookModifyTypeName = AddHook<DelegateModifyTypeName>(g => g.ModifyTypeName);
	public static string ModifyTypeName(NPC npc, string typeName)
	{
		if (npc.ModNPC != null)
			npc.ModNPC.ModifyTypeName(ref typeName);

		foreach (GlobalNPC g in HookModifyTypeName.Enumerate(npc.globalNPCs)) {
			g.ModifyTypeName(npc, ref typeName);
		}

		return typeName;
	}

	private delegate void DelegateModifyHoverBoundingBox(NPC npc, ref Rectangle boundingBox);
	private static HookList HookModifyHoverBoundingBox = AddHook<DelegateModifyHoverBoundingBox>(g => g.ModifyHoverBoundingBox);
	public static void ModifyHoverBoundingBox(NPC npc, ref Rectangle boundingBox)
	{
		if (npc.ModNPC != null)
			npc.ModNPC.ModifyHoverBoundingBox(ref boundingBox);

		foreach (GlobalNPC g in HookModifyHoverBoundingBox.Enumerate(npc.globalNPCs)) {
			g.ModifyHoverBoundingBox(npc, ref boundingBox);
		}
	}

	private static HookList HookModifyNPCNameList = AddHook<Action<NPC, List<string>>>(g => g.ModifyNPCNameList);
	public static List<string> ModifyNPCNameList(NPC npc, List<string> nameList)
	{
		if (npc.ModNPC != null)
			nameList = npc.ModNPC.SetNPCNameList();

		foreach (GlobalNPC g in HookModifyNPCNameList.Enumerate(npc.globalNPCs)) {
			g.ModifyNPCNameList(npc, nameList);
		}

		return nameList;
	}

	public static bool UsesPartyHat(NPC npc)
	{
		return npc.ModNPC?.UsesPartyHat() ?? true;
	}

	private static HookList HookCanChat = AddHook<Func<NPC, bool?>>(g => g.CanChat);

	public static bool? CanChat(NPC npc)
	{
		bool? ret = npc.ModNPC?.CanChat();

		foreach (GlobalNPC g in HookCanChat.Enumerate(npc.globalNPCs)) {
			if (g.CanChat(npc) is bool canChat) {
				if (!canChat)
					return false;
				
				ret = true;
			}
		}

		return ret;
	}

	private delegate void DelegateGetChat(NPC npc, ref string chat);
	private static HookList HookGetChat = AddHook<DelegateGetChat>(g => g.GetChat);

	public static void GetChat(NPC npc, ref string chat)
	{
		if (npc.ModNPC != null) {
			chat = npc.ModNPC.GetChat();
		}
		else if (chat.Equals("")) {
			chat = Language.GetTextValue("tModLoader.DefaultTownNPCChat");
		}
		foreach (GlobalNPC g in HookGetChat.Enumerate(npc.globalNPCs)) {
			g.GetChat(npc, ref chat);
		}
	}

	public static void SetChatButtons(ref string button, ref string button2)
	{
		Main.LocalPlayer.TalkNPC?.ModNPC?.SetChatButtons(ref button, ref button2);
	}

	private static HookList HookPreChatButtonClicked = AddHook<Func<NPC, bool, bool>>(g => g.PreChatButtonClicked);

	public static bool PreChatButtonClicked(bool firstButton)
	{
		NPC npc = Main.LocalPlayer.TalkNPC;

		bool result = true;
		foreach (GlobalNPC g in HookPreChatButtonClicked.Enumerate(npc.globalNPCs)) {
			result &= g.PreChatButtonClicked(npc, firstButton);
		}

		if (!result) {
			SoundEngine.PlaySound(SoundID.MenuTick);
			return false;
		}

		return true;
	}

	private delegate void DelegateOnChatButtonClicked(NPC npc, bool firstButton);
	private static HookList HookOnChatButtonClicked = AddHook<DelegateOnChatButtonClicked>(g => g.OnChatButtonClicked);

	public static void OnChatButtonClicked(bool firstButton)
	{
		NPC npc = Main.LocalPlayer.TalkNPC;
		string shopName = null;

		if (npc.ModNPC != null) {
			npc.ModNPC.OnChatButtonClicked(firstButton, ref shopName);
			SoundEngine.PlaySound(SoundID.MenuTick);

			if (shopName != null) {
				// Copied from Main.OpenShop
				Main.playerInventory = true;
				Main.stackSplit = 9999;
				Main.npcChatText = "";
				Main.SetNPCShopIndex(1);
				Main.instance.shop[Main.npcShop].SetupShop(NPCShopDatabase.GetShopName(npc.type, shopName), npc);
			}
		}

		foreach (GlobalNPC g in HookOnChatButtonClicked.Enumerate(npc.globalNPCs)) {
			g.OnChatButtonClicked(npc, firstButton);
		}
	}

	public static void AddShops(int type) {
		GetNPC(type)?.AddShops();
	}

	private delegate void DelegateModifyShop(NPCShop shop);
	private static HookList HookModifyShop = AddHook<DelegateModifyShop>(g => g.ModifyShop);

	public static void ModifyShop(NPCShop shop)
	{
		foreach (GlobalNPC g in HookModifyShop.Enumerate(globalNPCs)) {
			g.ModifyShop(shop);
		}
	}

	private delegate void DelegateModifyActiveShop(NPC npc, string shopName, Item[] items);
	private static HookList HookModifyActiveShop = AddHook<DelegateModifyActiveShop>(g => g.ModifyActiveShop);

	public static void ModifyActiveShop(NPC npc, string shopName, Item[] shopContents)
	{
		GetNPC(npc.type)?.ModifyActiveShop(shopName, shopContents);
		foreach (GlobalNPC g in HookModifyActiveShop.Enumerate(globalNPCs)) {
			g.ModifyActiveShop(npc, shopName, shopContents);
		}
	}

	private delegate void DelegateSetupTravelShop(int[] shop, ref int nextSlot);
	private static HookList HookSetupTravelShop = AddHook<DelegateSetupTravelShop>(g => g.SetupTravelShop);

	public static void SetupTravelShop(int[] shop, ref int nextSlot)
	{
		foreach (GlobalNPC g in HookSetupTravelShop.Enumerate(globalNPCs)) {
			g.SetupTravelShop(shop, ref nextSlot);
		}
	}

	private static HookList HookCanGoToStatue = AddHook<Func<NPC, bool, bool?>>(g => g.CanGoToStatue);

	public static bool? CanGoToStatue(NPC npc, bool toKingStatue)
	{
		bool? ret = npc.ModNPC?.CanGoToStatue(toKingStatue);

		foreach (GlobalNPC g in HookCanGoToStatue.Enumerate(npc.globalNPCs)) {
			if (g.CanGoToStatue(npc, toKingStatue) is bool canGo) {
				if (!canGo)
					return false;
				
				ret = true;
			}
		}

		return ret;
	}

	private static HookList HookOnGoToStatue = AddHook<Action<NPC, bool>>(g => g.OnGoToStatue);

	public static void OnGoToStatue(NPC npc, bool toKingStatue)
	{
		npc.ModNPC?.OnGoToStatue(toKingStatue);

		foreach (GlobalNPC g in HookOnGoToStatue.Enumerate(npc.globalNPCs)) {
			g.OnGoToStatue(npc, toKingStatue);
		}
	}

	private delegate void DelegateBuffTownNPC(ref float damageMult, ref int defense);
	private static HookList HookBuffTownNPC = AddHook<DelegateBuffTownNPC>(g => g.BuffTownNPC);

	public static void BuffTownNPC(ref float damageMult, ref int defense)
	{
		foreach (GlobalNPC g in HookBuffTownNPC.Enumerate(globalNPCs)) {
			g.BuffTownNPC(ref damageMult, ref defense);
		}
	}
	//attack type 0 = throwing
	//  num405 = type, num406 = damage, knockBack, scaleFactor7 = speed multiplier, num407 = attack delay
	//  num408 = unknown, maxValue3 = unknown, num409 = gravity correction factor, num411 = random speed offset
	//attack type 1 = shooting
	//  num413 = type, num414 = damage, scaleFactor8 = speed multiplier, num415 = attack delay,
	//  num416 = unknown, maxValue4 = unknown, knockBack2, num417 = gravity correction,
	//  flag53 = in between shots, num418 = random speed offset
	//attack type 2 = magic
	//  num423 = type, num424 = damage, scaleFactor9 = speed multiplier, num425 = attack delay,
	//  num426 = unknown, maxValue5 = unknown, knockBack3, num427 = gravity correction factor,
	//  num429 = aura light multiplier, num430 = random speed offset
	//attack type 3 = swinging
	//  num439 = unknown, maxValue6 = unknown, num440 = damage, num441 = knockback,
	//  num442 = item width, num443 = item height
	//unknowns are associated with ai[1], localAI[1], and localAI[3] when ai[0] is either 0 or 8
	//check patch files for town NPC attacks
	private delegate void DelegateTownNPCAttackStrength(NPC npc, ref int damage, ref float knockback);
	private static HookList HookTownNPCAttackStrength = AddHook<DelegateTownNPCAttackStrength>(g => g.TownNPCAttackStrength);

	public static void TownNPCAttackStrength(NPC npc, ref int damage, ref float knockback)
	{
		npc.ModNPC?.TownNPCAttackStrength(ref damage, ref knockback);

		foreach (GlobalNPC g in HookTownNPCAttackStrength.Enumerate(npc.globalNPCs)) {
			g.TownNPCAttackStrength(npc, ref damage, ref knockback);
		}
	}

	private delegate void DelegateTownNPCAttackCooldown(NPC npc, ref int cooldown, ref int randExtraCooldown);
	private static HookList HookTownNPCAttackCooldown = AddHook<DelegateTownNPCAttackCooldown>(g => g.TownNPCAttackCooldown);

	public static void TownNPCAttackCooldown(NPC npc, ref int cooldown, ref int randExtraCooldown)
	{
		npc.ModNPC?.TownNPCAttackCooldown(ref cooldown, ref randExtraCooldown);

		foreach (GlobalNPC g in HookTownNPCAttackCooldown.Enumerate(npc.globalNPCs)) {
			g.TownNPCAttackCooldown(npc, ref cooldown, ref randExtraCooldown);
		}
	}

	private delegate void DelegateTownNPCAttackProj(NPC npc, ref int projType, ref int attackDelay);
	private static HookList HookTownNPCAttackProj = AddHook<DelegateTownNPCAttackProj>(g => g.TownNPCAttackProj);

	public static void TownNPCAttackProj(NPC npc, ref int projType, ref int attackDelay)
	{
		npc.ModNPC?.TownNPCAttackProj(ref projType, ref attackDelay);

		foreach (GlobalNPC g in HookTownNPCAttackProj.Enumerate(npc.globalNPCs)) {
			g.TownNPCAttackProj(npc, ref projType, ref attackDelay);
		}
	}

	private delegate void DelegateTownNPCAttackProjSpeed(NPC npc, ref float multiplier, ref float gravityCorrection,
		ref float randomOffset);
	private static HookList HookTownNPCAttackProjSpeed = AddHook<DelegateTownNPCAttackProjSpeed>(g => g.TownNPCAttackProjSpeed);

	public static void TownNPCAttackProjSpeed(NPC npc, ref float multiplier, ref float gravityCorrection,
		ref float randomOffset)
	{
		npc.ModNPC?.TownNPCAttackProjSpeed(ref multiplier, ref gravityCorrection, ref randomOffset);

		foreach (GlobalNPC g in HookTownNPCAttackProjSpeed.Enumerate(npc.globalNPCs)) {
			g.TownNPCAttackProjSpeed(npc, ref multiplier, ref gravityCorrection, ref randomOffset);
		}
	}

	private delegate void DelegateTownNPCAttackShoot(NPC npc, ref bool inBetweenShots);
	private static HookList HookTownNPCAttackShoot = AddHook<DelegateTownNPCAttackShoot>(g => g.TownNPCAttackShoot);

	public static void TownNPCAttackShoot(NPC npc, ref bool inBetweenShots)
	{
		npc.ModNPC?.TownNPCAttackShoot(ref inBetweenShots);

		foreach (GlobalNPC g in HookTownNPCAttackShoot.Enumerate(npc.globalNPCs)) {
			g.TownNPCAttackShoot(npc, ref inBetweenShots);
		}
	}

	private delegate void DelegateTownNPCAttackMagic(NPC npc, ref float auraLightMultiplier);
	private static HookList HookTownNPCAttackMagic = AddHook<DelegateTownNPCAttackMagic>(g => g.TownNPCAttackMagic);

	public static void TownNPCAttackMagic(NPC npc, ref float auraLightMultiplier)
	{
		npc.ModNPC?.TownNPCAttackMagic(ref auraLightMultiplier);

		foreach (GlobalNPC g in HookTownNPCAttackMagic.Enumerate(npc.globalNPCs)) {
			g.TownNPCAttackMagic(npc, ref auraLightMultiplier);
		}
	}

	private delegate void DelegateTownNPCAttackSwing(NPC npc, ref int itemWidth, ref int itemHeight);
	private static HookList HookTownNPCAttackSwing = AddHook<DelegateTownNPCAttackSwing>(g => g.TownNPCAttackSwing);

	public static void TownNPCAttackSwing(NPC npc, ref int itemWidth, ref int itemHeight)
	{
		npc.ModNPC?.TownNPCAttackSwing(ref itemWidth, ref itemHeight);

		foreach (GlobalNPC g in HookTownNPCAttackSwing.Enumerate(npc.globalNPCs)) {
			g.TownNPCAttackSwing(npc, ref itemWidth, ref itemHeight);
		}
	}

	private delegate void DelegateDrawTownAttackGun(NPC npc, ref float scale, ref int item, ref int closeness);
	private static HookList HookDrawTownAttackGun = AddHook<DelegateDrawTownAttackGun>(g => g.DrawTownAttackGun);

	public static void DrawTownAttackGun(NPC npc, ref float scale, ref int item, ref int closeness)
	{
		npc.ModNPC?.DrawTownAttackGun(ref scale, ref item, ref closeness);

		foreach (GlobalNPC g in HookDrawTownAttackGun.Enumerate(npc.globalNPCs)) {
			g.DrawTownAttackGun(npc, ref scale, ref item, ref closeness);
		}
	}

	private delegate void DelegateDrawTownAttackSwing(NPC npc, ref Texture2D item, ref Rectangle itemFrame, ref int itemSize, ref float scale, ref Vector2 offset);
	private static HookList HookDrawTownAttackSwing = AddHook<DelegateDrawTownAttackSwing>(g => g.DrawTownAttackSwing);

	public static void DrawTownAttackSwing(NPC npc, ref Texture2D item, ref Rectangle itemFrame, ref int itemSize, ref float scale, ref Vector2 offset)
	{
		npc.ModNPC?.DrawTownAttackSwing(ref item, ref itemFrame, ref itemSize, ref scale, ref offset);

		foreach (GlobalNPC g in HookDrawTownAttackSwing.Enumerate(npc.globalNPCs)) {
			g.DrawTownAttackSwing(npc, ref item, ref itemFrame, ref itemSize, ref scale, ref offset);
		}
	}

	private delegate bool DelegateModifyCollisionData(NPC npc, Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox);
	private static HookList HookModifyCollisionData = AddHook<DelegateModifyCollisionData>(g => g.ModifyCollisionData);

	public static bool ModifyCollisionData(NPC npc, Rectangle victimHitbox, ref int immunityCooldownSlot, ref float damageMultiplier, ref Rectangle npcHitbox)
	{
		MultipliableFloat damageMult = MultipliableFloat.One;

		bool result = true;
		foreach (GlobalNPC g in HookModifyCollisionData.Enumerate(npc.globalNPCs)) {
			result &= g.ModifyCollisionData(npc, victimHitbox, ref immunityCooldownSlot, ref damageMult, ref npcHitbox);
		}

		if (result && npc.ModNPC != null) {
			result = npc.ModNPC.ModifyCollisionData(victimHitbox, ref immunityCooldownSlot, ref damageMult, ref npcHitbox);
		}

		damageMultiplier *= damageMult.Value;
		return result;
	}

	internal static void VerifyGlobalNPC(GlobalNPC npc)
	{
		var type = npc.GetType();

		bool hasInstanceFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			.Any(f => f.DeclaringType.IsSubclassOf(typeof(GlobalNPC)));

		if (hasInstanceFields) {
			if (!npc.InstancePerEntity) {
				throw new Exception(type + " has instance fields but does not set InstancePerEntity to true. Either use static fields, or per instance globals");
			}
		}
	}

	private delegate bool DelegateNeedSaving(NPC npc);
	private static HookList HookNeedSaving = AddHook<DelegateNeedSaving>(g => g.NeedSaving);

	public static bool SavesAndLoads(NPC npc)
	{
		if (npc.townNPC && npc.type != NPCID.TravellingMerchant)
			return true;

		if (NPCID.Sets.SavesAndLoads[npc.type] || (npc.ModNPC?.NeedSaving() == true))
			return true;

		foreach (GlobalNPC globalNPC in HookNeedSaving.Enumerate(npc.globalNPCs)) {
			if (globalNPC.NeedSaving(npc))
				return true;
		}

		return false;
	}
}
