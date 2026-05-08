using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Buffs
{
	public class ExampleWhipBuff : ModBuff
	{
		public override void Update(Player player, ref int buffIndex) {
			// Simply increase the SummonMeleeSpeed by +12% while the player has the buff.
			player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += 0.12f;
		}
	}
}
