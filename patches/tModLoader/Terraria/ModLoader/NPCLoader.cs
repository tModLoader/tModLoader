using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.Utilities;
using HookList = Terraria.ModLoader.Core.GlobalHookList<Terraria.ModLoader.GlobalNPC>;
using Terraria.ModLoader.IO;
using Terraria.GameContent.Personalities;
using System.Diagnostics;

namespace Terraria.ModLoader;

//todo: further documentation
/// <summary>
/// This serves as the central class from which NPC-related functions are carried out. It also stores a list of mod NPCs by ID.
/// </summary>
public static class NPCLoader
{
	public static int NPCCount { get; private set; } = NPCID.Count;
	internal static readonly IList<ModNPC> npcs = new List<ModNPC>();
	internal static readonly IDictionary<int, int> bannerToItem = new Dictionary<int, int>();
	internal static readonly IDictionary<int, int> itemToBanner = new Dictionary<int, int>();
	/// <summary>
	/// Allows you to stop an NPC from dropping specific loot by adding item IDs to this list. This list will be cleared whenever NPCLoot ends. Useful for dynamically removing an item in the NPC's loot table. To remove an item drop use the <see cref="ModNPC.PreKill"/> hook to add the item's ID to this list. Editing the drop rules themselves is usually the better and more compatible approach, however.
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

	internal static int Register(ModNPC npc)
	{
		npcs.Add(npc);
		return NPCCount++;
	}

	/// <summary>
	/// Gets the ModNPC template instance corresponding to the specified type (not the clone/new instance which gets added to NPCs as the game is played).
	/// </summary>
	/// <param name="type">The type of the npc</param>
	/// <returns>The ModNPC instance in the <see cref="npcs"/> array, null if not found.</returns>
	public static ModNPC GetNPC(int type)
	{
		return type >= NPCID.Count && type < NPCCount ? npcs[type - NPCID.Count] : null;
	}

	internal static void ResizeArrays(bool unloading)
	{
		if (!unloading)
			GlobalList<GlobalNPC>.FinishLoading(NPCCount);

		// Textures
		Array.Resize(ref TextureAssets.Npc, NPCCount);

		// Sets
		LoaderUtils.ResetStaticMembers(typeof(NPCID));
		Main.ShopHelper.ReinitializePersonalityDatabase();
		NPCHappiness.RegisterVanillaNpcRelationships();

		// Etc
		Array.Resize(ref Main.townNPCCanSpawn, NPCCount);
		Array.Resize(ref Main.slimeRainNPC, NPCCount);
		Array.Resize(ref Main.npcCatchable, NPCCount);
		Array.Resize(ref Main.npcFrameCount, NPCCount);
		Array.Resize(ref Main.SceneMetrics.NPCBannerBuff, NPCCount);
		Array.Resize(ref NPC.killCount, NPCCount);
		Array.Resize(ref NPC.ShimmeredTownNPCs, NPCCount);
		Array.Resize(ref NPC.npcsFoundForCheckActive, NPCCount);
		Array.Resize(ref Lang._npcNameCache, NPCCount);
		Array.Resize(ref EmoteBubble.CountNPCs, NPCCount);
		Array.Resize(ref WorldGen.TownManager._hasRoom, NPCCount);

		foreach (var player in Main.player) {
			Array.Resize(ref player.npcTypeNoAggro, NPCCount);
		}

		for (int k = NPCID.Count; k < NPCCount; k++) {
			Main.npcFrameCount[k] = 1;
			Lang._npcNameCache[k] = LocalizedText.Empty;
		}

		ContentSamples.NpcBestiaryRarityStars.Clear();
	}

	internal static void FinishSetup()
	{
		var temp = new NPC();
		GlobalLoaderUtils<GlobalNPC, NPC>.BuildTypeLookups(type => temp.SetDefaults(type));
		UpdateHookLists();
		GlobalTypeLookups<GlobalNPC>.LogStats();

		foreach (ModNPC npc in npcs) {
			Lang._npcNameCache[npc.Type] = npc.DisplayName;
			RegisterTownNPCMoodLocalizations(npc);

			// Detect various mismatched Banner/BannerItem issues:
			// Detect NPC with BannerItem set but Banner not set.
			// Detect modded Banner values with no associated Banner item.
			// Detect NPC with BannerItem values that don't match the BannerItem associated with the Banner.
			if (npc.BannerItem != 0 && npc.Banner == 0 || npc.Banner >= NPCID.Count && (!bannerToItem.ContainsKey(npc.Banner) || npc.BannerItem != 0 && bannerToItem[npc.Banner] != npc.BannerItem)) {
				Logging.tML.Warn(Language.GetTextValue("tModLoader.LoadWarningBannerOrBannerItemNotSet", npc.Mod.Name, npc.Name));
			}
		}
	}

	private static void UpdateHookLists()
	{
		foreach (var hook in hooks.Union(modHooks)) {
			hook.Update();
		}
	}

