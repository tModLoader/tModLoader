using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExampleMod;

namespace ExampleMod.Buffs
{
	public class Undead : ModBuff
	{
		public override void SetDefaults()
		{
			Main.buffName[this.Type] = "Undead";
			Main.buffTip[this.Type] = "Recovering harms you";
			Main.debuff[Type] = true;
			Main.pvpBuff[Type] = true;
			Main.buffNoSave[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			((ExamplePlayer)player.GetModPlayer(mod, "ExamplePlayer")).badHeal = true;
		}
	}
}
