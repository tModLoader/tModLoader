using ExampleMod.Content.CreativeCategories;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.CreativePowers
{
	// A ModCreativeButton on ExampleCreativeCategory's strip, which opens when that category is clicked.
	public class ExampleHealButton : ModCreativeButton
	{
		public override string Texture => $"Terraria/Images/Item_{ItemID.HealingPotion}";

		public override int Category => ModContent.GetInstance<ExampleCreativeCategory>().Type;

		// Uncomment this line to only allow the host to use this power in multiplayer. The button is grayed out for everyone else.
		// public override bool CanUse() => Main.netMode == NetmodeID.SinglePlayer || Main.countsAsHostForGameplay[Main.myPlayer];

		public override void OnClick() {
			Main.LocalPlayer.Heal(Main.LocalPlayer.statLifeMax2);
		}
	}
}
