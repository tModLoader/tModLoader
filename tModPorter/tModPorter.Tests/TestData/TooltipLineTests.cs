using Microsoft.Xna.Framework;
using Terraria.ModLoader;

public class TooltipLineTests
{
	void Method() {
		TooltipLine line = new TooltipLine(null, "", "");
		string mod = line.mod;
		line.text = "";
		line.isModifier = true;
		line.isModifierBad = false;
		line.overrideColor = null;

		line = new TooltipLine(null, "", "") {
			text = "",
			isModifier = true,
			isModifierBad = false,
			overrideColor = null
		};

		line.IsModifier = true;
		line.IsModifierBad = false;
		line.OverrideColor = null;

		DrawableTooltipLine drawableTooltipLine = new DrawableTooltipLine(line, 0, 0, 0, Color.White);
		_ = drawableTooltipLine.IsModifier == true;
		_ = drawableTooltipLine.IsModifierBad == false;
		_ = drawableTooltipLine.OverrideColor == null;
		_ = drawableTooltipLine.Color == Color.White;
	}
}