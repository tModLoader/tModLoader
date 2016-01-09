using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;

namespace Terraria.ModLoader
{
	public static class BuffLoader
	{
		private static int nextBuff = BuffID.Count;
		internal static readonly IDictionary<int, ModBuff> buffs = new Dictionary<int, ModBuff>();
		internal static readonly IList<GlobalBuff> globalBuffs = new List<GlobalBuff>();
		private static readonly bool[] vanillaLongerExpertDebuff = new bool[BuffID.Count];
		private static readonly bool[] vanillaCanBeCleared = new bool[BuffID.Count];

		static BuffLoader()
		{
			for (int k = 0; k < BuffID.Count; k++)
			{
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
		}

		internal static int ReserveBuffID()
		{
			int reserveID = nextBuff;
			nextBuff++;
			return reserveID;
		}

		internal static int BuffCount()
		{
			return nextBuff;
		}

		public static ModBuff GetBuff(int type)
		{
			if (buffs.ContainsKey(type))
			{
				return buffs[type];
			}
			else
			{
				return null;
			}
		}

		internal static void ResizeArrays()
		{
			Array.Resize(ref Main.pvpBuff, nextBuff);
			Array.Resize(ref Main.persistentBuff, nextBuff);
			Array.Resize(ref Main.vanityPet, nextBuff);
			Array.Resize(ref Main.lightPet, nextBuff);
			Array.Resize(ref Main.meleeBuff, nextBuff);
			Array.Resize(ref Main.debuff, nextBuff);
			Array.Resize(ref Main.buffName, nextBuff);
			Array.Resize(ref Main.buffTip, nextBuff);
			Array.Resize(ref Main.buffNoSave, nextBuff);
			Array.Resize(ref Main.buffNoTimeDisplay, nextBuff);
			Array.Resize(ref Main.buffDoubleApply, nextBuff);
			Array.Resize(ref Main.buffAlpha, nextBuff);
			Array.Resize(ref Main.buffTexture, nextBuff);
		}

		internal static void Unload()
		{
			buffs.Clear();
			nextBuff = BuffID.Count;
			globalBuffs.Clear();
		}

		internal static bool IsModBuff(int type)
		{
			return type >= BuffID.Count;
		}
		//in Terraria.Player.UpdateBuffs at end of if else chain add BuffLoader.Update(this.buffType[k], this, ref k);
		public static void Update(int buff, Player player, ref int buffIndex)
		{
			int originalIndex = buffIndex;
			if (IsModBuff(buff))
			{
				GetBuff(buff).Update(player, ref buffIndex);
			}
			foreach (GlobalBuff globalBuff in globalBuffs)
			{
				if (buffIndex != originalIndex)
				{
					break;
				}
				globalBuff.Update(buff, player, ref buffIndex);
			}
		}

		public static void Update(int buff, NPC npc, ref int buffIndex)
		{
			if (IsModBuff(buff))
			{
				GetBuff(buff).Update(npc, ref buffIndex);
			}
			foreach (GlobalBuff globalBuff in globalBuffs)
			{
				globalBuff.Update(buff, npc, ref buffIndex);
			}
		}

		public static bool ReApply(int buff, Player player, int time, int buffIndex)
		{
			foreach (GlobalBuff globalBuff in globalBuffs)
			{
				if (globalBuff.ReApply(buff, player, time, buffIndex))
				{
					return true;
				}
			}
			if (IsModBuff(buff))
			{
				return GetBuff(buff).ReApply(player, time, buffIndex);
			}
			return false;
		}

		public static bool ReApply(int buff, NPC npc, int time, int buffIndex)
		{
			foreach (GlobalBuff globalBuff in globalBuffs)
			{
				if (globalBuff.ReApply(buff, npc, time, buffIndex))
				{
					return true;
				}
			}
			if (IsModBuff(buff))
			{
				return GetBuff(buff).ReApply(npc, time, buffIndex);
			}
			return false;
		}

		public static bool LongerExpertDebuff(int buff)
		{
			if (IsModBuff(buff))
			{
				return GetBuff(buff).longerExpertDebuff;
			}
			else
			{
				return vanillaLongerExpertDebuff[buff];
			}
		}

		public static bool CanBeCleared(int buff)
		{
			if (IsModBuff(buff))
			{
				return GetBuff(buff).canBeCleared;
			}
			else
			{
				return vanillaCanBeCleared[buff];
			}
		}
	}
}
