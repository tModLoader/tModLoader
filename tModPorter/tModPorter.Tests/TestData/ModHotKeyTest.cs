using Terraria;
using Terraria.ModLoader;

public class ModHotKeyTest : Mod
{
	public static ModHotKey CustomHotkey;

	void Method(Item item) {
		Mod mod = this;
		CustomHotkey = mod.RegisterHotKey("Custom Hotkey", "C");
		CustomHotkey = RegisterHotKey("Custom Hotkey", "C");
		CustomHotkey = item.ModItem.Mod.RegisterHotKey("Custom Hotkey", "C");
		CustomHotkey = /* comment */mod.RegisterHotKey("Custom Hotkey", "C");
		CustomHotkey = /* comment */RegisterHotKey("Custom Hotkey", "C");

		// comment duplication test
		/* comment */mod.RegisterHotKey("Custom Hotkey", "C");
	}

	public override void HotKeyPressed(string name) { /* Empty */ }
}