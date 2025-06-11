using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.Buffs
{
	public class ExampleDefenseBuff : ModBuff
	{
		public static readonly int DefenseBonus = 10;

		public override LocalizedText Description => base.Description.WithFormatArgs(DefenseBonus);

		public override void Update(Player player, ref int buffIndex) {
			player.statDefense += DefenseBonus; // Grant a +10 defense boost to the player while the buff is active.
		}
	}
}
