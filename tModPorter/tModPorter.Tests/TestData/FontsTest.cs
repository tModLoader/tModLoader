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

		var len = Main.fontDeathText.MeasureString("");

		int index = 0;
		font = Main.fontCombatText[index];
	}
}