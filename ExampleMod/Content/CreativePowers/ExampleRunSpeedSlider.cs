using ExampleMod.Common.Players;
using ExampleMod.Content.CreativeCategories;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.CreativePowers
{
	// A ModCreativeSlider is a Journey Mode power with a slider, like vanilla's Rain Control. Clicking its button opens the slider beside the strip.
	// This one is on ExampleCreativeCategory's strip, and changes the player's run speed.
	public class ExampleRunSpeedSlider : ModCreativeSlider
	{
		public override string Texture => $"Terraria/Images/Item_{ItemID.HermesBoots}";

		public override int Category => ModContent.GetInstance<ExampleCreativeCategory>().Type;

		// Labels at the top, middle and bottom of the slider. Clicking one moves the slider there.
		public override LocalizedText TopLabel => this.GetLocalization(nameof(TopLabel));
		public override LocalizedText MiddleLabel => this.GetLocalization(nameof(MiddleLabel));
		public override LocalizedText BottomLabel => this.GetLocalization(nameof(BottomLabel));

		// The slider goes from 0 at the bottom to 1 at the top.
		public override float Value {
			get => Main.LocalPlayer.GetModPlayer<ExampleCreativePowersPlayer>().runSpeedSlider;
			set => Main.LocalPlayer.GetModPlayer<ExampleCreativePowersPlayer>().runSpeedSlider = MathHelper.Clamp(value, 0f, 1f);
		}

		// Shown when hovering the slider. By default this is Value as a percentage.
		public override string ValueText => "x" + Main.LocalPlayer.GetModPlayer<ExampleCreativePowersPlayer>().RunSpeedMultiplier.ToString("0.0");
	}
}
