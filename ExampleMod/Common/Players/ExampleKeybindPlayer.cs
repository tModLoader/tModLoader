using ExampleMod.Common.Systems;
using System;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	// See Common/Systems/KeybindSystem for keybind registration.
	public class ExampleKeybindPlayer : ModPlayer
	{
		private int LearningExampleKeybindHeldTimer;
		private int LearningExampleKeybindDoubleTapTimer;

		public override void ProcessTriggers(TriggersSet triggersSet) {
			// The most common way to use keybinds is to use JustPressed to run code whenever the keybind is pressed
			if (KeybindSystem.RandomBuffKeybind.JustPressed) {
				int buff = Main.rand.Next(BuffID.Count);
				Player.AddBuff(buff, 600);
				Main.NewText($"ExampleMod's ModKeybind was just pressed. The {Lang.GetBuffName(buff)} buff was given to the player.");
			}

			// These examples show other potential behaviors of keybinds, such as a double tap and being held down.
			
			// We can use Current and a timer to run code after the keybind has been held for some time
			if (KeybindSystem.LearningExampleKeybind.Current) {
				LearningExampleKeybindHeldTimer++;
				if (LearningExampleKeybindHeldTimer == 30) {
					Main.NewText("LearningExampleKeybind held for half a second");
				}
			}
			else {
				LearningExampleKeybindHeldTimer = 0;
			}

			// We can use JustPressed and a timer to implement a double tap behavior as well.
			LearningExampleKeybindDoubleTapTimer = Math.Max(0, LearningExampleKeybindDoubleTapTimer - 1);
			if (KeybindSystem.LearningExampleKeybind.JustPressed) {
				if (LearningExampleKeybindDoubleTapTimer > 0) {
					Main.NewText("LearningExampleKeybind double tapped within a quarter of a a second");
				}
				else {
					// On 1st press, set timer for 15, if a 2nd press happens before it reaches 0, it will be a double tap.
					LearningExampleKeybindDoubleTapTimer = 15;
				}
			}
		}
	}
}
