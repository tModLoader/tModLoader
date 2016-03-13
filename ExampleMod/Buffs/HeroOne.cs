using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Buffs
{
	public class HeroOne : ModBuff
	{
		public override void SetDefaults()
		{
			Main.buffName[this.Type] = "Hero";
			Main.buffTip[this.Type] = "You are a hero of Terraria!";
			Main.buffNoSave[Type] = true;
			Main.debuff[Type] = true;
			canBeCleared = false;
		}
	}
}
