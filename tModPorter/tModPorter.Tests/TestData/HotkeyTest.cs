using Terraria.ModLoader;

public class HotkeyTest : Mod
{
	public static ModHotKey CustomHotkey;

	void Method() {
		CustomHotkey = this.RegisterHotKey("Custom Hotkey", "C");
	}
}