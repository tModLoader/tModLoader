using ExampleMod.Common.Players;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content
{
	class ExampleInfoDisplay : InfoDisplay
	{
		public override void SetupContent() {
			InfoName.SetDefault("Minion Count");
		}

		public override bool Active() {
			return Main.LocalPlayer.GetModPlayer<ExamplePlayer>().ShowMinionCount;
		}

		public override string DisplayValue() {
			int minionCount = Main.projectile.Count(x =>x.active && x.minion && x.owner == Main.LocalPlayer.whoAmI);
			return minionCount > 0 ? $"{minionCount} minions." : "No minions";
		}
	}
}