	internal static void RegisterTownNPCMoodLocalizations(ModNPC npc)
	{
		if (npc.NPC.townNPC && !NPCID.Sets.IsTownPet[npc.NPC.type] && !NPCID.Sets.NoTownNPCHappiness[npc.NPC.type]) {
			string prefix = npc.GetLocalizationKey("TownNPCMood");
			List<string> keys = new List<string> {
				"Content", "NoHome", "FarFromHome", "LoveSpace", "DislikeCrowded", "HateCrowded"
			};

			if (Main.ShopHelper._database.TryGetProfileByNPCID(npc.NPC.type, out var personalityProfile)) {
				var shopModifiers = personalityProfile.ShopModifiers;

				var biomePreferenceList = (BiomePreferenceListTrait)shopModifiers.SingleOrDefault(t => t is BiomePreferenceListTrait);
				if (biomePreferenceList != null) {
					if(biomePreferenceList.Preferences.Any(x => x.Affection == AffectionLevel.Love))
						keys.Add("LoveBiome");
					if(biomePreferenceList.Preferences.Any(x => x.Affection == AffectionLevel.Like))
						keys.Add("LikeBiome");
					if(biomePreferenceList.Preferences.Any(x => x.Affection == AffectionLevel.Dislike))
						keys.Add("DislikeBiome");
					if(biomePreferenceList.Preferences.Any(x => x.Affection == AffectionLevel.Hate))
						keys.Add("HateBiome");
				}

				if(shopModifiers.Any(t => t is NPCPreferenceTrait { Level: AffectionLevel.Love }))
					keys.Add("LoveNPC");
				if (shopModifiers.Any(t => t is NPCPreferenceTrait { Level: AffectionLevel.Like }))
					keys.Add("LikeNPC");
				if (shopModifiers.Any(t => t is NPCPreferenceTrait { Level: AffectionLevel.Dislike }))
					keys.Add("DislikeNPC");
				if (shopModifiers.Any(t => t is NPCPreferenceTrait { Level: AffectionLevel.Hate }))
					keys.Add("HateNPC");
			}

			keys.Add("LikeNPC_Princess"); // Added here because it makes sense to order this at end.
			keys.Add("Princess_LovesNPC");

			foreach (var key in keys) {
				string oldKey = npc.Mod.GetLocalizationKey($"TownNPCMood.{npc.Name}.{key}");
				if (key == "Princess_LovesNPC")
					oldKey = $"TownNPCMood_Princess.LoveNPC_{npc.FullName}";
				string fullKey = $"{prefix}.{key}";
				string defaultValueKey = "TownNPCMood." + key;
				// Register current language translation rather than vanilla text substitution so modder can see the {BiomeName} and {NPCName} usages. Might result in non-English values, but modder is expected to change the translation value anyway.
				Language.GetOrRegister(fullKey, () => Language.Exists(oldKey) ? $"{{${oldKey}}}" : Language.GetTextValue(defaultValueKey));
			}
		}
	}

	internal static void Unload()
	{
		NPCCount = NPCID.Count;
		npcs.Clear();
		GlobalList<GlobalNPC>.Reset();
		bannerToItem.Clear();
		modHooks.Clear();
		UpdateHookLists();

		if (!Main.dedServ) // dedicated servers implode with texture swaps and I've never understood why, so here's a fix for that     -thomas
			TownNPCProfiles.Instance.ResetTexturesAccordingToVanillaProfiles();
	}

	internal static bool IsModNPC(NPC npc)
	{
		return npc.type >= NPCID.Count;
	}

	/// <summary>
	/// Returns the type of a ModNPC corresponding to the provided banner item type. Note that this is equivalent to the banner type as well, since ModNPC type and banner type are the same. Returns -1 if not found.
	/// </summary>
	public static int BannerItemToNPC(int itemType) => itemToBanner.TryGetValue(itemType, out var id) ? id : -1;

	internal static void SetDefaults(NPC npc, bool createModNPC = true)
	{
		if (IsModNPC(npc)) {
			if (createModNPC) {
				npc.ModNPC = GetNPC(npc.type).NewInstance(npc);
			}
			else {
				//the default NPCs created and bound to ModNPCs are initialized before ResizeArrays. They come here during SetupContent.
				Array.Resize(ref npc.buffImmune, BuffLoader.BuffCount);
			}
		}

		GlobalLoaderUtils<GlobalNPC, NPC>.SetDefaults(npc, ref npc._globals, static n => n.ModNPC?.SetDefaults());
	}

	private static HookList HookSetVariantDefaults = AddHook<Action<NPC>>(g => g.SetDefaultsFromNetId);
	internal static void SetDefaultsFromNetId(NPC npc)
	{
		foreach (var g in HookSetVariantDefaults.Enumerate(npc)) {
			g.SetDefaultsFromNetId(npc);
		}
	}

	private static HookList HookOnSpawn = AddHook<Action<NPC, IEntitySource>>(g => g.OnSpawn);

	internal static void OnSpawn(NPC npc, IEntitySource source)
	{
		npc.ModNPC?.OnSpawn(source);

		foreach (var g in HookOnSpawn.Enumerate(npc)) {
			g.OnSpawn(npc, source);
		}
	}

	private static HookList HookApplyDifficultyAndPlayerScaling = AddHook<Action<NPC, int, float, float>>(g => g.ApplyDifficultyAndPlayerScaling);

