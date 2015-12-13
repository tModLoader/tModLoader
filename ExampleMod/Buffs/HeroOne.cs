using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExampleMod;

namespace ExampleMod.Buffs
{
	public class HeroOne : ModBuff
	{
		public override void SetDefaults()
		{
			Main.buffName[this.Type] = "Hero";
			Main.buffTip[this.Type] = "You are a hero of Terraria!";
			Main.buffNoSave[Type] = true;
		}
	}
}
