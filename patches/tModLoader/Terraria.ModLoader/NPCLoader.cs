using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;

namespace Terraria.ModLoader
{
	public static class NPCLoader
	{
		private static int nextNPC = NPCID.Count;
		internal static readonly IDictionary<int, ModNPC> npcs = new Dictionary<int, ModNPC>();
		internal static readonly IList<GlobalNPC> globalNPCs = new List<GlobalNPC>();
		private static int vanillaSkeletonCount = NPCID.Sets.Skeletons.Count;
		private static readonly int[] shopToNPC = new int[Main.numShops - 1];
		//in Terraria.Item.NewItem after setting Main.item[400] add
		//  if(NPCLoader.blockLoot.Contains(Type)) { return num; }
		public static readonly IList<int> blockLoot = new List<int>();

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
		}

		internal static int ReserveNPCID()
		{
			int reserveID = nextNPC;
			nextNPC++;
			return reserveID;
		}

		internal static int NPCCount()
		{
			return nextNPC;
		}

		public static ModNPC GetNPC(int type)
		{
			if (npcs.ContainsKey(type))
			{
				return npcs[type];
			}
			else
			{
				return null;
			}
		}
		//change initial size of Terraria.Player.npcTypeNoAggro and NPCBannerBuff to NPCLoader.NPCCount()
		//in Terraria.Main.MouseText replace 251 with NPCLoader.NPCCount()
		//in Terraria.Main.DrawNPCs and Terraria.NPC.NPCLoot remove type too high check
		//replace a lot of 540 immediates
		//in Terraria.GameContent.UI.EmoteBubble make CountNPCs internal
		internal static void ResizeArrays()
		{
			Array.Resize(ref Main.NPCLoaded, nextNPC);
			Array.Resize(ref Main.nextNPC, nextNPC);
			Array.Resize(ref Main.slimeRainNPC, nextNPC);
			Array.Resize(ref Main.npcTexture, nextNPC);
			Array.Resize(ref Main.npcCatchable, nextNPC);
			Array.Resize(ref Main.npcName, nextNPC);
			Array.Resize(ref Main.npcFrameCount, nextNPC);
			Array.Resize(ref NPC.killCount, nextNPC);
			Array.Resize(ref EmoteBubble.CountNPCs, nextNPC);
			Array.Resize(ref NPCID.Sets.NeedsExpertScaling, nextNPC);
			Array.Resize(ref NPCID.Sets.ProjectileNPC, nextNPC);
			Array.Resize(ref NPCID.Sets.SavesAndLoads, nextNPC);
			Array.Resize(ref NPCID.Sets.TrailCacheLength, nextNPC);
			Array.Resize(ref NPCID.Sets.MPAllowedEnemies, nextNPC);
			Array.Resize(ref NPCID.Sets.TownCritter, nextNPC);
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
			for (int k = NPCID.Count; k < nextNPC; k++)
			{
				Main.NPCLoaded[k] = true;
				Main.npcFrameCount[k] = 1;
				NPCID.Sets.TrailCacheLength[k] = 10;
				NPCID.Sets.DangerDetectRange[k] = -1;
				NPCID.Sets.AttackTime[k] = -1;
				NPCID.Sets.AttackAverageChance[k] = 1;
				NPCID.Sets.AttackType[k] = -1;
				NPCID.Sets.PrettySafe[k] = -1;
				NPCID.Sets.MagicAuraColor[k] = Color.White;
				NPCID.Sets.BossHeadTextures[k] = -1;
			}
		}

