using Terraria.GameContent;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	// Showcases modifying eye state and frames under certain condition.
	public class ExampleBlinkingPlayer : ModPlayer
	{
		public override void PostUpdate() {
			// Set eye state to our custom eye state if player is in water and is in state of normal blinking.
			if (Player.eyeHelper.CurrentEyeState == PlayerEyeHelper.EyeState.NormalBlinking && Player.wet) {
				Player.eyeHelper.CurrentEyeFrame = PlayerEyeHelper.EyeFrame.EyeClosed;
				Player.eyeHelper.TimeInState = 0;
			}

			// Override vanilla blind eye state with our own.
			// It's enough to check for `Player.blind`, but that way it wouldn't replace `IsBlind` eye state set by other mods.
			// Decide yourself whatever option fits your needs.
			if (Player.eyeHelper.CurrentEyeState == PlayerEyeHelper.EyeState.IsBlind) {
				// Close players eyes for 115 ticks out of 120 ticks.
				// The remaining 5 ticks player will have half closed eyes.
				if ((Player.eyeHelper.TimeInState % 120 - 115) < 0) {
					Player.eyeHelper.CurrentEyeFrame = PlayerEyeHelper.EyeFrame.EyeClosed;
				}
				else {
					Player.eyeHelper.CurrentEyeFrame = PlayerEyeHelper.EyeFrame.EyeHalfClosed;
				}
			}
		}
	}
}
