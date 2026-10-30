using ExampleMod.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.CreativePowers
{
	// A ModCreativeToggle is a Journey Mode power turned on and off by clicking it, like vanilla's God Mode.
	// Category defaults to CreativeCategoryID.None, which puts it on the main strip next to vanilla's Infection Spread toggle.
	// Its name and description come from Mods.ExampleMod.CreativePowers.ExampleGlowToggle in the localization files.
	// Modded powers only run on the player's own client. Nothing is synced or saved, and the server's power permissions don't apply. See ExampleHealButton for CanUse.
	public class ExampleGlowToggle : ModCreativeToggle
	{
		// Uses a vanilla item texture as the icon. Without this, ExampleGlowToggle.png next to this file would be used, like other content.
		public override string Texture => $"Terraria/Images/Item_{ItemID.Torch}";

		// Checked every frame to highlight the button while the power is on.
		public override bool IsActive => Main.LocalPlayer.GetModPlayer<ExampleCreativePowersPlayer>().glow;

		public override void OnClick() {
			ExampleCreativePowersPlayer modPlayer = Main.LocalPlayer.GetModPlayer<ExampleCreativePowersPlayer>();
			modPlayer.glow = !modPlayer.glow;
		}
	}
}
