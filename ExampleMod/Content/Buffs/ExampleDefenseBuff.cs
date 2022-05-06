using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Buffs
{
	public class ExampleDefenseBuff : ModBuff
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Defensive Buff");
			Description.SetDefault("Grants +4 defense.");
		}

		public override void Update(Player player, ref int buffIndex) {
			player.statDefense += 4; // Grant a +4 defense boost to the player while the buff is active.
		}
	}
}
