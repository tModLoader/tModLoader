using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;

namespace Terraria.ModLoader
{
	public static class NPCLoader
	{
		private static int nextNPC = NPCID.Count;
		internal static readonly IDictionary<int, ModNPC> npcs = new Dictionary<int, ModNPC>();
		internal static readonly IList<GlobalNPC> globalNPCs = new List<GlobalNPC>();
		private static int vanillaSkeletonCount = NPCID.Sets.Skeletons.Count;
		//in Terraria.Item.NewItem after setting Main.item[400] add
		//  if(NPCLoader.blockLoot.Contains(Type)) { return num; }
		public static readonly IList<int> blockLoot = new List<int>();

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
				if (npc.npc.townNPC && !NPC.AnyNPCs(npc.npc.type) && npc.CanTownNPCSpawn(numTownNPCs, money))
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
	}
}
