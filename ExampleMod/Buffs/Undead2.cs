using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExampleMod;

namespace ExampleMod.Buffs
{
	public class Undead2 : ModBuff
	{
		public override void SetDefaults()
		{
			Main.buffName[this.Type] = "Undead Sickness";
			Main.buffTip[this.Type] = "You are being harmed by recovery";
			Main.debuff[Type] = true;
			Main.pvpBuff[Type] = true;
			Main.buffNoSave[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			int extra = player.buffTime[buffIndex] / 60;
			player.buffTime[buffIndex] -= extra;
			((ExamplePlayer)player.GetModPlayer(mod, "ExamplePlayer")).healHurt = extra + 1;
		}
	}
}
