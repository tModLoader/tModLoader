using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;

namespace Terraria.GameInput
{
	partial class PlayerInputProfile
	{
		public void CopyModKeybindSettingsFrom(PlayerInputProfile profile, InputMode mode) {
			foreach (var modKeybind in KeybindLoader.Keybinds) {
				InputModes[mode].KeyStatus[modKeybind.uniqueName].Clear();

				if (!string.IsNullOrEmpty(modKeybind.defaultBinding))
					InputModes[mode].KeyStatus[modKeybind.uniqueName].Add(modKeybind.defaultBinding);
			}
		}

		public void CopyIndividualModKeybindSettingsFrom(PlayerInputProfile profile, InputMode mode, string uniqueName) {
			var modKeybind = KeybindLoader.modKeybinds[uniqueName];

			InputModes[mode].KeyStatus[uniqueName].Clear();

			if (!string.IsNullOrEmpty(modKeybind.defaultBinding))
				InputModes[mode].KeyStatus[uniqueName].Add(modKeybind.defaultBinding);
		}
	}
}
