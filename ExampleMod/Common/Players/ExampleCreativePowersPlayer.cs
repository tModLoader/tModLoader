using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	// Holds the state changed by ExampleGlowToggle and ExampleRunSpeedSlider. Nothing here is saved, so it resets each time the character is loaded.
	public class ExampleCreativePowersPlayer : ModPlayer
	{
		public bool glow;
		public float runSpeedSlider;

		// The slider's 0 to 1 value as a run speed multiplier from 1x to 3x.
		public float RunSpeedMultiplier => 1f + runSpeedSlider * 2f;

		public override void PostUpdateRunSpeeds() {
			Player.maxRunSpeed *= RunSpeedMultiplier;
			Player.accRunSpeed *= RunSpeedMultiplier;
		}

		public override void PostUpdate() {
			if (glow) {
				Lighting.AddLight(Player.Center, Color.Orange.ToVector3());
			}
		}
	}
}