	public static void ApplyDifficultyAndPlayerScaling(NPC npc, int numPlayers, float balance, float bossAdjustment)
	{
		npc.ModNPC?.ApplyDifficultyAndPlayerScaling(numPlayers, balance, bossAdjustment);

		foreach (var g in HookApplyDifficultyAndPlayerScaling.Enumerate(npc)) {
			g.ApplyDifficultyAndPlayerScaling(npc, numPlayers, balance, bossAdjustment);
		}
	}

	private delegate void DelegateSetBestiary(NPC npc, BestiaryDatabase database, BestiaryEntry bestiaryEntry);
	private static HookList HookSetBestiary = AddHook<DelegateSetBestiary>(g => g.SetBestiary);
	public static void SetBestiary(NPC npc, BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		if (IsModNPC(npc)) {
			bestiaryEntry.Info.Add(npc.ModNPC.Mod.ModSourceBestiaryInfoElement);
			foreach (var type in npc.ModNPC.SpawnModBiomes) {
				bestiaryEntry.Info.Add(LoaderManager.Get<BiomeLoader>().Get(type).ModBiomeBestiaryInfoElement);
			}
		}

		npc.ModNPC?.SetBestiary(database, bestiaryEntry);

		foreach (var g in HookSetBestiary.Enumerate(npc)) {
			g.SetBestiary(npc, database, bestiaryEntry);
		}
	}

	private delegate ITownNPCProfile DelegateModifyTownNPCProfile(NPC npc);
	private static HookList HookModifyTownNPCProfile = AddHook<DelegateModifyTownNPCProfile>(g => g.ModifyTownNPCProfile);
	public static void ModifyTownNPCProfile(NPC npc, ref ITownNPCProfile profile)
	{
		profile = npc.ModNPC?.TownNPCProfile() ?? profile;

		foreach (var g in HookModifyTownNPCProfile.Enumerate(npc)) {
			profile = g.ModifyTownNPCProfile(npc) ?? profile;
		}
	}

	private static HookList HookResetEffects = AddHook<Action<NPC>>(g => g.ResetEffects);

