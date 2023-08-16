using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content
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
	}

	public class ExampleBuilderToggleDimmedLight : BuilderToggle
	{
		public override string Texture => "ExampleMod/Content/ExampleBuilderToggle";
		public override bool Active() => true;

		public override int NumberOfStates => 2;
		public override string DisplayValue() {
			return CurrentState == 0 ? "Example On" : "Example Off";
		}

		public override Color DisplayColorTexture() {
			return CurrentState == 0 ? Color.White : new Color(127, 127, 127);
		}
	}

	public class GlobalToggle : GlobalBuilderToggle
	{
		public override void ModifyNumberOfStates(BuilderToggle builderToggle, ref int numberOfStates) {
			if (builderToggle == BuilderToggle.RulerGrid) {
				numberOfStates = 3;
			}
		}

		public override void ModifyDisplayValue(BuilderToggle builderToggle, ref string displayValue) {
			if (builderToggle == BuilderToggle.RulerGrid) {
				if (builderToggle.CurrentState == 2) {
					displayValue = "This text was modified by Example Mod!";
				}
			}
		}

		public override void ModifyDisplayColor(BuilderToggle builderToggle, ref Color displayColor) {
			if (builderToggle == BuilderToggle.RulerGrid) {
				if (builderToggle.CurrentState == 2) {
					displayColor = Color.OrangeRed;
				}
			}
		}
	}
}