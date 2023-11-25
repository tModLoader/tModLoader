using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.BuilderToggles
{
	// The examples in this file don't actually affect anything, they just show typical approaches for the BuilderToggle half of the effect.
	// A full example would have code doing something, such as drawing an overlay, after checking ModContent.GetInstance<YourBuilderToggle>().Active and ModContent.GetInstance<YourBuilderToggle>().CurrentState.
	// That code is highly dependent on what you want to accomplish.

	public class ExampleBuilderToggle : BuilderToggle
	{
		public override bool Active() => Main.LocalPlayer.HeldItem.IsAir;

		public override int NumberOfStates => 4;

		public override string DisplayValue() {
			string text = "Color: ";
			string[] textMessages = new[] { "Red", "Blue", "Green", "Yellow" };

			return text + textMessages[CurrentState];
		}

		public override Color DisplayColorTexture() {
			Color[] colors = new[] { Color.Red, Color.Blue, Color.Green, Color.Yellow };

			return colors[CurrentState];
		}

		// Right click to cycle through states backwards.
		public override void OnRightClick() {
			CurrentState -= 1;
			if (CurrentState < 0) {
				CurrentState = NumberOfStates - 1;
			}

			SoundEngine.PlaySound(SoundID.Coins);
		}
	}

	public class ExampleBuilderToggleDimmedLight : BuilderToggle
	{
		public static LocalizedText OnText { get; private set; }
		public static LocalizedText OffText { get; private set; }

		public override string Texture => "ExampleMod/Content/BuilderToggles/ExampleBuilderToggle";
		public override bool Active() => true;
		public override int NumberOfStates => 2;

		public override void SetStaticDefaults() {
			OnText = this.GetLocalization(nameof(OnText));
			OffText = this.GetLocalization(nameof(OffText));
		}

		public override string DisplayValue() {
			return CurrentState == 0 ? OnText.Value : OffText.Value;
		}

		public override Color DisplayColorTexture() {
			return CurrentState == 0 ? Color.White : new Color(127, 127, 127);
		}
	}
}