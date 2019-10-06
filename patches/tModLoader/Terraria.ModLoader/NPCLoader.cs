using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.Localization;

namespace Terraria.ModLoader
{
	//todo: further documentation
	/// <summary>
	/// This serves as the central class from which NPC-related functions are carried out. It also stores a list of mod NPCs by ID.
	/// </summary>
	public static class NPCLoader
	{
		internal static bool loaded = false;
		private static int nextNPC = NPCID.Count;
		internal static readonly IList<ModNPC> npcs = new List<ModNPC>();
		internal static readonly IList<GlobalNPC> globalNPCs = new List<GlobalNPC>();
		internal static GlobalNPC[] InstancedGlobals = new GlobalNPC[0];
		internal static readonly IDictionary<string, int> globalIndexes = new Dictionary<string, int>();
		internal static readonly IDictionary<Type, int> globalIndexesByType = new Dictionary<Type, int>();
		internal static readonly IDictionary<int, int> bannerToItem = new Dictionary<int, int>();
		private static int vanillaSkeletonCount = NPCID.Sets.Skeletons.Count;
		private static readonly int[] shopToNPC = new int[Main.MaxShopIDs - 1];
		/// <summary>
		/// Allows you to stop an NPC from dropping loot by adding item IDs to this list. This list will be cleared whenever NPCLoot ends. Useful for either removing an item or change the drop rate of an item in the NPC's loot table. To change the drop rate of an item, use the PreNPCLoot hook, spawn the item yourself, then add the item's ID to this list.
		/// </summary>
		public static readonly IList<int> blockLoot = new List<int>();

		private class HookList
		{
			public GlobalNPC[] arr = new GlobalNPC[0];
			public readonly MethodInfo method;

			public HookList(MethodInfo method) {
				this.method = method;
			}
		}

		private static List<HookList> hooks = new List<HookList>();

		private static HookList AddHook<F>(Expression<Func<GlobalNPC, F>> func) {
			var hook = new HookList(ModLoader.Method(func));
			hooks.Add(hook);
			return hook;
		}

		static NPCLoader() {
			shopToNPC[1] = NPCID.Merchant;
			shopToNPC[2] = NPCID.ArmsDealer;
			shopToNPC[3] = NPCID.Dryad;
			shopToNPC[4] = NPCID.Demolitionist;
			shopToNPC[5] = NPCID.Clothier;
			shopToNPC[6] = NPCID.GoblinTinkerer;
			shopToNPC[7] = NPCID.Wizard;
			shopToNPC[8] = NPCID.Mechanic;
			shopToNPC[9] = NPCID.SantaClaus;
			shopToNPC[10] = NPCID.Truffle;
			shopToNPC[11] = NPCID.Steampunker;
			shopToNPC[12] = NPCID.DyeTrader;
			shopToNPC[13] = NPCID.PartyGirl;
			shopToNPC[14] = NPCID.Cyborg;
			shopToNPC[15] = NPCID.Painter;
			shopToNPC[16] = NPCID.WitchDoctor;
			shopToNPC[17] = NPCID.Pirate;
			shopToNPC[18] = NPCID.Stylist;
			shopToNPC[19] = NPCID.TravellingMerchant;
			shopToNPC[20] = NPCID.SkeletonMerchant;
			shopToNPC[21] = NPCID.DD2Bartender;
		}

		internal static int ReserveNPCID() {
			if (ModNet.AllowVanillaClients) throw new Exception("Adding npcs breaks vanilla client compatibility");

			int reserveID = nextNPC;
			nextNPC++;
			return reserveID;
		}

		public static int NPCCount => nextNPC;

