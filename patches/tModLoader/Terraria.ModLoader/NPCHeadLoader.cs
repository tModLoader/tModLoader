using System;
using System.Collections.Generic;
using Terraria.ID;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class serves as a central place from which NPC head slots are stored and NPC head textures are assigned. This can be used to obtain the corresponding slots to head textures.
	/// </summary>
	public static class NPCHeadLoader
	{
		/// <summary>
		/// The number of vanilla town NPC head textures that exist.
		/// </summary>
		public static readonly int vanillaHeadCount = Main.npcHeadTexture.Length;
		/// <summary>
		/// The number of vanilla boss head textures that exist.
		/// </summary>
		public static readonly int vanillaBossHeadCount = Main.npcHeadBossTexture.Length;
		private static int nextHead = vanillaHeadCount;
		private static int nextBossHead = vanillaBossHeadCount;
		internal static IDictionary<string, int> heads = new Dictionary<string, int>();
		internal static IDictionary<string, int> bossHeads = new Dictionary<string, int>();
		internal static IDictionary<int, int> npcToHead = new Dictionary<int, int>();
		internal static IDictionary<int, int> headToNPC = new Dictionary<int, int>();
		internal static IDictionary<int, int> npcToBossHead = new Dictionary<int, int>();

		internal static int ReserveHeadSlot() {
			int reserve = nextHead;
			nextHead++;
			return reserve;
		}

		internal static int ReserveBossHeadSlot(string texture) {
			if (bossHeads.TryGetValue(texture, out int existing)) {
				return existing;
			}
			int reserve = nextBossHead;
			nextBossHead++;
			return reserve;
		}

		/// <summary>
		/// Gets the index of the head texture corresponding to the given texture path.
		/// </summary>
		/// <param name="texture">Relative texture path</param>
		/// <returns>The index of the texture in the heads array, -1 if not found.</returns>
		public static int GetHeadSlot(string texture) => heads.TryGetValue(texture, out int slot) ? slot : -1;

		/// <summary>
		/// Gets the index of the boss head texture corresponding to the given texture path.
		/// </summary>
		/// <param name="texture"></param>
		/// <returns></returns>
		public static int GetBossHeadSlot(string texture) => bossHeads.TryGetValue(texture, out int slot) ? slot : -1;

		internal static void ResizeAndFillArrays() {
			Array.Resize(ref Main.npcHeadTexture, nextHead);
			Array.Resize(ref Main.npcHeadBossTexture, nextBossHead);
			foreach (string texture in heads.Keys) {
				Main.npcHeadTexture[heads[texture]] = ModContent.GetTexture(texture);
			}
			foreach (string texture in bossHeads.Keys) {
				Main.npcHeadBossTexture[bossHeads[texture]] = ModContent.GetTexture(texture);
			}
			foreach (int npc in npcToBossHead.Keys) {
				NPCID.Sets.BossHeadTextures[npc] = npcToBossHead[npc];
			}
		}

		internal static void Unload() {
			nextHead = vanillaHeadCount;
			nextBossHead = vanillaBossHeadCount;
			heads.Clear();
			bossHeads.Clear();
			npcToHead.Clear();
			headToNPC.Clear();
			npcToBossHead.Clear();
		}
		//in Terraria.NPC.TypeToNum replace final return with this
		internal static int GetNPCHeadSlot(int type) => npcToHead.TryGetValue(type, out int slot) ? slot : -1;

		//in Terraria.NPC.NumToType replace final return with this
		internal static int GetNPCFromHeadSlot(int slot) => headToNPC.TryGetValue(slot, out int type) ? type : -1;
	}
}
