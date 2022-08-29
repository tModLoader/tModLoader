using ExampleMod.Common.Players;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Buffs
{
	public class ExampleCrateBuff : ModBuff
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Example Crate");
			Description.SetDefault("Greater chance of fishing up a crate");
		}

		public override void Update(Player player, ref int buffIndex) {
			// Use a ModPlayer to keep track of the buff being active
			player.GetModPlayer<ExampleFishingPlayer>().hasExampleCrateBuff = true;
		}
	}
}