		/// <summary>
		/// Gets the ModNPC instance corresponding to the specified type.
		/// </summary>
		/// <param name="type">The type of the npc</param>
		/// <returns>The ModNPC instance in the npcs array, null if not found.</returns>
		public static ModNPC GetNPC(int type) {
			return type >= NPCID.Count && type < NPCCount ? npcs[type - NPCID.Count] : null;
		}
		//change initial size of Terraria.Player.npcTypeNoAggro and NPCBannerBuff to NPCLoader.NPCCount()
		//in Terraria.Main.MouseText replace 251 with NPCLoader.NPCCount()
		//in Terraria.Main.DrawNPCs and Terraria.NPC.NPCLoot remove type too high check
		//replace a lot of 540 immediates
		//in Terraria.GameContent.UI.EmoteBubble make CountNPCs internal
		internal static void ResizeArrays(bool unloading) {
			Array.Resize(ref Main.NPCLoaded, nextNPC);
			Array.Resize(ref Main.townNPCCanSpawn, nextNPC);
			Array.Resize(ref Main.slimeRainNPC, nextNPC);
			Array.Resize(ref Main.npcTexture, nextNPC);
			Array.Resize(ref Main.npcAltTextures, nextNPC);
			Array.Resize(ref Main.npcCatchable, nextNPC);
			Array.Resize(ref Main.npcFrameCount, nextNPC);
			Array.Resize(ref NPC.killCount, nextNPC);
			Array.Resize(ref NPC.npcsFoundForCheckActive, nextNPC);
			Array.Resize(ref Lang._npcNameCache, nextNPC);
			Array.Resize(ref EmoteBubble.CountNPCs, nextNPC);
			Array.Resize(ref WorldGen.TownManager._hasRoom, nextNPC);
			Array.Resize(ref NPCID.Sets.TrailingMode, nextNPC);
			Array.Resize(ref NPCID.Sets.BelongsToInvasionOldOnesArmy, nextNPC);
			Array.Resize(ref NPCID.Sets.TeleportationImmune, nextNPC);
			Array.Resize(ref NPCID.Sets.UsesNewTargetting, nextNPC);
			Array.Resize(ref NPCID.Sets.FighterUsesDD2PortalAppearEffect, nextNPC);
			Array.Resize(ref NPCID.Sets.StatueSpawnedDropRarity, nextNPC);
			Array.Resize(ref NPCID.Sets.NoEarlymodeLootWhenSpawnedFromStatue, nextNPC);
			Array.Resize(ref NPCID.Sets.NeedsExpertScaling, nextNPC);
			Array.Resize(ref NPCID.Sets.ProjectileNPC, nextNPC);
			Array.Resize(ref NPCID.Sets.SavesAndLoads, nextNPC);
			Array.Resize(ref NPCID.Sets.TrailCacheLength, nextNPC);
			Array.Resize(ref NPCID.Sets.MPAllowedEnemies, nextNPC);
			Array.Resize(ref NPCID.Sets.TownCritter, nextNPC);
			Array.Resize(ref NPCID.Sets.HatOffsetY, nextNPC);
			Array.Resize(ref NPCID.Sets.FaceEmote, nextNPC);
			Array.Resize(ref NPCID.Sets.ExtraFramesCount, nextNPC);
			Array.Resize(ref NPCID.Sets.AttackFrameCount, nextNPC);
			Array.Resize(ref NPCID.Sets.DangerDetectRange, nextNPC);
			Array.Resize(ref NPCID.Sets.AttackTime, nextNPC);
			Array.Resize(ref NPCID.Sets.AttackAverageChance, nextNPC);
			Array.Resize(ref NPCID.Sets.AttackType, nextNPC);
			Array.Resize(ref NPCID.Sets.PrettySafe, nextNPC);
			Array.Resize(ref NPCID.Sets.MagicAuraColor, nextNPC);
			Array.Resize(ref NPCID.Sets.BossHeadTextures, nextNPC);
			Array.Resize(ref NPCID.Sets.ExcludedFromDeathTally, nextNPC);
			Array.Resize(ref NPCID.Sets.TechnicallyABoss, nextNPC);
			Array.Resize(ref NPCID.Sets.MustAlwaysDraw, nextNPC);
			Array.Resize(ref NPCID.Sets.ExtraTextureCount, nextNPC);
			Array.Resize(ref NPCID.Sets.NPCFramingGroup, nextNPC);
			Array.Resize(ref NPCID.Sets.TownNPCsFramingGroups, nextNPC);
			for (int k = NPCID.Count; k < nextNPC; k++) {
				Main.NPCLoaded[k] = true;
				Main.npcFrameCount[k] = 1;
				Lang._npcNameCache[k] = LocalizedText.Empty;
				NPCID.Sets.TrailingMode[k] = -1;
				NPCID.Sets.StatueSpawnedDropRarity[k] = -1f;
				NPCID.Sets.TrailCacheLength[k] = 10;
				NPCID.Sets.DangerDetectRange[k] = -1;
				NPCID.Sets.AttackTime[k] = -1;
				NPCID.Sets.AttackAverageChance[k] = 1;
				NPCID.Sets.AttackType[k] = -1;
				NPCID.Sets.PrettySafe[k] = -1;
				NPCID.Sets.MagicAuraColor[k] = Color.White;
				NPCID.Sets.BossHeadTextures[k] = -1;
			}

			InstancedGlobals = globalNPCs.Where(g => g.InstancePerEntity).ToArray();
			for (int i = 0; i < InstancedGlobals.Length; i++) {
				InstancedGlobals[i].instanceIndex = i;
			}
			foreach (var hook in hooks) {
				hook.arr = ModLoader.BuildGlobalHook(globalNPCs, hook.method);
			}

			if (!unloading) {
				loaded = true;
			}
		}

		internal static void Unload() {
			loaded = false;
			npcs.Clear();
			nextNPC = NPCID.Count;
			globalNPCs.Clear();
			globalIndexes.Clear();
			globalIndexesByType.Clear();
			bannerToItem.Clear();
			while (NPCID.Sets.Skeletons.Count > vanillaSkeletonCount) {
				NPCID.Sets.Skeletons.RemoveAt(NPCID.Sets.Skeletons.Count - 1);
			}
		}

		internal static bool IsModNPC(NPC npc) {
			return npc.type >= NPCID.Count;
		}

		private static HookList HookSetDefaults = AddHook<Action<NPC>>(g => g.SetDefaults);

		internal static void SetDefaults(NPC npc, bool createModNPC = true) {
			if (IsModNPC(npc)) {
				if (createModNPC) {
					npc.modNPC = GetNPC(npc.type).NewInstance(npc);
				}
				else //the default NPCs created and bound to ModNPCs are initialized before ResizeArrays. They come here during SetupContent.
				{
					Array.Resize(ref npc.buffImmune, BuffLoader.BuffCount);
				}
			}
			npc.globalNPCs = InstancedGlobals.Select(g => g.NewInstance(npc)).ToArray();
			npc.modNPC?.SetDefaults();
			foreach (GlobalNPC g in HookSetDefaults.arr) {
				g.Instance(npc).SetDefaults(npc);
			}
		}

		internal static GlobalNPC GetGlobalNPC(NPC npc, Mod mod, string name) {
			int index;
			return globalIndexes.TryGetValue(mod.Name + ':' + name, out index) ? globalNPCs[index].Instance(npc) : null;
		}

		internal static GlobalNPC GetGlobalNPC(NPC npc, Type type) {
			int index;
			return globalIndexesByType.TryGetValue(type, out index) ? (index > -1 ? globalNPCs[index].Instance(npc) : null) : null;
		}

		private static HookList HookScaleExpertStats = AddHook<Action<NPC, int, float>>(g => g.ScaleExpertStats);

		public static void ScaleExpertStats(NPC npc, int numPlayers, float bossLifeScale) {
			npc.modNPC?.ScaleExpertStats(numPlayers, bossLifeScale);

			foreach (GlobalNPC g in HookScaleExpertStats.arr) {
				g.Instance(npc).ScaleExpertStats(npc, numPlayers, bossLifeScale);
			}
		}

		private static HookList HookResetEffects = AddHook<Action<NPC>>(g => g.ResetEffects);

