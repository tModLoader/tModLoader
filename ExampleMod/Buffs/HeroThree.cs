using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExampleMod;

namespace ExampleMod.Buffs
{
	public class HeroThree : ModBuff
	{
		public override void SetDefaults()
		{
			Main.buffName[this.Type] = "Hero";
			Main.buffTip[this.Type] = "You are a hero of Terraria! (3 Lives)";
			Main.buffNoSave[Type] = true;
		}
	}
}
