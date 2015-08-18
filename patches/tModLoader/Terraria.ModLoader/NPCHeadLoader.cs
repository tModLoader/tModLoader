using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace Terraria.ModLoader
{
	public static class NPCHeadLoader
	{
		public const int vanillaHeadCount = 24;
		public const int vanillaBossHeadCount = 31;
		private static int nextHead = vanillaHeadCount;
		private static int nextBossHead = vanillaBossHeadCount;
		internal static IDictionary<string, int> heads = new Dictionary<string, int>();
		internal static IDictionary<string, int> bossHeads = new Dictionary<string, int>();
		internal static IDictionary<int, int> npcToHead = new Dictionary<int, int>();
		internal static IDictionary<int, int> headToNPC = new Dictionary<int, int>();
		internal static IDictionary<int, int> npcToBossHead = new Dictionary<int, int>();

		internal static int ReserveHeadSlot()
		{
			int reserve = nextHead;
			nextHead++;
			return reserve;
		}

		internal static int ReserveBossHeadSlot(string texture)
		{
			if (bossHeads.ContainsKey(texture))
			{
				return bossHeads[texture];
			}
			int reserve = nextBossHead;
			nextBossHead++;
			ErrorLogger.Log("" + reserve);
			return reserve;
		}

		public static int GetHeadSlot(string texture)
		{
			if (heads.ContainsKey(texture))
			{
				return heads[texture];
			}
			else
			{
				return -1;
			}
		}

		public static int GetBossHeadSlot(string texture)
		{
			if (bossHeads.ContainsKey(texture))
			{
				return bossHeads[texture];
			}
			else
			{
				return -1;
			}
		}

		internal static void ResizeAndFillArrays()
		{
			Array.Resize(ref Main.npcHeadTexture, nextHead);
			Array.Resize(ref Main.npcHeadBossTexture, nextBossHead);
			foreach (string texture in heads.Keys)
			{
				Main.npcHeadTexture[heads[texture]] = ModLoader.GetTexture(texture);
			}
			foreach (string texture in bossHeads.Keys)
			{
				Main.npcHeadBossTexture[bossHeads[texture]] = ModLoader.GetTexture(texture);
			}
			foreach (int npc in npcToBossHead.Keys)
			{
				NPCID.Sets.BossHeadTextures[npc] = npcToBossHead[npc];
			}
		}

		internal static void Unload()
		{
			nextHead = vanillaHeadCount;
			nextBossHead = vanillaBossHeadCount;
			heads.Clear();
			bossHeads.Clear();
			npcToHead.Clear();
			headToNPC.Clear();
			npcToBossHead.Clear();
		}
		//in Terraria.NPC.TypeToNum replace final return with this
		internal static int GetNPCHeadSlot(int type)
		{
			if (npcToHead.ContainsKey(type))
			{
				return npcToHead[type];
			}
			return -1;
		}
		//in Terraria.NPC.NumToType replace final return with this
		internal static int GetNPCFromHeadSlot(int slot)
		{
			if (headToNPC.ContainsKey(slot))
			{
				return headToNPC[slot];
			}
			return -1;
		}
	}
}