		public static void ResetEffects(NPC npc) {
			npc.modNPC?.ResetEffects();

			foreach (GlobalNPC g in HookResetEffects.arr) {
				g.Instance(npc).ResetEffects(npc);
			}
		}

		public static void NPCAI(NPC npc) {
			if (PreAI(npc)) {
				int type = npc.type;
				bool useAiType = npc.modNPC != null && npc.modNPC.aiType > 0;
				if (useAiType) {
					npc.type = npc.modNPC.aiType;
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

		public static bool PreAI(NPC npc) {
			bool result = true;
			foreach (GlobalNPC g in HookPreAI.arr) {
				result &= g.Instance(npc).PreAI(npc);
			}
			if (result && npc.modNPC != null) {
				return npc.modNPC.PreAI();
			}
			return result;
		}

		private static HookList HookAI = AddHook<Action<NPC>>(g => g.AI);

		public static void AI(NPC npc) {
			npc.modNPC?.AI();

			foreach (GlobalNPC g in HookAI.arr) {
				g.Instance(npc).AI(npc);
			}
		}

		private static HookList HookPostAI = AddHook<Action<NPC>>(g => g.PostAI);

		public static void PostAI(NPC npc) {
			npc.modNPC?.PostAI();

			foreach (GlobalNPC g in HookPostAI.arr) {
				g.Instance(npc).PostAI(npc);
			}
		}

		public static void SendExtraAI(NPC npc, BinaryWriter writer) {
			if (npc.modNPC != null) {
				byte[] data;
				using (MemoryStream stream = new MemoryStream()) {
					using (BinaryWriter modWriter = new BinaryWriter(stream)) {
						npc.modNPC.SendExtraAI(modWriter);
						modWriter.Flush();
						data = stream.ToArray();
					}
				}
				writer.Write((byte)data.Length);
				if (data.Length > 0) {
					writer.Write(data);
				}
			}
		}

		public static void ReceiveExtraAI(NPC npc, BinaryReader reader) {
			if (npc.modNPC != null) {
				byte[] extraAI = reader.ReadBytes(reader.ReadByte());
				if (extraAI.Length > 0) {
					using (MemoryStream stream = new MemoryStream(extraAI)) {
						using (BinaryReader modReader = new BinaryReader(stream)) {
							npc.modNPC.ReceiveExtraAI(modReader);
						}
					}
				}
			}
		}

		private static HookList HookFindFrame = AddHook<Action<NPC, int>>(g => g.FindFrame);

		public static void FindFrame(NPC npc, int frameHeight) {
			int type = npc.type;
			if (npc.modNPC != null && npc.modNPC.animationType > 0) {
				npc.type = npc.modNPC.animationType;
			}
			npc.VanillaFindFrame(frameHeight);
			npc.type = type;
			npc.modNPC?.FindFrame(frameHeight);

			foreach (GlobalNPC g in HookFindFrame.arr) {
				g.Instance(npc).FindFrame(npc, frameHeight);
			}
		}

		private static HookList HookHitEffect = AddHook<Action<NPC, int, double>>(g => g.HitEffect);

		public static void HitEffect(NPC npc, int hitDirection, double damage) {
			npc.VanillaHitEffect(hitDirection, damage);
			npc.modNPC?.HitEffect(hitDirection, damage);

			foreach (GlobalNPC g in HookHitEffect.arr) {
				g.Instance(npc).HitEffect(npc, hitDirection, damage);
			}
		}

		private delegate void DelegateUpdateLifeRegen(NPC npc, ref int damage);
		private static HookList HookUpdateLifeRegen = AddHook<DelegateUpdateLifeRegen>(g => g.UpdateLifeRegen);

		public static void UpdateLifeRegen(NPC npc, ref int damage) {
			npc.modNPC?.UpdateLifeRegen(ref damage);

			foreach (GlobalNPC g in HookUpdateLifeRegen.arr) {
				g.Instance(npc).UpdateLifeRegen(npc, ref damage);
			}
		}

		private static HookList HookCheckActive = AddHook<Func<NPC, bool>>(g => g.CheckActive);

		public static bool CheckActive(NPC npc) {
			if (npc.modNPC != null && !npc.modNPC.CheckActive()) {
				return false;
			}
			foreach (GlobalNPC g in HookCheckActive.arr) {
				if (!g.Instance(npc).CheckActive(npc)) {
					return false;
				}
			}
			return true;
		}

		private static HookList HookCheckDead = AddHook<Func<NPC, bool>>(g => g.CheckDead);

		public static bool CheckDead(NPC npc) {
			bool result = true;

			if (npc.modNPC != null) {
				result = npc.modNPC.CheckDead();
			}

			foreach (GlobalNPC g in HookCheckDead.arr) {
				result &= g.Instance(npc).CheckDead(npc);
			}

			return result;
		}

		private static HookList HookSpecialNPCLoot = AddHook<Func<NPC, bool>>(g => g.SpecialNPCLoot);

		public static bool SpecialNPCLoot(NPC npc) {
			foreach (GlobalNPC g in HookSpecialNPCLoot.arr) {
				if (g.Instance(npc).SpecialNPCLoot(npc)) {
					return true;
				}
			}
			if (npc.modNPC != null) {
				return npc.modNPC.SpecialNPCLoot();
			}
			return false;
		}

		private static HookList HookPreNPCLoot = AddHook<Func<NPC, bool>>(g => g.PreNPCLoot);

		public static bool PreNPCLoot(NPC npc) {
			bool result = true;
			foreach (GlobalNPC g in HookPreNPCLoot.arr) {
				result &= g.Instance(npc).PreNPCLoot(npc);
			}

			if (result && npc.modNPC != null) {
				result = npc.modNPC.PreNPCLoot();
			}

			if (!result) {
				blockLoot.Clear();
				return false;
			}

			return true;
		}

		private static HookList HookNPCLoot = AddHook<Action<NPC>>(g => g.NPCLoot);

		public static void NPCLoot(NPC npc) {
			npc.modNPC?.NPCLoot();

			foreach (GlobalNPC g in HookNPCLoot.arr) {
				g.Instance(npc).NPCLoot(npc);
			}
			blockLoot.Clear();
		}

		public static void BossLoot(NPC npc, ref string name, ref int potionType) {
			npc.modNPC?.BossLoot(ref name, ref potionType);
		}

		public static void BossBag(NPC npc, ref int bagType) {
			if (npc.modNPC != null) {
				bagType = npc.modNPC.bossBag;
			}
		}

		private static HookList HookOnCatchNPC = AddHook<Action<NPC, Player, Item>>(g => g.OnCatchNPC);

		public static void OnCatchNPC(NPC npc, Player player, Item item) {
			npc.modNPC?.OnCatchNPC(player, item);

			foreach (GlobalNPC g in HookOnCatchNPC.arr) {
				g.Instance(npc).OnCatchNPC(npc, player, item);
			}
		}

		private delegate bool DelegateCanHitPlayer(NPC npc, Player target, ref int cooldownSlot);
		private static HookList HookCanHitPlayer = AddHook<DelegateCanHitPlayer>(g => g.CanHitPlayer);

		public static bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot) {
			foreach (GlobalNPC g in HookCanHitPlayer.arr) {
				if (!g.Instance(npc).CanHitPlayer(npc, target, ref cooldownSlot)) {
					return false;
				}
			}
			if (npc.modNPC != null) {
				return npc.modNPC.CanHitPlayer(target, ref cooldownSlot);
			}
			return true;
		}

		private delegate void DelegateModifyHitPlayer(NPC npc, Player target, ref int damage, ref bool crit);
		private static HookList HookModifyHitPlayer = AddHook<DelegateModifyHitPlayer>(g => g.ModifyHitPlayer);

		public static void ModifyHitPlayer(NPC npc, Player target, ref int damage, ref bool crit) {
			npc.modNPC?.ModifyHitPlayer(target, ref damage, ref crit);

			foreach (GlobalNPC g in HookModifyHitPlayer.arr) {
				g.Instance(npc).ModifyHitPlayer(npc, target, ref damage, ref crit);
			}
		}

		private static HookList HookOnHitPlayer = AddHook<Action<NPC, Player, int, bool>>(g => g.OnHitPlayer);

		public static void OnHitPlayer(NPC npc, Player target, int damage, bool crit) {
			npc.modNPC?.OnHitPlayer(target, damage, crit);

			foreach (GlobalNPC g in HookOnHitPlayer.arr) {
				g.Instance(npc).OnHitPlayer(npc, target, damage, crit);
			}
		}

		private static HookList HookCanHitNPC = AddHook<Func<NPC, NPC, bool?>>(g => g.CanHitNPC);

		public static bool? CanHitNPC(NPC npc, NPC target) {
			bool? flag = null;
			foreach (GlobalNPC g in HookCanHitNPC.arr) {
				bool? canHit = g.Instance(npc).CanHitNPC(npc, target);
				if (canHit.HasValue && !canHit.Value) {
					return false;
				}
				if (canHit.HasValue) {
					flag = canHit.Value;
				}
			}
			if (npc.modNPC != null) {
				bool? canHit = npc.modNPC.CanHitNPC(target);
				if (canHit.HasValue && !canHit.Value) {
					return false;
				}
				if (canHit.HasValue) {
					flag = canHit.Value;
				}
			}
			return flag;
		}

		private delegate void DelegateModifyHitNPC(NPC npc, NPC target, ref int damage, ref float knockback, ref bool crit);
		private static HookList HookModifyHitNPC = AddHook<DelegateModifyHitNPC>(g => g.ModifyHitNPC);

		public static void ModifyHitNPC(NPC npc, NPC target, ref int damage, ref float knockback, ref bool crit) {
			npc.modNPC?.ModifyHitNPC(target, ref damage, ref knockback, ref crit);

			foreach (GlobalNPC g in HookModifyHitNPC.arr) {
				g.Instance(npc).ModifyHitNPC(npc, target, ref damage, ref knockback, ref crit);
			}
		}

		private static HookList HookOnHitNPC = AddHook<Action<NPC, NPC, int, float, bool>>(g => g.OnHitNPC);

		public static void OnHitNPC(NPC npc, NPC target, int damage, float knockback, bool crit) {
			npc.modNPC?.OnHitNPC(target, damage, knockback, crit);
			foreach (GlobalNPC g in HookOnHitNPC.arr) {
				g.Instance(npc).OnHitNPC(npc, target, damage, knockback, crit);
			}
		}

		private static HookList HookCanBeHitByItem = AddHook<Func<NPC, Player, Item, bool?>>(g => g.CanBeHitByItem);

		public static bool? CanBeHitByItem(NPC npc, Player player, Item item) {
			bool? flag = null;
			foreach (GlobalNPC g in HookCanBeHitByItem.arr) {
				bool? canHit = g.Instance(npc).CanBeHitByItem(npc, player, item);
				if (canHit.HasValue && !canHit.Value) {
					return false;
				}
				if (canHit.HasValue) {
					flag = canHit.Value;
				}
			}
			if (npc.modNPC != null) {
				bool? canHit = npc.modNPC.CanBeHitByItem(player, item);
				if (canHit.HasValue && !canHit.Value) {
					return false;
				}
				if (canHit.HasValue) {
					flag = canHit.Value;
				}
			}
			return flag;
		}

		private delegate void DelegateModifyHitByItem(NPC npc, Player player, Item item, ref int damage, ref float knockback, ref bool crit);
		private static HookList HookModifyHitByItem = AddHook<DelegateModifyHitByItem>(g => g.ModifyHitByItem);

		public static void ModifyHitByItem(NPC npc, Player player, Item item, ref int damage, ref float knockback, ref bool crit) {
			npc.modNPC?.ModifyHitByItem(player, item, ref damage, ref knockback, ref crit);

			foreach (GlobalNPC g in HookModifyHitByItem.arr) {
				g.Instance(npc).ModifyHitByItem(npc, player, item, ref damage, ref knockback, ref crit);
			}
		}

		private static HookList HookOnHitByItem = AddHook<Action<NPC, Player, Item, int, float, bool>>(g => g.OnHitByItem);

		public static void OnHitByItem(NPC npc, Player player, Item item, int damage, float knockback, bool crit) {
			npc.modNPC?.OnHitByItem(player, item, damage, knockback, crit);

			foreach (GlobalNPC g in HookOnHitByItem.arr) {
				g.Instance(npc).OnHitByItem(npc, player, item, damage, knockback, crit);
			}
		}

		private static HookList HookCanBeHitByProjectile = AddHook<Func<NPC, Projectile, bool?>>(g => g.CanBeHitByProjectile);

		public static bool? CanBeHitByProjectile(NPC npc, Projectile projectile) {
			bool? flag = null;
			foreach (GlobalNPC g in HookCanBeHitByProjectile.arr) {
				bool? canHit = g.Instance(npc).CanBeHitByProjectile(npc, projectile);
				if (canHit.HasValue && !canHit.Value) {
					return false;
				}
				if (canHit.HasValue) {
					flag = canHit.Value;
				}
			}
			if (npc.modNPC != null) {
				bool? canHit = npc.modNPC.CanBeHitByProjectile(projectile);
				if (canHit.HasValue && !canHit.Value) {
					return false;
				}
				if (canHit.HasValue) {
					flag = canHit.Value;
				}
			}
			return flag;
		}

		private delegate void DelegateModifyHitByProjectile(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection);
		private static HookList HookModifyHitByProjectile = AddHook<DelegateModifyHitByProjectile>(g => g.ModifyHitByProjectile);

		public static void ModifyHitByProjectile(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
			npc.modNPC?.ModifyHitByProjectile(projectile, ref damage, ref knockback, ref crit, ref hitDirection);

			foreach (GlobalNPC g in HookModifyHitByProjectile.arr) {
				g.Instance(npc).ModifyHitByProjectile(npc, projectile, ref damage, ref knockback, ref crit, ref hitDirection);
			}
		}

		private static HookList HookOnHitByProjectile = AddHook<Action<NPC, Projectile, int, float, bool>>(g => g.OnHitByProjectile);

		public static void OnHitByProjectile(NPC npc, Projectile projectile, int damage, float knockback, bool crit) {
			npc.modNPC?.OnHitByProjectile(projectile, damage, knockback, crit);

			foreach (GlobalNPC g in HookOnHitByProjectile.arr) {
				g.Instance(npc).OnHitByProjectile(npc, projectile, damage, knockback, crit);
			}
		}

		private delegate bool DelegateStrikeNPC(NPC npc, ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit);
		private static HookList HookStrikeNPC = AddHook<DelegateStrikeNPC>(g => g.StrikeNPC);

		public static bool StrikeNPC(NPC npc, ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit) {
			bool flag = true;
			if (npc.modNPC != null) {
				flag = npc.modNPC.StrikeNPC(ref damage, defense, ref knockback, hitDirection, ref crit);
			}
			foreach (GlobalNPC g in HookStrikeNPC.arr) {
				if (!g.Instance(npc).StrikeNPC(npc, ref damage, defense, ref knockback, hitDirection, ref crit)) {
					flag = false;
				}
			}
			return flag;
		}

		private delegate void DelegateBossHeadSlot(NPC npc, ref int index);
		private static HookList HookBossHeadSlot = AddHook<DelegateBossHeadSlot>(g => g.BossHeadSlot);

		public static void BossHeadSlot(NPC npc, ref int index) {
			npc.modNPC?.BossHeadSlot(ref index);

			foreach (GlobalNPC g in HookBossHeadSlot.arr) {
				g.Instance(npc).BossHeadSlot(npc, ref index);
			}
		}

		private delegate void DelegateBossHeadRotation(NPC npc, ref float rotation);
		private static HookList HookBossHeadRotation = AddHook<DelegateBossHeadRotation>(g => g.BossHeadRotation);

		public static void BossHeadRotation(NPC npc, ref float rotation) {
			npc.modNPC?.BossHeadRotation(ref rotation);

			foreach (GlobalNPC g in HookBossHeadRotation.arr) {
				g.Instance(npc).BossHeadRotation(npc, ref rotation);
			}
		}

		private delegate void DelegateBossHeadSpriteEffects(NPC npc, ref SpriteEffects spriteEffects);
		private static HookList HookBossHeadSpriteEffects = AddHook<DelegateBossHeadSpriteEffects>(g => g.BossHeadSpriteEffects);

		public static void BossHeadSpriteEffects(NPC npc, ref SpriteEffects spriteEffects) {
			npc.modNPC?.BossHeadSpriteEffects(ref spriteEffects);

			foreach (GlobalNPC g in HookBossHeadSpriteEffects.arr) {
				g.Instance(npc).BossHeadSpriteEffects(npc, ref spriteEffects);
			}
		}

		private static HookList HookGetAlpha = AddHook<Func<NPC, Color, Color?>>(g => g.GetAlpha);

		public static Color? GetAlpha(NPC npc, Color lightColor) {
			foreach (GlobalNPC g in HookGetAlpha.arr) {
				Color? color = g.Instance(npc).GetAlpha(npc, lightColor);
				if (color.HasValue) {
					return color.Value;
				}
			}
			return npc.modNPC?.GetAlpha(lightColor);
		}

		private delegate void DelegateDrawEffects(NPC npc, ref Color drawColor);
		private static HookList HookDrawEffects = AddHook<DelegateDrawEffects>(g => g.DrawEffects);

		public static void DrawEffects(NPC npc, ref Color drawColor) {
			npc.modNPC?.DrawEffects(ref drawColor);

			foreach (GlobalNPC g in HookDrawEffects.arr) {
				g.Instance(npc).DrawEffects(npc, ref drawColor);
			}
		}

		private static HookList HookPreDraw = AddHook<Func<NPC, SpriteBatch, Color, bool>>(g => g.PreDraw);

		public static bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor) {
			bool result = true;
			foreach (GlobalNPC g in HookPreDraw.arr) {
				result &= g.Instance(npc).PreDraw(npc, spriteBatch, drawColor);
			}
			if (result && npc.modNPC != null) {
				return npc.modNPC.PreDraw(spriteBatch, drawColor);
			}
			return result;
		}

		private static HookList HookPostDraw = AddHook<Action<NPC, SpriteBatch, Color>>(g => g.PostDraw);

		public static void PostDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor) {
			npc.modNPC?.PostDraw(spriteBatch, drawColor);

			foreach (GlobalNPC g in HookPostDraw.arr) {
				g.Instance(npc).PostDraw(npc, spriteBatch, drawColor);
			}
		}

