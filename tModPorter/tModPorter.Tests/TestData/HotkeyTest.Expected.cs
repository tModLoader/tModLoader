using Terraria.ModLoader;

public class HotkeyTest : Mod
{
	public static ModKeybind CustomHotkey;

	void Method() {
		CustomHotkey = KeybindLoader.RegisterKeybind(this, "Custom Hotkey", "C");
	}
}