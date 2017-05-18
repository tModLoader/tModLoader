using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
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
		internal static readonly IList<NPCInfo> infoList = new List<NPCInfo>();
		internal static readonly IDictionary<string, int> infoIndexes = new Dictionary<string, int>();
		internal static readonly IDictionary<int, int> bannerToItem = new Dictionary<int, int>();
		private static int vanillaSkeletonCount = NPCID.Sets.Skeletons.Count;
		private static readonly int[] shopToNPC = new int[Main.MaxShopIDs - 1];
		//in Terraria.Item.NewItem after setting Main.item[400] add
		//  if(NPCLoader.blockLoot.Contains(Type)) { return num; }
		/// <summary>
		/// Allows you to stop an NPC from dropping loot by adding item IDs to this list. This list will be cleared whenever NPCLoot ends. Useful for either removing an item or change the drop rate of an item in the NPC's loot table. To change the drop rate of an item, use the PreNPCLoot hook, spawn the item yourself, then add the item's ID to this list.
		/// </summary>
		public static readonly IList<int> blockLoot = new List<int>();
		
		private static Action<NPC>[] HookSetDefaults = new Action<NPC>[0];
		private static Action<NPC, int, float>[] HookScaleExpertStats = new Action<NPC, int, float>[0];
		private static Action<NPC>[] HookResetEffects;
		private static Func<NPC, bool>[] HookPreAI;
		private static Action<NPC>[] HookAI;
		private static Action<NPC>[] HookPostAI;
		private static Action<NPC, int>[] HookFindFrame;
		private static Action<NPC, int, double>[] HookHitEffect;
		private delegate void DelegateUpdateLifeRegen(NPC npc, ref int damage);
		private static DelegateUpdateLifeRegen[] HookUpdateLifeRegen;
		private static Func<NPC, bool>[] HookCheckActive;
		private static Func<NPC, bool>[] HookCheckDead;
		private static Func<NPC, bool>[] HookPreNPCLoot;
		private static Action<NPC>[] HookNPCLoot;
		private delegate bool DelegateCanHitPlayer(NPC npc, Player target, ref int cooldownSlot);
		private static DelegateCanHitPlayer[] HookCanHitPlayer;
		private delegate void DelegateModifyHitPlayer(NPC npc, Player target, ref int damage, ref bool crit);
		private static DelegateModifyHitPlayer[] HookModifyHitPlayer;
		private static Action<NPC, Player, int, bool>[] HookOnHitPlayer;
		private static Func<NPC, NPC, bool?>[] HookCanHitNPC;
		private delegate void DelegateModifyHitNPC(NPC npc, NPC target, ref int damage, ref float knockback, ref bool crit);
		private static DelegateModifyHitNPC[] HookModifyHitNPC;
		private static Action<NPC, NPC, int, float, bool>[] HookOnHitNPC;
		private static Func<NPC, Player, Item, bool?>[] HookCanBeHitByItem;
		private delegate void DelegateModifyHitByItem(NPC npc, Player player, Item item, ref int damage, ref float knockback, ref bool crit);
		private static DelegateModifyHitByItem[] HookModifyHitByItem;
		private static Action<NPC, Player, Item, int, float, bool>[] HookOnHitByItem;
		private static Func<NPC, Projectile, bool?>[] HookCanBeHitByProjectile;
		private delegate void DelegateModifyHitByProjectile(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection);
		private static DelegateModifyHitByProjectile[] HookModifyHitByProjectile;
		private static Action<NPC, Projectile, int, float, bool>[] HookOnHitByProjectile;
		private delegate bool DelegateStrikeNPC(NPC npc, ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit);
		private static DelegateStrikeNPC[] HookStrikeNPC;
		private delegate void DelegateBossHeadSlot(NPC npc, ref int index);
		private static DelegateBossHeadSlot[] HookBossHeadSlot;
		private delegate void DelegateBossHeadRotation(NPC npc, ref float rotation);
		private static DelegateBossHeadRotation[] HookBossHeadRotation;
		private delegate void DelegateBossHeadSpriteEffects(NPC npc, ref SpriteEffects spriteEffects);
		private static DelegateBossHeadSpriteEffects[] HookBossHeadSpriteEffects;
		private static Func<NPC, Color, Color?>[] HookGetAlpha;
		private delegate void DelegateDrawEffects(NPC npc, ref Color drawColor);
		private static DelegateDrawEffects[] HookDrawEffects;
		private static Func<NPC, SpriteBatch, Color, bool>[] HookPreDraw;
		private static Action<NPC, SpriteBatch, Color>[] HookPostDraw;
		private delegate bool? DelegateDrawHealthBar(NPC npc, byte hbPosition, ref float scale, ref Vector2 offset);
		private static DelegateDrawHealthBar[] HookDrawHealthBar;
		private delegate void DelegateEditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns);
		private static DelegateEditSpawnRate[] HookEditSpawnRate;
		private delegate void DelegateEditSpawnRange(Player player, ref int spawnRangeX, ref int spawnRangeY, ref int safeRangeX, ref int safeRangeY);
		private static DelegateEditSpawnRange[] HookEditSpawnRange;
		private static Action<IDictionary<int, float>, NPCSpawnInfo>[] HookEditSpawnPool;
		private static Action<int, int, int>[] HookSpawnNPC;
		private delegate void DelegateGetChat(NPC npc, ref string chat);
		private static DelegateGetChat[] HookGetChat;
		private delegate void DelegateSetupShop(int type, Chest shop, ref int nextSlot);
		private static DelegateSetupShop[] HookSetupShop = new DelegateSetupShop[0];
		private delegate void DelegateSetupTravelShop(int[] shop, ref int nextSlot);
		private static DelegateSetupTravelShop[] HookSetupTravelShop = new DelegateSetupTravelShop[0];
		private delegate void DelegateBuffTownNPC(ref float damageMult, ref int defense);
		private static DelegateBuffTownNPC[] HookBuffTownNPC;
		private delegate void DelegateTownNPCAttackStrength(NPC npc, ref int damage, ref float knockback);
		private static DelegateTownNPCAttackStrength[] HookTownNPCAttackStrength;
		private delegate void DelegateTownNPCAttackCooldown(NPC npc, ref int cooldown, ref int randExtraCooldown);
		private static DelegateTownNPCAttackCooldown[] HookTownNPCAttackCooldown;
		private delegate void DelegateTownNPCAttackProj(NPC npc, ref int projType, ref int attackDelay);
		private static DelegateTownNPCAttackProj[] HookTownNPCAttackProj;
		private delegate void DelegateTownNPCAttackProjSpeed(NPC npc, ref float multiplier, ref float gravityCorrection, ref float randomOffset);
		private static DelegateTownNPCAttackProjSpeed[] HookTownNPCAttackProjSpeed;
		private delegate void DelegateTownNPCAttackShoot(NPC npc, ref bool inBetweenShots);
		private static DelegateTownNPCAttackShoot[] HookTownNPCAttackShoot;
		private delegate void DelegateTownNPCAttackMagic(NPC npc, ref float auraLightMultiplier);
		private static DelegateTownNPCAttackMagic[] HookTownNPCAttackMagic;
		private delegate void DelegateTownNPCAttackSwing(NPC npc, ref int itemWidth, ref int itemHeight);
		private static DelegateTownNPCAttackSwing[] HookTownNPCAttackSwing;
		private delegate void DelegateDrawTownAttackGun(NPC npc, ref float scale, ref int item, ref int closeness);
		private static DelegateDrawTownAttackGun[] HookDrawTownAttackGun;
		private delegate void DelegateDrawTownAttackSwing(NPC npc, ref Texture2D item, ref int itemSize, ref float scale, ref Vector2 offset);
		private static DelegateDrawTownAttackSwing[] HookDrawTownAttackSwing;

		static NPCLoader()
		{
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

		internal static int ReserveNPCID()
		{
			if (ModNet.AllowVanillaClients) throw new Exception("Adding npcs breaks vanilla client compatiblity");

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
		public static ModNPC GetNPC(int type)
		{
			return type >= NPCID.Count && type < NPCCount ? npcs[type - NPCID.Count] : null;
		}
		//change initial size of Terraria.Player.npcTypeNoAggro and NPCBannerBuff to NPCLoader.NPCCount()
		//in Terraria.Main.MouseText replace 251 with NPCLoader.NPCCount()
		//in Terraria.Main.DrawNPCs and Terraria.NPC.NPCLoot remove type too high check
		//replace a lot of 540 immediates
		//in Terraria.GameContent.UI.EmoteBubble make CountNPCs internal
		internal static void ResizeArrays(bool unloading)
		{
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
			for (int k = NPCID.Count; k < nextNPC; k++)
			{
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

			ModLoader.BuildGlobalHook(ref HookSetDefaults, globalNPCs, g => g.SetDefaults);
			ModLoader.BuildGlobalHook(ref HookScaleExpertStats, globalNPCs, g => g.ScaleExpertStats);
			ModLoader.BuildGlobalHook(ref HookResetEffects, globalNPCs, g => g.ResetEffects);
			ModLoader.BuildGlobalHook(ref HookPreAI, globalNPCs, g => g.PreAI);
			ModLoader.BuildGlobalHook(ref HookAI, globalNPCs, g => g.AI);
			ModLoader.BuildGlobalHook(ref HookPostAI, globalNPCs, g => g.PostAI);
			ModLoader.BuildGlobalHook(ref HookFindFrame, globalNPCs, g => g.FindFrame);
			ModLoader.BuildGlobalHook(ref HookHitEffect, globalNPCs, g => g.HitEffect);
			ModLoader.BuildGlobalHook(ref HookUpdateLifeRegen, globalNPCs, g => g.UpdateLifeRegen);
			ModLoader.BuildGlobalHook(ref HookCheckActive, globalNPCs, g => g.CheckActive);
			ModLoader.BuildGlobalHook(ref HookCheckDead, globalNPCs, g => g.CheckDead);
			ModLoader.BuildGlobalHook(ref HookPreNPCLoot, globalNPCs, g => g.PreNPCLoot);
			ModLoader.BuildGlobalHook(ref HookNPCLoot, globalNPCs, g => g.NPCLoot);
			ModLoader.BuildGlobalHook(ref HookCanHitPlayer, globalNPCs, g => g.CanHitPlayer);
			ModLoader.BuildGlobalHook(ref HookModifyHitPlayer, globalNPCs, g => g.ModifyHitPlayer);
			ModLoader.BuildGlobalHook(ref HookOnHitPlayer, globalNPCs, g => g.OnHitPlayer);
			ModLoader.BuildGlobalHook(ref HookCanHitNPC, globalNPCs, g => g.CanHitNPC);
			ModLoader.BuildGlobalHook(ref HookModifyHitNPC, globalNPCs, g => g.ModifyHitNPC);
			ModLoader.BuildGlobalHook(ref HookOnHitNPC, globalNPCs, g => g.OnHitNPC);
			ModLoader.BuildGlobalHook(ref HookCanBeHitByItem, globalNPCs, g => g.CanBeHitByItem);
			ModLoader.BuildGlobalHook(ref HookModifyHitByItem, globalNPCs, g => g.ModifyHitByItem);
			ModLoader.BuildGlobalHook(ref HookOnHitByItem, globalNPCs, g => g.OnHitByItem);
			ModLoader.BuildGlobalHook(ref HookCanBeHitByProjectile, globalNPCs, g => g.CanBeHitByProjectile);
			ModLoader.BuildGlobalHook(ref HookModifyHitByProjectile, globalNPCs, g => g.ModifyHitByProjectile);
			ModLoader.BuildGlobalHook(ref HookOnHitByProjectile, globalNPCs, g => g.OnHitByProjectile);
			ModLoader.BuildGlobalHook(ref HookStrikeNPC, globalNPCs, g => g.StrikeNPC);
			ModLoader.BuildGlobalHook(ref HookBossHeadSlot, globalNPCs, g => g.BossHeadSlot);
			ModLoader.BuildGlobalHook(ref HookBossHeadRotation, globalNPCs, g => g.BossHeadRotation);
			ModLoader.BuildGlobalHook(ref HookBossHeadSpriteEffects, globalNPCs, g => g.BossHeadSpriteEffects);
			ModLoader.BuildGlobalHook(ref HookGetAlpha, globalNPCs, g => g.GetAlpha);
			ModLoader.BuildGlobalHook(ref HookDrawEffects, globalNPCs, g => g.DrawEffects);
			ModLoader.BuildGlobalHook(ref HookPreDraw, globalNPCs, g => g.PreDraw);
			ModLoader.BuildGlobalHook(ref HookPostDraw, globalNPCs, g => g.PostDraw);
			ModLoader.BuildGlobalHook(ref HookDrawHealthBar, globalNPCs, g => g.DrawHealthBar);
			ModLoader.BuildGlobalHook(ref HookEditSpawnRate, globalNPCs, g => g.EditSpawnRate);
			ModLoader.BuildGlobalHook(ref HookEditSpawnRange, globalNPCs, g => g.EditSpawnRange);
			ModLoader.BuildGlobalHook(ref HookEditSpawnPool, globalNPCs, g => g.EditSpawnPool);
			ModLoader.BuildGlobalHook(ref HookSpawnNPC, globalNPCs, g => g.SpawnNPC);
			ModLoader.BuildGlobalHook(ref HookGetChat, globalNPCs, g => g.GetChat);
			ModLoader.BuildGlobalHook(ref HookSetupShop, globalNPCs, g => g.SetupShop);
			ModLoader.BuildGlobalHook(ref HookSetupTravelShop, globalNPCs, g => g.SetupTravelShop);
			ModLoader.BuildGlobalHook(ref HookBuffTownNPC, globalNPCs, g => g.BuffTownNPC);
			ModLoader.BuildGlobalHook(ref HookTownNPCAttackStrength, globalNPCs, g => g.TownNPCAttackStrength);
			ModLoader.BuildGlobalHook(ref HookTownNPCAttackCooldown, globalNPCs, g => g.TownNPCAttackCooldown);
			ModLoader.BuildGlobalHook(ref HookTownNPCAttackProj, globalNPCs, g => g.TownNPCAttackProj);
			ModLoader.BuildGlobalHook(ref HookTownNPCAttackProjSpeed, globalNPCs, g => g.TownNPCAttackProjSpeed);
			ModLoader.BuildGlobalHook(ref HookTownNPCAttackShoot, globalNPCs, g => g.TownNPCAttackShoot);
			ModLoader.BuildGlobalHook(ref HookTownNPCAttackMagic, globalNPCs, g => g.TownNPCAttackMagic);
			ModLoader.BuildGlobalHook(ref HookTownNPCAttackSwing, globalNPCs, g => g.TownNPCAttackSwing);
			ModLoader.BuildGlobalHook(ref HookDrawTownAttackGun, globalNPCs, g => g.DrawTownAttackGun);
			ModLoader.BuildGlobalHook(ref HookDrawTownAttackSwing, globalNPCs, g => g.DrawTownAttackSwing);

			if (!unloading)
			{
				loaded = true;
			}
		}

		internal static void Unload()
		{
			loaded = false;
			npcs.Clear();
			nextNPC = NPCID.Count;
			globalNPCs.Clear();
			infoList.Clear();
			infoIndexes.Clear();
			bannerToItem.Clear();
			while (NPCID.Sets.Skeletons.Count > vanillaSkeletonCount)
			{
				NPCID.Sets.Skeletons.RemoveAt(NPCID.Sets.Skeletons.Count - 1);
			}
		}

		internal static bool IsModNPC(NPC npc)
		{
			return npc.type >= NPCID.Count;
		}
		//in Terraria.NPC.SetDefaults after if else setting properties call NPCLoader.SetupNPC(this);
		//in Terraria.NPC.SetDefaults move Lang stuff before SetupNPC and replace this.netID with this.type
		internal static void SetupNPC(NPC npc)
		{
			SetupNPCInfo(npc);
			if (IsModNPC(npc))
			{
				GetNPC(npc.type).SetupNPC(npc);
			}
			foreach (var hook in HookSetDefaults)
			{
				hook(npc);
			}
		}

		internal static void SetupNPCInfo(NPC npc)
		{
			npc.npcInfo = infoList.Select(info => info.Clone()).ToArray();
		}

		internal static NPCInfo GetNPCInfo(NPC npc, Mod mod, string name)
		{
			int index;
			return infoIndexes.TryGetValue(mod.Name + ':' + name, out index) ? npc.npcInfo[index] : null;
		}
		//in Terraria.NPC.scaleStats before setting def fields call
		//  NPCLoader.ScaleExpertStats(this, num4, num5);
		public static void ScaleExpertStats(NPC npc, int numPlayers, float bossLifeScale)
		{
			npc.modNPC?.ScaleExpertStats(numPlayers, bossLifeScale);

			foreach (var hook in HookScaleExpertStats)
			{
				hook(npc, numPlayers, bossLifeScale);
			}
		}

		public static void ResetEffects(NPC npc)
		{
			npc.modNPC?.ResetEffects();

			foreach (var hook in HookResetEffects)
			{
				hook(npc);
			}
		}
		//in Terraria.NPC rename AI to VanillaAI then make AI call NPCLoader.NPCAI(this)
		public static void NPCAI(NPC npc)
		{
			if (PreAI(npc))
			{
				int type = npc.type;
				bool useAiType = npc.modNPC != null && npc.modNPC.aiType > 0;
				if (useAiType)
				{
					npc.type = npc.modNPC.aiType;
				}
				npc.VanillaAI();
				if (useAiType)
				{
					npc.type = type;
				}
				AI(npc);
			}
			PostAI(npc);
		}

		public static bool PreAI(NPC npc)
		{
			foreach (var hook in HookPreAI)
			{
				if (!hook(npc))
				{
					return false;
				}
			}
			if (npc.modNPC != null)
			{
				return npc.modNPC.PreAI();
			}
			return true;
		}

		public static void AI(NPC npc)
		{
			npc.modNPC?.AI();

			foreach (var hook in HookAI)
			{
				hook(npc);
			}
		}

		public static void PostAI(NPC npc)
		{
			npc.modNPC?.PostAI();

			foreach (var hook in HookPostAI)
			{
				hook(npc);
			}
		}
		//in Terraria.NetMessage.SendData at end of case 23 call
		//  NPCLoader.SendExtraAI(nPC, writer);
		public static void SendExtraAI(NPC npc, BinaryWriter writer)
		{
			if (npc.modNPC != null)
			{
				byte[] data;
				using (MemoryStream stream = new MemoryStream())
				{
					using (BinaryWriter modWriter = new BinaryWriter(stream))
					{
						npc.modNPC.SendExtraAI(modWriter);
						modWriter.Flush();
						data = stream.ToArray();
					}
				}
				writer.Write((byte)data.Length);
				if (data.Length > 0)
				{
					writer.Write(data);
				}
			}
		}
		//in Terraria.MessageBuffer.GetData at end of case 27 add
		//  NPCLoader.ReceiveExtraAI(nPC, reader);
		public static void ReceiveExtraAI(NPC npc, BinaryReader reader)
		{
			if (npc.modNPC != null)
			{
				byte[] extraAI = reader.ReadBytes(reader.ReadByte());
				if (extraAI.Length > 0)
				{
					using (MemoryStream stream = new MemoryStream(extraAI))
					{
						using (BinaryReader modReader = new BinaryReader(stream))
						{
							npc.modNPC.ReceiveExtraAI(modReader);
						}
					}
				}
			}
		}
		//in Terraria.NPC split VanillaFindFrame from FindFrame and make FindFrame call this
		public static void FindFrame(NPC npc, int frameHeight)
		{
			int type = npc.type;
			if (npc.modNPC != null && npc.modNPC.animationType > 0)
			{
				npc.type = npc.modNPC.animationType;
			}
			npc.VanillaFindFrame(frameHeight);
			npc.type = type;
			npc.modNPC?.FindFrame(frameHeight);

			foreach (var hook in HookFindFrame)
			{
				hook(npc, frameHeight);
			}
		}
		//in Terraria.NPC rename HitEffect to vanillaHitEffect and make HitEffect call this
		public static void HitEffect(NPC npc, int hitDirection, double damage)
		{
			npc.VanillaHitEffect(hitDirection, damage);
			npc.modNPC?.HitEffect(hitDirection, damage);

			foreach (var hook in HookHitEffect)
			{
				hook(npc, hitDirection, damage);
			}
		}

		public static void UpdateLifeRegen(NPC npc, ref int damage)
		{
			npc.modNPC?.UpdateLifeRegen(ref damage);

			foreach (var hook in HookUpdateLifeRegen)
			{
				hook(npc, ref damage);
			}
		}

		public static bool CheckActive(NPC npc)
		{
			if (npc.modNPC != null && !npc.modNPC.CheckActive())
			{
				return false;
			}
			foreach (var hook in HookCheckActive)
			{
				if (!hook(npc))
				{
					return false;
				}
			}
			return true;
		}

		public static bool CheckDead(NPC npc)
		{
			if (npc.modNPC != null && !npc.modNPC.CheckDead())
			{
				return false;
			}
			foreach (var hook in HookCheckDead)
			{
				if (!hook(npc))
				{
					return false;
				}
			}
			return true;
		}
		//in Terraria.NPC.NPCLoot after hardmode meteor head check add
		//  if(!NPCLoader.PreNPCLoot(this)) { return; }
		public static bool PreNPCLoot(NPC npc)
		{
			foreach (var hook in HookPreNPCLoot)
			{
				if (!hook(npc))
				{
					blockLoot.Clear();
					return false;
				}
			}
			if (npc.modNPC != null && !npc.modNPC.PreNPCLoot())
			{
				blockLoot.Clear();
				return false;
			}
			return true;
		}
		//in Terraria.NPC.NPCLoot before heart and star drops add NPCLoader.NPCLoot(this);
		public static void NPCLoot(NPC npc)
		{
			npc.modNPC?.NPCLoot();

			foreach (var hook in HookNPCLoot)
			{
				hook(npc);
			}
			blockLoot.Clear();
		}
		//in Terraria.NPC.NPCLoot after determing potion type call
		//  NPCLoader.BossLoot(this, ref name, ref num70);
		public static void BossLoot(NPC npc, ref string name, ref int potionType)
		{
			npc.modNPC?.BossLoot(ref name, ref potionType);
		}

		//in Terraria.NPC.DropBossBags after if statements setting bag type call
		//  NPCLoader.BossBag(this, ref num);
		public static void BossBag(NPC npc, ref int bagType)
		{
			if (npc.modNPC != null)
			{
				bagType = npc.modNPC.bossBag;
			}
		}
		//in Terraria.Player.Update for damage from NPCs in if statement checking immunities, etc.
		//  add NPCLoader.CanHitPlayer(Main.npc[num249], this, ref num250) &&
		public static bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
		{
			foreach (var hook in HookCanHitPlayer)
			{
				if (!hook(npc, target, ref cooldownSlot))
				{
					return false;
				}
			}
			if (npc.modNPC != null)
			{
				return npc.modNPC.CanHitPlayer(target, ref cooldownSlot);
			}
			return true;
		}
		//in Terraria.Player.Update for damage from NPCs after applying banner buff
		//  add local crit variable and call this
		public static void ModifyHitPlayer(NPC npc, Player target, ref int damage, ref bool crit)
		{
			npc.modNPC?.ModifyHitPlayer(target, ref damage, ref crit);

			foreach (var hook in HookModifyHitPlayer)
			{
				hook(npc, target, ref damage, ref crit);
			}
		}
		//in Terraria.Player.Update for damage from NPCs
		//  assign return value from Player.Hurt to local variable then call this
		public static void OnHitPlayer(NPC npc, Player target, int damage, bool crit)
		{
			npc.modNPC?.OnHitPlayer(target, damage, crit);

			foreach (var hook in HookOnHitPlayer)
			{
				hook(npc, target, damage, crit);
			}
		}
		//Terraria.NPC.UpdateNPC for friendly NPC taking damage (check patch files)
		//Terraria.NPC.AI in aiStyle 7 for detecting threats (check patch files)
		public static bool? CanHitNPC(NPC npc, NPC target)
		{
			bool? flag = null;
			foreach (var hook in HookCanHitNPC)
			{
				bool? canHit = hook(npc, target);
				if (canHit.HasValue && !canHit.Value)
				{
					return false;
				}
				if (canHit.HasValue)
				{
					flag = canHit.Value;
				}
			}
			if (npc.modNPC != null)
			{
				bool? canHit = npc.modNPC.CanHitNPC(target);
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
		//in Terraria.NPC.UpdateNPC for friendly NPC taking damage add local crit variable then call this
		public static void ModifyHitNPC(NPC npc, NPC target, ref int damage, ref float knockback, ref bool crit)
		{
			npc.modNPC?.ModifyHitNPC(target, ref damage, ref knockback, ref crit);

			foreach (var hook in HookModifyHitNPC)
			{
				hook(npc, target, ref damage, ref knockback, ref crit);
			}
		}
		//in Terraria.NPC.UpdateNPC for friendly NPC taking damage before dryad ward call this
		public static void OnHitNPC(NPC npc, NPC target, int damage, float knockback, bool crit)
		{
			npc.modNPC?.OnHitNPC(target, damage, knockback, crit);
			foreach (var hook in HookOnHitNPC)
			{
				hook(npc, target, damage, knockback, crit);
			}
		}
		//in Terraria.Player.ItemCheck call after ItemLoader.CanHitNPC
		public static bool? CanBeHitByItem(NPC npc, Player player, Item item)
		{
			bool? flag = null;
			foreach (var hook in HookCanBeHitByItem)
			{
				bool? canHit = hook(npc, player, item);
				if (canHit.HasValue && !canHit.Value)
				{
					return false;
				}
				if (canHit.HasValue)
				{
					flag = canHit.Value;
				}
			}
			if (npc.modNPC != null)
			{
				bool? canHit = npc.modNPC.CanBeHitByItem(player, item);
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
		//in Terraria.Player.ItemCheck call after ItemLoader.ModifyHitNPC
		public static void ModifyHitByItem(NPC npc, Player player, Item item, ref int damage, ref float knockback, ref bool crit)
		{
			npc.modNPC?.ModifyHitByItem(player, item, ref damage, ref knockback, ref crit);

			foreach (var hook in HookModifyHitByItem)
			{
				hook(npc, player, item, ref damage, ref knockback, ref crit);
			}
		}
		//in Terraria.Player.ItemCheck call after ItemLoader.OnHitNPC
		public static void OnHitByItem(NPC npc, Player player, Item item, int damage, float knockback, bool crit)
		{
			npc.modNPC?.OnHitByItem(player, item, damage, knockback, crit);

			foreach (var hook in HookOnHitByItem)
			{
				hook(npc, player, item, damage, knockback, crit);
			}
		}
		//in Terraria.Projectile.Damage call after ProjectileLoader.CanHitNPC
		public static bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
		{
			bool? flag = null;
			foreach (var hook in HookCanBeHitByProjectile)
			{
				bool? canHit = hook(npc, projectile);
				if (canHit.HasValue && !canHit.Value)
				{
					return false;
				}
				if (canHit.HasValue)
				{
					flag = canHit.Value;
				}
			}
			if (npc.modNPC != null)
			{
				bool? canHit = npc.modNPC.CanBeHitByProjectile(projectile);
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
		//in Terraria.Projectile.Damage call after ProjectileLoader.ModifyHitNPC
		public static void ModifyHitByProjectile(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			npc.modNPC?.ModifyHitByProjectile(projectile, ref damage, ref knockback, ref crit, ref hitDirection);

			foreach (var hook in HookModifyHitByProjectile)
			{
				hook(npc, projectile, ref damage, ref knockback, ref crit, ref hitDirection);
			}
		}
		//in Terraria.Projectile.Damage call after ProjectileLoader.OnHitNPC
		public static void OnHitByProjectile(NPC npc, Projectile projectile, int damage, float knockback, bool crit)
		{
			npc.modNPC?.OnHitByProjectile(projectile, damage, knockback, crit);

			foreach (var hook in HookOnHitByProjectile)
			{
				hook(npc, projectile, damage, knockback, crit);
			}
		}
		//in Terraria.NPC.StrikeNPC place modifications to num in if statement checking this
		public static bool StrikeNPC(NPC npc, ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit)
		{
			bool flag = true;
			if (npc.modNPC != null)
			{
				flag = npc.modNPC.StrikeNPC(ref damage, defense, ref knockback, hitDirection, ref crit);
			}
			foreach (var hook in HookStrikeNPC)
			{
				if (!hook(npc, ref damage, defense, ref knockback, hitDirection, ref crit))
				{
					flag = false;
				}
			}
			return flag;
		}
		//in Terraria.NPC.GetBossHeadTextureIndex call this before returning
		public static void BossHeadSlot(NPC npc, ref int index)
		{
			npc.modNPC?.BossHeadSlot(ref index);

			foreach (var hook in HookBossHeadSlot)
			{
				hook(npc, ref index);
			}
		}
		//in Terraria.NPC.GetBossHeadRotation call this before returning
		public static void BossHeadRotation(NPC npc, ref float rotation)
		{
			npc.modNPC?.BossHeadRotation(ref rotation);

			foreach (var hook in HookBossHeadRotation)
			{
				hook(npc, ref rotation);
			}
		}
		//in Terraria.NPC.GetBossHeadSpriteEffects call this before returning
		public static void BossHeadSpriteEffects(NPC npc, ref SpriteEffects spriteEffects)
		{
			npc.modNPC?.BossHeadSpriteEffects(ref spriteEffects);

			foreach (var hook in HookBossHeadSpriteEffects)
			{
				hook(npc, ref spriteEffects);
			}
		}
		//at beginning of Terraria.NPC.GetAlpha add
		//  Color? modColor = NPCLoader.GetAlpha(this, new Color); if(modColor.HasValue) { return modColor.Value; }
		public static Color? GetAlpha(NPC npc, Color lightColor)
		{
			foreach (var hook in HookGetAlpha)
			{
				Color? color = hook(npc, lightColor);
				if (color.HasValue)
				{
					return color.Value;
				}
			}
			return npc.modNPC?.GetAlpha(lightColor);
		}

		public static void DrawEffects(NPC npc, ref Color drawColor)
		{
			npc.modNPC?.DrawEffects(ref drawColor);

			foreach (var hook in HookDrawEffects)
			{
				hook(npc, ref drawColor);
			}
		}
		//in Terraria.Main.DrawNPC after modifying draw color add
		//  if(!NPCLoader.PreDraw(Main.npc[i], Main.spriteBatch, color9))
		//  { NPCLoader.PostDraw(Main.npc[k], Main.spriteBatch, color9); return; }
		public static bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor)
		{
			foreach (var hook in HookPreDraw)
			{
				if (!hook(npc, spriteBatch, drawColor))
				{
					return false;
				}
			}
			if (npc.modNPC != null)
			{
				return npc.modNPC.PreDraw(spriteBatch, drawColor);
			}
			return true;
		}
		//call this at end of Terraria.Main.DrawNPC
		public static void PostDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor)
		{
			npc.modNPC?.PostDraw(spriteBatch, drawColor);

			foreach (var hook in HookPostDraw)
			{
				hook(npc, spriteBatch, drawColor);
			}
		}

		public static bool DrawHealthBar(NPC npc, ref float scale)
		{
			Vector2 position = new Vector2(npc.position.X + npc.width / 2, npc.position.Y + npc.gfxOffY);
			if (Main.HealthBarDrawSettings == 1)
			{
				position.Y += npc.height + 10f + Main.NPCAddHeight(npc.whoAmI);
			}
			else if (Main.HealthBarDrawSettings == 2)
			{
				position.Y -= 24f + Main.NPCAddHeight(npc.whoAmI) / 2f;
			}
			foreach (var hook in HookDrawHealthBar)
			{
				bool? result = hook(npc, Main.HealthBarDrawSettings, ref scale, ref position);
				if (result.HasValue)
				{
					if (result.Value)
					{
						DrawHealthBar(npc, position, scale);
					}
					return false;
				}
			}
			if (NPCLoader.IsModNPC(npc))
			{
				bool? result = npc.modNPC.DrawHealthBar(Main.HealthBarDrawSettings, ref scale, ref position);
				if (result.HasValue)
				{
					if (result.Value)
					{
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
		//in Terraria.NPC.SpawnNPC after modifying NPC.spawnRate and NPC.maxSpawns call
		//  NPCLoader.EditSpawnRate(Main.player[j], ref NPC.spawnRate, ref NPC.maxSpawns);
		public static void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
		{
			foreach (var hook in HookEditSpawnRate)
			{
				hook(player, ref spawnRate, ref maxSpawns);
			}
		}
		//in Terraria.NPC.SpawnNPC after modifying spawn ranges call
		//  NPCLoader.EditSpawnRange(Main.player[j], ref NPC.spawnRangeX, ref NPC.spawnRangeY,
		//  ref NPC.safeRangeX, ref NPC.safeRangeY);
		public static void EditSpawnRange(Player player, ref int spawnRangeX, ref int spawnRangeY,
			ref int safeRangeX, ref int safeRangeY)
		{
			foreach (var hook in HookEditSpawnRange)
			{
				hook(player, ref spawnRangeX, ref spawnRangeY, ref safeRangeX, ref safeRangeY);
			}
		}
		//in Terraria.NPC.SpawnNPC after initializing variables and before actual spawning add
		//  int? spawnChoice = NPCLoader.ChooseSpawn(spawnInfo); if(!spawnChoice.HasValue) { return; }
		//  int spawn = spawnChoice.Value; if(spawn != 0) { goto endVanillaSpawn; }
		public static int? ChooseSpawn(NPCSpawnInfo spawnInfo)
		{
			NPCSpawnHelper.Reset();
			NPCSpawnHelper.DoChecks(spawnInfo);
			IDictionary<int, float> pool = new Dictionary<int, float>();
			pool[0] = 1f;
			foreach (ModNPC npc in npcs)
			{
				float weight = npc.CanSpawn(spawnInfo);
				if (weight > 0f)
				{
					pool[npc.npc.type] = weight;
				}
			}
			foreach (var hook in HookEditSpawnPool)
			{
				hook(pool, spawnInfo);
			}
			float totalWeight = 0f;
			foreach (int type in pool.Keys)
			{
				totalWeight += pool[type];
			}
			float choice = (float)Main.rand.NextDouble() * totalWeight;
			foreach (int type in pool.Keys)
			{
				float weight = pool[type];
				if (choice < weight)
				{
					return type;
				}
				choice -= weight;
			}
			return null;
		}
		//in Terraria.NPC.SpawnNPC before spawning pinky add
		//  endVanillaSpawn: if(spawn != 0) { num46 = NPCLoader.SpawnNPC(spawn, num, num2); }
		public static int SpawnNPC(int type, int tileX, int tileY)
		{
			var npc = type >= NPCID.Count ? 
				GetNPC(type).SpawnNPC(tileX, tileY) : 
				NPC.NewNPC(tileX * 16 + 8, tileY * 16, type);

			foreach (var hook in HookSpawnNPC)
			{
				hook(npc, tileX, tileY);
			}

			return npc;
		}
		//at end of Terraria.Main.UpdateTime inside blocks add NPCLoader.CanTownNPCSpawn(num40, num42);
		public static void CanTownNPCSpawn(int numTownNPCs, int money)
		{
			foreach (ModNPC npc in npcs)
			{
				if (npc.npc.townNPC && NPC.TypeToHeadIndex(npc.npc.type) >= 0 && !NPC.AnyNPCs(npc.npc.type) &&
					npc.CanTownNPCSpawn(numTownNPCs, money))
				{
					Main.townNPCCanSpawn[npc.npc.type] = true;
					if (WorldGen.prioritizedTownNPC == 0)
					{
						WorldGen.prioritizedTownNPC = npc.npc.type;
					}
				}
			}
		}
		//at beginning of Terraria.WorldGen.CheckConditions add
		//  if(!NPCLoader.CheckConditions(type)) { return false; }
		public static bool CheckConditions(int type)
		{
			return GetNPC(type)?.CheckConditions(WorldGen.roomX1, WorldGen.roomX2, WorldGen.roomY1, WorldGen.roomY2) ?? true;
		}
		//in Terraria.NPC.getNewNPCName replace final return with return NPCLoader.TownNPCName(npcType);
		public static string TownNPCName(int type)
		{
			return GetNPC(type)?.TownNPCName() ?? "";
		}

		public static bool UsesPartyHat(NPC npc)
		{
			return npc.modNPC?.UsesPartyHat() ?? true;
		}
		//in Terraria.NPC.GetChat before returning result add NPCLoader.GetChat(this, ref result);
		public static void GetChat(NPC npc, ref string chat)
		{
			if (npc.modNPC != null)
			{
				chat = npc.modNPC.GetChat();
			}
			foreach (var hook in HookGetChat)
			{
				hook(npc, ref chat);
			}
		}
		//in Terraria.Main.GUIChatDrawInner after if/else chain setting text and text2 call
		//  NPCLoader.SetChatButtons(ref text, ref text2);
		public static void SetChatButtons(ref string button, ref string button2)
		{
			if (Main.player[Main.myPlayer].talkNPC >= 0)
			{
				NPC npc = Main.npc[Main.player[Main.myPlayer].talkNPC];
				npc.modNPC?.SetChatButtons(ref button, ref button2);
			}
		}
		//add 1 to Terraria.Main.numShops
		//in Terraria.Main.GUIChatDrawInner after type checks for first button click call
		//  NPCLoader.OnChatButtonClicked(true);
		//  after type checks for second button click call NPCLoader.OnChatButtonClicked(false);
		public static void OnChatButtonClicked(bool firstButton)
		{
			NPC npc = Main.npc[Main.player[Main.myPlayer].talkNPC];
			if (npc.modNPC != null)
			{
				bool shop = false;
				npc.modNPC.OnChatButtonClicked(firstButton, ref shop);
				Main.PlaySound(12, -1, -1, 1);
				if (shop)
				{
					Main.playerInventory = true;
					Main.npcChatText = "";
					Main.npcShop = Main.MaxShopIDs - 1;
					Main.instance.shop[Main.npcShop].SetupShop(npc.type);
				}
			}
		}
		//in Terraria.Chest.SetupShop before discount call NPCLoader.SetupShop(type, this, ref num);
		public static void SetupShop(int type, Chest shop, ref int nextSlot)
		{
			if (type < shopToNPC.Length)
			{
				type = shopToNPC[type];
			}
			else
			{
				GetNPC(type)?.SetupShop(shop, ref nextSlot);
			}
			foreach (var hook in HookSetupShop)
			{
				hook(type, shop, ref nextSlot);
			}
		}
		//at end of Terraria.Chest.SetupTravelShop call NPCLoader.SetupTravelShop(Main.travelShop, ref num2);
		public static void SetupTravelShop(int[] shop, ref int nextSlot)
		{
			foreach (var hook in HookSetupTravelShop)
			{
				hook(shop, ref nextSlot);
			}
		}
		//in Terraria.NPC.AI in aiStyle 7 after buffing damage multiplier and defense add
		//  NPCLoader.BuffTownNPC(ref num378, ref this.defense);
		public static void BuffTownNPC(ref float damageMult, ref int defense)
		{
			foreach (var hook in HookBuffTownNPC)
			{
				hook(ref damageMult, ref defense);
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
		public static void TownNPCAttackStrength(NPC npc, ref int damage, ref float knockback)
		{
			npc.modNPC?.TownNPCAttackStrength(ref damage, ref knockback);

			foreach (var hook in HookTownNPCAttackStrength)
			{
				hook(npc, ref damage, ref knockback);
			}
		}

		public static void TownNPCAttackCooldown(NPC npc, ref int cooldown, ref int randExtraCooldown)
		{
			npc.modNPC?.TownNPCAttackCooldown(ref cooldown, ref randExtraCooldown);

			foreach (var hook in HookTownNPCAttackCooldown)
			{
				hook(npc, ref cooldown, ref randExtraCooldown);
			}
		}

		public static void TownNPCAttackProj(NPC npc, ref int projType, ref int attackDelay)
		{
			npc.modNPC?.TownNPCAttackProj(ref projType, ref attackDelay);

			foreach (var hook in HookTownNPCAttackProj)
			{
				hook(npc, ref projType, ref attackDelay);
			}
		}

		public static void TownNPCAttackProjSpeed(NPC npc, ref float multiplier, ref float gravityCorrection,
			ref float randomOffset)
		{
			npc.modNPC?.TownNPCAttackProjSpeed(ref multiplier, ref gravityCorrection, ref randomOffset);

			foreach (var hook in HookTownNPCAttackProjSpeed)
			{
				hook(npc, ref multiplier, ref gravityCorrection, ref randomOffset);
			}
		}

		public static void TownNPCAttackShoot(NPC npc, ref bool inBetweenShots)
		{
			npc.modNPC?.TownNPCAttackShoot(ref inBetweenShots);

			foreach (var hook in HookTownNPCAttackShoot)
			{
				hook(npc, ref inBetweenShots);
			}
		}

		public static void TownNPCAttackMagic(NPC npc, ref float auraLightMultiplier)
		{
			npc.modNPC?.TownNPCAttackMagic(ref auraLightMultiplier);

			foreach (var hook in HookTownNPCAttackMagic)
			{
				hook(npc, ref auraLightMultiplier);
			}
		}

		public static void TownNPCAttackSwing(NPC npc, ref int itemWidth, ref int itemHeight)
		{
			npc.modNPC?.TownNPCAttackSwing(ref itemWidth, ref itemHeight);

			foreach (var hook in HookTownNPCAttackSwing)
			{
				hook(npc, ref itemWidth, ref itemHeight);
			}
		}
		//in Terraria.Main.DrawNPCExtras for attack type 1 after if else chain setting num2-4 call
		//  NPCLoader.DrawTownAttackGun(n, ref num2, ref num3, ref num4);
		public static void DrawTownAttackGun(NPC npc, ref float scale, ref int item, ref int closeness)
		{
			npc.modNPC?.DrawTownAttackGun(ref scale, ref item, ref closeness);

			foreach (var hook in HookDrawTownAttackGun)
			{
				hook(npc, ref scale, ref item, ref closeness);
			}
		}
		//in Terraria.Main.DrawNPCExtras for attack type 3 after if else chain call
		//  NPCLoader.DrawTownAttackSwing(n, ref texture2D5, ref num6, ref scaleFactor, ref zero);
		public static void DrawTownAttackSwing(NPC npc, ref Texture2D item, ref int itemSize, ref float scale, ref Vector2 offset)
		{
			npc.modNPC?.DrawTownAttackSwing(ref item, ref itemSize, ref scale, ref offset);

			foreach (var hook in HookDrawTownAttackSwing)
			{
				hook(npc, ref item, ref itemSize, ref scale, ref offset);
			}
		}
	}
}