		private delegate bool? DelegateDrawHealthBar(NPC npc, byte bhPosition, ref float scale, ref Vector2 position);
		private static HookList HookDrawHealthBar = AddHook<DelegateDrawHealthBar>(g => g.DrawHealthBar);

		public static bool DrawHealthBar(NPC npc, ref float scale) {
			Vector2 position = new Vector2(npc.position.X + npc.width / 2, npc.position.Y + npc.gfxOffY);
			if (Main.HealthBarDrawSettings == 1) {
				position.Y += npc.height + 10f + Main.NPCAddHeight(npc.whoAmI);
			}
			else if (Main.HealthBarDrawSettings == 2) {
				position.Y -= 24f + Main.NPCAddHeight(npc.whoAmI) / 2f;
			}
			foreach (GlobalNPC g in HookDrawHealthBar.arr) {
				bool? result = g.Instance(npc).DrawHealthBar(npc, Main.HealthBarDrawSettings, ref scale, ref position);
				if (result.HasValue) {
					if (result.Value) {
						DrawHealthBar(npc, position, scale);
					}
					return false;
				}
			}
			if (NPCLoader.IsModNPC(npc)) {
				bool? result = npc.modNPC.DrawHealthBar(Main.HealthBarDrawSettings, ref scale, ref position);
				if (result.HasValue) {
					if (result.Value) {
						DrawHealthBar(npc, position, scale);
					}
					return false;
				}
			}
			return true;
		}

