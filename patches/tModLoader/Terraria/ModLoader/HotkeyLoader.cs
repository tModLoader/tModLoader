using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Terraria.ModLoader
{
	public sealed class HotkeyLoader : Loader
	{
		internal static readonly IDictionary<string, ModHotkey> modHotkeys = new Dictionary<string, ModHotkey>();

		internal static IEnumerable<ModHotkey> Hotkeys => modHotkeys.Values;

		internal override void Unload() {
			modHotkeys.Clear();
		}

		/// <summary>
		/// Registers a hotkey with a <paramref name="name"/> and <paramref name="defaultBinding"/>. Use the returned <see cref="ModHotkey"/> to detect when buttons are pressed.
		/// </summary>
		/// <param name="mod"> The mod that this hotkey will belong to. Usually, this would be your mod instance. </param>
		/// <param name="name"> The name of the keybind. </param>
		/// <param name="defaultBinding"> The default binding. </param>
		public static ModHotkey RegisterHotkey(Mod mod, string name, Keys defaultBinding)
			=> RegisterHotkey(mod, name, defaultBinding.ToString());

		/// <summary>
		/// Registers a hotkey with a <paramref name="name"/> and <paramref name="defaultBinding"/>. Use the returned <see cref="ModHotkey"/> to detect when buttons are pressed.
		/// </summary>
		/// <param name="mod"> The mod that this hotkey will belong to. Usually, this would be your mod instance. </param>
		/// <param name="name"> The name of the keybind. </param>
		/// <param name="defaultBinding"> The default binding. </param>
		public static ModHotkey RegisterHotkey(Mod mod, string name, string defaultBinding) {
			if (mod == null)
				throw new ArgumentNullException(nameof(mod));

			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException(nameof(name));

			if (string.IsNullOrWhiteSpace(defaultBinding))
				throw new ArgumentNullException(nameof(defaultBinding));

			return RegisterHotkey(new ModHotkey(mod, name, defaultBinding));
		}

		internal static ModHotkey RegisterHotkey(ModHotkey hotkey) {
			modHotkeys[hotkey.uniqueName] = hotkey;

			return hotkey;
		}
	}
}
