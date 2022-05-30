using Terraria.ModLoader;

public class ModHotKeyTest : Mod
{
	public static ModHotKey CustomHotkey;

	void Method() {
		Mod mod = this;
		CustomHotkey = mod.RegisterHotKey("Custom Hotkey", "C");

		CustomHotkey = RegisterHotKey("Custom Hotkey Via Identifier", "C");
	}
}