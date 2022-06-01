using Terraria;
using Terraria.ModLoader;
using ReLogic.Graphics;

public class FontsTest : Mod
{
	void Method() {
		DynamicSpriteFont font = null;

		font = Main.fontDeathText;
		font = Main.fontItemText;
		font = Main.fontMouseText;

		int index = 0;
		font = Main.fontCombatText[index];
	}
}