		internal static void Unload()
		{
			npcs.Clear();
			nextNPC = NPCID.Count;
			globalNPCs.Clear();
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
			if (IsModNPC(npc))
			{
				GetNPC(npc.type).SetupNPC(npc);
			}
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.SetDefaults(npc);
			}
		}
		//at beginning of Terraria.Lang.npcName add
		//  if (l >= Main.maxNPCTypes) { return NPCLoader.DisplayName(l); }
		internal static string DisplayName(int type)
		{
			ModNPC npc = GetNPC(type);
			string name = "";
			if (npc != null)
			{
				if (npc.npc.displayName != null)
				{
					name = npc.npc.displayName;
				}
				if (name == "" && npc.npc.name != null)
				{
					name = npc.npc.name;
				}
			}
			if (name == "" && Main.npcName[type] != null)
			{
				name = Main.npcName[type];
			}
			return name;
		}
		//in Terraria.NPC.scaleStats before setting def fields call
		//  NPCLoader.ScaleExpertStats(this, num4, num5);
		internal static void ScaleExpertStats(NPC npc, int numPlayers, float bossLifeScale)
		{
			if (IsModNPC(npc))
			{
				npc.modNPC.ScaleExpertStats(numPlayers, bossLifeScale);
			}
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.ScaleExpertStats(npc, numPlayers, bossLifeScale);
			}
		}
		//in Terraria.NPC rename AI to VanillaAI then make AI call NPCLoader.NPCAI(this)
		internal static void NPCAI(NPC npc)
		{
			if (PreAI(npc))
			{
				int type = npc.type;
				bool useAiType = IsModNPC(npc) && npc.modNPC.aiType > 0;
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

		internal static bool PreAI(NPC npc)
		{
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				if (!globalNPC.PreAI(npc))
				{
					return false;
				}
			}
			if (IsModNPC(npc))
			{
				return npc.modNPC.PreAI();
			}
			return true;
		}

		internal static void AI(NPC npc)
		{
			if (IsModNPC(npc))
			{
				npc.modNPC.AI();
			}
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.AI(npc);
			}
		}

		internal static void PostAI(NPC npc)
		{
			if (IsModNPC(npc))
			{
				npc.modNPC.PostAI();
			}
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.PostAI(npc);
			}
		}
		//in Terraria.NetMessage.SendData at end of case 23 call
		//  NPCLoader.SendExtraAI(nPC, writer);
		internal static void SendExtraAI(NPC npc, BinaryWriter writer)
		{
			if (IsModNPC(npc))
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
		internal static void ReceiveExtraAI(NPC npc, BinaryReader reader)
		{
			byte[] extraAI = reader.ReadBytes(reader.ReadByte());
			if (extraAI.Length > 0 && IsModNPC(npc))
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
		//in Terraria.NPC split VanillaFindFrame from FindFrame and make FindFrame call this
		internal static void FindFrame(NPC npc, int frameHeight)
		{
			int type = npc.type;
			if (IsModNPC(npc) && npc.modNPC.animationType > 0)
			{
				npc.type = npc.modNPC.animationType;
			}
			npc.VanillaFindFrame(frameHeight);
			npc.type = type;
			if (IsModNPC(npc))
			{
				npc.modNPC.FindFrame(frameHeight);
			}
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.FindFrame(npc, frameHeight);
			}
		}
		//in Terraria.NPC rename HitEffect to vanillaHitEffect and make HitEffect call this
		internal static void HitEffect(NPC npc, int hitDirection, double damage)
		{
			npc.VanillaHitEffect(hitDirection, damage);
			if (IsModNPC(npc))
			{
				npc.modNPC.HitEffect(hitDirection, damage);
			}
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.HitEffect(npc, hitDirection, damage);
			}
		}
		//in Terraria.NPC.NPCLoot after hardmode meteor head check add
		//  if(!NPCLoader.PreNPCLoot(this)) { return; }
		internal static bool PreNPCLoot(NPC npc)
		{
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				if (!globalNPC.PreNPCLoot(npc))
				{
					blockLoot.Clear();
					return false;
				}
			}
			if (IsModNPC(npc) && !npc.modNPC.PreNPCLoot())
			{
				blockLoot.Clear();
				return false;
			}
			return true;
		}
		//in Terraria.NPC.NPCLoot before heart and star drops add NPCLoader.NPCLoot(this);
		internal static void NPCLoot(NPC npc)
		{
			if (IsModNPC(npc))
			{
				npc.modNPC.NPCLoot();
			}
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.NPCLoot(npc);
			}
			blockLoot.Clear();
		}
		//in Terraria.NPC.NPCLoot after determing potion type call
		//  NPCLoader.BossLoot(this, ref name, ref num70);
		internal static void BossLoot(NPC npc, ref string name, ref int potionType)
		{
			if (IsModNPC(npc))
			{
				npc.modNPC.BossLoot(ref name, ref potionType);
			}
		}
		//in Terraria.NPC.DropBossBags after if statements setting bag type call
		//  NPCLoader.BossBag(this, ref num);
		internal static void BossBag(NPC npc, ref int bagType)
		{
			if (IsModNPC(npc))
			{
				bagType = npc.modNPC.bossBag;
			}
		}
		//in Terraria.Player.Update for damage from NPCs in if statement checking immunities, etc.
		//  add NPCLoader.CanHitPlayer(Main.npc[num249], this, ref num250) &&
		internal static bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
		{
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				if (!globalNPC.CanHitPlayer(npc, target, ref cooldownSlot))
				{
					return false;
				}
			}
			if (IsModNPC(npc))
			{
				return npc.modNPC.CanHitPlayer(target, ref cooldownSlot);
			}
			return true;
		}
		//in Terraria.Player.Update for damage from NPCs after applying banner buff
		//  add local crit variable and call this
		internal static void ModifyHitPlayer(NPC npc, Player target, ref int damage, ref bool crit)
		{
			if (IsModNPC(npc))
			{
				npc.modNPC.ModifyHitPlayer(target, ref damage, ref crit);
			}
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.ModifyHitPlayer(npc, target, ref damage, ref crit);
			}
		}
		//in Terraria.Player.Update for damage from NPCs
		//  assign return value from Player.Hurt to local variable then call this
		internal static void OnHitPlayer(NPC npc, Player target, int damage, bool crit)
		{
			if (IsModNPC(npc))
			{
				npc.modNPC.OnHitPlayer(target, damage, crit);
			}
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.OnHitPlayer(npc, target, damage, crit);
			}
		}
		//Terraria.NPC.UpdateNPC for friendly NPC taking damage (check patch files)
		//Terraria.NPC.AI in aiStyle 7 for detecting threats (check patch files)
		internal static bool? CanHitNPC(NPC npc, NPC target)
		{
			bool? flag = null;
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				bool? canHit = globalNPC.CanHitNPC(npc, target);
				if (canHit.HasValue && !canHit.Value)
				{
					return false;
				}
				if (canHit.HasValue)
				{
					flag = canHit.Value;
				}
			}
			if (IsModNPC(npc))
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
		internal static void ModifyHitNPC(NPC npc, NPC target, ref int damage, ref float knockback, ref bool crit)
		{
			if (IsModNPC(npc))
			{
				npc.modNPC.ModifyHitNPC(target, ref damage, ref knockback, ref crit);
			}
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.ModifyHitNPC(npc, target, ref damage, ref knockback, ref crit);
			}
		}
		//in Terraria.NPC.UpdateNPC for friendly NPC taking damage before dryad ward call this
		internal static void OnHitNPC(NPC npc, NPC target, int damage, float knockback, bool crit)
		{
			if (IsModNPC(npc))
			{
				npc.modNPC.OnHitNPC(target, damage, knockback, crit);
			}
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.OnHitNPC(npc, target, damage, knockback, crit);
			}
		}
		//in Terraria.Player.ItemCheck call after ItemLoader.CanHitNPC
		internal static bool? CanBeHitByItem(NPC npc, Player player, Item item)
		{
			bool? flag = null;
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				bool? canHit = globalNPC.CanBeHitByItem(npc, player, item);
				if (canHit.HasValue && !canHit.Value)
				{
					return false;
				}
				if (canHit.HasValue)
				{
					flag = canHit.Value;
				}
			}
			if (IsModNPC(npc))
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
		internal static void ModifyHitByItem(NPC npc, Player player, Item item, ref int damage, ref float knockback, ref bool crit)
		{
			if (IsModNPC(npc))
			{
				npc.modNPC.ModifyHitByItem(player, item, ref damage, ref knockback, ref crit);
			}
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.ModifyHitByItem(npc, player, item, ref damage, ref knockback, ref crit);
			}
		}
		//in Terraria.Player.ItemCheck call after ItemLoader.OnHitNPC
		internal static void OnHitByItem(NPC npc, Player player, Item item, int damage, float knockback, bool crit)
		{
			if (IsModNPC(npc))
			{
				npc.modNPC.OnHitByItem(player, item, damage, knockback, crit);
			}
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.OnHitByItem(npc, player, item, damage, knockback, crit);
			}
		}
		//in Terraria.Projectile.Damage call after ProjectileLoader.CanHitNPC
		internal static bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
		{
			bool? flag = null;
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				bool? canHit = globalNPC.CanBeHitByProjectile(npc, projectile);
				if (canHit.HasValue && !canHit.Value)
				{
					return false;
				}
				if (canHit.HasValue)
				{
					flag = canHit.Value;
				}
			}
			if (IsModNPC(npc))
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
		internal static void ModifyHitByProjectile(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit)
		{
			if (IsModNPC(npc))
			{
				npc.modNPC.ModifyHitByProjectile(projectile, ref damage, ref knockback, ref crit);
			}
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.ModifyHitByProjectile(npc, projectile, ref damage, ref knockback, ref crit);
			}
		}
		//in Terraria.Projectile.Damage call after ProjectileLoader.OnHitNPC
		internal static void OnHitByProjectile(NPC npc, Projectile projectile, int damage, float knockback, bool crit)
		{
			if (IsModNPC(npc))
			{
				npc.modNPC.OnHitByProjectile(projectile, damage, knockback, crit);
			}
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.OnHitByProjectile(npc, projectile, damage, knockback, crit);
			}
		}
		//in Terraria.NPC.StrikeNPC place modifications to num in if statement checking this
		internal static bool StrikeNPC(NPC npc, ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit)
		{
			bool flag = true;
			if (IsModNPC(npc))
			{
				flag = npc.modNPC.StrikeNPC(ref damage, defense, ref knockback, hitDirection, ref crit);
			}
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				if (!globalNPC.StrikeNPC(npc, ref damage, defense, ref knockback, hitDirection, ref crit))
				{
					flag = false;
				}
			}
			return flag;
		}
		//in Terraria.NPC.GetBossHeadTextureIndex call this before returning
		internal static void BossHeadSlot(NPC npc, ref int index)
		{
			if (IsModNPC(npc))
			{
				npc.modNPC.BossHeadSlot(ref index);
			}
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.BossHeadSlot(npc, ref index);
			}
		}
		//in Terraria.NPC.GetBossHeadRotation call this before returning
		internal static void BossHeadRotation(NPC npc, ref float rotation)
		{
			if (IsModNPC(npc))
			{
				npc.modNPC.BossHeadRotation(ref rotation);
			}
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.BossHeadRotation(npc, ref rotation);
			}
		}
		//in Terraria.NPC.GetBossHeadSpriteEffects call this before returning
		internal static void BossHeadSpriteEffects(NPC npc, ref SpriteEffects spriteEffects)
		{
			if (IsModNPC(npc))
			{
				npc.modNPC.BossHeadSpriteEffects(ref spriteEffects);
			}
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.BossHeadSpriteEffects(npc, ref spriteEffects);
			}
		}
		//at beginning of Terraria.NPC.GetAlpha add
		//  Color? modColor = NPCLoader.GetAlpha(this, new Color); if(modColor.HasValue) { return modColor.Value; }
		internal static Color? GetAlpha(NPC npc, Color lightColor)
		{
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				Color? color = globalNPC.GetAlpha(npc, lightColor);
				if (color.HasValue)
				{
					return color.Value;
				}
			}
			if (IsModNPC(npc))
			{
				return npc.modNPC.GetAlpha(lightColor);
			}
			return null;
		}
		//in Terraria.Main.DrawNPC after modifying draw color add
		//  if(!NPCLoader.PreDraw(Main.npc[i], Main.spriteBatch, color9))
		//  { NPCLoader.PostDraw(Main.npc[k], Main.spriteBatch, color9); return; }
		internal static bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor)
		{
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				if (!globalNPC.PreDraw(npc, spriteBatch, drawColor))
				{
					return false;
				}
			}
			if (IsModNPC(npc))
			{
				return npc.modNPC.PreDraw(spriteBatch, drawColor);
			}
			return true;
		}
		//call this at end of Terraria.Main.DrawNPC
		internal static void PostDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor)
		{
			if (IsModNPC(npc))
			{
				npc.modNPC.PostDraw(spriteBatch, drawColor);
			}
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.PostDraw(npc, spriteBatch, drawColor);
			}
		}
		//in Terraria.NPC.SpawnNPC after modifying NPC.spawnRate and NPC.maxSpawns call
		//  NPCLoader.EditSpawnRate(Main.player[j], ref NPC.spawnRate, ref NPC.maxSpawns);
		internal static void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
		{
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.EditSpawnRate(player, ref spawnRate, ref maxSpawns);
			}
		}
		//in Terraria.NPC.SpawnNPC after modifying spawn ranges call
		//  NPCLoader.EditSpawnRange(Main.player[j], ref NPC.spawnRangeX, ref NPC.spawnRangeY,
		//  ref NPC.safeRangeX, ref NPC.safeRangeY);
		internal static void EditSpawnRange(Player player, ref int spawnRangeX, ref int spawnRangeY,
			ref int safeRangeX, ref int safeRangeY)
		{
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.EditSpawnRange(player, ref spawnRangeX, ref spawnRangeY, ref safeRangeX, ref safeRangeY);
			}
		}
		//in Terraria.NPC.SpawnNPC after initializing variables and before actual spawning add
		//  int? spawnChoice = NPCLoader.ChooseSpawn(spawnInfo); if(!spawnChoice.HasValue) { return; }
		//  int spawn = spawnChoice.Value; if(spawn != 0) { goto endVanillaSpawn; }
		internal static int? ChooseSpawn(NPCSpawnInfo spawnInfo)
		{
			IDictionary<int, float> pool = new Dictionary<int, float>();
			pool[0] = 1f;
			foreach (ModNPC npc in npcs.Values)
			{
				float weight = npc.CanSpawn(spawnInfo);
				if (weight > 0f)
				{
					pool[npc.npc.type] = weight;
				}
			}
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.EditSpawnPool(pool, spawnInfo);
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
		internal static int SpawnNPC(int type, int tileX, int tileY)
		{
			int npc;
			if (type >= NPCID.Count)
			{
				npc = NPCLoader.npcs[type].SpawnNPC(tileX, tileY);
			}
			else
			{
				npc = NPC.NewNPC(tileX * 16 + 8, tileY * 16, type);
			}
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.SpawnNPC(npc, tileX, tileY);
			}
			return npc;
		}
		//at end of Terraria.Main.UpdateTime inside blocks add NPCLoader.CanTownNPCSpawn(num40, num42);
		internal static void CanTownNPCSpawn(int numTownNPCs, int money)
		{
			foreach (ModNPC npc in npcs.Values)
			{
				if (npc.npc.townNPC && NPC.TypeToNum(npc.npc.type) >= 0 && !NPC.AnyNPCs(npc.npc.type) &&
				    npc.CanTownNPCSpawn(numTownNPCs, money))
				{
					Main.nextNPC[npc.npc.type] = true;
					if (WorldGen.spawnNPC == 0)
					{
						WorldGen.spawnNPC = npc.npc.type;
					}
				}
			}
		}
		//at beginning of Terraria.WorldGen.CheckConditions add
		//  if(!NPCLoader.CheckConditions(type)) { return false; }
		internal static bool CheckConditions(int type)
		{
			if (type < NPCID.Count)
			{
				return true;
			}
			return GetNPC(type).CheckConditions(WorldGen.roomX1, WorldGen.roomX2, WorldGen.roomY1, WorldGen.roomY2);
		}
		//in Terraria.NPC.getNewNPCName replace final return with return NPCLoader.TownNPCName(npcType);
		internal static string TownNPCName(int type)
		{
			if (type < NPCID.Count)
			{
				return "";
			}
			return GetNPC(type).TownNPCName();
		}
		//in Terraria.NPC.GetChat before returning result add NPCLoader.GetChat(this, ref result);
		internal static void GetChat(NPC npc, ref string chat)
		{
			if (IsModNPC(npc))
			{
				chat = npc.modNPC.GetChat();
			}
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.GetChat(npc, ref chat);
			}
		}
		//in Terraria.Main.GUIChatDrawInner after if/else chain setting text and text2 call
		//  NPCLoader.SetChatButtons(ref text, ref text2);
		internal static void SetChatButtons(ref string button, ref string button2)
		{
			NPC npc = Main.npc[Main.player[Main.myPlayer].talkNPC];
			if (IsModNPC(npc))
			{
				npc.modNPC.SetChatButtons(ref button, ref button2);
			}
		}
		//add 1 to Terraria.Main.numShops
		//in Terraria.Main.GUIChatDrawInner after type checks for first button click call
		//  NPCLoader.OnChatButtonClicked(true);
		//  after type checks for second button click call NPCLoader.OnChatButtonClicked(false);
		internal static void OnChatButtonClicked(bool firstButton)
		{
			NPC npc = Main.npc[Main.player[Main.myPlayer].talkNPC];
			if (IsModNPC(npc))
			{
				bool shop = false;
				npc.modNPC.OnChatButtonClicked(firstButton, ref shop);
				Main.PlaySound(12, -1, -1, 1);
				if (shop)
				{
					Main.playerInventory = true;
					Main.npcChatText = "";
					Main.npcShop = Main.numShops - 1;
					Main.instance.shop[Main.npcShop].SetupShop(npc.type);
				}
			}
		}
		//in Terraria.Chest.SetupShop before discount call NPCLoader.SetupShop(type, this, ref num);
		internal static void SetupShop(int type, Chest shop, ref int nextSlot)
		{
			if (type < shopToNPC.Length)
			{
				type = shopToNPC[type];
			}
			else if (type >= NPCID.Count)
			{
				GetNPC(type).SetupShop(shop, ref nextSlot);
			}
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.SetupShop(type, shop, ref nextSlot);
			}
		}
		//at end of Terraria.Chest.SetupTravelShop call NPCLoader.SetupTravelShop(Main.travelShop, ref num2);
		internal static void SetupTravelShop(int[] shop, ref int nextSlot)
		{
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.SetupTravelShop(shop, ref nextSlot);
			}
		}
		//in Terraria.NPC.AI in aiStyle 7 after buffing damage multiplier and defense add
		//  NPCLoader.BuffTownNPC(ref num378, ref this.defense);
		internal static void BuffTownNPC(ref float damageMult, ref int defense)
		{
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.BuffTownNPC(ref damageMult, ref defense);
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
		internal static void TownNPCAttackStrength(NPC npc, ref int damage, ref float knockback)
		{
			if (IsModNPC(npc))
			{
				npc.modNPC.TownNPCAttackStrength(ref damage, ref knockback);
			}
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.TownNPCAttackStrength(npc, ref damage, ref knockback);
			}
		}

		internal static void TownNPCAttackCooldown(NPC npc, ref int cooldown, ref int randExtraCooldown)
		{
			if (IsModNPC(npc))
			{
				npc.modNPC.TownNPCAttackCooldown(ref cooldown, ref randExtraCooldown);
			}
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.TownNPCAttackCooldown(npc, ref cooldown, ref randExtraCooldown);
			}
		}

		internal static void TownNPCAttackProj(NPC npc, ref int projType, ref int attackDelay)
		{
			if (IsModNPC(npc))
			{
				npc.modNPC.TownNPCAttackProj(ref projType, ref attackDelay);
			}
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.TownNPCAttackProj(npc, ref projType, ref attackDelay);
			}
		}

		internal static void TownNPCAttackProjSpeed(NPC npc, ref float multiplier, ref float gravityCorrection,
			ref float randomOffset)
		{
			if (IsModNPC(npc))
			{
				npc.modNPC.TownNPCAttackProjSpeed(ref multiplier, ref gravityCorrection, ref randomOffset);
			}
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.TownNPCAttackProjSpeed(npc, ref multiplier, ref gravityCorrection, ref randomOffset);
			}
		}

		internal static void TownNPCAttackShoot(NPC npc, ref bool inBetweenShots)
		{
			if (IsModNPC(npc))
			{
				npc.modNPC.TownNPCAttackShoot(ref inBetweenShots);
			}
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.TownNPCAttackShoot(npc, ref inBetweenShots);
			}
		}

		internal static void TownNPCAttackMagic(NPC npc, ref float auraLightMultiplier)
		{
			if (IsModNPC(npc))
			{
				npc.modNPC.TownNPCAttackMagic(ref auraLightMultiplier);
			}
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.TownNPCAttackMagic(npc, ref auraLightMultiplier);
			}
		}

		internal static void TownNPCAttackSwing(NPC npc, ref int itemWidth, ref int itemHeight)
		{
			if (IsModNPC(npc))
			{
				npc.modNPC.TownNPCAttackSwing(ref itemWidth, ref itemHeight);
			}
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.TownNPCAttackSwing(npc, ref itemWidth, ref itemHeight);
			}
		}
	}
}
