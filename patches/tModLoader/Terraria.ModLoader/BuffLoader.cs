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
		//   internal static readonly IList<GlobalBuff> globalBuffs = new List<GlobalBuff>();
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
			//   globalBuffs.Clear();
		}

		internal static bool IsModBuff(int type)
		{
			return type >= BuffID.Count;
		}
		//internal static void SetupBuff(Buff buff)
		//{
		//    if (IsModBuff(buff))
		//    {
		//        GetBuff(buff.type).SetupBuff(buff);
		//    }
		//    foreach (GlobalBuff globalBuff in globalBuffs)
		//    {
		//        globalBuff.SetDefaults(buff);
		//    }
		//}
		// TODO hooks here
		//in Terraria.Player.UpdateBuffs at end of if else chain add BuffLoader.Update(this.buffType[k], this, ref k);
		internal static void Update(int buff, Player player, ref int buffIndex)
		{
			if (IsModBuff(buff))
			{
				GetBuff(buff).Update(player, ref buffIndex);
			}
		}
	}
}
