using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using ReLogic.Graphics;

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
