using System;
using System.Collections.Generic;

namespace Terraria.ModLoader
{
	internal static class HotKeyLoader
	{
		internal static readonly IDictionary<string, ModHotKey> modHotKeys = new Dictionary<string, ModHotKey>();

		internal static IEnumerable<ModHotKey> HotKeys => modHotKeys.Values;

		internal static ModHotKey RegisterHotKey(ModHotKey hotkey) {
			modHotKeys[hotkey.uniqueName] = hotkey;
			return hotkey;
		}

		internal static void Unload() {
			modHotKeys.Clear();
		}
	}
}
