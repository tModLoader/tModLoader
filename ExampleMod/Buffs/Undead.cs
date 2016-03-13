using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Buffs
{
	public class Undead : ModBuff
	{
		public override void SetDefaults()
		{
			Main.buffName[Type] = "Undead";
			Main.buffTip[Type] = "Recovering harms you";
			Main.debuff[Type] = true;
			Main.pvpBuff[Type] = true;
			Main.buffNoSave[Type] = true;
			longerExpertDebuff = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
            player.GetModPlayer<ExamplePlayer>(mod).badHeal = true;
		}
	}
}
