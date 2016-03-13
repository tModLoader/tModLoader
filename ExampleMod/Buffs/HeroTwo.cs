using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Buffs
{
	public class HeroTwo : ModBuff
	{
		public override void SetDefaults()
		{
			Main.buffName[this.Type] = "Hero";
			Main.buffTip[this.Type] = "You are a hero of Terraria! (2 Lives)";
			Main.buffNoSave[Type] = true;
			Main.debuff[Type] = true;
			canBeCleared = false;
		}
	}
}
