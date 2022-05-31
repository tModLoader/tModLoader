using Terraria.ModLoader;
using ReLogic.Graphics;
using Terraria.GameContent;

public class FontsTest : Mod
{
	void Method() {
		DynamicSpriteFont font = null;

		font = FontAssets.DeathText.Value;
		font = FontAssets.ItemStack.Value;
		font = FontAssets.MouseText.Value;

		int index = 0;
		font = FontAssets.CombatText[index].Value;
	}
}