		private static void DrawHealthBar(NPC npc, Vector2 position, float scale) {
			float alpha = Lighting.Brightness((int)(npc.Center.X / 16f), (int)(npc.Center.Y / 16f));
			Main.instance.DrawHealthBar(position.X, position.Y, npc.life, npc.lifeMax, alpha, scale);
		}

		private delegate void DelegateEditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns);
		private static HookList HookEditSpawnRate = AddHook<DelegateEditSpawnRate>(g => g.EditSpawnRate);

		public static void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns) {
			foreach (GlobalNPC g in HookEditSpawnRate.arr) {
				g.EditSpawnRate(player, ref spawnRate, ref maxSpawns);
			}
		}

		private delegate void DelegateEditSpawnRange(Player player, ref int spawnRangeX, ref int spawnRangeY,
			ref int safeRangeX, ref int safeRangeY);
		private static HookList HookEditSpawnRange = AddHook<DelegateEditSpawnRange>(g => g.EditSpawnRange);

		public static void EditSpawnRange(Player player, ref int spawnRangeX, ref int spawnRangeY,
			ref int safeRangeX, ref int safeRangeY) {
			foreach (GlobalNPC g in HookEditSpawnRange.arr) {
				g.EditSpawnRange(player, ref spawnRangeX, ref spawnRangeY, ref safeRangeX, ref safeRangeY);
			}
		}

		private static HookList HookEditSpawnPool = AddHook<Action<Dictionary<int, float>, NPCSpawnInfo>>(g => g.EditSpawnPool);

		public static int? ChooseSpawn(NPCSpawnInfo spawnInfo) {
			NPCSpawnHelper.Reset();
			NPCSpawnHelper.DoChecks(spawnInfo);
			IDictionary<int, float> pool = new Dictionary<int, float>();
			pool[0] = 1f;
			foreach (ModNPC npc in npcs) {
				float weight = npc.SpawnChance(spawnInfo);
				if (weight > 0f) {
					pool[npc.npc.type] = weight;
				}
			}
			foreach (GlobalNPC g in HookEditSpawnPool.arr) {
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

		public static int SpawnNPC(int type, int tileX, int tileY) {
			var npc = type >= NPCID.Count ?
				GetNPC(type).SpawnNPC(tileX, tileY) :
				NPC.NewNPC(tileX * 16 + 8, tileY * 16, type);

			foreach (GlobalNPC g in HookSpawnNPC.arr) {
				g.Instance(Main.npc[npc]).SpawnNPC(npc, tileX, tileY);
			}

			return npc;
		}

		public static void CanTownNPCSpawn(int numTownNPCs, int money) {
			foreach (ModNPC npc in npcs) {
				if (npc.npc.townNPC && NPC.TypeToHeadIndex(npc.npc.type) >= 0 && !NPC.AnyNPCs(npc.npc.type) &&
					npc.CanTownNPCSpawn(numTownNPCs, money)) {
					Main.townNPCCanSpawn[npc.npc.type] = true;
					if (WorldGen.prioritizedTownNPC == 0) {
						WorldGen.prioritizedTownNPC = npc.npc.type;
					}
				}
			}
		}

		public static bool CheckConditions(int type) {
			return GetNPC(type)?.CheckConditions(WorldGen.roomX1, WorldGen.roomX2, WorldGen.roomY1, WorldGen.roomY2) ?? true;
		}

		public static string TownNPCName(int type) {
			return GetNPC(type)?.TownNPCName() ?? "";
		}

		public static bool UsesPartyHat(NPC npc) {
			return npc.modNPC?.UsesPartyHat() ?? true;
		}

		private static HookList HookCanChat = AddHook<Func<NPC, bool?>>(g => g.CanChat);

		public static bool CanChat(NPC npc, bool vanillaCanChat) {
			bool defaultCanChat = npc.modNPC?.CanChat() ?? vanillaCanChat;

			foreach (GlobalNPC g in HookCanChat.arr) {
				bool? canChat = g.Instance(npc).CanChat(npc);
				if (canChat.HasValue) {
					if (!canChat.Value) {
						return false;
					}
					defaultCanChat = true;
				}
			}

			return defaultCanChat;
		}

		private delegate void DelegateGetChat(NPC npc, ref string chat);
		private static HookList HookGetChat = AddHook<DelegateGetChat>(g => g.GetChat);

		public static void GetChat(NPC npc, ref string chat) {
			if (npc.modNPC != null) {
				chat = npc.modNPC.GetChat();
			}
			else if (chat.Equals("")) {
				chat = Language.GetTextValue("tModLoader.DefaultTownNPCChat");
			}
			foreach (GlobalNPC g in HookGetChat.arr) {
				g.Instance(npc).GetChat(npc, ref chat);
			}
		}

		public static void SetChatButtons(ref string button, ref string button2) {
			if (Main.player[Main.myPlayer].talkNPC >= 0) {
				NPC npc = Main.npc[Main.player[Main.myPlayer].talkNPC];
				npc.modNPC?.SetChatButtons(ref button, ref button2);
			}
		}

		private static HookList HookPreChatButtonClicked = AddHook<Func<NPC, bool, bool>>(g => g.PreChatButtonClicked);

		public static bool PreChatButtonClicked(bool firstButton) {
			NPC npc = Main.npc[Main.LocalPlayer.talkNPC];

			bool result = true;
			foreach (GlobalNPC g in HookPreChatButtonClicked.arr) {
				result &= g.Instance(npc).PreChatButtonClicked(npc, firstButton);
			}

			if (!result) {
				Main.PlaySound(SoundID.MenuTick);
				return false;
			}

			return true;
		}

		private delegate void DelegateOnChatButtonClicked(NPC npc, bool firstButton);
		private static HookList HookOnChatButtonClicked = AddHook<DelegateOnChatButtonClicked>(g => g.OnChatButtonClicked);

		public static void OnChatButtonClicked(bool firstButton) {
			NPC npc = Main.npc[Main.LocalPlayer.talkNPC];
			bool shop = false;

			if (npc.modNPC != null) {
				npc.modNPC.OnChatButtonClicked(firstButton, ref shop);
				Main.PlaySound(SoundID.MenuTick);
				if (shop) {
					Main.playerInventory = true;
					Main.npcChatText = "";
					Main.npcShop = Main.MaxShopIDs - 1;
					Main.instance.shop[Main.npcShop].SetupShop(npc.type);
				}
			}
			foreach (GlobalNPC g in HookOnChatButtonClicked.arr) {
				g.Instance(npc).OnChatButtonClicked(npc, firstButton);
			}
		}

		private delegate void DelegateSetupShop(int type, Chest shop, ref int nextSlot);
		private static HookList HookSetupShop = AddHook<DelegateSetupShop>(g => g.SetupShop);

		public static void SetupShop(int type, Chest shop, ref int nextSlot) {
			if (type < shopToNPC.Length) {
				type = shopToNPC[type];
			}
			else {
				GetNPC(type)?.SetupShop(shop, ref nextSlot);
			}
			foreach (GlobalNPC g in HookSetupShop.arr) {
				g.SetupShop(type, shop, ref nextSlot);
			}
		}

		private delegate void DelegateSetupTravelShop(int[] shop, ref int nextSlot);
		private static HookList HookSetupTravelShop = AddHook<DelegateSetupTravelShop>(g => g.SetupTravelShop);

		public static void SetupTravelShop(int[] shop, ref int nextSlot) {
			foreach (GlobalNPC g in HookSetupTravelShop.arr) {
				g.SetupTravelShop(shop, ref nextSlot);
			}
		}

		private static HookList HookCanGoToStatue = AddHook<Func<NPC, bool, bool?>>(g => g.CanGoToStatue);

		public static bool CanGoToStatue(NPC npc, bool toKingStatue, bool vanillaCanGo) {
			bool defaultCanGo = npc.modNPC?.CanGoToStatue(toKingStatue) ?? vanillaCanGo;

			foreach (GlobalNPC g in HookCanGoToStatue.arr) {
				bool? canGo = g.Instance(npc).CanGoToStatue(npc, toKingStatue);
				if (canGo.HasValue) {
					if (!canGo.Value) {
						return false;
					}
					defaultCanGo = true;
				}
			}

			return defaultCanGo;
		}

		private static HookList HookOnGoToStatue = AddHook<Action<NPC, bool>>(g => g.OnGoToStatue);

		public static void OnGoToStatue(NPC npc, bool toKingStatue) {
			npc.modNPC?.OnGoToStatue(toKingStatue);

			foreach (GlobalNPC g in HookOnGoToStatue.arr) {
				g.OnGoToStatue(npc, toKingStatue);
			}
		}

		private delegate void DelegateBuffTownNPC(ref float damageMult, ref int defense);
		private static HookList HookBuffTownNPC = AddHook<DelegateBuffTownNPC>(g => g.BuffTownNPC);

		public static void BuffTownNPC(ref float damageMult, ref int defense) {
			foreach (GlobalNPC g in HookBuffTownNPC.arr) {
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

		public static void TownNPCAttackStrength(NPC npc, ref int damage, ref float knockback) {
			npc.modNPC?.TownNPCAttackStrength(ref damage, ref knockback);

			foreach (GlobalNPC g in HookTownNPCAttackStrength.arr) {
				g.Instance(npc).TownNPCAttackStrength(npc, ref damage, ref knockback);
			}
		}

		private delegate void DelegateTownNPCAttackCooldown(NPC npc, ref int cooldown, ref int randExtraCooldown);
		private static HookList HookTownNPCAttackCooldown = AddHook<DelegateTownNPCAttackCooldown>(g => g.TownNPCAttackCooldown);

		public static void TownNPCAttackCooldown(NPC npc, ref int cooldown, ref int randExtraCooldown) {
			npc.modNPC?.TownNPCAttackCooldown(ref cooldown, ref randExtraCooldown);

			foreach (GlobalNPC g in HookTownNPCAttackCooldown.arr) {
				g.Instance(npc).TownNPCAttackCooldown(npc, ref cooldown, ref randExtraCooldown);
			}
		}

		private delegate void DelegateTownNPCAttackProj(NPC npc, ref int projType, ref int attackDelay);
		private static HookList HookTownNPCAttackProj = AddHook<DelegateTownNPCAttackProj>(g => g.TownNPCAttackProj);

		public static void TownNPCAttackProj(NPC npc, ref int projType, ref int attackDelay) {
			npc.modNPC?.TownNPCAttackProj(ref projType, ref attackDelay);

			foreach (GlobalNPC g in HookTownNPCAttackProj.arr) {
				g.Instance(npc).TownNPCAttackProj(npc, ref projType, ref attackDelay);
			}
		}

		private delegate void DelegateTownNPCAttackProjSpeed(NPC npc, ref float multiplier, ref float gravityCorrection,
			ref float randomOffset);
		private static HookList HookTownNPCAttackProjSpeed = AddHook<DelegateTownNPCAttackProjSpeed>(g => g.TownNPCAttackProjSpeed);

		public static void TownNPCAttackProjSpeed(NPC npc, ref float multiplier, ref float gravityCorrection,
			ref float randomOffset) {
			npc.modNPC?.TownNPCAttackProjSpeed(ref multiplier, ref gravityCorrection, ref randomOffset);

			foreach (GlobalNPC g in HookTownNPCAttackProjSpeed.arr) {
				g.Instance(npc).TownNPCAttackProjSpeed(npc, ref multiplier, ref gravityCorrection, ref randomOffset);
			}
		}

		private delegate void DelegateTownNPCAttackShoot(NPC npc, ref bool inBetweenShots);
		private static HookList HookTownNPCAttackShoot = AddHook<DelegateTownNPCAttackShoot>(g => g.TownNPCAttackShoot);

		public static void TownNPCAttackShoot(NPC npc, ref bool inBetweenShots) {
			npc.modNPC?.TownNPCAttackShoot(ref inBetweenShots);

			foreach (GlobalNPC g in HookTownNPCAttackShoot.arr) {
				g.Instance(npc).TownNPCAttackShoot(npc, ref inBetweenShots);
			}
		}

		private delegate void DelegateTownNPCAttackMagic(NPC npc, ref float auraLightMultiplier);
		private static HookList HookTownNPCAttackMagic = AddHook<DelegateTownNPCAttackMagic>(g => g.TownNPCAttackMagic);

		public static void TownNPCAttackMagic(NPC npc, ref float auraLightMultiplier) {
			npc.modNPC?.TownNPCAttackMagic(ref auraLightMultiplier);

			foreach (GlobalNPC g in HookTownNPCAttackMagic.arr) {
				g.Instance(npc).TownNPCAttackMagic(npc, ref auraLightMultiplier);
			}
		}

		private delegate void DelegateTownNPCAttackSwing(NPC npc, ref int itemWidth, ref int itemHeight);
		private static HookList HookTownNPCAttackSwing = AddHook<DelegateTownNPCAttackSwing>(g => g.TownNPCAttackSwing);

		public static void TownNPCAttackSwing(NPC npc, ref int itemWidth, ref int itemHeight) {
			npc.modNPC?.TownNPCAttackSwing(ref itemWidth, ref itemHeight);

			foreach (GlobalNPC g in HookTownNPCAttackSwing.arr) {
				g.Instance(npc).TownNPCAttackSwing(npc, ref itemWidth, ref itemHeight);
			}
		}

		private delegate void DelegateDrawTownAttackGun(NPC npc, ref float scale, ref int item, ref int closeness);
		private static HookList HookDrawTownAttackGun = AddHook<DelegateDrawTownAttackGun>(g => g.DrawTownAttackGun);

		public static void DrawTownAttackGun(NPC npc, ref float scale, ref int item, ref int closeness) {
			npc.modNPC?.DrawTownAttackGun(ref scale, ref item, ref closeness);

			foreach (GlobalNPC g in HookDrawTownAttackGun.arr) {
				g.Instance(npc).DrawTownAttackGun(npc, ref scale, ref item, ref closeness);
			}
		}

		private delegate void DelegateDrawTownAttackSwing(NPC npc, ref Texture2D item, ref int itemSize, ref float scale, ref Vector2 offset);
		private static HookList HookDrawTownAttackSwing = AddHook<DelegateDrawTownAttackSwing>(g => g.DrawTownAttackSwing);

		public static void DrawTownAttackSwing(NPC npc, ref Texture2D item, ref int itemSize, ref float scale, ref Vector2 offset) {
			npc.modNPC?.DrawTownAttackSwing(ref item, ref itemSize, ref scale, ref offset);

			foreach (GlobalNPC g in HookDrawTownAttackSwing.arr) {
				g.Instance(npc).DrawTownAttackSwing(npc, ref item, ref itemSize, ref scale, ref offset);
			}
		}

		private static bool HasMethod(Type t, string method, params Type[] args) {
			return t.GetMethod(method, args).DeclaringType != typeof(GlobalNPC);
		}

		internal static void VerifyGlobalNPC(GlobalNPC npc) {
			var type = npc.GetType();

			bool hasInstanceFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				.Any(f => f.DeclaringType != typeof(GlobalNPC));
			if (hasInstanceFields) {
				if (!npc.InstancePerEntity) {
					throw new Exception(type + " has instance fields but does not set InstancePerEntity to true. Either use static fields, or per instance globals");
				}
			}
		}
	}
}
