using Terraria.ModLoader;

public class HotkeyTest : Mod
{
	public static ModKeybind CustomHotkey;

	void Method() {
		Mod mod = this;
		CustomHotkey = KeybindLoader.RegisterKeybind(mod, "Custom Hotkey", "C");

		CustomHotkey = KeybindLoader.RegisterKeybind(this, "Custom Hotkey Via Identifier", "C");
	}
}