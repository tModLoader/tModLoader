using Terraria;
using Terraria.ModLoader;

public class ModHotKeyTest : Mod
{
	public static ModKeybind CustomHotkey;

	void Method(Item item) {
		Mod mod = this;
		CustomHotkey = KeybindLoader.RegisterKeybind(mod, "Custom Hotkey", "C");
		CustomHotkey = KeybindLoader.RegisterKeybind(this, "Custom Hotkey", "C");
		CustomHotkey = KeybindLoader.RegisterKeybind(item.ModItem.Mod, "Custom Hotkey", "C");
		CustomHotkey = /* comment */KeybindLoader.RegisterKeybind(mod, "Custom Hotkey", "C");
		CustomHotkey = /* comment */KeybindLoader.RegisterKeybind(this, "Custom Hotkey", "C");

		// comment duplication test
		/* comment */KeybindLoader.RegisterKeybind(mod, "Custom Hotkey", "C");
	}

#if COMPILE_ERROR
	public override void HotKeyPressed(string name)/* tModPorter Note: Removed. Use ModPlayer.ProcessTriggers */ { /* Empty */ }
#endif
}