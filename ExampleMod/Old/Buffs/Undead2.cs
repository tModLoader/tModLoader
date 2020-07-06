using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Buffs
{
	public class Undead2 : ModBuff
	{
		public override void SetDefaults() {
			DisplayName.SetDefault("Undead Sickness");
			Description.SetDefault("You are being harmed by recovery");
			Main.debuff[Type] = true;
			Main.pvpBuff[Type] = true;
			Main.buffNoSave[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex) {
			int extra = player.buffTime[buffIndex] / 60;
			player.buffTime[buffIndex] -= extra;
			player.GetModPlayer<ExamplePlayer>().healHurt = extra + 1;
		}

		public override bool ReApply(Player player, int time, int buffIndex) {
			player.buffTime[buffIndex] += time;
			return true;
		}
	}
}
