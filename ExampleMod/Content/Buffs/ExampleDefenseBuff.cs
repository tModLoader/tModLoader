using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Buffs
{
	public class ExampleDefenseBuff : ModBuff
	{
		public override void SetDefaults() {
			DisplayName.SetDefault("Defensive Buff");
			Description.SetDefault("Grants +4 defense.");
			Main.buffNoTimeDisplay[Type] = false; //Set this to true so the remaining buff time should not be displayed (by default it also won't get displayed if buffTime is less than 3).
			Main.debuff[Type] = false; //Set this to true so the nurse doesn't remove the buff when healing.
		}

		public override void Update(Player player, ref int buffIndex) {
			player.statDefense += 4; //Grant a +4 defense boost to the player while the buff is active.
		}
	}
}
