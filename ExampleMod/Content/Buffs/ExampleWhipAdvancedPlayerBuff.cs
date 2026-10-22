using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Buffs
{
	// This is a buff applied to the player when they hit an enemy with the Example Whip Advanced's tag effect. See ExampleWhipAdvanced for more details.
	public class ExampleWhipAdvancedPlayerBuff : ModBuff
	{
		public override void Update(Player player, ref int buffIndex) {
			// Simply increase the SummonMeleeSpeed by +12% while the player has the buff.
			player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += 0.12f;
		}
	}
}
