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
		//in Terraria.NPC.NPCLoot after hardmode meteor head check add
		//  if(!NPCLoader.PreNPCLoot(this)) { return; }
		internal static bool PreNPCLoot(NPC npc)
		{
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				if (!globalNPC.PreNPCLoot(npc))
				{
					return false;
				}
			}
			return true;
		}
		//in Terraria.NPC.NPCLoot before heart and star drops add NPCLoader.NPCLoot(this);
		internal static void NPCLoot(NPC npc)
		{
			foreach (GlobalNPC globalNPC in globalNPCs)
			{
				globalNPC.NPCLoot(npc);
			}
		}
	}
}