	public static void ResetEffects(NPC npc)
	{
		npc.ModNPC?.ResetEffects();

		foreach (var g in HookResetEffects.Enumerate(npc)) {
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
		foreach (var g in HookPreAI.Enumerate(npc)) {
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

		foreach (var g in HookAI.Enumerate(npc)) {
			g.AI(npc);
		}
	}

	private static HookList HookPostAI = AddHook<Action<NPC>>(g => g.PostAI);

	public static void PostAI(NPC npc)
	{
		npc.ModNPC?.PostAI();

		foreach (var g in HookPostAI.Enumerate(npc)) {
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

	private static HookList HookSendExtraAI = AddHook<Action<NPC, BitWriter, BinaryWriter>>(g => g.SendExtraAI);

	public static byte[] WriteExtraAI(NPC npc)
	{
		// Data is ordered as follows: GlobalProjectile BitWriter, ModProjectile Bytes, GlobalProjectile Bytes
		using var stream = new MemoryStream();
		using var modWriter = new BinaryWriter(stream);

		using var bufferedStream = new MemoryStream();
		using var binaryWriter = new BinaryWriter(bufferedStream);

		BitWriter bitWriter = new BitWriter();

		npc.ModNPC?.SendExtraAI(binaryWriter);

		foreach (var g in HookSendExtraAI.Enumerate(npc)) {
			g.SendExtraAI(npc, bitWriter, binaryWriter);
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

	private static HookList HookReceiveExtraAI = AddHook<Action<NPC, BitReader, BinaryReader>>(g => g.ReceiveExtraAI);

	public static void ReceiveExtraAI(NPC npc, byte[] extraAI)
	{
		using var stream = extraAI.ToMemoryStream();
		using var modReader = new BinaryReader(stream);

		GlobalNPC lastGlobalNPC = null;
		try {
			BitReader bitReader = new BitReader(modReader);

			var bitReaderEnd = stream.Position;
			npc.ModNPC?.ReceiveExtraAI(modReader);

			foreach (var g in HookReceiveExtraAI.Enumerate(npc)) {
				lastGlobalNPC = g;
				g.ReceiveExtraAI(npc, bitReader, modReader);
			}

			if (bitReader.BitsRead < bitReader.MaxBits) {
				throw new IOException($"Read underflow {bitReader.MaxBits - bitReader.BitsRead} of {bitReader.MaxBits} compressed bits in ReceiveExtraAI, more info below");
			}

			if (stream.Position < stream.Length) {
				throw new IOException($"Read underflow {stream.Length - stream.Position} of {stream.Length - bitReaderEnd} bytes in ReceiveExtraAI, more info below");
			}
		}
		catch (Exception) {
			string message = $"Error in ReceiveExtraAI for NPC {npc.ModNPC?.FullName ?? npc.TypeName}";
			if (lastGlobalNPC != null) {
				message += ", may be caused by one of these GlobalNPCs:";
				foreach (var g in HookReceiveExtraAI.Enumerate(npc)) {
					message += $"\n\t{g.FullName}";
					if (lastGlobalNPC == g)
						break;
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

		foreach (var g in HookFindFrame.Enumerate(npc)) {
			g.FindFrame(npc, frameHeight);
		}
	}

	private static HookList HookHitEffect = AddHook<Action<NPC, NPC.HitInfo>>(g => g.HitEffect);

	public static void HitEffect(NPC npc, in NPC.HitInfo hit)
	{
		npc.ModNPC?.HitEffect(hit);

		foreach (var g in HookHitEffect.Enumerate(npc)) {
			g.HitEffect(npc, hit);
		}
	}

	private delegate void DelegateUpdateLifeRegen(NPC npc, ref int damage);
	private static HookList HookUpdateLifeRegen = AddHook<DelegateUpdateLifeRegen>(g => g.UpdateLifeRegen);

	public static void UpdateLifeRegen(NPC npc, ref int damage)
	{
		npc.ModNPC?.UpdateLifeRegen(ref damage);

		foreach (var g in HookUpdateLifeRegen.Enumerate(npc)) {
			g.UpdateLifeRegen(npc, ref damage);
		}
	}

	private static HookList HookCheckActive = AddHook<Func<NPC, bool>>(g => g.CheckActive);

	public static bool CheckActive(NPC npc)
	{
		if (npc.ModNPC != null && !npc.ModNPC.CheckActive()) {
			return false;
		}
		foreach (var g in HookCheckActive.Enumerate(npc)) {
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

		foreach (var g in HookCheckDead.Enumerate(npc)) {
			result &= g.CheckDead(npc);
		}

		return result;
	}

	private static HookList HookSpecialOnKill = AddHook<Func<NPC, bool>>(g => g.SpecialOnKill);

	public static bool SpecialOnKill(NPC npc)
	{
		foreach (var g in HookSpecialOnKill.Enumerate(npc)) {
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
		foreach (var g in HookPreKill.Enumerate(npc)) {
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

		foreach (var g in HookOnKill.Enumerate(npc)) {
			g.OnKill(npc);
		}

		blockLoot.Clear();
	}

	private static HookList HookModifyNPCLoot = AddHook<Action<NPC, NPCLoot>>(g => g.ModifyNPCLoot);
	public static void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
	{
		npc.ModNPC?.ModifyNPCLoot(npcLoot);

		foreach (var g in HookModifyNPCLoot.Enumerate(npc)) {
			g.ModifyNPCLoot(npc, npcLoot);
		}
	}

	private static HookList HookModifyGlobalLoot = AddHook<Action<GlobalLoot>>(g => g.ModifyGlobalLoot);
	public static void ModifyGlobalLoot(GlobalLoot globalLoot)
	{
		foreach (var g in HookModifyGlobalLoot.Enumerate()) {
			g.ModifyGlobalLoot(globalLoot);
		}
	}

	public static void BossLoot(NPC npc, ref string name, ref int potionType)
	{
		npc.ModNPC?.BossLoot(ref name, ref potionType);
		npc.ModNPC?.BossLoot(ref potionType);
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

		foreach (var g in HookCanFallThroughPlatforms.Enumerate(npc)) {
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
		foreach (var g in HookCanBeCaughtBy.Enumerate(npc)) {
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

		foreach (var g in HookOnCaughtBy.Enumerate(npc)) {
			g.OnCaughtBy(npc, player, item, failed);
		}
	}

	private static HookList HookPickEmote = AddHook<Func<NPC, Player, List<int>, WorldUIAnchor, int?>>(g => g.PickEmote);

	public static int? PickEmote(NPC npc, Player closestPlayer, List<int> emoteList, WorldUIAnchor anchor) {
		int? result = null;

		if (npc.ModNPC != null) {
			result = npc.ModNPC.PickEmote(closestPlayer, emoteList, anchor);
		}

		foreach (GlobalNPC globalNPC in HookPickEmote.Enumerate(npc)) {
			int? emote = globalNPC.PickEmote(npc, closestPlayer, emoteList, anchor);
			if (emote != null)
				result = emote;
		}

		return result;
	}

	private delegate bool DelegateCanHitPlayer(NPC npc, Player target, ref int cooldownSlot);
	private static HookList HookCanHitPlayer = AddHook<DelegateCanHitPlayer>(g => g.CanHitPlayer);

	public static bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
	{
		foreach (var g in HookCanHitPlayer.Enumerate(npc)) {
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

		foreach (var g in HookModifyHitPlayer.Enumerate(npc)) {
			g.ModifyHitPlayer(npc, target, ref modifiers);
		}
	}

	private static HookList HookOnHitPlayer = AddHook<Action<NPC, Player, Player.HurtInfo>>(g => g.OnHitPlayer);

	public static void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
	{
		npc.ModNPC?.OnHitPlayer(target, hurtInfo);

		foreach (var g in HookOnHitPlayer.Enumerate(npc)) {
			g.OnHitPlayer(npc, target, hurtInfo);
		}
	}

	private static HookList HookCanHitNPC = AddHook<Func<NPC, NPC, bool>>(g => g.CanHitNPC);
	private static HookList HookCanBeHitByNPC = AddHook<Func<NPC, NPC, bool>>(g => g.CanBeHitByNPC);

	public static bool CanHitNPC(NPC npc, NPC target)
	{
		foreach (var g in HookCanHitNPC.Enumerate(npc)) {
			if (!g.CanHitNPC(npc, target))
				return false;
		}

		foreach (var g in HookCanBeHitByNPC.Enumerate(target)) {
			if (!g.CanBeHitByNPC(target, npc))
				return false;
		}

		if (npc.ModNPC?.CanHitNPC(target) is false)
			return false;

		return target.ModNPC?.CanBeHitByNPC(npc) ?? true;
	}

	private delegate void DelegateModifyHitNPC(NPC npc, NPC target, ref NPC.HitModifiers modifiers);
	private static HookList HookModifyHitNPC = AddHook<DelegateModifyHitNPC>(g => g.ModifyHitNPC);

	public static void ModifyHitNPC(NPC npc, NPC target, ref NPC.HitModifiers modifiers)
	{
		npc.ModNPC?.ModifyHitNPC(target, ref modifiers);

		foreach (var g in HookModifyHitNPC.Enumerate(npc)) {
			g.ModifyHitNPC(npc, target, ref modifiers);
		}
	}

	private static HookList HookOnHitNPC = AddHook<Action<NPC, NPC, NPC.HitInfo>>(g => g.OnHitNPC);

	public static void OnHitNPC(NPC npc, NPC target, in NPC.HitInfo hit)
	{
		npc.ModNPC?.OnHitNPC(target, hit);

		foreach (var g in HookOnHitNPC.Enumerate(npc)) {
			g.OnHitNPC(npc, target, hit);
		}
	}

	private static HookList HookCanBeHitByItem = AddHook<Func<NPC, Player, Item, bool?>>(g => g.CanBeHitByItem);

	public static bool? CanBeHitByItem(NPC npc, Player player, Item item)
	{
		bool? flag = null;

		foreach (var g in HookCanBeHitByItem.Enumerate(npc)) {
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
		foreach (var g in HookCanCollideWithPlayerMeleeAttack.Enumerate(npc)) {
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

		foreach (var g in HookModifyHitByItem.Enumerate(npc)) {
			g.ModifyHitByItem(npc, player, item, ref modifiers);
		}
	}

	private static HookList HookOnHitByItem = AddHook<Action<NPC, Player, Item, NPC.HitInfo, int>>(g => g.OnHitByItem);

	public static void OnHitByItem(NPC npc, Player player, Item item, in NPC.HitInfo hit, int damageDone)
	{
		npc.ModNPC?.OnHitByItem(player, item, hit, damageDone);

		foreach (var g in HookOnHitByItem.Enumerate(npc)) {
			g.OnHitByItem(npc, player, item, hit, damageDone);
		}
	}

	private static HookList HookCanBeHitByProjectile = AddHook<Func<NPC, Projectile, bool?>>(g => g.CanBeHitByProjectile);

	public static bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
	{
		bool? flag = null;
		foreach (var g in HookCanBeHitByProjectile.Enumerate(npc)) {
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

		foreach (var g in HookModifyHitByProjectile.Enumerate(npc)) {
			g.ModifyHitByProjectile(npc, projectile, ref modifiers);
		}
	}

	private static HookList HookOnHitByProjectile = AddHook<Action<NPC, Projectile, NPC.HitInfo, int>>(g => g.OnHitByProjectile);

	public static void OnHitByProjectile(NPC npc, Projectile projectile, in NPC.HitInfo hit, int damageDone)
	{
		npc.ModNPC?.OnHitByProjectile(projectile, hit, damageDone);

		foreach (var g in HookOnHitByProjectile.Enumerate(npc)) {
			g.OnHitByProjectile(npc, projectile, hit, damageDone);
		}
	}

	private delegate void DelegateAddModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers);
	private static HookList HookAddModifyIncomingHit = AddHook<DelegateAddModifyIncomingHit>(g => g.ModifyIncomingHit);

	public static void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
	{
		npc.ModNPC?.ModifyIncomingHit(ref modifiers);
		foreach (var g in HookAddModifyIncomingHit.Enumerate(npc)) {
			g.ModifyIncomingHit(npc, ref modifiers);
		}
	}

	private delegate void DelegateBossHeadSlot(NPC npc, ref int index);
	private static HookList HookBossHeadSlot = AddHook<DelegateBossHeadSlot>(g => g.BossHeadSlot);

	public static void BossHeadSlot(NPC npc, ref int index)
	{
		npc.ModNPC?.BossHeadSlot(ref index);

		foreach (var g in HookBossHeadSlot.Enumerate(npc)) {
			g.BossHeadSlot(npc, ref index);
		}
	}

	private delegate void DelegateBossHeadRotation(NPC npc, ref float rotation);
	private static HookList HookBossHeadRotation = AddHook<DelegateBossHeadRotation>(g => g.BossHeadRotation);

	public static void BossHeadRotation(NPC npc, ref float rotation)
	{
		npc.ModNPC?.BossHeadRotation(ref rotation);

		foreach (var g in HookBossHeadRotation.Enumerate(npc)) {
			g.BossHeadRotation(npc, ref rotation);
		}
	}

	private delegate void DelegateBossHeadSpriteEffects(NPC npc, ref SpriteEffects spriteEffects);
	private static HookList HookBossHeadSpriteEffects = AddHook<DelegateBossHeadSpriteEffects>(g => g.BossHeadSpriteEffects);

	public static void BossHeadSpriteEffects(NPC npc, ref SpriteEffects spriteEffects)
	{
		npc.ModNPC?.BossHeadSpriteEffects(ref spriteEffects);

		foreach (var g in HookBossHeadSpriteEffects.Enumerate(npc)) {
			g.BossHeadSpriteEffects(npc, ref spriteEffects);
		}
	}

	private static HookList HookGetAlpha = AddHook<Func<NPC, Color, Color?>>(g => g.GetAlpha);

	public static Color? GetAlpha(NPC npc, Color lightColor)
	{
		foreach (var g in HookGetAlpha.Enumerate(npc)) {
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

		foreach (var g in HookDrawEffects.Enumerate(npc)) {
			g.DrawEffects(npc, ref drawColor);
		}
	}

	private delegate bool DelegatePreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor);
	private static HookList HookPreDraw = AddHook<DelegatePreDraw>(g => g.PreDraw);

	public static bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		bool result = true;
		foreach (var g in HookPreDraw.Enumerate(npc)) {
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

		foreach (var g in HookPostDraw.Enumerate(npc)) {
			g.PostDraw(npc, spriteBatch, screenPos, drawColor);
		}
	}

	private static HookList HookDrawBehind = AddHook<Action<NPC, int>>(g => g.DrawBehind);

	internal static void DrawBehind(NPC npc, int index)
	{
		npc.ModNPC?.DrawBehind(index);

		foreach (var g in HookDrawBehind.Enumerate(npc)) {
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
		foreach (var g in HookDrawHealthBar.Enumerate(npc)) {
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
		foreach (var g in HookEditSpawnRate.Enumerate()) {
			g.EditSpawnRate(player, ref spawnRate, ref maxSpawns);
		}
	}

	private delegate void DelegateEditSpawnRange(Player player, ref int spawnRangeX, ref int spawnRangeY,
		ref int safeRangeX, ref int safeRangeY);
	private static HookList HookEditSpawnRange = AddHook<DelegateEditSpawnRange>(g => g.EditSpawnRange);

	public static void EditSpawnRange(Player player, ref int spawnRangeX, ref int spawnRangeY,
		ref int safeRangeX, ref int safeRangeY)
	{
		foreach (var g in HookEditSpawnRange.Enumerate()) {
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
		foreach (var g in HookEditSpawnPool.Enumerate()) {
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

		foreach (var g in HookSpawnNPC.Enumerate(Main.npc[npc])) {
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

		foreach (var g in HookModifyNPCHappiness.Enumerate()) {
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

		foreach (var g in HookModifyTypeName.Enumerate(npc)) {
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

		foreach (var g in HookModifyHoverBoundingBox.Enumerate(npc)) {
			g.ModifyHoverBoundingBox(npc, ref boundingBox);
		}
	}

	private delegate bool DelegatePreHoverInteract(NPC npc, bool mouseIntersects);
	private static HookList HookPreHoverInteract = AddHook<DelegatePreHoverInteract>(g => g.PreHoverInteract);
	public static bool PreHoverInteract(NPC npc, bool mouseIntersects)
	{
		foreach (var g in HookPreHoverInteract.Enumerate(npc)) {
			if (!g.PreHoverInteract(npc, mouseIntersects))
				return false;
		}

		return npc.ModNPC?.PreHoverInteract(mouseIntersects) ?? true;
	}

	private static HookList HookModifyNPCNameList = AddHook<Action<NPC, List<string>>>(g => g.ModifyNPCNameList);
	public static List<string> ModifyNPCNameList(NPC npc, List<string> nameList)
	{
		if (npc.ModNPC != null)
			nameList = npc.ModNPC.SetNPCNameList();

		foreach (var g in HookModifyNPCNameList.Enumerate(npc)) {
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

		foreach (var g in HookCanChat.Enumerate(npc)) {
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
		foreach (var g in HookGetChat.Enumerate(npc)) {
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
		foreach (var g in HookPreChatButtonClicked.Enumerate(npc)) {
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

		foreach (var g in HookOnChatButtonClicked.Enumerate(npc)) {
			g.OnChatButtonClicked(npc, firstButton);
		}
	}

	public static void AddShops(int type)
	{
		GetNPC(type)?.AddShops();
	}

	private delegate void DelegateModifyShop(NPCShop shop);
	private static HookList HookModifyShop = AddHook<DelegateModifyShop>(g => g.ModifyShop);

	public static void ModifyShop(NPCShop shop)
	{
		foreach (var g in HookModifyShop.Enumerate(shop.NpcType)) {
			g.ModifyShop(shop);
		}
	}

	private delegate void DelegateModifyActiveShop(NPC npc, string shopName, Item[] items);
	private static HookList HookModifyActiveShop = AddHook<DelegateModifyActiveShop>(g => g.ModifyActiveShop);

	public static void ModifyActiveShop(NPC npc, string shopName, Item[] shopContents)
	{
		GetNPC(npc.type)?.ModifyActiveShop(shopName, shopContents);
		foreach (var g in HookModifyActiveShop.Enumerate(npc)) {
			g.ModifyActiveShop(npc, shopName, shopContents);
		}
	}

	private delegate void DelegateSetupTravelShop(int[] shop, ref int nextSlot);
	private static HookList HookSetupTravelShop = AddHook<DelegateSetupTravelShop>(g => g.SetupTravelShop);

	public static void SetupTravelShop(int[] shop, ref int nextSlot)
	{
		foreach (var g in HookSetupTravelShop.Enumerate()) {
			g.SetupTravelShop(shop, ref nextSlot);
		}
	}

	private static HookList HookCanGoToStatue = AddHook<Func<NPC, bool, bool?>>(g => g.CanGoToStatue);

	public static bool? CanGoToStatue(NPC npc, bool toKingStatue)
	{
		bool? ret = npc.ModNPC?.CanGoToStatue(toKingStatue);

		foreach (var g in HookCanGoToStatue.Enumerate(npc)) {
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

		foreach (var g in HookOnGoToStatue.Enumerate(npc)) {
			g.OnGoToStatue(npc, toKingStatue);
		}
	}

	private delegate void DelegateBuffTownNPC(ref float damageMult, ref int defense);
	private static HookList HookBuffTownNPC = AddHook<DelegateBuffTownNPC>(g => g.BuffTownNPC);

	public static void BuffTownNPC(ref float damageMult, ref int defense)
	{
		foreach (var g in HookBuffTownNPC.Enumerate()) {
			g.BuffTownNPC(ref damageMult, ref defense);
		}
	}

	private delegate bool DelegateModifyDeathMessage(NPC npc, ref NetworkText custom, ref Color color);
	private static HookList HookModifyDeathMessage = AddHook<DelegateModifyDeathMessage>(g => g.ModifyDeathMessage);

	public static bool ModifyDeathMessage(NPC npc, ref NetworkText customText, ref Color color)
	{
		foreach (var g in HookModifyDeathMessage.Enumerate()) {
			if (!g.ModifyDeathMessage(npc, ref customText, ref color))
				return true;
		}

		return !npc.ModNPC?.ModifyDeathMessage(ref customText, ref color) ?? false;
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

		foreach (var g in HookTownNPCAttackStrength.Enumerate(npc)) {
			g.TownNPCAttackStrength(npc, ref damage, ref knockback);
		}
	}

	private delegate void DelegateTownNPCAttackCooldown(NPC npc, ref int cooldown, ref int randExtraCooldown);
	private static HookList HookTownNPCAttackCooldown = AddHook<DelegateTownNPCAttackCooldown>(g => g.TownNPCAttackCooldown);

	public static void TownNPCAttackCooldown(NPC npc, ref int cooldown, ref int randExtraCooldown)
	{
		npc.ModNPC?.TownNPCAttackCooldown(ref cooldown, ref randExtraCooldown);

		foreach (var g in HookTownNPCAttackCooldown.Enumerate(npc)) {
			g.TownNPCAttackCooldown(npc, ref cooldown, ref randExtraCooldown);
		}
	}

	private delegate void DelegateTownNPCAttackProj(NPC npc, ref int projType, ref int attackDelay);
	private static HookList HookTownNPCAttackProj = AddHook<DelegateTownNPCAttackProj>(g => g.TownNPCAttackProj);

	public static void TownNPCAttackProj(NPC npc, ref int projType, ref int attackDelay)
	{
		npc.ModNPC?.TownNPCAttackProj(ref projType, ref attackDelay);

		foreach (var g in HookTownNPCAttackProj.Enumerate(npc)) {
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

		foreach (var g in HookTownNPCAttackProjSpeed.Enumerate(npc)) {
			g.TownNPCAttackProjSpeed(npc, ref multiplier, ref gravityCorrection, ref randomOffset);
		}
	}

	private delegate void DelegateTownNPCAttackShoot(NPC npc, ref bool inBetweenShots);
	private static HookList HookTownNPCAttackShoot = AddHook<DelegateTownNPCAttackShoot>(g => g.TownNPCAttackShoot);

	public static void TownNPCAttackShoot(NPC npc, ref bool inBetweenShots)
	{
		npc.ModNPC?.TownNPCAttackShoot(ref inBetweenShots);

		foreach (var g in HookTownNPCAttackShoot.Enumerate(npc)) {
			g.TownNPCAttackShoot(npc, ref inBetweenShots);
		}
	}

	private delegate void DelegateTownNPCAttackMagic(NPC npc, ref float auraLightMultiplier);
	private static HookList HookTownNPCAttackMagic = AddHook<DelegateTownNPCAttackMagic>(g => g.TownNPCAttackMagic);

	public static void TownNPCAttackMagic(NPC npc, ref float auraLightMultiplier)
	{
		npc.ModNPC?.TownNPCAttackMagic(ref auraLightMultiplier);

		foreach (var g in HookTownNPCAttackMagic.Enumerate(npc)) {
			g.TownNPCAttackMagic(npc, ref auraLightMultiplier);
		}
	}

	private delegate void DelegateTownNPCAttackSwing(NPC npc, ref int itemWidth, ref int itemHeight);
	private static HookList HookTownNPCAttackSwing = AddHook<DelegateTownNPCAttackSwing>(g => g.TownNPCAttackSwing);

	public static void TownNPCAttackSwing(NPC npc, ref int itemWidth, ref int itemHeight)
	{
		npc.ModNPC?.TownNPCAttackSwing(ref itemWidth, ref itemHeight);

		foreach (var g in HookTownNPCAttackSwing.Enumerate(npc)) {
			g.TownNPCAttackSwing(npc, ref itemWidth, ref itemHeight);
		}
	}

	private delegate void DelegateDrawTownAttackGun(NPC npc, ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset);
	private static HookList HookDrawTownAttackGun = AddHook<DelegateDrawTownAttackGun>(g => g.DrawTownAttackGun);

	public static void DrawTownAttackGun(NPC npc, ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset)
	{
		npc.ModNPC?.DrawTownAttackGun(ref item, ref itemFrame, ref scale, ref horizontalHoldoutOffset);

		foreach (var g in HookDrawTownAttackGun.Enumerate(npc)) {
			g.DrawTownAttackGun(npc, ref item, ref itemFrame, ref scale, ref horizontalHoldoutOffset);
		}
	}

	private delegate void DelegateDrawTownAttackSwing(NPC npc, ref Texture2D item, ref Rectangle itemFrame, ref int itemSize, ref float scale, ref Vector2 offset);
	private static HookList HookDrawTownAttackSwing = AddHook<DelegateDrawTownAttackSwing>(g => g.DrawTownAttackSwing);

	public static void DrawTownAttackSwing(NPC npc, ref Texture2D item, ref Rectangle itemFrame, ref int itemSize, ref float scale, ref Vector2 offset)
	{
		npc.ModNPC?.DrawTownAttackSwing(ref item, ref itemFrame, ref itemSize, ref scale, ref offset);

		foreach (var g in HookDrawTownAttackSwing.Enumerate(npc)) {
			g.DrawTownAttackSwing(npc, ref item, ref itemFrame, ref itemSize, ref scale, ref offset);
		}
	}

	private delegate bool DelegateModifyCollisionData(NPC npc, Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox);
	private static HookList HookModifyCollisionData = AddHook<DelegateModifyCollisionData>(g => g.ModifyCollisionData);

	public static bool ModifyCollisionData(NPC npc, Rectangle victimHitbox, ref int immunityCooldownSlot, ref float damageMultiplier, ref Rectangle npcHitbox)
	{
		MultipliableFloat damageMult = MultipliableFloat.One;

		bool result = true;
		foreach (var g in HookModifyCollisionData.Enumerate(npc)) {
			result &= g.ModifyCollisionData(npc, victimHitbox, ref immunityCooldownSlot, ref damageMult, ref npcHitbox);
		}

		if (result && npc.ModNPC != null) {
			result = npc.ModNPC.ModifyCollisionData(victimHitbox, ref immunityCooldownSlot, ref damageMult, ref npcHitbox);
		}

		damageMultiplier *= damageMult.Value;
		return result;
	}

	private delegate bool DelegateNeedSaving(NPC npc);
	private static HookList HookNeedSaving = AddHook<DelegateNeedSaving>(g => g.NeedSaving);

	public static bool SavesAndLoads(NPC npc)
	{
		if (npc.townNPC && npc.type != NPCID.TravellingMerchant)
			return true;

		if (NPCID.Sets.SavesAndLoads[npc.type] || (npc.ModNPC?.NeedSaving() == true))
			return true;

		foreach (var g in HookNeedSaving.Enumerate(npc)) {
			if (g.NeedSaving(npc))
				return true;
		}

		return false;
	}

	internal static HookList HookSaveData = AddHook<Action<NPC, TagCompound>>(g => g.SaveData);

	private delegate void DelegateChatBubblePosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects);
	private static HookList HookChatBubblePosition = AddHook<DelegateChatBubblePosition>(g => g.ChatBubblePosition);

	public static void ChatBubblePosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects)
	{
		npc.ModNPC?.ChatBubblePosition(ref position, ref spriteEffects);

		foreach (var g in HookChatBubblePosition.Enumerate(npc)) {
			g.ChatBubblePosition(npc, ref position, ref spriteEffects);
		}
	}

	private delegate void DelegatePartyHatPosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects);
	private static HookList HookPartyHatPosition = AddHook<DelegatePartyHatPosition>(g => g.PartyHatPosition);

	public static void PartyHatPosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects)
	{
		npc.ModNPC?.PartyHatPosition(ref position, ref spriteEffects);

		foreach (var g in HookPartyHatPosition.Enumerate(npc)) {
			g.PartyHatPosition(npc, ref position, ref spriteEffects);
		}
	}

	private delegate void DelegateEmoteBubblePosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects);
	private static HookList HookEmoteBubblePosition = AddHook<DelegateEmoteBubblePosition>(g => g.EmoteBubblePosition);

	public static void EmoteBubblePosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects)
	{
		npc.ModNPC?.EmoteBubblePosition(ref position, ref spriteEffects);

		foreach (var g in HookEmoteBubblePosition.Enumerate(npc)) {
			g.EmoteBubblePosition(npc, ref position, ref spriteEffects);
		}
	}
}
