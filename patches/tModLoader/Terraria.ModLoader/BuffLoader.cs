using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.Localization;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This serves as the central class from which buff-related functions are supported and carried out.
	/// </summary>
	public static class BuffLoader
	{
		internal static int extraPlayerBuffCount;
		private static int nextBuff = BuffID.Count;
		internal static readonly IList<ModBuff> buffs = new List<ModBuff>();
		internal static readonly IList<GlobalBuff> globalBuffs = new List<GlobalBuff>();
		private static readonly bool[] vanillaLongerExpertDebuff = new bool[BuffID.Count];
		private static readonly bool[] vanillaCanBeCleared = new bool[BuffID.Count];

		private delegate void DelegateUpdatePlayer(int type, Player player, ref int buffIndex);
		private static DelegateUpdatePlayer[] HookUpdatePlayer;
		private delegate void DelegateUpdateNPC(int type, NPC npc, ref int buffIndex);
		private static DelegateUpdateNPC[] HookUpdateNPC;
		private static Func<int, Player, int, int, bool>[] HookReApplyPlayer;
		private static Func<int, NPC, int, int, bool>[] HookReApplyNPC;
		private delegate void DelegateModifyBuffTip(int type, ref string tip, ref int rare);
		private static DelegateModifyBuffTip[] HookModifyBuffTip;
		private static Action<string, List<Vector2>>[] HookCustomBuffTipSize;
		private static Action<string, SpriteBatch, int, int>[] HookDrawCustomBuffTip;

		static BuffLoader() {
			for (int k = 0; k < BuffID.Count; k++) {
				vanillaCanBeCleared[k] = true;
			}
			vanillaLongerExpertDebuff[BuffID.Poisoned] = true;
			vanillaLongerExpertDebuff[BuffID.Darkness] = true;
			vanillaLongerExpertDebuff[BuffID.Cursed] = true;
			vanillaLongerExpertDebuff[BuffID.OnFire] = true;
			vanillaLongerExpertDebuff[BuffID.Bleeding] = true;
			vanillaLongerExpertDebuff[BuffID.Confused] = true;
			vanillaLongerExpertDebuff[BuffID.Slow] = true;
			vanillaLongerExpertDebuff[BuffID.Weak] = true;
			vanillaLongerExpertDebuff[BuffID.Silenced] = true;
			vanillaLongerExpertDebuff[BuffID.BrokenArmor] = true;
			vanillaLongerExpertDebuff[BuffID.CursedInferno] = true;
			vanillaLongerExpertDebuff[BuffID.Frostburn] = true;
			vanillaLongerExpertDebuff[BuffID.Chilled] = true;
			vanillaLongerExpertDebuff[BuffID.Frozen] = true;
			vanillaLongerExpertDebuff[BuffID.Ichor] = true;
			vanillaLongerExpertDebuff[BuffID.Venom] = true;
			vanillaLongerExpertDebuff[BuffID.Blackout] = true;
			vanillaCanBeCleared[BuffID.PotionSickness] = false;
			vanillaCanBeCleared[BuffID.Werewolf] = false;
			vanillaCanBeCleared[BuffID.Merfolk] = false;
			vanillaCanBeCleared[BuffID.WaterCandle] = false;
			vanillaCanBeCleared[BuffID.Campfire] = false;
			vanillaCanBeCleared[BuffID.HeartLamp] = false;
			vanillaCanBeCleared[BuffID.NoBuilding] = false;
		}

		internal static int ReserveBuffID() {
			if (ModNet.AllowVanillaClients) throw new Exception("Adding buffs breaks vanilla client compatibility");

			int reserveID = nextBuff;
			nextBuff++;
			return reserveID;
		}

		public static int BuffCount => nextBuff;

		/// <summary>
		/// Gets the ModBuff instance with the given type. If no ModBuff with the given type exists, returns null.
		/// </summary>
		public static ModBuff GetBuff(int type) {
			return type >= BuffID.Count && type < BuffCount ? buffs[type - BuffID.Count] : null;
		}

		internal static void ResizeArrays() {
			Array.Resize(ref Main.pvpBuff, nextBuff);
			Array.Resize(ref Main.persistentBuff, nextBuff);
			Array.Resize(ref Main.vanityPet, nextBuff);
			Array.Resize(ref Main.lightPet, nextBuff);
			Array.Resize(ref Main.meleeBuff, nextBuff);
			Array.Resize(ref Main.debuff, nextBuff);
			Array.Resize(ref Main.buffNoSave, nextBuff);
			Array.Resize(ref Main.buffNoTimeDisplay, nextBuff);
			Array.Resize(ref Main.buffDoubleApply, nextBuff);
			Array.Resize(ref Main.buffAlpha, nextBuff);
			Array.Resize(ref Main.buffTexture, nextBuff);
			Array.Resize(ref Lang._buffNameCache, nextBuff);
			Array.Resize(ref Lang._buffDescriptionCache, nextBuff);
			for (int k = BuffID.Count; k < nextBuff; k++) {
				Lang._buffNameCache[k] = LocalizedText.Empty;
				Lang._buffDescriptionCache[k] = LocalizedText.Empty;
			}
			extraPlayerBuffCount = ModLoader.Mods.Any() ? ModLoader.Mods.Max(m => (int)m.ExtraPlayerBuffSlots) : 0;

			ModLoader.BuildGlobalHook(ref HookUpdatePlayer, globalBuffs, g => g.Update);
			ModLoader.BuildGlobalHook(ref HookUpdateNPC, globalBuffs, g => g.Update);
			ModLoader.BuildGlobalHook(ref HookReApplyPlayer, globalBuffs, g => g.ReApply);
			ModLoader.BuildGlobalHook(ref HookReApplyNPC, globalBuffs, g => g.ReApply);
			ModLoader.BuildGlobalHook(ref HookModifyBuffTip, globalBuffs, g => g.ModifyBuffTip);
			ModLoader.BuildGlobalHook(ref HookCustomBuffTipSize, globalBuffs, g => g.CustomBuffTipSize);
			ModLoader.BuildGlobalHook(ref HookDrawCustomBuffTip, globalBuffs, g => g.DrawCustomBuffTip);
		}

		internal static void Unload() {
			buffs.Clear();
			nextBuff = BuffID.Count;
			globalBuffs.Clear();
		}

		internal static bool IsModBuff(int type) {
			return type >= BuffID.Count;
		}
		//in Terraria.Player.UpdateBuffs at end of if else chain add BuffLoader.Update(this.buffType[k], this, ref k);
		public static void Update(int buff, Player player, ref int buffIndex) {
			int originalIndex = buffIndex;
			if (IsModBuff(buff)) {
				GetBuff(buff).Update(player, ref buffIndex);
			}
			foreach (var hook in HookUpdatePlayer) {
				if (buffIndex != originalIndex) {
					break;
				}
				hook(buff, player, ref buffIndex);
			}
		}

		public static void Update(int buff, NPC npc, ref int buffIndex) {
			if (IsModBuff(buff)) {
				GetBuff(buff).Update(npc, ref buffIndex);
			}
			foreach (var hook in HookUpdateNPC) {
				hook(buff, npc, ref buffIndex);
			}
		}

		public static bool ReApply(int buff, Player player, int time, int buffIndex) {
			foreach (var hook in HookReApplyPlayer) {
				if (hook(buff, player, time, buffIndex)) {
					return true;
				}
			}
			if (IsModBuff(buff)) {
				return GetBuff(buff).ReApply(player, time, buffIndex);
			}
			return false;
		}

		public static bool ReApply(int buff, NPC npc, int time, int buffIndex) {
			foreach (var hook in HookReApplyNPC) {
				if (hook(buff, npc, time, buffIndex)) {
					return true;
				}
			}
			if (IsModBuff(buff)) {
				return GetBuff(buff).ReApply(npc, time, buffIndex);
			}
			return false;
		}

		public static bool LongerExpertDebuff(int buff) {
			return GetBuff(buff)?.longerExpertDebuff ?? vanillaLongerExpertDebuff[buff];
		}

		public static bool CanBeCleared(int buff) {
			return GetBuff(buff)?.canBeCleared ?? vanillaCanBeCleared[buff];
		}

		public static void ModifyBuffTip(int buff, ref string tip, ref int rare) {
			if (IsModBuff(buff)) {
				GetBuff(buff).ModifyBuffTip(ref tip, ref rare);
			}
			foreach (var hook in HookModifyBuffTip) {
				hook(buff, ref tip, ref rare);
			}
		}

		public static void CustomBuffTipSize(string buffTip, List<Vector2> sizes) {
			foreach (var hook in HookCustomBuffTipSize) {
				hook(buffTip, sizes);
			}
		}

		public static void DrawCustomBuffTip(string buffTip, SpriteBatch spriteBatch, int originX, int originY) {
			foreach (var hook in HookDrawCustomBuffTip) {
				hook(buffTip, spriteBatch, originX, originY);
			}
		}
	}
}
