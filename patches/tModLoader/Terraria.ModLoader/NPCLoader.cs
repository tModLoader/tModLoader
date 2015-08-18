using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
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
		//change initial size of Terraria.Player.npcTypeNoAggro to NPCLoader.NPCCount()
		//in Terraria.Main.DrawNPCs and Terraria.NPC.NPCLoot remove type too high check
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
		//in Terraria.NPC.SetDefaults move Lang stuff before SetupNPC
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
		//in Terraria.NPC rename AI to VanillaAI then make AI call NPCLoader.NPCAI(this)
		internal static void NPCAI(NPC npc)
		{
			if (PreAI(npc))
			{
				int type = npc.type;
				if (IsModNPC(npc) && npc.modNPC.aiType > 0)
				{
					npc.type = npc.modNPC.aiType;
				}
				npc.VanillaAI();
				npc.type = type;
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
	}
}
